using Assets.Scripts.CarScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.MainMenuScripts
{
    public class MultiplayerMenu:MonoBehaviour
    {
        public InputField UserName;
        public InputField IpAddress;
        public InputField SumoPort;
        public InputField TcpPort;
        public Slider ColorR;
        public Slider ColorG;
        public Slider ColorB;

        public Image ColorPreview;
        public ColorCar PreviewCar;

        public void Start()
        {
            UserName.text = Settings.userName;
            IpAddress.text = Settings.multiplayerIpAddress;
            SumoPort.text = Settings.sumoPort.ToString();
            TcpPort.text = Settings.multiplayerPort.ToString();
            ColorR.value = Settings.multiplayerColorRed;
            ColorG.value = Settings.multiplayerColorGreen;
            ColorB.value = Settings.multiplayerColorBlue;

            UserName.onValueChanged.AddListener(delegate { ApplyUserName(); });
            IpAddress.onValueChanged.AddListener(delegate { ApplyIP(); });
            SumoPort.onValueChanged.AddListener(delegate { ApplySumoPort(); });
            TcpPort.onValueChanged.AddListener(delegate { ApplyTcpPort(); });
            ColorR.onValueChanged.AddListener(delegate { ApplyColor(); });
            ColorG.onValueChanged.AddListener(delegate { ApplyColor(); });
            ColorB.onValueChanged.AddListener(delegate { ApplyColor(); });

            ApplyColor();
        }

        public void ApplyUserName()
        {
            Settings.userName = UserName.text;
        }

        public void ApplyIP()
        {
            Settings.multiplayerIpAddress = IpAddress.text;
        }

        public void ApplySumoPort()
        {
            try
            {
                Settings.sumoPort = int.Parse(SumoPort.text);
                SumoPort.GetComponent<Image>().color = Color.white;
            }
            catch
            {
                SumoPort.GetComponent<Image>().color = Color.red;
            }
        }

        public void ApplyTcpPort()
        {
            try
            {
                Settings.multiplayerPort = int.Parse(TcpPort.text);
                SumoPort.GetComponent<Image>().color = Color.white;
            }
            catch
            {
                SumoPort.GetComponent<Image>().color = Color.red;
            }
        }

        public void ApplyColor()
        {
            Settings.multiplayerColorBlue = ColorB.value;
            Settings.multiplayerColorRed = ColorR.value;
            Settings.multiplayerColorGreen = ColorG.value;

            PreviewCar.Apply(new Color(Settings.multiplayerColorRed, Settings.multiplayerColorGreen, Settings.multiplayerColorBlue));
            ColorPreview.color = new Color(Settings.multiplayerColorRed, Settings.multiplayerColorGreen, Settings.multiplayerColorBlue);
        }


    }
}
