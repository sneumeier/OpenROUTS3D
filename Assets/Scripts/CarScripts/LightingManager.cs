using SUMOConnectionScripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightingManager : MonoBehaviour
{
    // StatisticsContainer for data exchange
    public StatisticsContainer carStatistic;
    // SumoVehicle for data exchange
    public SumoVehicle sumoVehicle;

    public List<Light> frontLights;
    public List<Light> tailLights;

    public List<Light> brakeLights;

    public List<Light> indicatorLeftLights;
    public List<Light> indicatorRightLights;

    private bool isSumoCar;
    private bool frontTailLight;
    private bool brakeLight;

    public AudioSource indicatorSound;
    public AudioClip tikClip;
    public AudioClip tokClip;
    
    public void Start()
    {
        if(carStatistic != null)
        {
            isSumoCar = false;
        }else if(sumoVehicle != null)
        {
            isSumoCar = true;
        }
        if(sumoVehicle == null && carStatistic == null)
        {
            throw  new ArgumentException("The LightingManager needs a StatisticsContainer or a SomoVehicle");
        }
    }

    public void Update()
    {
        if (isSumoCar)
        {
            if(sumoVehicle.light != frontTailLight)
            {
                frontTailLight = sumoVehicle.light;
                LightOnOff(sumoVehicle.light, frontLights, 0, 15);
                LightOnOff(sumoVehicle.light, tailLights, 0, 4);
            }
            if(sumoVehicle.brakelight != brakeLight)
            {
                brakeLight = sumoVehicle.brakelight;
                LightOnOff(sumoVehicle.brakelight, brakeLights, 0, 4);
            }
            Indicator(sumoVehicle.indicatorLeft, sumoVehicle.indicatorRight, true);
        }
        else
        {
            LightOnOff(carStatistic.light, frontLights, 0, 15);
            LightOnOff(carStatistic.light, tailLights, 0, 4);
            LightOnOff(carStatistic.brake < 0, brakeLights, 0, 4);
            Indicator(carStatistic.indicatorLeft, carStatistic.indicatorRight, false);
        }
    }

    private float t = 0;
    private bool lastIndicatorState;

    void Indicator(bool left, bool right, bool sumoCar)
    {
        t += Time.deltaTime;
        float min = 0.4f;
        float max = 0.8f;
        if (left)
        {
            bool IndicatorLightOn = t < min ? false : true;
            if (!sumoCar && lastIndicatorState != IndicatorLightOn)
            {
                PlayIndicatorSound(IndicatorLightOn ? tikClip : tokClip);
            }
            lastIndicatorState = IndicatorLightOn;
            LightOnOff(IndicatorLightOn, indicatorLeftLights, 0, 6);
        }
        else
        {
            LightOnOff(false, indicatorLeftLights, 0, 6);
        }
        if (right)
        {
            bool IndicatorLightOn = t < min ? false : true;
            if (!sumoCar && lastIndicatorState != IndicatorLightOn)
            {
                PlayIndicatorSound(IndicatorLightOn ? tikClip : tokClip);
            }
            lastIndicatorState = IndicatorLightOn;
            LightOnOff(IndicatorLightOn, indicatorRightLights, 0, 6);
        }
        else
        {
            LightOnOff(false, indicatorRightLights, 0, 6);
        }
        if (t > max)
        {
            t = 0;
        }
        if(!right && !left)
        {
            if (t > min)
            {
                t = 0;
                lastIndicatorState = false;
            }
        }
    }

    void PlayIndicatorSound(AudioClip clip)
    {
        indicatorSound.clip = clip;
        indicatorSound.Play();
    }

    void LightOnOff(bool on, List<Light> lights, int low, int high)
    {
        foreach (var light in lights)
        {
            light.intensity = on ? high : low;
        }
    }
}
