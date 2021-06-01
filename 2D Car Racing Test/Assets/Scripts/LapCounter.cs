using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LapCounter : MonoBehaviour
{
    [SerializeField] int totalLaps = 3;
    [SerializeField] Text lapCounter;

    int currentLap = 0;

    bool raceIsCompleted = false;

    private void Update()
    {
        if (currentLap > totalLaps)
            raceIsCompleted = true;

        if (currentLap == totalLaps)
            lapCounter.text = "Final Lap!";
        else
            lapCounter.text = $"Lap: {currentLap}/{totalLaps}";

    }
    public void IncrementLap() => currentLap++;

    public bool GetIsCompleted() { return raceIsCompleted; }

}
