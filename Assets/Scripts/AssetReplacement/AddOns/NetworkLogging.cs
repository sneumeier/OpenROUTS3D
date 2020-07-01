using Assets.Scripts.CarScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AssetReplacement.AddOns
{
    public class NetworkLogging : MonoBehaviour, IStreamLogger
    {

        public int Port;
        public Hoepp.TcpLib.Host.NetworkHost Host;

        public void Start()
        {
            Host = new Hoepp.TcpLib.Host.NetworkHost(Port);
            Host.Start();
            AnchorMapping.GetAnchor("CarPhysics").GetComponent<StatisticsLogger>().StreamLogger = this;
        }

        public void Log(string serializedFrame)
        {
            lock (Host.ActiveClients)
            {
                foreach (var client in Host.ActiveClients)
                {
                    client.Socket.Send(Encoding.ASCII.GetBytes(serializedFrame));  //Accessing Socket directly so it's not sending package overhead from Hoepp.TcpLib. The payload is always assumed to be a string. Network stream can be logged and serialized without having to worry about cutting out package overhead
                }
            }

        }
    }
}
