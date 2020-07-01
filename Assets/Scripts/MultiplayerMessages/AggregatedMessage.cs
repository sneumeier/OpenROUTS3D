using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Assets.Scripts.MultiplayerMessages
{
    public class AggregatedMessage:NetworkMessage
    {
        public List<NetworkMessage> messages = new List<NetworkMessage>();

        public override XElement Serialize()
        {
            XElement root = new XElement(typeof(AggregatedMessage).ToString());

            XElement msgs = new XElement("Messages");
            root.Add(msgs);
            foreach(var msg in messages)
            {
                msgs.Add(msg.Serialize());
            }
            return root;
        }

        public static AggregatedMessage Deserialize(XElement root)
        {
            AggregatedMessage message = new AggregatedMessage();
            foreach (XElement elem in root.Elements())
            {
                switch (elem.Name.ToString())
                { 
                    case "Messages":
                        foreach (XElement subelem in elem.Elements())
                        {
                            message.messages.Add(NetworkMessage.DeserializeFromRoot(subelem)); 
                        }
                        break;
                }
            }
            return message;
        }
    }
}
