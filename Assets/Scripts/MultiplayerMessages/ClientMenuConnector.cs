using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.MultiplayerMessages
{
    public class ClientMenuConnector:MonoBehaviour
    {
        public AssetReplacement.AssetReplacement addonPanel;
        public Text displayText;

        private ScenarioSettingsMessage relevantMessage = null;

        public void LoadScene()
        {
            addonPanel.EndMenuScene(1);
        }

        public void EstablishConnection()
        {
            Settings.isHost = false;
            MultiplayerCommunication.Client = new Hoepp.TcpLib.Client.NetworkClient(Settings.multiplayerIpAddress,Settings.multiplayerPort);
            MultiplayerCommunication.Client.StringMessageRecieved += Client_StringMessageRecieved;
            MultiplayerCommunication.StartLogging();
            MultiplayerCommunication.Client.Start();

            UnityEngine.Debug.Log("Started Client with IP: "+Settings.multiplayerIpAddress+", Port: "+Settings.multiplayerPort);
        }

        void Client_StringMessageRecieved(string stringmessage)
        {
            UnityEngine.Debug.Log("Message Recieved: " + stringmessage);
            NetworkMessage nm = NetworkMessage.DeserializeFromRoot(stringmessage);
            
            bool isScenarioMessage = nm is ScenarioSettingsMessage;
            //UnityEngine.Debug.Log("Message Type: "+nm.GetType());
            if (isScenarioMessage)
            {
                Debug.Log("Recieved Scenario Settings!");
                ScenarioSettingsMessage ssm = nm as ScenarioSettingsMessage;
                lock (this)
                {
                    relevantMessage = ssm;
                }
                MultiplayerCommunication.Client.StringMessageRecieved -= Client_StringMessageRecieved;
            }
        }

        public void Update()
        {
            lock (this)
            {
                if (relevantMessage != null)
                {
                    Debug.Log("Parsing Settings...");
                    //Settings.mapPath = Path.Combine(Settings.mapPrefix, relevantMessage.mapPath);
                    Settings.polyPath = Settings.mapPath.Replace("net.xml", "poly.xml");
                    lock (MultiplayerCommunication.LoggedMessages)
                    {
                        foreach (var joinedPlayer in relevantMessage.playerCars)
                        {
                            MultiplayerCommunication.LoggedMessages.Add(joinedPlayer);
                        }
                    }
                    displayText.text = "Loading map...";
                    Debug.Log("Is Host : "+Settings.isHost);
                    Settings.isHost = false;
                    LoadScene();
                }
            }
        }


        void OnApplicationQuit()
        {
            MultiplayerCommunication.CloseChannels();
        }

    }
}
