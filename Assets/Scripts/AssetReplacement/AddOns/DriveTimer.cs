using Assets.Scripts.CarScripts;
using Assets.Scripts.InterfaceScene;
using CarScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.AssetReplacement.AddOns
{
    public class DriveTimer:MonoBehaviour
    {
        
        public Text text;

        private float startTime = 0.0f;
        public bool hasStarted = false;

        public WheelDrive physics
        {
            get { return AnchorMapping.GetAnchor("CarPhysics").GetComponent<WheelDrive>(); }
        }

        void Update()
        {
            
            if (!hasStarted)
            {
                if (physics.carStatistic.velocity > 1)
                {
                    hasStarted = true;
                    startTime = Time.time;
                }
            }
            else {
                float diffTime = Time.time - startTime;
                int secs = (int)(diffTime % 60);
                int mins = (int)(diffTime / 60);
                text.text = mins.ToString("00") + ":" + secs.ToString("00");
                if (Settings.scenarioTime <= diffTime && Settings.automatedPlay)
                {
                    AnchorMapping.GetAnchor("CarPhysics").GetComponent<StatisticsLogger>().WriteXML();
                    AutorunDeserializer.NextScene();
                    AutorunDeserializer.LoadScene();
                }
            }
        }
    }
}
