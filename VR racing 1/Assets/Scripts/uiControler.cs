using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class uiControler : MonoBehaviour
{
    // game objects
    public TMP_Text timer;
    public TMP_Text speedo;
    public TMP_Text revs;
    public GameObject checkpoints;
    public GameObject player;

    List<GameObject> cPoints = new List<GameObject>();

    // simple variables 
    float mil, returnTimer = 0;
    int min, sec, curPoint = 0;
    string mins, secs, mils;
    bool disp = true;


    void Start()
    {
        foreach (Transform child in checkpoints.transform) {
            cPoints.Add(child.gameObject);
        }
    }

    void Update()
    {
        mil += Time.deltaTime;
        if (mil > 1){
            sec++;
            mil -= 1;
        }
        if (sec > 59){
            min++;
            sec -= 60;
        }

        mins = format(min);
        secs = format(sec);

        mils = format(mil * 1000);
        if (mil < .1f){
            mils = "0" + mils;
        }

        returnTimer -= Time.deltaTime;
        if (returnTimer < 0) {
            disp = true;
        }

        if (disp) {
            timer.text = mins + ":" + secs + ":" + mils;
        }
        speedo.text = ((int)player.GetComponent<CarControllerNew>().speed).ToString() + " mph";
        revs.text = ((int)player.GetComponent<CarControllerNew>().motorRPM).ToString() + " rpm";

        Debug.Log(cPoints[curPoint].GetComponent<checkpoint>().passed);
        if (cPoints[curPoint].GetComponent<checkpoint>().passed) {
            disp = false;
            if (curPoint != cPoints.Count - 1) {
                curPoint++;
                returnTimer = 4.0f;
            }
            else {
                returnTimer = 999;
            }
        }

    }

    string format(float i) {
        if (i < 10) {
            if (i == 0){
                return "00";
            }
            return "0" + Math.Round(i).ToString();
        }
        else {
            return Math.Round(i).ToString();
        }
    }
}
