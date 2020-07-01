using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Assets.Scripts.InterfaceScene
{
    public class ScenarioScene:AutomatedScene
    {
        public float interframeDelay;
        public float inputDelay;
        public float outputDelay;
        public float resolutionFactor =1;
        public string mapPath;

        public float time;
        public bool skippable;

        public ScenarioScene(string path, float interframeDelay, float inputDelay, float outputDelay, float resolutionFactor)
        {
            this.mapPath = path;
            this.interframeDelay = interframeDelay;
            this.inputDelay = inputDelay;
            this.outputDelay = outputDelay;
            this.resolutionFactor = resolutionFactor;
        }

        public ScenarioScene(XElement root)
        {
            foreach (XElement elem in root.Elements())
            {
                switch (elem.Name.ToString())
                {
                    case "InterFrameDelay":
                        interframeDelay = float.Parse(elem.Value, System.Globalization.CultureInfo.InvariantCulture);
                        break;

                    case "InputDelay":
                        inputDelay = float.Parse(elem.Value, System.Globalization.CultureInfo.InvariantCulture);
                        break;

                    case "OutputDelay":
                        outputDelay = float.Parse(elem.Value, System.Globalization.CultureInfo.InvariantCulture);
                        break;

                    case "ResolutionFactor":
                        resolutionFactor = float.Parse(elem.Value, System.Globalization.CultureInfo.InvariantCulture);
                        break;

                    case "Path":
                        mapPath = elem.Value;
                        break;

                    case "Time":
                        time = float.Parse(elem.Value,System.Globalization.CultureInfo.InvariantCulture);
                        break;

                    case "Skippable":
                        skippable = bool.Parse(elem.Value);
                        break;
                }
            }
        }

    }
}
