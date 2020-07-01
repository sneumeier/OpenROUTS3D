using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace Assets.Scripts.ScenarioEditor
{
    public class ScenarioObject:MonoBehaviour
    {

        public virtual ScenarioObject Deserialize(XElement elem)
        {
            LoadPosition(elem);
            return this;
        }

        public virtual void LoadPosition(XElement elem)
        {
            Vector3 angles = new Vector3(0,0,0);
            Vector3 pos = new Vector3(0,0,0);
            foreach(XElement subelem in elem.Elements())
            {
                switch(subelem.Name.ToString()){
                    case "X":
                        pos.x = float.Parse(subelem.Value);
                        break;
                    case "Y":
                        pos.y = float.Parse(subelem.Value);
                        break;
                    case "Z":
                        pos.z = float.Parse(subelem.Value);
                        break;
                    case "RX":
                        angles.x = float.Parse(subelem.Value);
                        break;
                    case "RY":
                        angles.y = float.Parse(subelem.Value);
                        break;
                    case "RZ":
                        angles.z = float.Parse(subelem.Value);
                        break;
                }
            }

            transform.position = pos;
            transform.eulerAngles = angles;
        }

        public virtual XElement Serialize()
        {
            XElement elem = new XElement("ScenarioObject");

            XElement subelem;

            subelem = new XElement("X");
            subelem.Value = this.transform.position.x.ToString();
            elem.Add(subelem);

            subelem = new XElement("Y");
            subelem.Value = this.transform.position.y.ToString();
            elem.Add(subelem);

            subelem = new XElement("Z");
            subelem.Value = this.transform.position.z.ToString();
            elem.Add(subelem);

            subelem = new XElement("RX");
            subelem.Value = this.transform.eulerAngles.x.ToString();
            elem.Add(subelem);

            subelem = new XElement("RY");
            subelem.Value = this.transform.eulerAngles.y.ToString();
            elem.Add(subelem);

            subelem = new XElement("RZ");
            subelem.Value = this.transform.eulerAngles.z.ToString();
            elem.Add(subelem);


            return elem;
        }

    }
}
