using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class Timer : MonoBehaviour {

    public static Timer instance;
    public Stopwatch stopwatch = new Stopwatch();

    private void Awake() {
        if (instance == null)
            instance = this;
    }


    public string ElapsedTimeAndRestart() {
        stopwatch.Stop();

        TimeSpan _timeSpan = stopwatch.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}", _timeSpan.Minutes, _timeSpan.Seconds, _timeSpan.Milliseconds);

        stopwatch.Reset();

        return elapsedTime;
    }
}
