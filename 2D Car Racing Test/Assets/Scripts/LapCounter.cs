using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LapCounter : MonoBehaviour
{
    [SerializeField] int totalLaps = 4;

    int currentLap = 0;

    bool raceIsCompleted = false;

    private void Update()
    {
        if (currentLap > totalLaps)
            raceIsCompleted = true;
    }
    public void IncrementLap() => currentLap++;

    public bool GetIsCompleted() { return raceIsCompleted; }

}
