using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControllerNew : MonoBehaviour
{
    public Transform car;
    public Transform wfl, wfr, wbl, wbr;

    public float curTorque = 100f, powerShift = 100f, motorRPM = 0f;
    public bool brake, shift, neutral = true, backwards = false;

    private float steer = 0f, accel = 0f, lastSpeed = -10f;
    private float shiftTime = 0f, shiftDelay = 0f
    private bool shiftMotor;

    public class wheelSetting{
        public float radius = .4f, weight = 1000f, dist = 0f;
    }


    public class carSettings{
        public Transform steer;
        public Ground[] ground;

        // suspension
        public float springs = 25000f, dampers = 1500f;

        // power
        public float carPower = 120f, shiftPower = 150f, brakePower = 8000f;

        // RPM
        public float shiftDownRPM = 1500f, shiftUpRPM = 2500f, idleRPM = 500f;

        public float maxSteerAngle = 25f;

        public float[] gears = {-10f, 9f, 6f, 4.5f, 3, 2.5f};

        // speed limits
        public float limitForward = 220f, limitBackwards = 60f;
    }


    public class Ground{
        public string tag = "ground";
        public bool grounded = false;
    }

    void Awake()
    {
        
    }

    void Update()
    {
        
    }
}
