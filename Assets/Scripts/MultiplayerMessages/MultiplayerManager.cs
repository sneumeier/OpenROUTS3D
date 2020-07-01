using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Hoepp.TcpLib;
using Hoepp.TcpLib.Host;
using Hoepp.TcpLib.Client;
using System.Security.Cryptography;
using System.IO;
using CarScripts;
using SUMOConnectionScripts;
using Assets.Scripts.CarScripts;
using System.Threading;
using System.Globalization;

namespace Assets.Scripts.MultiplayerMessages
{
    public class MultiplayerManager : MonoBehaviour
    {
        public bool debugMode;
        public WheelDrive ownCar;
        public NetworkVehicle nvPrefab;
        public SumoUnityConnection sumoImporter;
        public ScenarioSettingsMessage scenarioMessage;
        public string playerID
        {
            get { return Settings.userName; }
        }

        public List<NetworkVehicle> networkVehicles = new List<NetworkVehicle>();
        public List<string> playerNames = new List<string>();
        public List<NetworkMessage> recievedMessages = new List<NetworkMessage>();

        public void Start()
        {
            var culture = System.Globalization.CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = culture;
            Debug.Log("Multiplayer Started: IsHost = " + Settings.isHost);
            if (Settings.isHost)
            {
                MultiplayerCommunication.StartHost();
                MultiplayerCommunication.Host.ClientAdded += Host_ClientAdded;
                scenarioMessage = CreateSettingsMessage();
                Debug.Log("Host Started");
            }
            else if (Settings.isMPServer && !Settings.isHost)
            {
                MultiplayerCommunication.StopLogging();
                if (MultiplayerCommunication.Client == null)
                {
                    Debug.LogError("Multiplayer Client not available! Have you started the simulation scene in client mode without starting the Menu Scene first?");
                    return;
                }
                MultiplayerCommunication.Client.StringMessageRecieved += Client_StringMessageRecieved;
                lock (MultiplayerCommunication.LoggedMessages)
                {
                    foreach (var msg in MultiplayerCommunication.LoggedMessages)
                    {
                        lock (recievedMessages)
                        {
                            recievedMessages.Add(msg);
                        }
                    }
                }
                MultiplayerCommunication.Client.SendStringMessage(CreatePlayerJoinedMessage(ownCar).StringSerialize());
                Debug.Log("Client Continued and joined");
            }
        }

        void Client_StringMessageRecieved(string stringmessage)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            lock (recievedMessages)
            {
                recievedMessages.Add(NetworkMessage.DeserializeFromRoot(stringmessage));
            }
        }

        void OnApplicationQuit()
        {
            MultiplayerCommunication.CloseChannels();
        }


        void Host_ClientAdded(RecievingClient client)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            client.StringMessageRecieved += RecievingClient_StringMessageRecieved;
            client.SendStringMessage(scenarioMessage.StringSerialize());

            Debug.Log("Client Connected");
            Debug.Log("Sent Message: " + scenarioMessage.Serialize());
        }

        void RecievingClient_StringMessageRecieved(RecievingClient sender, string stringmessage)
        {
            //Deserialize message from client to host
            lock (recievedMessages)
            {
                recievedMessages.Add(NetworkMessage.DeserializeFromRoot(stringmessage));
            }
            //Debug.Log("Message recieved");
        }

        public ScenarioSettingsMessage CreateSettingsMessage()
        {
            ScenarioSettingsMessage ssm = new ScenarioSettingsMessage();
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(Settings.mapPath))
                {
                    ssm.mapChecksum = md5.ComputeHash(stream).ToString();
                }
            }
            ssm.mapPath = Settings.partialMapPath;
            ssm.playerCars = new List<PlayerJoinedMessage>();
            foreach (var vehicle in networkVehicles)
            {
                ssm.playerCars.Add(CreatePlayerJoinedMessage(vehicle));
            }
            ssm.playerCars.Add(CreatePlayerJoinedMessage(ownCar));

            ssm.scenarioChecksum = "";  //TODO
            ssm.scenarioPath = "";  //TODO

            ssm.spawnPosition = sumoImporter.startLocation;
            ssm.spawnRotation = sumoImporter.startRotation.eulerAngles;

            return ssm;
        }

        public PlayerJoinedMessage CreatePlayerJoinedMessage(NetworkVehicle nv)
        {
            PlayerJoinedMessage pjm = new PlayerJoinedMessage();
            pjm.colorR = nv.color.r;
            pjm.colorG = nv.color.g;
            pjm.colorB = nv.color.b;

            pjm.identifier = nv.id;
            pjm.specialPrefabIdentifier = "";

            pjm.pcm = CreatePlayerCarMessage(nv);

            return pjm;
        }

        public PlayerJoinedMessage CreatePlayerJoinedMessage(WheelDrive wd)
        {
            PlayerJoinedMessage pjm = new PlayerJoinedMessage();
            pjm.colorR = Settings.multiplayerColorRed;
            pjm.colorG = Settings.multiplayerColorGreen;
            pjm.colorB = Settings.multiplayerColorBlue;

            pjm.identifier = Settings.userName;
            pjm.specialPrefabIdentifier = "";

            pjm.pcm = CreatePlayerCarMessage(wd);

            return pjm;
        }

        public void SendRemoveVehicle(string key)
        {
            ForeignCarDestroyMessage fcdm = new ForeignCarDestroyMessage();
            fcdm.identifier = key;
            foreach (var client in MultiplayerCommunication.Host.ActiveClients)
            {
                client.SendStringMessage(fcdm.StringSerialize());
            }
        }

        public void SendVehiclePosition(string key, SumoVehicle sv)
        {
            ForeignCarMessage fdm = ForeignCarMessage.Create(key, sv);


            foreach (var client in MultiplayerCommunication.Host.ActiveClients)
            {
                client.SendStringMessage(fdm.StringSerialize());
            }
        }

        public PlayerCarMessage CreatePlayerCarMessage(NetworkVehicle nv)
        {
            PlayerCarMessage pcm = PlayerCarMessage.Create(nv.id, nv.rig, nv.acceleration);
            pcm.messageNr = nv.highestId;
            return pcm;
        }

        public PlayerCarMessage CreatePlayerCarMessage(WheelDrive wd)
        {
            PlayerCarMessage pcm = PlayerCarMessage.Create(playerID, wd.rb, wd.acceleration);
            return pcm;
        }

        public void Update()
        {
            AggregatedMessage am = new AggregatedMessage();
            lock (recievedMessages)
            {
                foreach (var rm in recievedMessages)
                {
                    Apply(rm);
                    if (Settings.isHost)
                    {
                        am.messages.Add(rm);
                    }
                }
                recievedMessages.Clear();
            }
            PlayerCarMessage ownMessage = CreatePlayerCarMessage(ownCar);
            am.messages.Add(ownMessage);
            if (Settings.isHost)
            {
                string messagePayload = am.StringSerialize();
                if (debugMode)
                {
                    Debug.Log("Aggregated Message: " + messagePayload);
                }
                MultiplayerCommunication.BroadcastMessage(messagePayload);
            }
            else if (Settings.isMPServer && !Settings.isHost)
            {
                MultiplayerCommunication.Client.SendStringMessage(ownMessage.StringSerialize());
            }
        }

        public void Apply(NetworkMessage nm, bool ignoreAggregation = false)
        {
            if (nm is AggregatedMessage)
            {
                AggregatedMessage am = nm as AggregatedMessage;
                foreach (NetworkMessage subNm in am.messages)
                {
                    Apply(subNm, true);
                }
            }
            else if (nm is PlayerCarMessage)
            {
                PlayerCarMessage pcm = nm as PlayerCarMessage;
                if (pcm.identifier == this.playerID)
                {
                    //No need to apply PCM if it's our own car
                    return;
                }
                if (debugMode)
                {
                    Debug.Log("Player car message: " + pcm.identifier + " at " + pcm.position);
                }
                NetworkVehicle chosenVehicle = null;
                foreach (NetworkVehicle nv in networkVehicles)
                {
                    if (nv.id == pcm.identifier)
                    {
                        chosenVehicle = nv;
                        break;
                    }
                }
                if (chosenVehicle != null)
                {
                    chosenVehicle.Apply(pcm);
                }
                else
                { Debug.Log("Couldn't find associated Vehicle: " + pcm.identifier); }
            }
            else if (nm is PlayerJoinedMessage)
            {

                PlayerJoinedMessage pjm = nm as PlayerJoinedMessage;
                Debug.Log("Creating new Vehicle for " + pjm.identifier);
                if (pjm.identifier == Settings.userName)
                {
                    Debug.Log("Nevermind, it's our own car");
                    return;
                }
                NetworkVehicle newNV = GameObject.Instantiate(nvPrefab);    //Make dependent on special Prefab identifier
                newNV.id = pjm.identifier;
                ColorCar cc = newNV.GetComponent<ColorCar>();
                if (cc != null)
                {
                    cc.Apply(new Color(pjm.colorR, pjm.colorG, pjm.colorB));
                }
                newNV.color = new Color(pjm.colorR, pjm.colorG, pjm.colorB);
                networkVehicles.Add(newNV);
                Apply(pjm.pcm, true);
            }
            else if (nm is ForeignCarMessage)
            {
                ForeignCarMessage fcm = nm as ForeignCarMessage;
                 if (!playerNames.Contains(fcm.identifier))
                {
                    sumoImporter.AddForeignCarMessage(fcm);
                }
            }
            else if (nm is ForeignCarDestroyMessage)
            {
                ForeignCarDestroyMessage fcdm = nm as ForeignCarDestroyMessage;
                sumoImporter.DeleteSumoVehicle(fcdm.identifier);
            }


        }


    }
}
