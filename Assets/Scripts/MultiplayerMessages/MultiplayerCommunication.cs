using Hoepp.TcpLib.Client;
using Hoepp.TcpLib.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.MultiplayerMessages
{
    public static class MultiplayerCommunication
    {
        public static NetworkHost Host;    //Server instance
        public static NetworkClient Client; //only active if is client

        private static List<RecievingClient> _activeRecievingClients = new List<RecievingClient>();
        public static List<NetworkMessage> LoggedMessages = new List<NetworkMessage>();

        public static void StartHost()
        {
            Host = new NetworkHost(Settings.multiplayerPort);
            UnityEngine.Debug.Log("Host Port:"+Settings.multiplayerPort);
            Host.ClientAdded += Host_ClientAdded;
            Host.Start();
        }

        public static void StartLogging()
        {
            Client.StringMessageRecieved += Client_StringMessageRecieved;
        }

        static void Client_StringMessageRecieved(string stringmessage)
        {
            lock (LoggedMessages)
            {
                LoggedMessages.Add(NetworkMessage.DeserializeFromRoot(stringmessage));
            }
            //UnityEngine.Debug.Log("Logged some message");
        }

        public static void CloseChannels()
        {
            if (Client != null)
            {
                Client.Connection.Close();
                Client.Exit = true;
            }
            if (Host != null)
            {
                Host.Exit = true;
                foreach (var reClient in Host.ActiveClients)
                {
                    reClient.Active = false;
                    reClient.Socket.Close();
                }
            }
        }

        public static void StopLogging()
        {
            Client.StringMessageRecieved -= Client_StringMessageRecieved;
        }

        private static void Host_ClientAdded(RecievingClient client)
        {
            lock (_activeRecievingClients)
            {
                _activeRecievingClients.Add(client);
                client.OnDisconnect += client_OnDisconnect;
            }
            UnityEngine.Debug.Log("MultiplayerCommunication: Client Added");
        }

        private static void client_OnDisconnect(RecievingClient sender)
        {
            lock (_activeRecievingClients)
            {
                _activeRecievingClients.Remove(sender);
            }

            UnityEngine.Debug.Log("Client disconnected");
        }

        public static void BroadcastMessage(string msg)
        {
            lock (_activeRecievingClients)
            {
                foreach (var client in _activeRecievingClients)
                {
                    client.SendStringMessage(msg);
                }
            }
        }


    }
}
