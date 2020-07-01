using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Assets.Scripts.MultiplayerMessages
{
    public class PlayerDisconnectedMessage:NetworkMessage
    {
        public string identifier;


        public override XElement Serialize()
        {
            XElement root = new XElement(typeof(PlayerDisconnectedMessage).ToString());

            XElement id = new XElement("identifier");
            id.Value = identifier;
            root.Add(id);
            
            return root;
        }

        public static PlayerDisconnectedMessage Deserialize(XElement root)
        {
            PlayerDisconnectedMessage message = new PlayerDisconnectedMessage();
            foreach (XElement elem in root.Elements())
            {
                switch (elem.Name.ToString())
                {
                    case "identifier":
                        message.identifier = elem.Value;
                        break;
                }
            }
            return message;
        }

    }
}
