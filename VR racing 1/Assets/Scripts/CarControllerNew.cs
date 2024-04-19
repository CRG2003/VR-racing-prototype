using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarControllerNew : MonoBehaviour
{
    // Transforms
    public Transform car;
    public Transform wfl, wfr, wbl, wbr;


    // public variables
    public int curGear = 1;
    public float curTorque = 100f, powerShift = 100f, motorRPM = 0f, speed = 0f;
    public bool brake, shift, neutral = true, backwards = false;


    // private variables
    private float steer = 0f, accel = 0f, lastSpeed = -10f;
    private float shiftTime = 0f, shiftDelay = 0f, wantedRPM = 0f;
    private float w_rotate, slip, slip2 = 0f;
    private bool shiftMotor;


    private Rigidbody rb;
    private Vector3 steerCurAngle;
    private PlayerInput input;


    // efficiency table (used to effect motorRPM)
    float[] efficiencyTable = { 0.6f, 0.65f, 0.7f, 0.75f, 0.8f, 0.85f, 0.9f, 1.0f, 1.0f, 0.95f, 0.80f, 0.70f, 0.60f, 0.5f, 0.45f, 0.40f, 0.36f, 0.33f, 0.30f, 0.20f, 0.10f, 0.05f };
    float efficiencyTableStep = 250.0f;



    [System.Serializable]

    // aspects of the car itself (suspension and engine mostly)
    public class CarSettings{

        public Transform carSteer;
        public Ground[] ground;

        // suspension
        public float springs = 25000f, dampers = 15000f;

        // power
        public float carPower = 120f, shiftPower = 150f, brakePower = 8000f;

        // RPM
        public float shiftDownRPM = 1500f, shiftUpRPM = 2500f, idleRPM = 500f;

        public float maxSteerAngle = 25f;

        public float[] gears = {-10f, 9f, 6f, 4.5f, 3, 2.5f};

        // speed limits
        public float limitForward = 220f, limitBackwards = 60f;
    }

    public CarSettings carSettings;



    // aspects of the wheels
    private class wheelComponent    {

        public Transform wheel;
        public WheelCollider collider;
        public Vector3 startPos;
        public float rotation = 0f, rotation2 = 0f, maxSteer, pos_y = 0f;
        public bool drive;
    }

    private wheelComponent[] wheels;



    // function to set each wheels components
    private wheelComponent setWheel(Transform wheel, float maxSteer, bool drive, float pos_y){

        GameObject wheelCol = new GameObject(wheel.name + "WheelCollider");

        wheelCol.transform.parent = transform;
        wheelCol.transform.position = wheel.position;
        wheelCol.transform.eulerAngles = transform.eulerAngles;
        pos_y = wheelCol.transform.localPosition.y;

        WheelCollider col = (WheelCollider)wheelCol.AddComponent(typeof(WheelCollider));

        wheelComponent result = new wheelComponent();
        result.wheel = wheel;
        result.collider = wheelCol.GetComponent<WheelCollider>();
        result.drive = drive;
        result.pos_y = pos_y;
        result.maxSteer = maxSteer;
        result.startPos = wheelCol.transform.localPosition;

        return result;
    }



    public class Ground{
        public string tag = "ground";
        public bool grounded = false;
    }



    // runs once when object is loaded
    void Awake(){

        rb = transform.GetComponent<Rigidbody>();

        if (carSettings.carSteer) {
            steerCurAngle = carSettings.carSteer.localEulerAngles;
        }

        wheels = new wheelComponent[4];

        wheels[0] = setWheel(wfr, carSettings.maxSteerAngle, true, wfr.position.y);
        wheels[1] = setWheel(wfl, carSettings.maxSteerAngle, true, wfl.position.y);
        wheels[2] = setWheel(wbr, carSettings.maxSteerAngle, false, wbr.position.y);
        wheels[3] = setWheel(wbl, carSettings.maxSteerAngle, false, wbl.position.y);


        // sets settings and initial friction of each wheel
        foreach (wheelComponent w in wheels) {
            WheelCollider col = w.collider;
            col.suspensionDistance = 0f;
            JointSpring js = col.suspensionSpring;

            js.spring = carSettings.springs;
            js.damper = carSettings.dampers;
            col.suspensionSpring = js;

            col.radius = .4f;
            col.mass = 1000f;


            WheelFrictionCurve fc = col.forwardFriction;

            fc.asymptoteValue = 5000f;
            fc.asymptoteSlip = 20f;
            fc.extremumSlip = 2f;
            fc.stiffness = 2f;
            col.forwardFriction = fc;

            fc = col.sidewaysFriction;
            fc.asymptoteValue = 7500f;
            fc.asymptoteSlip = 2f;
            fc.stiffness = 2f;
            col.sidewaysFriction = fc;
        }

        input = GetComponent<PlayerInput>();
    }


    public void shiftUp() {
        float now = Time.timeSinceLevelLoad;

        if (now < shiftDelay) {
            return;
        }

        if (curGear < carSettings.gears.Length - 1) {
            curGear++;
        }

        shiftDelay = now + 1;
        shiftTime = 1.5f;
    }


    public void shiftDown() {
        float now = Time.timeSinceLevelLoad;

        if (now < shiftDelay) {
            return;
        }

        if (curGear > 0) {
            curGear--;
        }

        shiftDelay = now + 0.1f;
        shiftTime = 2;
    }


    void FixedUpdate(){
        speed = rb.velocity.magnitude * 2.7f;
        rb.centerOfMass = new Vector3(0, -0.8f, 0);
        
        if (speed < lastSpeed - 10 && slip < 10) {
            slip = lastSpeed / 15;
        }
        lastSpeed = speed;

        if (slip2 != 0) {
            slip2 = Mathf.MoveTowards(slip2, 0f, .1f);
        }


        // inputs
        steer = Mathf.MoveTowards(steer, input.actions.FindAction("steer").ReadValue<float>(), 0.2f);
        accel = input.actions.FindAction("Accel").ReadValue<float>();
        brake = Input.GetKey(KeyCode.Space);


        if (carSettings.carSteer){
            carSettings.carSteer.localEulerAngles = new Vector3(steerCurAngle.x, steerCurAngle.y, steerCurAngle.z + (steer * -120.0f));
        }

        if (curGear == 1 && accel < 0 && speed < 5) {
            shiftDown();
        }
        else if (curGear == 0 && accel > 0 && speed < 5) {
            shiftUp();
        }
        else if (motorRPM > carSettings.shiftUpRPM && accel > 0 && speed > 10 && !brake) {
            shiftUp();
        }
        else if (motorRPM < carSettings.shiftDownRPM && curGear > 1) {
            shiftDown();
        }



        wantedRPM = (5500.0f * accel) * 0.1f + wantedRPM * 0.9f;
        float rpm = 0;
        int motorWheels = 0;
        int curWheel = 0;


        foreach (wheelComponent w in wheels) {
            WheelHit hit;
            WheelCollider col = w.collider;

            if (w.drive) {
                if (brake && curGear < 2) {
                    rpm += accel * carSettings.idleRPM;
                }
                else {
                    rpm += col.rpm;
                }

                motorWheels++;
            }



            if (brake || accel < 0) {
                if ((accel < 0) || (brake && (w == wheels[2] || w == wheels[3]))) {

                    if (brake && (accel > 0)) {
                        slip = Mathf.Lerp(slip, 5, accel * 0.01f);
                    }
                    else if (speed > 1) {
                        slip = Mathf.Lerp(slip, 1, 0.002f);
                    }
                    else {
                        slip = Mathf.Lerp(slip, 1, 0.02f);
                    }

                    wantedRPM = 0;
                    col.brakeTorque = carSettings.brakePower;
                    w.rotation = w_rotate;
                }
            }


            else {
                col.brakeTorque = accel == 0 || neutral ? col.brakeTorque = 1000 : col.brakeTorque = 0;

                slip = speed > 0 ? (speed > 100 ? slip = Mathf.Lerp(slip, 1 + Mathf.Abs(steer), 0.02f) : slip = Mathf.Lerp(slip, 1.5f, 0.02f)) : slip = Mathf.Lerp(slip, 0.01f, 0.02f);

                w_rotate = w.rotation;
            }


            WheelFrictionCurve fc = col.forwardFriction;


            fc.asymptoteValue = 50000.0f;
            fc.extremumSlip = 2.0f;
            fc.asymptoteSlip = 20.0f;
            fc.stiffness = 2.0f / (slip + slip2);
            col.forwardFriction = fc;

            fc = col.sidewaysFriction;
            fc.stiffness = 2.0f / (slip + slip2);
            fc.extremumSlip = 0.2f + Mathf.Abs(steer);
            col.sidewaysFriction = fc;



            if (shift && curGear > 1 && speed > 50 && shiftMotor && Mathf.Abs(steer) < 0.2f) {

                if (powerShift == 0) {
                    shiftMotor = false;
                }
                powerShift = Mathf.MoveTowards(powerShift, 0, Time.deltaTime * 10);

                curTorque = powerShift > 0 ? carSettings.shiftPower : carSettings.carPower;
            }
            else {

                if (powerShift > 20) {
                    shiftMotor = true;
                }
                powerShift = Mathf.MoveTowards(powerShift, 100, Time.deltaTime * 10);   

                curTorque = carSettings.carPower;
            }

            if (!w.drive) {
                w.rotation2 = Mathf.Lerp(w.rotation2, col.steerAngle, 0.1f);
            }
            else {
                w.rotation2 = Mathf.Lerp(w.rotation2, -col.steerAngle / 5, 0.1f);
            }
            w.rotation = Mathf.Repeat(w.rotation + Time.deltaTime * col.rpm * 360 / 60, 360);
            w.wheel.localRotation = Quaternion.Euler(w.rotation, w.rotation2, 0);

            Vector3 lp = w.wheel.localPosition;



            if (col.GetGroundHit(out hit)) {
                lp.y -= Vector3.Dot(w.wheel.position - hit.point, transform.TransformDirection(0, 1, 0) / transform.lossyScale.x) - (col.radius);
                lp.y = Mathf.Clamp(lp.y, -10.0f, w.pos_y);
            }

            else {
                lp.y = w.startPos.y - 0.0f;
                rb.AddForce(Vector3.down * 5000);
            }

            curWheel++;
            w.wheel.localPosition = lp;
        }

        if (motorWheels > 1) {
            rpm = rpm / motorWheels;
        }



        motorRPM = 0.95f * motorRPM + 0.05f * Mathf.Abs(rpm * carSettings.gears[curGear]);
        if (motorRPM > 5500) motorRPM = 5200;


        int index = (int)(motorRPM / efficiencyTableStep);
        if (index >= efficiencyTable.Length) index = efficiencyTable.Length - 1;
        if (index < 0) index = 0;

        float newTorque = curTorque * carSettings.gears[curGear] * efficiencyTable[index];


        foreach (wheelComponent w in wheels) {
            WheelCollider col = w.collider;

            if (w.drive) {

                if (Mathf.Abs(col.rpm) > Mathf.Abs(wantedRPM)) {
                    col.motorTorque = 0;
                }

                else {
                    float curTorqueCol = col.motorTorque;

                    if (!brake && accel != 0) {
                        if ((speed < carSettings.limitForward) || (speed < carSettings.limitBackwards)) {
                            col.motorTorque = curTorqueCol * 0.9f + newTorque * 1f;
                        }
                        else {
                            col.motorTorque = 0f;
                            col.brakeTorque = 2000f;
                        }
                    }

                    else {
                        col.motorTorque = 0;
                    }
                }
            }


            if (brake || slip2 > 2) {
                col.steerAngle = Mathf.Lerp(col.steerAngle, steer * w.maxSteer, 0.02f);
            }
            else {
                float steerAngle = Mathf.Clamp(speed / carSettings.maxSteerAngle, 1.0f, carSettings.maxSteerAngle);
                col.steerAngle = steer * (w.maxSteer / steerAngle);
            }
        }
    }
}
