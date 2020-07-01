using MainMenuScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace Assets.Scripts.ScenarioEditor
{
    public class ScenarioDelayBeacon:ScenarioObject
    {

        public float[] distanceArray;       //TODO: This is only there for dynamic editing in the unity editor for scenario building. Will be removed as soon as scenario editior gets an UI
        public float[] inputDelayArray;     //TODO: ^ this
        public float[] frameDelayArray;     //TODO: ^ this
        public float[] bufferDelayArray;     //TODO: ^ this
        public List<KeyValuePair<float, float>> inputDelayZones = new List<KeyValuePair<float, float>>();
        public List<KeyValuePair<float, float>> frameDelayZones = new List<KeyValuePair<float, float>>();
        public List<KeyValuePair<float, float>> bufferDelayZones = new List<KeyValuePair<float, float>>();


        public override ScenarioObject Deserialize(XElement elem)
        {
            base.Deserialize(elem);

            foreach (XElement subelem in elem.Elements()) {
                switch (subelem.Name.ToString()) {
                    case "inputDelayZones":
                        inputDelayZones.Clear();
                        foreach (XElement kvpelem in subelem.Elements()) {
                            float key = 0;
                            float value = 0;
                            foreach (XElement minorelem in kvpelem.Elements())
                            {
                                if (minorelem.Name == "Key") {
                                    key = float.Parse(minorelem.Value);
                                }
                                else if (minorelem.Name == "Value") {
                                    value = float.Parse(minorelem.Value);
                                }
                            }
                            inputDelayZones.Add(new KeyValuePair<float, float>(key,value));
                        }
                        break;
                    case "frameDelayZones":
                        frameDelayZones.Clear();
                        foreach (XElement kvpelem in subelem.Elements()) {
                            float key = 0;
                            float value = 0;
                            foreach (XElement minorelem in kvpelem.Elements())
                            {
                                if (minorelem.Name == "Key") {
                                    key = float.Parse(minorelem.Value);
                                }
                                else if (minorelem.Name == "Value") {
                                    value = float.Parse(minorelem.Value);
                                }
                            }
                            frameDelayZones.Add(new KeyValuePair<float, float>(key, value));
                        }
                        break;
                    case "bufferDelayZones":
                        bufferDelayZones.Clear();
                        foreach (XElement kvpelem in subelem.Elements())
                        {
                            float key = 0;
                            float value = 0;
                            foreach (XElement minorelem in kvpelem.Elements())
                            {
                                if (minorelem.Name == "Key")
                                {
                                    key = float.Parse(minorelem.Value);
                                }
                                else if (minorelem.Name == "Value")
                                {
                                    value = float.Parse(minorelem.Value);
                                }
                            }
                            bufferDelayZones.Add(new KeyValuePair<float, float>(key, value));
                        }
                        break;
                }
            }

            return this;
        }

        //TODO: Will be removed as soon as scenario editor gets UI
        public void CreateDelayZones()
        {
            inputDelayZones.Clear();
            for (int i = 0; i < distanceArray.Length && i < inputDelayArray.Length; i++)
            {
                inputDelayZones.Add(new KeyValuePair<float, float>(distanceArray[i], inputDelayArray[i]));
            }

            frameDelayZones.Clear();
            for (int i = 0; i < distanceArray.Length && i < frameDelayArray.Length; i++)
            {
                frameDelayZones.Add(new KeyValuePair<float, float>(distanceArray[i], frameDelayArray[i]));
            }

            bufferDelayZones.Clear();
            for (int i = 0; i < distanceArray.Length && i < bufferDelayArray.Length; i++)
            {
                bufferDelayZones.Add(new KeyValuePair<float, float>(distanceArray[i], bufferDelayArray[i]));
            }
        }

        public override XElement Serialize()
        {
            CreateDelayZones();
            XElement elem = base.Serialize();
            XElement subelem;
            elem.Name = "ScenarioDelayBeacon";

            subelem = new XElement("inputDelayZones");
            foreach (KeyValuePair<float, float> kvp in inputDelayZones)
            {
                XElement kvpElem = new XElement("KeyValuePair");

                XElement keyElem = new XElement("Key");
                keyElem.Value = kvp.Key.ToString();
                kvpElem.Add(keyElem);

                XElement valueElem = new XElement("Value");
                valueElem.Value = kvp.Value.ToString();
                kvpElem.Add(valueElem);

                subelem.Add(kvpElem);
                
            }
            elem.Add(subelem);

            subelem = new XElement("frameDelayZones");
            foreach (KeyValuePair<float, float> kvp in frameDelayZones)
            {
                XElement kvpElem = new XElement("KeyValuePair");

                XElement keyElem = new XElement("Key");
                keyElem.Value = kvp.Key.ToString();
                kvpElem.Add(keyElem);

                XElement valueElem = new XElement("Value");
                valueElem.Value = kvp.Value.ToString();
                kvpElem.Add(valueElem);

                subelem.Add(kvpElem);

            }
            elem.Add(subelem);

            subelem = new XElement("bufferDelayZones");
            foreach (KeyValuePair<float, float> kvp in bufferDelayZones)
            {
                XElement kvpElem = new XElement("KeyValuePair");

                XElement keyElem = new XElement("Key");
                keyElem.Value = kvp.Key.ToString();
                kvpElem.Add(keyElem);

                XElement valueElem = new XElement("Value");
                valueElem.Value = kvp.Value.ToString();
                kvpElem.Add(valueElem);

                subelem.Add(kvpElem);

            }
            elem.Add(subelem);

            return elem;
        }
    }

}
