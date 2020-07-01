using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using Assets.Scripts.CarScripts;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace MainMenuScripts
{
    public class SettingsMenu : MonoBehaviour 
    {
        public InputField resolutionXField;
        public InputField resolutionYField;
        public InputField targetFPSField;

        public Toggle vsyncToggle;

        public Dropdown videoModeSelector;
        public Dropdown graphicsSelector;
        public Dropdown qualitySelector;
        public Dropdown languageSelector;

        public Slider volumeSlider;

        public FileBrowser logDirectoryBrowser;

        public StatisticsLogger statisticsLogger;

        public InputActionAsset control;

        private int resolutionX;
        private int resolutionY;
        private int videoMode;
        private int graphics;
        private int quality;
        private int targetFPS;
        private bool vsync;
        private float volume;

        public void Start()
        {
            // Initialize local variables
            this.resolutionX = Settings.resolutionX;
            this.resolutionY = Settings.resolutionY;
            this.videoMode = Settings.videoMode;
            this.vsync = Settings.vSync;
            this.targetFPS = Settings.targetFPS;
            this.graphics = Settings.graphics;
            this.quality = Settings.quality;
            this.volume = Settings.volume;

            // Initialize Menu
            if(resolutionXField != null)
            {
                resolutionXField.text = this.resolutionX.ToString();
                resolutionXField.onValueChanged.AddListener(delegate { ApplyResolutionX(); });
            }
            if(resolutionYField != null)
            {
                resolutionYField.text = this.resolutionY.ToString();
                resolutionYField.onValueChanged.AddListener(delegate { ApplyResolutionY(); });
            }
            if(videoModeSelector != null)
            {
                videoModeSelector.value = this.videoMode;
                videoModeSelector.onValueChanged.AddListener(delegate { ApplyVideoMode(); });
            }
            if(vsyncToggle != null)
            {
                vsyncToggle.isOn = this.vsync;
                vsyncToggle.onValueChanged.AddListener(delegate { ApplyVsync(); });
            }
            if(targetFPSField != null)
            {
                targetFPSField.text = this.targetFPS.ToString();
                targetFPSField.onValueChanged.AddListener(delegate { ApplyTargetFPS(); });
            }
            if(graphicsSelector != null)
            {
                graphicsSelector.value = this.graphics;
                graphicsSelector.onValueChanged.AddListener(delegate { ApplyGraphics(); });
            }
            if(qualitySelector != null)
            {
                qualitySelector.value = this.quality;
                qualitySelector.onValueChanged.AddListener(delegate { ApplyQuality(); });
            }
            if(volumeSlider != null)
            {
                volumeSlider.value = this.volume;
                volumeSlider.onValueChanged.AddListener(delegate { ApplyVolume(); });
            }
            LockFields();
        }

        public void OnEnable()
        {
            if(resolutionXField != null)
            {
                resolutionXField.text = this.resolutionX.ToString();
            }
            if(resolutionYField != null)
            {
                resolutionYField.text = this.resolutionY.ToString();
            }
            if(videoModeSelector != null)
            {
                videoModeSelector.value = this.videoMode;
            }
            if(vsyncToggle != null)
            {
                vsyncToggle.isOn = this.vsync;
            }
            if(targetFPSField != null)
            {
                targetFPSField.text = this.targetFPS.ToString();
            }
            if(graphicsSelector != null)
            {
                graphicsSelector.value = this.graphics;
            }
            if(qualitySelector != null)
            {
                qualitySelector.value = this.quality;
            }
            if(volumeSlider != null)
            {
                volumeSlider.value = this.volume;
            }
            LockFields();
        }

        public void Reset()
        {
            this.resolutionX = Settings.resolutionX;
            this.resolutionY = Settings.resolutionY;
            this.videoMode = Settings.videoMode;
            this.targetFPS = Settings.targetFPS;
            this.vsync = Settings.vSync;
            this.graphics = Settings.graphics;
            this.quality = Settings.quality;
            this.volume = Settings.volume;
        }

        public void LockFields()
        {
           targetFPSField.interactable = !this.vsync;
        }
        
        // OnValueChanged
        public void ApplyResolutionX()
        {
            try
            {
                int resX = 0;
                resX = int.Parse(resolutionXField.text);
                if(resX >= 800)
                {
                    this.resolutionX = resX;
                    resolutionXField.GetComponent<Image>().color = Color.white;
                }
                else
                {
                    resolutionXField.GetComponent<Image>().color = Color.red;
                }
            }
            catch (Exception)
            {
                resolutionXField.GetComponent<Image>().color = Color.red;
            }
        }

        public void ApplyResolutionY()
        {
            try
            {
                int resY = 0;
                resY = int.Parse(resolutionYField.text);
                if(resY >= 600)
                {
                    this.resolutionY = resY;
                    resolutionYField.GetComponent<Image>().color = Color.white;
                }
                else
                {
                    resolutionYField.GetComponent<Image>().color = Color.red;
                }
            }
            catch (Exception)
            {
                resolutionYField.GetComponent<Image>().color = Color.red;
            }
        }

        public void ApplyVideoMode()
        {
            int vid = 0;
            vid = videoModeSelector.value;
            this.videoMode = vid;
        }

        public void ApplyVsync()
        {
            this.vsync = vsyncToggle.isOn;
            LockFields();
        }

        public void ApplyTargetFPS()
        {
            try
            {
                int fps = 0;
                fps = int.Parse(targetFPSField.text);
                if(fps >= 30 && fps <= 200)
                {
                    this.targetFPS = fps;
                    targetFPSField.GetComponent<Image>().color = Color.white;
                }
                else
                {
                    targetFPSField.GetComponent<Image>().color = Color.red;
                }
            }
            catch (Exception)
            {
                targetFPSField.GetComponent<Image>().color = Color.red;
            }
        }

        public void ApplyGraphics()
        {
            int graph = 0;
            graph = graphicsSelector.value;
            this.graphics = graph;
        }

        public void ApplyQuality()
        {
            int qual = 0;
            qual = qualitySelector.value;
            this.quality = qual;
        }

        public void ApplyVolume()
        {
            float vol = 0.0f;
            vol = volumeSlider.value;
            this.volume = vol;
        }

        // LogDirectory Functions
        public void BrowseLogDirectory()
        {
            if (!string.IsNullOrEmpty(Settings.lastLogPath) && Directory.Exists(Settings.lastLogPath))
            {
                logDirectoryBrowser.Browse(Settings.lastLogPath);
                Debug.Log("Preparing browser with last location where a dir was loaded");
            }
            else
            {
                Debug.Log("No last map path available, starting at documents");
            }
            logDirectoryBrowser.gameObject.SetActive(true);
            gameObject.SetActive(false);
            logDirectoryBrowser.FileSelected += LogBrowser_FileSelected;
        }

        private void LogBrowser_FileSelected(object sender, string path)
        {
            logDirectoryBrowser.gameObject.SetActive(false);
            gameObject.SetActive(true);
            logDirectoryBrowser.FileSelected -= LogBrowser_FileSelected;
            if (path != "")
            {
                Settings.lastLogPath = path;
                if(statisticsLogger!=null)
                { 
                    statisticsLogger.targetDirectory = path;
                }
                Debug.Log("Selected " + path + " as Log Dir");
            }
        }

        // Save and Apply Settings
        public void Save()
        {
            Settings.resolutionX = this.resolutionX;
            Settings.resolutionY = this.resolutionY;
            Settings.videoMode = this.videoMode;
            Settings.vSync = this.vsync;
            Settings.targetFPS = this.targetFPS;
            Settings.graphics = this.graphics;
            Settings.quality = this.quality;
            Settings.volume = this.volume;
            ConfigSave();
            ControlSave();
            ApplySettings();
        }

        public void ConfigSave()
        {
            ConfigFile.IniWriteValue(
                ConfigFile.Sections.General,
                ConfigFile.Keys.resolutionX,
                Settings.resolutionX.ToString()
            );

            ConfigFile.IniWriteValue(
                ConfigFile.Sections.General,
                ConfigFile.Keys.resolutionY,
                Settings.resolutionY.ToString()
            );

            ConfigFile.IniWriteValue(
                ConfigFile.Sections.General,
                ConfigFile.Keys.videoMode,
                Settings.videoMode.ToString()
            );

            ConfigFile.IniWriteValue(
                ConfigFile.Sections.General,
                ConfigFile.Keys.vSync,
                Settings.vSync.ToString()
            );

            ConfigFile.IniWriteValue(
                ConfigFile.Sections.General,
                ConfigFile.Keys.targetFPS,
                Settings.targetFPS.ToString()
            );

            ConfigFile.IniWriteValue(
                ConfigFile.Sections.General,
                ConfigFile.Keys.graphics,
                Settings.graphics.ToString()
            );

            ConfigFile.IniWriteValue(
                ConfigFile.Sections.General,
                ConfigFile.Keys.quality,
                Settings.quality.ToString()
            );

            ConfigFile.IniWriteValue(
                ConfigFile.Sections.General,
                ConfigFile.Keys.volume,
                Settings.volume.ToString()
            );

            ConfigFile.IniWriteValue(
                ConfigFile.Sections.General,
                ConfigFile.Keys.logDirectory,
                Settings.lastLogPath
            );
        }

        private void ControlSave()
        {
            // TODO Save Control overrides once it's available
        }

        public void ControlLoad()
        {
            // TODO Restore Controls once it's available
        }

        public static void ApplySettings()
        {
            if(Settings.videoMode == 0)
            {
                Screen.SetResolution(Settings.resolutionX, Settings.resolutionY,
                FullScreenMode.ExclusiveFullScreen, Settings.targetFPS);
            }
            else if(Settings.videoMode == 1)
            {
                Screen.SetResolution(Settings.resolutionX, Settings.resolutionY,
                FullScreenMode.FullScreenWindow, Settings.targetFPS);
            }
            else if(Settings.videoMode == 2)
            {
                Screen.SetResolution(Settings.resolutionX, Settings.resolutionY,
                FullScreenMode.MaximizedWindow, Settings.targetFPS);
            }
            else if(Settings.videoMode == 3)
            {
                Screen.SetResolution(Settings.resolutionX, Settings.resolutionY,
                FullScreenMode.Windowed, Settings.targetFPS);
            }
            if(Settings.graphics == 0)
            {
                Graphics.activeTier = GraphicsTier.Tier1;
            }
            else if(Settings.graphics == 1)
            {
                Graphics.activeTier = GraphicsTier.Tier2;
            }
            else if(Settings.graphics == 2)
            {
                Graphics.activeTier = GraphicsTier.Tier3;
            }
            QualitySettings.SetQualityLevel(Settings.quality, true);
            if(Settings.vSync)
            {
                QualitySettings.vSyncCount = 1;
            }
            else
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = Settings.targetFPS;
            }
            AudioListener.volume = Settings.volume;
        }
    }
}