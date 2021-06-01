using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] Text timer;
    LapCounter lapCounter;

    float bestTime;
    bool timeSaved;

    double currentTime;

    // Start is called before the first frame update
    void Start()
    {
        lapCounter = GetComponent<LapCounter>();

        bestTime = PlayerPrefs.GetFloat("FastestTime", 0);

        Debug.Log(bestTime);
    }

    // Update is called once per frame
    void Update()
    {
        timer.text = $" Time: {Time.realtimeSinceStartup:0.000} \n Best Time: {bestTime}";

        currentTime = Time.realtimeSinceStartup;

        if ((lapCounter.GetIsCompleted() && Time.realtimeSinceStartup < bestTime && !timeSaved) || bestTime == 0)
        {
            PlayerPrefs.SetFloat("FastestTime", Convert.ToSingle(Math.Round(currentTime, 3)));
            PlayerPrefs.Save();
            timeSaved = true;
        }
    }
}
