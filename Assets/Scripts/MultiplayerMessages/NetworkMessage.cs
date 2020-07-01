using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace Assets.Scripts.MultiplayerMessages
{

    public abstract class NetworkMessage
    {
        public abstract XElement Serialize();

        public string StringSerialize()
        {
            return Serialize().ToString();
        }

        public static string SerializeVector(Vector3 v3)
        {
            return v3.x+";"+v3.y+";"+v3.z;
        }

        public static Vector3 DeserializeVector(string v3)
        {
            string[] parts = v3.Split(';');
            if (parts.Count() < 3)
            {
                return Vector3.zero;
            }
            try
            {
                return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
            }
            catch (Exception)
            {
                return Vector3.zero;
            }
        }

        public static NetworkMessage DeserializeFromRoot(string xml)
        { 
             return DeserializeFromRoot(XElement.Parse(xml));
        }

        public static NetworkMessage DeserializeFromRoot(XElement root)
        {
            
            string rootname = root.Name.ToString();
            if (typeof(AggregatedMessage).ToString() == rootname)
            {
                return AggregatedMessage.Deserialize(root);
            }
            else if (typeof(PlayerCarMessage).ToString() == rootname)
            {
                return PlayerCarMessage.Deserialize(root);
            }
            else if (typeof(PlayerDisconnectedMessage).ToString() == rootname)
            {
                return PlayerDisconnectedMessage.Deserialize(root);
            }
            else if (typeof(ScenarioSettingsMessage).ToString() == rootname)
            {
                return ScenarioSettingsMessage.Deserialize(root);
            }
            else if (typeof(PlayerJoinedMessage).ToString() == rootname)
            {
                return PlayerJoinedMessage.Deserialize(root);
            }
            else return null;
        }
    }
}
