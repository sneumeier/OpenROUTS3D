using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Assets.Scripts.MultiplayerMessages
{
    public class ForeignCarDestroyMessage : NetworkMessage
    {
        public string identifier;


        public override XElement Serialize()
        {
            XElement root = new XElement(typeof(ForeignCarDestroyMessage).ToString());

            XElement id = new XElement("identifier");
            id.Value = identifier;
            root.Add(id);

            return root;
        }

        public static ForeignCarDestroyMessage Deserialize(XElement root)
        {
            ForeignCarDestroyMessage message = new ForeignCarDestroyMessage();
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
