using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;
using Assets.Scripts.WeatherScripts;

public class WeatherObject : MonoBehaviour
{
    // This is the player object. It's the Car Root
    public GameObject player;
    private StatisticsContainer carStatistic;
    private const float defaultStiffness = 2;
    private const float defaultBackWheelStiffnessFactor = 1;

    // This are the effects on the window of the car
    public GameObject carWindowEffects;
    public GameObject carWindowEffectsRain;

    // This is to control the sunlight
    // "sun" is assigned via the inspector
    // Wel'll get the "sunlight component in the Start() function
    public GameObject sun;
    public Light sunlight;

    // These are the skybox Materials
    public Material skyboxDefault;
    public Material skyboxCloudyLightRays;
    public Material skyboxDarkStormy;
    public Material skyboxFullMoon;
    public Material skyboxSunSet;
    public Material skyboxThickCloudsWater;
    public Material skyboxTropicalSunnyDay;

    // Following are objects containing particle systems for the weather conditions
    // The first object contains no particle system and is used to contain subsets of each weather condition
    public GameObject rain;
    public GameObject rainLight;
    public GameObject rainHeavy;
    public GameObject rainHeavyBigDrops;
    public GameObject splashesAndRipples;

    public GameObject snow;
    public GameObject snowSlowLight;
    public GameObject snowSlowHeavy;
    public GameObject snowFast;
    public GameObject meltingSnowFlakesSlowLight;
    public GameObject meltingSnowFlakesSlowHeavy;
    public GameObject meltingSnowFlakesFast;

    public GameObject fog;
    public GameObject fogLight;
    public GameObject fogMiddle;
    public GameObject fogHeavy;

    public GameObject lightning;
    public GameObject lightningThin;
    public GameObject lightningMedium;
    public GameObject lightningBig;


    // Enable each weather condition at the beginning
    void Start()
    {
        carStatistic = player.GetComponent<StatisticsContainer>();
        SetCarStiffness(defaultStiffness, defaultBackWheelStiffnessFactor);
        sunlight = sun.GetComponent<Light>();

        SetDefaultWeather();
        //SetWeather_HeavySnowfallBrightSky();
    }

    // We want the weather to follow the player
    // This is for performance reason
    void Update()
    {
        Vector3 playerPosition = player.transform.position;
        Vector3 updatedWeatherPosition = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);
        this.transform.position = updatedWeatherPosition;
    }


    /* These functions reset the weather to a sunny day */
    public void SetDefaultWeather()
    {
        ResetLight();

        DisableAllWeatherConditions();

        DisableCarWindowEffects();

        RenderSettings.skybox = skyboxTropicalSunnyDay;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
    }


    public void ResetLight()
    {
        RenderSettings.fog = false;
        RenderSettings.ambientIntensity = 1f;

        sunlight.intensity = 1f;
        sunlight.color = new Color32(255, 244, 214, 255);
    }

    public void SetWeatherByWeatherTrigger(WeatherTrigger trigger)
    {
        SetCarStiffness(defaultStiffness, defaultBackWheelStiffnessFactor);
        ResetLight();
        DisableAllWeatherConditions();
        DisableCarWindowEffects();

        switch (trigger.Skybox)
        {
            case SkyboxType.skyboxCloudyLightRays:
                RenderSettings.skybox = skyboxCloudyLightRays;
                break;
            case SkyboxType.skyboxDarkStormy:
                RenderSettings.skybox = skyboxDarkStormy;
                break;
            case SkyboxType.skyboxDefault:
                RenderSettings.skybox = skyboxDefault;
                break;
            case SkyboxType.skyboxFullMoon:
                RenderSettings.skybox = skyboxFullMoon;
                break;
            case SkyboxType.skyboxSunSet:
                RenderSettings.skybox = skyboxSunSet;
                break;
            case SkyboxType.skyboxThickCloudsWater:
                RenderSettings.skybox = skyboxThickCloudsWater;
                break;
            case SkyboxType.skyboxTropicalSunnyDay:
                RenderSettings.skybox = skyboxTropicalSunnyDay;
                break;
        }

        changeWeatherSound(rain, trigger.RainLevel);

        switch (trigger.RainLevel)
        {
            case 1:
                SetCarStiffness(2, 0.9f);
                rain.SetActive(true);
                rainLight.SetActive(true);
                rainHeavy.SetActive(false);
                rainHeavyBigDrops.SetActive(false);
                splashesAndRipples.SetActive(true);
                break;
            case 2:
                SetCarStiffness(2, 0.8f);
                rain.SetActive(true);
                rainLight.SetActive(true);
                rainHeavy.SetActive(false);
                rainHeavyBigDrops.SetActive(false);
                splashesAndRipples.SetActive(false);
                break;
            case 3:
                SetCarStiffness(1.8f, 0.8f);
                rain.SetActive(true);
                rainLight.SetActive(true);
                rainHeavy.SetActive(true);
                rainHeavyBigDrops.SetActive(false);
                splashesAndRipples.SetActive(true);
                break;
            case 4:
                SetCarStiffness(1.7f, 0.8f);
                rain.SetActive(true);
                rainLight.SetActive(true);
                rainHeavy.SetActive(true);
                rainHeavyBigDrops.SetActive(true);
                splashesAndRipples.SetActive(true);
                break;

            default:
                rain.SetActive(false);
                rainLight.SetActive(false);
                rainHeavy.SetActive(false);
                rainHeavyBigDrops.SetActive(false);
                splashesAndRipples.SetActive(false);
                break;
        }

        switch (trigger.SnowLevel)
        {
            default:
                snow.SetActive(false);
                snowSlowLight.SetActive(false);
                snowSlowHeavy.SetActive(false);
                snowFast.SetActive(false);
                meltingSnowFlakesSlowLight.SetActive(false);
                meltingSnowFlakesSlowHeavy.SetActive(false);
                meltingSnowFlakesFast.SetActive(false);
                break;
            case 1:
                SetCarStiffness(1.8f, 0.8f);
                snow.SetActive(false);
                snowSlowLight.SetActive(true);
                snowSlowHeavy.SetActive(false);
                snowFast.SetActive(false);
                meltingSnowFlakesSlowLight.SetActive(true);
                meltingSnowFlakesSlowHeavy.SetActive(false);
                meltingSnowFlakesFast.SetActive(false);
                break;
            case 2:
                SetCarStiffness(1.7f, 0.7f);
                snow.SetActive(true);
                snowSlowLight.SetActive(true);
                snowSlowHeavy.SetActive(false);
                snowFast.SetActive(false);
                meltingSnowFlakesSlowLight.SetActive(true);
                meltingSnowFlakesSlowHeavy.SetActive(true);
                meltingSnowFlakesFast.SetActive(false);
                break;
            case 3:
                SetCarStiffness(1.6f, 0.6f);
                snow.SetActive(true);
                snowSlowLight.SetActive(true);
                snowSlowHeavy.SetActive(true);
                snowFast.SetActive(false);
                meltingSnowFlakesSlowLight.SetActive(true);
                meltingSnowFlakesSlowHeavy.SetActive(true);
                meltingSnowFlakesFast.SetActive(true);
                break;
            case 4:
                SetCarStiffness(1.5f, 0.5f);
                snow.SetActive(true);
                snowSlowLight.SetActive(true);
                snowSlowHeavy.SetActive(true);
                snowFast.SetActive(true);
                meltingSnowFlakesSlowLight.SetActive(true);
                meltingSnowFlakesSlowHeavy.SetActive(true);
                meltingSnowFlakesFast.SetActive(true);
                break;
        }

        switch (trigger.FogLevel)
        {
            default:
                fog.SetActive(false);
                fogLight.SetActive(false);
                fogMiddle.SetActive(false);
                fogHeavy.SetActive(false);

                RenderSettings.fogDensity = 0.0f;
                RenderSettings.fog = false;
                break;

            case 1:
                fog.SetActive(false);
                fogLight.SetActive(true);
                fogMiddle.SetActive(false);
                fogHeavy.SetActive(false);

                RenderSettings.fogDensity = 0.003f;
                RenderSettings.fog = true;
                break;
            case 2:
                fog.SetActive(true);
                fogLight.SetActive(true);
                fogMiddle.SetActive(false);
                fogHeavy.SetActive(false);

                RenderSettings.fogDensity = 0.005f;
                RenderSettings.fog = true;
                break;
            case 3:
                fog.SetActive(true);
                fogLight.SetActive(true);
                fogMiddle.SetActive(true);
                fogHeavy.SetActive(false);

                RenderSettings.fogDensity = 0.009f;
                RenderSettings.fog = true;
                break;
            case 4:
                fog.SetActive(true);
                fogLight.SetActive(true);
                fogMiddle.SetActive(true);
                fogHeavy.SetActive(true);

                RenderSettings.fogDensity = 0.015f;
                RenderSettings.fog = true;
                break;
        }

        RenderSettings.ambientIntensity = trigger.AmbientIntensity;
        RenderSettings.ambientLight = trigger.AmbientColor;


    }

    public void DisableAllWeatherConditions()
    {
        rain.SetActive(false);
        rainLight.SetActive(false);
        rainHeavy.SetActive(false);
        rainHeavyBigDrops.SetActive(false);
        splashesAndRipples.SetActive(false);

        snow.SetActive(false);
        snowSlowLight.SetActive(false);
        snowSlowHeavy.SetActive(false);
        snowFast.SetActive(false);
        meltingSnowFlakesSlowLight.SetActive(false);
        meltingSnowFlakesSlowHeavy.SetActive(false);
        meltingSnowFlakesFast.SetActive(false);

        fog.SetActive(false);
        fogLight.SetActive(false);
        fogMiddle.SetActive(false);
        fogHeavy.SetActive(false);

        lightning.SetActive(false);
        lightningThin.SetActive(false);
        lightningMedium.SetActive(false);
        lightningBig.SetActive(false);

    }

    public void DisableCarWindowEffects()
    {
        carWindowEffects.SetActive(false);
        carWindowEffectsRain.SetActive(false);
    }

    public void SetCarStiffness(float stiffness, float backStiffnessFactor)
    {
        carStatistic.stiffness = stiffness;
        carStatistic.backWheelStiffnessFactor = backStiffnessFactor;
    }


    /* ***** Weather presets *****/
    public void SetWeather_DarkFoggyNight()
    {
        RenderSettings.skybox = skyboxFullMoon;

        RenderSettings.fog = true;
        RenderSettings.ambientIntensity = 0.2f;

        sunlight.intensity = 0.31f;
        sunlight.color = new Color32(159, 159, 171, 255);

        fog.SetActive(true);
        fogHeavy.SetActive(false);
    }

    public void SetWeather_DarkLightRain()
    {
        RenderSettings.skybox = skyboxDarkStormy;

        RenderSettings.ambientIntensity = 0.45f;

        sunlight.intensity = 0.3f;

        rain.SetActive(true);
        rainLight.SetActive(true);

        fog.SetActive(true);
        fogLight.SetActive(true);

        carWindowEffects.SetActive(true);
        carWindowEffectsRain.SetActive(true);
    }

    public void SetWeather_DarkHeavyRain()
    {
        RenderSettings.skybox = skyboxDarkStormy;

        RenderSettings.ambientIntensity = 0.45f;

        sunlight.intensity = 0.3f;

        rain.SetActive(true);
        rainLight.SetActive(true);
        rainHeavy.SetActive(true);
        rainHeavyBigDrops.SetActive(true);
        splashesAndRipples.SetActive(true);

        fog.SetActive(true);
        fogLight.SetActive(true);

        carWindowEffects.SetActive(true);
        carWindowEffectsRain.SetActive(true);
    }

    public void SetWeather_Thunderstorm()
    {
        RenderSettings.skybox = skyboxDarkStormy;

        RenderSettings.ambientIntensity = 0.45f;

        sunlight.intensity = 0.3f;

        rain.SetActive(true);
        rainLight.SetActive(true);
        rainHeavy.SetActive(true);
        rainHeavyBigDrops.SetActive(true);
        splashesAndRipples.SetActive(true);

        fog.SetActive(true);
        fogLight.SetActive(true);

        lightning.SetActive(true);
        lightningThin.SetActive(true);
        lightningMedium.SetActive(true);
        lightningBig.SetActive(true);

        carWindowEffects.SetActive(true);
        carWindowEffectsRain.SetActive(true);
    }

    public void SetWeather_SunnyDay()
    {
        SetDefaultWeather();
    }

    public void SetWeather_ClearNight()
    {
        RenderSettings.skybox = skyboxFullMoon;
        RenderSettings.ambientIntensity = 0.35f;

        sunlight.intensity = 0.7f;
        sunlight.color = new Color32(65, 70, 75, 255);
    }

    public void SetWeather_SunSet()
    {
        RenderSettings.skybox = skyboxSunSet;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;

        sunlight.intensity = 1.2f;
        sunlight.color = new Color32(188, 101, 12, 255);
    }

    public void SetWeather_HeavyFog()
    {
        RenderSettings.skybox = skyboxCloudyLightRays;
        RenderSettings.fogDensity = 0.009f;
        RenderSettings.fog = true;

        sunlight.intensity = 0.5f;
        sunlight.color = new Color32(188, 170, 119, 255);

        fog.SetActive(true);
        fogLight.SetActive(true);
        fogHeavy.SetActive(true);
    }

    public void SetWeather_MediumFog()
    {
        RenderSettings.skybox = skyboxCloudyLightRays;
        RenderSettings.fogDensity = 0.005f;
        RenderSettings.fog = true;

        sunlight.intensity = 0.5f;
        sunlight.color = new Color32(188, 170, 119, 255);

        fog.SetActive(true);
        fogLight.SetActive(true);
        fogMiddle.SetActive(true);
    }

    public void SetWeather_LightSnowfallBrightSky()
    {
        RenderSettings.skybox = skyboxCloudyLightRays;
        RenderSettings.ambientIntensity = 0.82f;

        sunlight.intensity = 0.7f;
        sunlight.color = new Color32(188, 170, 119, 255);

        snow.SetActive(true);
        snowSlowLight.SetActive(true);
        meltingSnowFlakesSlowLight.SetActive(true);
    }

    public void SetWeather_HeavySnowfallBrightSky()
    {
        RenderSettings.skybox = skyboxCloudyLightRays;
        RenderSettings.ambientIntensity = 0.82f;

        sunlight.intensity = 0.7f;
        sunlight.color = new Color32(188, 170, 119, 255);

        fog.SetActive(true);
        fogLight.SetActive(true);

        snow.SetActive(true);
        snowSlowLight.SetActive(true);
        meltingSnowFlakesSlowLight.SetActive(true);
        snowSlowHeavy.SetActive(true);
        meltingSnowFlakesSlowHeavy.SetActive(true);
        snowFast.SetActive(true);
        meltingSnowFlakesFast.SetActive(true);
    }

    public void SetWeather_LightSnowfallDark()
    {
        RenderSettings.skybox = skyboxDarkStormy;
        RenderSettings.ambientIntensity = 0.6f;

        RenderSettings.fogDensity = 0.003f;
        RenderSettings.fog = true;

        sunlight.intensity = 0.5f;
        sunlight.color = new Color32(188, 170, 119, 255);

        fog.SetActive(true);
        fogLight.SetActive(true);

        snow.SetActive(true);
        snowSlowLight.SetActive(true);
        meltingSnowFlakesSlowLight.SetActive(true);
    }

    public void SetWeather_HeavySnowfallDark()
    {
        RenderSettings.skybox = skyboxDarkStormy;
        RenderSettings.ambientIntensity = 0.5f;

        RenderSettings.fogDensity = 0.005f;
        RenderSettings.fog = true;

        sunlight.intensity = 0.4f;
        sunlight.color = new Color32(188, 170, 119, 255);

        fog.SetActive(true);
        fogLight.SetActive(true);
        fogMiddle.SetActive(true);

        snow.SetActive(true);
        snowSlowLight.SetActive(true);
        snowSlowHeavy.SetActive(true);
        snowFast.SetActive(true);
        meltingSnowFlakesSlowLight.SetActive(true);
        meltingSnowFlakesSlowHeavy.SetActive(true);
        meltingSnowFlakesFast.SetActive(true);
    }

    public void changeWeatherSound(GameObject parentObj, int level)
    {
        parentObj.GetComponent<AudioSource>().volume = 0.25f * level;
    }
} 
