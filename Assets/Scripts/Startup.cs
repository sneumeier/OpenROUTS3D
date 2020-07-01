using System;
using System.IO;
using UnityEngine;
using MainMenuScripts;

class Startup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoadRuntimeMethod()
    {
        // Initialize default values
        if(!PlayerPrefs.HasKey("resolutionX"))
        {
            Settings.resolutionX = Screen.currentResolution.width;
        }
        if(!PlayerPrefs.HasKey("resolutionY"))
        {
            Settings.resolutionY = Screen.currentResolution.height;
        }
        if(!PlayerPrefs.HasKey("videoMode"))
        {
            Settings.videoMode = 1;
        }
        if(!PlayerPrefs.HasKey("vSync"))
        {
            Settings.vSync = true;
        }
        if(!PlayerPrefs.HasKey("targetFPS"))
        {
            Settings.targetFPS = 60;
        }
        if(!PlayerPrefs.HasKey("graphics"))
        {
            Settings.graphics = 2;
        }
        if(!PlayerPrefs.HasKey("quality"))
        {
            Settings.quality = 3;
        }
        if(!PlayerPrefs.HasKey("volume"))
        {
            Settings.volume = 1.0f;
        }

        EnsureDefaultDirectoriesAreInitialized();
        LoadConfig();

        if(Application.isEditor)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
        
        SettingsMenu.ApplySettings();
    }

    static void EnsureDefaultDirectoriesAreInitialized()
    {
        int platform = (int)Environment.OSVersion.Platform;
        bool b_unix = (platform == 4) || (platform == 6) || (platform == 128);
        string str_default_path = b_unix ?
            "~/THI/osds3" :
            Environment.GetEnvironmentVariable("USERPROFILE") + "\\THI\\osds3";

        Settings.defaultPath = str_default_path;
        if (!Directory.Exists(Settings.defaultPath))
        {
            Directory.CreateDirectory(Settings.defaultPath);
        }

        if (!PlayerPrefs.HasKey("lastLogPath") || !Directory.Exists(Settings.lastLogPath))
        {
            Debug.Log("Last configured log dir is: '" + Settings.lastLogPath + "'");
            Settings.lastLogPath = Settings.defaultPath;
            Debug.Log("New config dir is: '" + Settings.lastConfigPath + "'"); 
        }

        if (!PlayerPrefs.HasKey("lastConfigPath") || !Directory.Exists(Settings.lastConfigPath))
        {
            String configPath = Path.Combine(Application.dataPath, "Settings.ini");
            // Config File is either created and then stored in the default directory
            // or already shipped with the program
            Debug.Log("Last configured config dir is: '" + Settings.lastConfigPath + "'"); 
            
            if(File.Exists(configPath))
            {
                Settings.lastConfigPath = Application.dataPath;
                Debug.Log("New config dir is: '" + Settings.lastConfigPath + "'"); 
            }
            else
            {
                Settings.lastConfigPath = Settings.defaultPath;
                Debug.Log("New config dir is: '" + Settings.lastConfigPath + "'"); 
            }  
        }
    }

    static void LoadConfig()
    {
        // Resolution X
        try
        {
            int resX = Settings.resolutionX;
            resX = int.Parse(ConfigFile.IniReadValue(ConfigFile.Sections.General, ConfigFile.Keys.resolutionX));
            if(resX >= 800)
            {
                Settings.resolutionX = resX;
            }
            else
            {
                ConfigFile.IniWriteValue
                (
                    ConfigFile.Sections.General, 
                    ConfigFile.Keys.resolutionX, 
                    Settings.resolutionX.ToString()
                );
            } 
        }
        catch (Exception)
        {
            ConfigFile.IniWriteValue
            (
                ConfigFile.Sections.General, 
                ConfigFile.Keys.resolutionX, 
                Settings.resolutionX.ToString()
            );
        }

        // Resolution Y
        try
        {
            int resY = Settings.resolutionY;
            resY = int.Parse(ConfigFile.IniReadValue(ConfigFile.Sections.General, ConfigFile.Keys.resolutionY));
            if(resY >= 600)
            {
                Settings.resolutionY = resY;
            }
            else
            {
                ConfigFile.IniWriteValue
                (
                    ConfigFile.Sections.General, 
                    ConfigFile.Keys.resolutionY, 
                    Settings.resolutionY.ToString()
                );
            } 
        }
        catch (Exception)
        {
            ConfigFile.IniWriteValue
            (
                ConfigFile.Sections.General, 
                ConfigFile.Keys.resolutionY, 
                Settings.resolutionY.ToString()
            );
        }

        // Video Mode
        try
        {
            int vMode = Settings.videoMode;
            vMode = int.Parse(ConfigFile.IniReadValue(ConfigFile.Sections.General, ConfigFile.Keys.videoMode));
            if(vMode >= 0 && vMode <= 3)
            {
                Settings.videoMode = vMode;
            }
            else
            {
                ConfigFile.IniWriteValue
                (
                    ConfigFile.Sections.General, 
                    ConfigFile.Keys.videoMode, 
                    Settings.videoMode.ToString()
                );
            } 
        }
        catch (Exception)
        {
            ConfigFile.IniWriteValue
            (
                ConfigFile.Sections.General, 
                ConfigFile.Keys.videoMode, 
                Settings.videoMode.ToString()
            );
        }

        // V-Sync
        try
        {
            bool sync = Settings.vSync;
            sync = Boolean.Parse(ConfigFile.IniReadValue(ConfigFile.Sections.General, ConfigFile.Keys.vSync));
            Settings.vSync = sync;
        }
        catch (Exception)
        {
            ConfigFile.IniWriteValue
            (
                ConfigFile.Sections.General, 
                ConfigFile.Keys.vSync, 
                Settings.vSync.ToString()
            );
        }

        // Target FPS
        try
        {
            int fps = Settings.targetFPS;
            fps = int.Parse(ConfigFile.IniReadValue(ConfigFile.Sections.General, ConfigFile.Keys.targetFPS));
            if(fps >= 30 && fps <= 200)
            {
                Settings.targetFPS = fps;
            }
            else
            {
                ConfigFile.IniWriteValue
                (
                    ConfigFile.Sections.General, 
                    ConfigFile.Keys.targetFPS, 
                    Settings.targetFPS.ToString()
                );
            } 
        }
        catch (Exception)
        {
            ConfigFile.IniWriteValue
            (
                ConfigFile.Sections.General, 
                ConfigFile.Keys.targetFPS, 
                Settings.targetFPS.ToString()
            );
        }

        // Graphics
        try
        {
            int gfx = Settings.graphics;
            gfx = int.Parse(ConfigFile.IniReadValue(ConfigFile.Sections.General, ConfigFile.Keys.graphics));
            if(gfx >= 0 && gfx <= 2)
            {
                Settings.graphics = gfx;
            }
            else
            {
                ConfigFile.IniWriteValue
                (
                    ConfigFile.Sections.General, 
                    ConfigFile.Keys.graphics, 
                    Settings.graphics.ToString()
                );
            }      
        }
        catch (Exception)
        {
            ConfigFile.IniWriteValue
            (
                ConfigFile.Sections.General, 
                ConfigFile.Keys.graphics, 
                Settings.graphics.ToString()
            );
        }

        // Quality
        try
        {
            int qual = Settings.quality;
            qual = int.Parse(ConfigFile.IniReadValue(ConfigFile.Sections.General, ConfigFile.Keys.quality));
            if(qual >= 0 && qual <= 5)
            {
                Settings.quality = qual;
            }
            else
            {
                ConfigFile.IniWriteValue
                (
                    ConfigFile.Sections.General, 
                    ConfigFile.Keys.quality, 
                    Settings.quality.ToString()
                );
            }
        }
        catch (Exception)
        {
            ConfigFile.IniWriteValue
            (
                ConfigFile.Sections.General, 
                ConfigFile.Keys.quality, 
                Settings.quality.ToString()
            );
        }

        // Volume
        try
        {
            float vol = Settings.volume;
            vol = float.Parse(ConfigFile.IniReadValue(ConfigFile.Sections.General, ConfigFile.Keys.volume));
            if(vol >= 0 && vol <= 1)
            {
                Settings.volume = vol;
            }
            else
            {
                ConfigFile.IniWriteValue
                (
                    ConfigFile.Sections.General, 
                    ConfigFile.Keys.volume, 
                    Settings.volume.ToString()
                );
            }          
        }
        catch (Exception)
        {
            ConfigFile.IniWriteValue
            (
                ConfigFile.Sections.General, 
                ConfigFile.Keys.volume, 
                Settings.volume.ToString()
            );
        }

        // Log Directory
        try
        {
            string log = Settings.lastLogPath;
            log = ConfigFile.IniReadValue(ConfigFile.Sections.General, ConfigFile.Keys.logDirectory);
            if(Directory.Exists(log))
            {
                Settings.lastLogPath = log;
            }
            else{
                ConfigFile.IniWriteValue
                (
                    ConfigFile.Sections.General, 
                    ConfigFile.Keys.logDirectory, 
                    Settings.lastLogPath
                );
            }
        }
        catch (Exception)
        {
            ConfigFile.IniWriteValue
            (
                ConfigFile.Sections.General, 
                ConfigFile.Keys.logDirectory, 
                Settings.lastLogPath
            );
        }
    }
}