using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public int CurrentFPS { get; private set; }
    public int HighestFPS { get; private set; }
    public int LowestFPS { get; private set; }

    int highest = 0;
    int lowest = int.MaxValue;

    void Update()
    {

        CurrentFPS = (int)(1f / Time.unscaledDeltaTime);
        if (CurrentFPS > highest)
        {
            highest = CurrentFPS;
        }
        if (CurrentFPS < lowest && CurrentFPS > 0)  //so we don't log 0 FPS as lowest value
        {
            lowest = CurrentFPS;
        }
        HighestFPS = highest;
        LowestFPS = lowest;
    }
    
}
