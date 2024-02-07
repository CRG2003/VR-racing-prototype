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
        if (mil > 0.1f) {
            mils = "0" + Math.Round(mil * 10).ToString();
        }
        else {
            mils = Math.Round(mil * 10).ToString();
        }

        timer.text = mins + ":" + secs + ":" + mils;


    }

    string format(int i) {
        if (i < 10) {
            return "0" + i.ToString();
        }
        else {
            return i.ToString();
        }
    }
}
