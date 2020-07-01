using Assets.Scripts.AssetReplacement;
using Assets.Scripts.CarScripts;
using CarCameraScripts;
using System;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenuScripts
{
    public class MainMenu : MonoBehaviour
    {
        public InputField FramePauseField;
        public InputField InputDelayField;
        public InputField FrameBufferField;
        public InputField ScreenFactorField;
        public InputField PacketLossField;
        public InputField UserNameField;
        
        public Toggle SumoServerToggle;
        public Toggle SumoGuiToggle;
        public Toggle ScenarioToggle;
        /* public Toggle DynamicDelayToggle; */

        public FileBrowser MapBrowser;

        public BufferedDisplay display;
        
        public GameObject settingWindow;
        public GameObject creditWindow;

        public GameObject LoadingScreen;

        // Use this for initialization
        void Start()
        {
            Cursor.visible = true;
            Time.timeScale = 1;
            if (UserNameField != null)
            {
                UserNameField.text = Settings.userName;
                UserNameField.onValueChanged.AddListener(delegate { ApplyUserName(); });
            }
            if (ScenarioToggle != null)
            {
                ScenarioToggle.isOn = Settings.enableScenario;
            }
            if (InputDelayField != null)
            {
                InputDelayField.text = Settings.inputTimeDelay.ToString();
            }
            if (FrameBufferField != null)
            {
                FrameBufferField.text = Settings.frameBufferDelay.ToString();
            }
            /* if (DynamicDelayToggle != null)
            {
                DynamicDelayToggle.isOn = Settings.dynamicDelay;
            } */
            if (ScreenFactorField != null)
            {
                ScreenFactorField.text = Settings.screenFactor.ToString();
                ScreenFactorField.onValueChanged.AddListener(delegate { ApplyScreenFactor(); });
                if(display != null)
                { 
                    display.scaleFactor = Settings.screenFactor;
                }
            }
            if (PacketLossField != null)
            {
                PacketLossField.text = (Settings.packetLoss * 100).ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            if (FramePauseField != null)
            {
                FramePauseField.text = Settings.displayTimeDelay.ToString();
                FramePauseField.onValueChanged.AddListener(delegate { ApplyDelay(); });
            }
            if (SumoServerToggle != null)
            {
                SumoServerToggle.isOn = Settings.isSumoServer;
                SumoServerToggle.onValueChanged.AddListener(delegate { ApplySumoServerChange(); });
            }
            if (SumoGuiToggle != null)
            {
                SumoGuiToggle.isOn = Settings.sumoGui;
                SumoGuiToggle.onValueChanged.AddListener(delegate { ApplySumoGuiChange(); });
                LockSumoGUI();
            }
            if (InputDelayField != null)
            {
                InputDelayField.onValueChanged.AddListener(delegate { ApplyInputDelay(); });
            }
            if (FrameBufferField != null)
            {
                FrameBufferField.onValueChanged.AddListener(delegate { ApplyFrameBuffer(); });
            }
            if (PacketLossField != null)
            {
                PacketLossField.onValueChanged.AddListener(delegate { ApplyPacketLoss(); });
            }
        }

        private void ApplyPacketLoss()
        {
            string content = PacketLossField.text;
            try
            {
                float rawValueInPercent = float.Parse(content, System.Globalization.CultureInfo.InvariantCulture);
                PacketLossField.GetComponent<Image>().color = Color.white;
                Settings.packetLoss = rawValueInPercent / 100f;
            } catch (Exception)
            {
                PacketLossField.GetComponent<Image>().color = Color.red;
            }
        }

        private void ApplyScreenFactor()
        {
            try
            {
                float factor = 0.0f;
                //Unity probably always uses Invariant culture anyways (point instead of comma for fraction)
                factor = float.Parse(ScreenFactorField.text, System.Globalization.CultureInfo.InvariantCulture);

                if (factor > 0 && factor <= 1)
                {
                    Settings.screenFactor = factor;
                    if (display != null)
                    {
                        display.scaleFactor = factor;
                    }
                    //Looks fine, mark field as valid
                    ScreenFactorField.GetComponent<Image>().color = Color.white;
                    Debug.Log("Applied screen Factor");
                }
                else
                {
                    ScreenFactorField.GetComponent<Image>().color = Color.red;
                }
            }
            catch (Exception)
            {
                //Catch all, mark field as invalid
                ScreenFactorField.GetComponent<Image>().color = Color.red;
            }
        }

        private void ApplyUserName()
        {
            Settings.userName = UserNameField.text;
        }

        public void BrowseMaps()
        {
            MapBrowser.FileExtension = ".xml";
            if (!string.IsNullOrEmpty(Settings.lastMapPath) && Directory.Exists(Path.GetDirectoryName(Settings.lastMapPath)))
            {
                MapBrowser.Browse(Settings.lastMapPath);
                Debug.Log("Preparing browser with last location where a file was loaded");

            }
            else
            {
                Debug.Log("No last map path available, starting at documents");

            }
            MapBrowser.gameObject.SetActive(true);
            gameObject.SetActive(false);
            MapBrowser.FileSelected += MapBrowser_FileSelected;
        }

        void MapBrowser_FileSelected(object sender, string path)
        {
            MapBrowser.gameObject.SetActive(false);
            gameObject.SetActive(true);
            MapBrowser.FileSelected -= MapBrowser_FileSelected;
            if (path != "")
            {
                Settings.mapPath = path;
                Settings.polyPath = path.Replace("net.xml", "poly.xml");
                Settings.scenarioPath = path.Replace("net.xml", "scenario.xml");

                ScenarioToggle.interactable = File.Exists(Settings.scenarioPath);
                if (File.Exists(Settings.scenarioPath))
                {
                    Debug.Log(Settings.scenarioPath + " does exist");
                }
                else { Debug.Log(Settings.scenarioPath + " does not exist"); }

                Settings.lastMapPath = Path.GetDirectoryName(path);
                Debug.Log("Selected " + path + " as Map File");
            }
        }
        
        public void ToggleScenario()
        {
            Settings.enableScenario = ScenarioToggle.isOn;
        }

        public void CloseApp()
        {

            Application.Quit();
        }

        void OnApplicationQuit()
        {
			
        }

        void ApplyDelay()
        {

            try
            {
                float delay = 0.0f;
                //Unity probably always uses Invariant culture anyways (point instead of comma for fraction)
                delay = float.Parse(FramePauseField.text, System.Globalization.CultureInfo.InvariantCulture);

                Settings.displayTimeDelay = delay;
                //Looks fine, mark field as valid
                FramePauseField.GetComponent<Image>().color = Color.white;
                Debug.Log("Applied Input Delay");
            }
            catch (Exception)
            {
                //Catch all, mark field as invalid
                FramePauseField.GetComponent<Image>().color = Color.red;
            }
        }

        void ApplyFrameBuffer()
        {
            try
            {
                float delay = 0.0f;
                //Unity probably always uses Invariant culture anyways (point instead of comma for fraction)
                delay = float.Parse(FrameBufferField.text, System.Globalization.CultureInfo.InvariantCulture);

                if (display != null)
                {
                    display.delayTime = delay;
                }
                Settings.frameBufferDelay = delay;
                //Looks fine, mark field as valid
                FrameBufferField.GetComponent<Image>().color = Color.white;
            }
            catch (Exception)
            {
                //Catch all, mark field as invalid
                FrameBufferField.GetComponent<Image>().color = Color.red;
            }
        }

        void ApplyInputDelay()
        {
            try
            {
                float delay = 0.0f;
                //Unity probably always uses Invariant culture anyways (point instead of comma for fraction)
                delay = float.Parse(InputDelayField.text, System.Globalization.CultureInfo.InvariantCulture);

                Settings.inputTimeDelay = delay;
                //Looks fine, mark field as valid
                InputDelayField.GetComponent<Image>().color = Color.white;
            }
            catch (Exception)
            {
                //Catch all, mark field as invalid
                InputDelayField.GetComponent<Image>().color = Color.red;
            }
        }

        void ApplySumoServerChange()
        {
            Settings.isSumoServer = SumoServerToggle.isOn;
            LockSumoGUI();
        }

        void ApplySumoGuiChange()
        {
            Settings.sumoGui = SumoGuiToggle.isOn;
        }

        void LockSumoGUI()
        {
            SumoGuiToggle.interactable = Settings.isSumoServer;
        }
        
        public void ShowSettings()
        {
            settingWindow.gameObject.SetActive(true);
            this.gameObject.SetActive(false);
        }

        public void CloseSettings()
        {
            settingWindow.gameObject.SetActive(false);
            this.gameObject.SetActive(true);
        }

        public void ShowCredits()
        {
            creditWindow.gameObject.SetActive(true);
            this.gameObject.SetActive(false);
        }

        public void CloseCredits()
        {
            creditWindow.gameObject.SetActive(false);
            this.gameObject.SetActive(true);
        }

        public void DisplayLoadinScreen()
        {
			LoadingScreen.SetActive(true);
		}
    }
}
