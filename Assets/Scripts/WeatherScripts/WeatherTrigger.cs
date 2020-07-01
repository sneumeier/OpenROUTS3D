using Assets.Scripts.AssetReplacement;
using Assets.Scripts.ScenarioEditor;
using CarScripts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace Assets.Scripts.WeatherScripts
{
    public class WeatherTrigger:ScenarioObject
    {
        public int FogLevel;//0 = no fog, 3 = Heavy
        public int SnowLevel;//0 = no snow, 3 = Heavy
        public int RainLevel;//0 = no Rain, 3 = Heavy,
        public SkyboxType Skybox;
        public float AmbientIntensity;
        public Color AmbientColor;
        public float BoxX;
        public float BoxY;
        public float BoxZ;

        public BoxCollider BoxCollider;
        private WeatherObject Weather;

        public void Init()
        {
            BoxCollider.size = new Vector3(BoxX, BoxY, BoxZ);
            Weather = AnchorMapping.GetAnchor("Weather").GetComponent<WeatherObject>();
        }

        private void OnTriggerEnter(Collider other)
        {

            if (other.attachedRigidbody.GetComponent<WheelDrive>() != null)
            {
                Weather.SetWeatherByWeatherTrigger(this);
            }
        }

        public override ScenarioObject Deserialize(XElement elem)
        {
            base.Deserialize(elem);
            foreach (XElement subelem in elem.Elements())
            {
                float colR = 0;
                float colG = 0;
                float colB = 0;
                float colA = 0;

                switch (subelem.Name.ToString())
                {
                    case "FogLevel":
                        FogLevel = int.Parse(subelem.Value);
                        break;
                    case "SnowLevel":
                        SnowLevel = int.Parse(subelem.Value);
                        break;
                    case "RainLevel":
                        RainLevel = int.Parse(subelem.Value);
                        break;
                    case "Skybox":
                        Skybox = (SkyboxType)Enum.Parse(typeof(SkyboxType), subelem.Value);
                        break;
                    case "AmbientIntensity":
                        AmbientIntensity = float.Parse(subelem.Value);
                        break;
                    case "AmbientColorR":
                        colR = float.Parse(subelem.Value);
                        break;
                    case "AmbientColorG":
                        colG = float.Parse(subelem.Value);
                        break;
                    case "AmbientColorB":
                        colB = float.Parse(subelem.Value);
                        break;
                    case "AmbientColorA":
                        colA = float.Parse(subelem.Value);
                        break;
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

                AmbientColor = new Color(colR,colG,colB,colA);
            }
            Init(); //Initialize the Collider
            return this;
        }

        public override XElement Serialize()
        {

            XElement elem = base.Serialize();

            XElement subelemFog = new XElement("FogLevel");
            XElement subelemSnow = new XElement("SnowLevel");
            XElement subelemRain = new XElement("RainLevel");

            XElement subelemSkybox = new XElement("Skybox");
            XElement subelemAmbientIntensity = new XElement("AmbientIntensity");
            XElement subelemAmbientColorR = new XElement("AmbientColorR");
            XElement subelemAmbientColorG = new XElement("AmbientColorG");
            XElement subelemAmbientColorB = new XElement("AmbientColorB");
            XElement subelemAmbientColorA = new XElement("AmbientColorA");

            XElement subelemBoxX = new XElement("BoxX");
            XElement subelemBoxY = new XElement("BoxY");
            XElement subelemBoxZ = new XElement("BoxZ");


            elem.Name = "WeatherTrigger";

            subelemFog.Value = FogLevel.ToString();
            subelemSnow.Value = SnowLevel.ToString();
            subelemRain.Value = RainLevel.ToString();
            subelemSkybox.Value = Skybox.ToString("g");
            subelemAmbientIntensity.Value = AmbientIntensity.ToString(CultureInfo.InvariantCulture);

            subelemAmbientColorR.Value = AmbientColor.r.ToString(CultureInfo.InvariantCulture);
            subelemAmbientColorG.Value = AmbientColor.g.ToString(CultureInfo.InvariantCulture);
            subelemAmbientColorB.Value = AmbientColor.b.ToString(CultureInfo.InvariantCulture);
            subelemAmbientColorA.Value = AmbientColor.a.ToString(CultureInfo.InvariantCulture);

            subelemBoxX.Value = BoxX.ToString();
            subelemBoxY.Value = BoxY.ToString();
            subelemBoxZ.Value = BoxZ.ToString();

            elem.Add(subelemFog);
            elem.Add(subelemSnow);
            elem.Add(subelemRain);
            elem.Add(subelemSkybox);
            elem.Add(subelemAmbientIntensity);
            elem.Add(subelemAmbientColorR);
            elem.Add(subelemAmbientColorG);
            elem.Add(subelemAmbientColorB);
            elem.Add(subelemAmbientColorA);
            elem.Add(subelemBoxX);
            elem.Add(subelemBoxY);
            elem.Add(subelemBoxZ);

            return elem;
        }
    }
}
