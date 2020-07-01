using Assets.Scripts.AssetReplacement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings {

	public static bool paused
    {
        get
        {
            return (PlayerPrefs.GetInt("paused") == 1);
        }
        set
        {
            if (value)
            {
                PlayerPrefs.SetInt("paused", 1);
            }
            else
            {
                PlayerPrefs.SetInt("paused", 0);
            }
        }
    }

    public static bool isMPServer
    {
        get
        {
            return (PlayerPrefs.GetInt("isMPServer") == 1);
        }
        set
        {
            if (value)
            {
                PlayerPrefs.SetInt("isMPServer", 1);
            }
            else
            {
                PlayerPrefs.SetInt("isMPServer", 0);
            }
        }
    }

    public static bool isHost
    {
        get
        {
            return (PlayerPrefs.GetInt("isHost") == 1);
        }
        set
        {
            if (value)
            {
                PlayerPrefs.SetInt("isHost", 1);
            }
            else
            {
                PlayerPrefs.SetInt("isHost", 0);
            }
        }
    }
    
    public static bool isSumoServer
    {
        get
        {
            return (PlayerPrefs.GetInt("isSumoServer") == 1);
        }
        set
        {
            if (value)
            {
                PlayerPrefs.SetInt("isSumoServer", 1);
            }
            else
            {
                PlayerPrefs.SetInt("isSumoServer", 0);
            }
        }
    }

    public static bool automatedPlay
    {
        get
        {
            return (PlayerPrefs.GetInt("automatedPlay") == 1);
        }
        set
        {
            if (value)
            {
                PlayerPrefs.SetInt("automatedPlay", 1);
            }
            else
            {
                PlayerPrefs.SetInt("automatedPlay", 0);
            }
        }
    }

    public static List<AssetPack> LoadedAssetPacks;


    public static float frameBufferDelay
    {
        get
        {
            return PlayerPrefs.GetFloat("frameBufferDelay");
        }
        set
        {
            PlayerPrefs.SetFloat("frameBufferDelay", value);
        }
    }

    public static float scenarioTime
    {
        get
        {
            return PlayerPrefs.GetFloat("scenarioTime");
        }
        set
        {
            PlayerPrefs.SetFloat("scenarioTime", value);
        }
    }

    public static float packetLoss
    {
        get
        {
            return PlayerPrefs.GetFloat("packetLoss");
        }
        set
        {
            PlayerPrefs.SetFloat("packetLoss", value);
        }
    }

    public static bool skippableScenario
    {
        get
        {
            return (PlayerPrefs.GetInt("skippableScenario") == 1);
        }
        set
        {
            if (value)
            {
                PlayerPrefs.SetInt("skippableScenario", 1);
            }
            else
            {
                PlayerPrefs.SetInt("skippableScenario", 0);
            }
        }
    }


    public static bool sumoGui
    {
        get
        {
            return (PlayerPrefs.GetInt("sumoGui") == 1);
        }
        set
        {
            if (value)
            {
                PlayerPrefs.SetInt("sumoGui", 1);
            }
            else
            {
                PlayerPrefs.SetInt("sumoGui", 0);
            }
        }
    }

    public static bool dynamicDelay
    {
        get
        {
            return (PlayerPrefs.GetInt("dynamicDelay") == 1);
        }
        set
        {
            if (value)
            {
                PlayerPrefs.SetInt("dynamicDelay", 1);
            }
            else
            {
                PlayerPrefs.SetInt("dynamicDelay", 0);
            }
        }
    }

    public static int sumoPort
    {
        get
        {
            return PlayerPrefs.GetInt("sumoPort");
        }
        set
        {
            PlayerPrefs.SetInt("sumoPort", value);
        }
    }

    public static int multiplayerPort
    {
        get
        {
            return PlayerPrefs.GetInt("multiplayerPort");
        }
        set
        {
            PlayerPrefs.SetInt("multiplayerPort", value);
        }
    }

    public static float multiplayerColorBlue
    {
        get
        {
            return PlayerPrefs.GetFloat("multiplayerColorBlue");
        }
        set
        {
            PlayerPrefs.SetFloat("multiplayerColorBlue", value);
        }
    }

    public static float multiplayerColorGreen
    {
        get
        {
            return PlayerPrefs.GetFloat("multiplayerColorGreen");
        }
        set
        {
            PlayerPrefs.SetFloat("multiplayerColorGreen", value);
        }
    }

    public static float multiplayerColorRed
    {
        get
        {
            return PlayerPrefs.GetFloat("multiplayerColorRed");
        }
        set
        {
            PlayerPrefs.SetFloat("multiplayerColorRed", value);
        }
    }

    public static float displayTimeDelay
    {
        get
        {
            return PlayerPrefs.GetFloat("displayTimeDelay");
        }
        set
        {
            PlayerPrefs.SetFloat("displayTimeDelay", value);
        }
    }

    public static float inputTimeDelay
    {
        get
        {
            return PlayerPrefs.GetFloat("inputTimeDelay");
        }
        set
        {
            PlayerPrefs.SetFloat("inputTimeDelay", value);
        }
    }

    public static float screenFactor
    {
        get
        {
            return PlayerPrefs.GetFloat("screenFactor");
        }
        set
        {
            PlayerPrefs.SetFloat("screenFactor", value);
        }
    }

    public static string userName
    {
        get
        {
            return PlayerPrefs.GetString("userName");
        }
        set
        {
            PlayerPrefs.SetString("userName", value);
        }
    }

    public static string multiplayerIpAddress
    {
        get
        {
            return PlayerPrefs.GetString("multiplayerIpAddress");
        }
        set
        {
            PlayerPrefs.SetString("multiplayerIpAddress", value);
        }
    }

    public static string autorunXmlPath
    {
        get
        {
            return PlayerPrefs.GetString("autorunXmlPath");
        }
        set
        {
            PlayerPrefs.SetString("autorunXmlPath", value);
        }
    }

    public static bool enableScenario
    {
        get
        {
            return (PlayerPrefs.GetInt("enableScenario") == 1);
        }
        set
        {
            if (value)
            {
                PlayerPrefs.SetInt("enableScenario", 1);
            }
            else
            {
                PlayerPrefs.SetInt("enableScenario", 0);
            }
        }
    }

    public static string mapPath
    {
        get
        {
            return PlayerPrefs.GetString("mapPath");
        }
        set
        {
            PlayerPrefs.SetString("mapPath", value);
        }
    }
    
    public static string osmPath
    {
        get
        {
            return PlayerPrefs.GetString("mapPath").Replace(".net.xml", ".osm");
        }
    }

    public static string correctionPath
    {
        get
        {
            return PlayerPrefs.GetString("mapPath").Replace(".net.xml", ".correction.xml");
        }
    }


    public static string partialMapPath
    {
        get
        {
            string mapPath = PlayerPrefs.GetString("mapPath");
            string mapDelimiter = "Maps";
            string substr = mapPath.Substring(mapPath.IndexOf(mapDelimiter,0)+mapDelimiter.Length);
            return substr;
        }
    }

    public static string mapPrefix
    {
        get
        {
            string mapPath = PlayerPrefs.GetString("mapPath");
            Debug.Log("Map Path = "+mapPath);
            string mapDelimiter = "Maps";
            string substr = mapPath.Substring(0,mapPath.IndexOf(mapDelimiter, 0));
            return substr;
        }

    }

    public static string polyPath
    {
        get
        {
            return PlayerPrefs.GetString("polyPath");
        }
        set
        {
            PlayerPrefs.SetString("polyPath", value);
        }
    }

    public static string scenarioPath
    {
        get
        {
            return PlayerPrefs.GetString("scenarioPath");
        }
        set
        {
            PlayerPrefs.SetString("scenarioPath", value);
        }
    }

    public static string lastMapPath
    {
        get
        {
            return PlayerPrefs.GetString("lastMapPath");
        }
        set
        {
            PlayerPrefs.SetString("lastMapPath", value);
        }
    }

    public static string defaultPath
    {
        get
        {
            return PlayerPrefs.GetString("defaultPath");
        }
        set
        {
            PlayerPrefs.SetString("defaultPath", value);
        }
    }

    public static string lastLogPath
    {
        get 
        { 
            return PlayerPrefs.GetString("lastLogPath"); 
        }
        set 
        { 
            PlayerPrefs.SetString("lastLogPath", value); 
        }
    }

    public static string lastConfigPath
    {
        get 
        { 
            return PlayerPrefs.GetString("lastConfigPath"); 
        }
        set 
        { 
            PlayerPrefs.SetString("lastConfigPath", value); 
        }
    }

    // SettingsMenu

    public static int resolutionX
    {
        get
        {
            return PlayerPrefs.GetInt("resolutionX");
        }
        set
        {
            PlayerPrefs.SetInt("resolutionX", value);
        }
    }

    public static int resolutionY
    {
        get
        {
            return PlayerPrefs.GetInt("resolutionY");
        }
        set
        {
            PlayerPrefs.SetInt("resolutionY", value);
        }
    }

    public static int videoMode
    {
        get
        {
            return PlayerPrefs.GetInt("videoMode");
        }
        set
        {
            PlayerPrefs.SetInt("videoMode", value);
        }
    }

    public static bool vSync
    {
        get
        {
            return (PlayerPrefs.GetInt("vSync") == 1);
        }
        set
        {
            if (value)
            {
                PlayerPrefs.SetInt("vSync", 1);
            }
            else
            {
                PlayerPrefs.SetInt("vSync", 0);
            }
        }
    }

    public static int targetFPS
    {
        get
        {
            return PlayerPrefs.GetInt("targetFPS");
        }
        set
        {
            PlayerPrefs.SetInt("targetFPS", value);
        }
    }

    public static int quality
    {
        get
        {
            return PlayerPrefs.GetInt("quality");
        }
        set
        {
            PlayerPrefs.SetInt("quality", value);
        }
    }

    public static int graphics
    {
        get
        {
            return PlayerPrefs.GetInt("graphics");
        }
        set
        {
            PlayerPrefs.SetInt("graphics", value);
        }
    }

    public static string language
    {
        get
        {
            return PlayerPrefs.GetString("language");
        }
        set
        {
            PlayerPrefs.SetString("language", value);
        }
    }

    public static float volume
    {
        get
        {
            return PlayerPrefs.GetFloat("volume");
        }
        set
        {
            PlayerPrefs.SetFloat("volume", value);
        }
    }
}
