using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Assets.Scripts.MultiplayerMessages
{
    public class PlayerJoinedMessage:NetworkMessage
    {
        public string identifier;
        public string specialPrefabIdentifier = "";     //Unused. Can be used by modding later to have cars with different appearences in the same scene
        public PlayerCarMessage pcm;
        public float colorR;
        public float colorG;
        public float colorB;


        public override XElement Serialize()
        {
            XElement root = new XElement(typeof(PlayerJoinedMessage).ToString());

            XElement id = new XElement("identifier");
            id.Value = identifier;
            root.Add(id);

            XElement spec = new XElement("specialPrefabIdentifier");
            spec.Value = specialPrefabIdentifier;
            root.Add(spec);

            XElement xpcm = pcm.Serialize();
            xpcm.Name = "pcm";
            root.Add(xpcm);

            XElement cR = new XElement("colorR");
            cR.Value = colorR.ToString();
            root.Add(cR);

            XElement cG = new XElement("colorG");
            cG.Value = colorG.ToString();
            root.Add(cG);

            XElement cB = new XElement("colorB");
            cB.Value = colorB.ToString();
            root.Add(cB);

            return root;
        }

        public static PlayerJoinedMessage Deserialize(XElement root)
        {
            PlayerJoinedMessage message = new PlayerJoinedMessage();
            foreach (XElement elem in root.Elements())
            {
                switch (elem.Name.ToString())
                {
                    case "identifier":
                        message.identifier = elem.Value;
                        break;

                    case "specialPrefabIdentifier":
                        message.specialPrefabIdentifier = elem.Value;
                        break;
                    case "pcm":
                        message.pcm = PlayerCarMessage.Deserialize(elem);
                        break;
                    case "colorR":
                        float.TryParse(elem.Value, out message.colorR);
                        break;
                    case "colorG":
                        float.TryParse(elem.Value, out message.colorG);
                        break;
                    case "colorB":
                        float.TryParse(elem.Value, out message.colorB);
                        break;
                   
                }
            }
            return message;
        }


    }
}
