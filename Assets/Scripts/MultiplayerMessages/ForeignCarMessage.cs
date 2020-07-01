using SUMOConnectionScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace Assets.Scripts.MultiplayerMessages
{
    [XmlInclude(typeof(NetworkMessage))]
    public class ForeignCarMessage:NetworkMessage
    {

        public string identifier;
        public Vector3 position;
        public Vector3 rotation;
        public float velocity;

        public static ForeignCarMessage Create(string identifier, SumoVehicle sv)
        {
            ForeignCarMessage fcm = new ForeignCarMessage();
            Transform trans = sv.GetComponent<Transform>();
            fcm.identifier = identifier;
            fcm.position = trans.position;
            fcm.rotation = trans.rotation.eulerAngles;
            fcm.velocity = sv.velocity;
            return fcm;
        }

        public override XElement Serialize()
        {
            XElement root = new XElement(typeof(ForeignCarMessage).ToString());

            XElement id = new XElement("identifier");
            id.Value = identifier;
            root.Add(id);
            XElement pos = new XElement("position");
            pos.Value = NetworkMessage.SerializeVector(position);
            root.Add(pos);
            XElement rot = new XElement("rotation");
            root.Add(rot);
            rot.Value = NetworkMessage.SerializeVector(rotation);
            XElement mom = new XElement("velocity");
            mom.Value = velocity.ToString() ;
            root.Add(mom);

            return root;
        }


        public static ForeignCarMessage Deserialize(XElement root)
        {

            ForeignCarMessage message = new ForeignCarMessage();
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
                    case "velocity":
                        message.velocity = float.Parse(elem.Value);
                        break;
                }
            }
            return message;
        }


    }
}
