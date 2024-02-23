using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class uiControler : MonoBehaviour
{
    public TMP_Text timer;
    float mil;
    int min, sec;
    string mins, secs, mils;


    void Start()
    {
        timer = GameObject.Find("timer").GetComponent<TMP_Text>();
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

        timer.text = mins + ":" + secs + ":" + mils;


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
