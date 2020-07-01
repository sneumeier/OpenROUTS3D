using CarScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace Assets.Scripts.MultiplayerMessages
{
    
    [XmlInclude(typeof(NetworkMessage))]
    public class PlayerCarMessage:NetworkMessage
    {
        public static ulong NextMessageNr = 0;

        public string identifier;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 momentum;
        public Vector3 torque;
        public Vector3 acceleration;
        public ulong messageNr;

        public PlayerCarMessage()
        {
        
        }

        public static PlayerCarMessage Create(string identifier,Rigidbody rig, Vector3 acceleration)
        {
            PlayerCarMessage pcm = new PlayerCarMessage();
            Transform trans = rig.GetComponent<Transform>();
            pcm.identifier = identifier;
            pcm.position = trans.position;
            pcm.rotation = trans.rotation.eulerAngles;
            pcm.momentum = rig.velocity;
            pcm.acceleration = acceleration;
            pcm.torque = rig.angularVelocity;
            pcm.messageNr = NextMessageNr++;
            return pcm;
        }

        public override XElement Serialize()
        {
            XElement root = new XElement(typeof(PlayerCarMessage).ToString());

            XElement id = new XElement("identifier");
            id.Value = identifier;
            root.Add(id);
            XElement pos = new XElement("position");
            pos.Value = NetworkMessage.SerializeVector(position);
            root.Add(pos);
            XElement rot = new XElement("rotation");
            root.Add(rot);
            rot.Value = NetworkMessage.SerializeVector(rotation);
            XElement mom = new XElement("momentum");
            mom.Value = NetworkMessage.SerializeVector(momentum);
            root.Add(mom);
            XElement tor = new XElement("torque");
            tor.Value = NetworkMessage.SerializeVector(torque);
            root.Add(tor);
            XElement nr = new XElement("messageNr");
            nr.Value = messageNr.ToString();
            root.Add(nr);
            

            return root;
        }

        public static PlayerCarMessage Deserialize(XElement root)
        {

            PlayerCarMessage message = new PlayerCarMessage();
            foreach (XElement elem in root.Elements())
            {
                switch (elem.Name.ToString())
                {
                    case "identifier":
                        message.identifier = elem.Value;
                        break;
                    case "rotation":
                        message.rotation = NetworkMessage.DeserializeVector(elem.Value);
                        break;
                    case "position":
                        message.position = NetworkMessage.DeserializeVector(elem.Value);
                        break;
                    case "momentum":
                        message.momentum = NetworkMessage.DeserializeVector(elem.Value);
                        break;
                    case "torque":
                        message.torque = NetworkMessage.DeserializeVector(elem.Value);
                        break;
                    case "messageNr":
                        ulong.TryParse(elem.Value.ToString(), out message.messageNr);
                        break;
                }
            }
            return message;
        }

    }
}
