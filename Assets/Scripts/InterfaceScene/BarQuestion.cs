using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Assets.Scripts.InterfaceScene
{
    public class BarQuestion:InterfaceQuestion
    {
        public float min;
        public float max;
        public string minDescription = "";
        public string maxDescription = "";
        public bool customDescription = false;
        public int subdivisions = 0;

        public BarQuestion(string text, string identifier, float min, float max, int subdivisions)
            : base(text, identifier)
        {
            this.min = min;
            this.max = max;
            this.subdivisions = subdivisions;
        }

        public BarQuestion(XElement root)
            : base("text", "identifier")
        {
            foreach (XElement elem in root.Elements())
            {
                switch (elem.Name.ToString())
                {
                    case "Text":
                        text = elem.Value;
                        break;
                    case "Identifier":
                        identifier = elem.Value;
                        break;
                    case "Min":
                        min = float.Parse(elem.Value);
                        break;
                    case "Max":
                        max = float.Parse(elem.Value);
                        break;
                    case "MinDescription":
                        minDescription = elem.Value;
                        break;
                    case "MaxDescription":
                        maxDescription = elem.Value;
                        break;
                    case "CustomDescription":
                        customDescription = bool.Parse(elem.Value);
                        break;
                    case "Subdivisions":
                        subdivisions = int.Parse(elem.Value);
                        break;
                }
            }
        }

    }
}
