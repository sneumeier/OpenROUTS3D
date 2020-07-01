using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Assets.Scripts.ScenarioEditor
{
    public class ScenarioTrigger : ScenarioObject
    {
        public float BoxX = 1;
        public float BoxY = 1;
        public float BoxZ = 1;

        public override ScenarioObject Deserialize(XElement elem)
        {
            base.Deserialize(elem);
            foreach (XElement subelem in elem.Elements())
            {
                switch (subelem.Name.ToString())
                {
                    case "BoxX":
                        BoxX = float.Parse(subelem.Value);
                        break;
                    case "BoxY":
                        BoxY = float.Parse(subelem.Value);
                        break;
                    case "BoxZ":
                        BoxZ = float.Parse(subelem.Value);
                        break;
                }
            }
            this.GetComponent<ScenarioTriggerMonoBehaviour>().Init(this);
            return this;
        }

        public override XElement Serialize()
        {
            XElement elem = base.Serialize();
            XElement subelemWidth = new XElement("BoxX");
            XElement subelemHeight = new XElement("BoxY");
            XElement subelemDepth = new XElement("BoxZ");
            elem.Name = "ScenarioTrigger";

            subelemWidth.Value = BoxX.ToString();
            subelemHeight.Value = BoxY.ToString();
            subelemDepth.Value = BoxZ.ToString();

            return elem;
        }
    }
}
