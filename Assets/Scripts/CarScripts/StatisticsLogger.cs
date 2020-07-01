using CarScripts;
using SUMOConnectionScripts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace Assets.Scripts.CarScripts
{
    public class StatisticsLogger : MonoBehaviour, IStreamLogger
    {
        public StatisticsContainer carPhysics;
        public FPSCounter fpsCounter;

        public InputManager input;
        public Rigidbody rBody;
        public SumoEgoCar egoCar;
        public CollisionDetection collisionDetection;

        public float startTime;
        public Vector3 lastForce = Vector3.zero;

        private float currentDistance = 0;
        private Vector3 lastPos;
        private bool lastPosInitialized = false;

        public IStreamLogger StreamLogger = null; //Uses itself if StreamLogger is null
        public string targetDirectory = "";
        public string fileName;
        public string csvFileName;

        private StreamWriter writer;

        private StreamWriter foreignCarWriter;

        public char delimiter = ';';

        //Data collected every frame:
        public float currentTime { get { return Time.time - startTime; } }
        public float steeringAngle { get { return carPhysics.angle; } }
        public float pedalPressure { get { return carPhysics.pedal; } }
        public float brakePressure { get { return carPhysics.brake; } }
        public float distanceToLine {
            get 
            {
                
                return 0;
            }
        }

        public float lateralAcc
        {
            get
            {
                Vector3 curForce = rBody.velocity;
                float diff = curForce.x - lastForce.x;
                return diff / Time.deltaTime;

            }
        }
        public float longitudinalAcc
        {
            get
            {
                Vector3 curForce = rBody.velocity;
                float diff = curForce.z - lastForce.z;
                return diff / Time.deltaTime;
            }
        }
        public float currentSpeed { get { return carPhysics.velocity; } }
        public float distanceDriven { get { return currentDistance; } }
        //Things I thought could be useful too:
        public float currentFrameDelay { get { return Settings.displayTimeDelay; } }
        public float currentBufferDelay { get { return Settings.frameBufferDelay; } }
        public float currentInputDelay { get { return Settings.inputTimeDelay; } }
        public Vector3 position
        {
            get
            {
                if (lastPosInitialized)
                {
                    currentDistance += Vector3.Distance(lastPos, carPhysics.transform.position);
                }
                lastPos = carPhysics.transform.position;
                lastPosInitialized = true;
                return carPhysics.transform.position;
            }

        }    //Might be useful for a replay feature that replaces the input manager and car physics
        public Vector3 eulerAngles { get { return carPhysics.transform.rotation.eulerAngles; } }

        //Summary Data
        private List<LoggedCrash> crashes = new List<LoggedCrash>();
                
        void Start()
        {
            startTime = Time.time;
            targetDirectory = Settings.lastLogPath;
            //check if target dirname is empty
            if (!(String.IsNullOrEmpty(targetDirectory)))
            {
                collisionDetection.OnCollisionDetected += CollisionDetection_OnCollisionDetected;
                DateTime now = DateTime.Now;
                fileName = Settings.userName + "_" + Path.GetFileNameWithoutExtension(Settings.mapPath).Replace(".net", "") + now.ToString("dd_MM_yyyy__HH_mm_ss");
                csvFileName = (targetDirectory + Path.DirectorySeparatorChar + fileName + ".csv");
                writer = File.CreateText(csvFileName);
                writer.WriteLine(
                    "Timestamp" + delimiter +
                    "SteeringAngle" + delimiter +
                    "PedalPressure" + delimiter +
                    "DistanceToRoadCenter" + delimiter +
                    "LateralAcceleration" + delimiter +
                    "LongitudinalAcceleration" + delimiter +
                    "Speed" + delimiter +
                    "DistanceDriven" + delimiter +
                    "FrameDelay" + delimiter +
                    "FrameBufferTime" + delimiter +
                    "InputDelay" + delimiter +
                    "CarPosition" + delimiter +
                    "CarRotation" + delimiter +
                    "DynamicDelay" + delimiter +
                    "Velocity" + delimiter +
                    "Brake" + delimiter +
                    "FPS" + delimiter
                    );

                string foreignCsvName = (targetDirectory + Path.DirectorySeparatorChar + fileName + "_foreignCars" + ".csv");
                foreignCarWriter = File.CreateText(foreignCsvName);
                foreignCarWriter.WriteLine(
                    "Timestamp" + delimiter +
                    "UserName" + delimiter +
                    "CarPosition" + delimiter +
                    "CarRotation" + delimiter +
                    "Velocity" + delimiter
                    );
            }
            else
            {
                Debug.LogError("No log directory found!");
            }
        }

        void OnDestroy()
        {
            WriteXML();
            writer.Close();
            foreignCarWriter.Close();
        }

        void OnApplicationQuit()
        {
            WriteXML();
            writer.Close();
            foreignCarWriter.Close();
        }

        public void WriteXML() {
            //check if target dirname is empty
            if (!(String.IsNullOrEmpty(targetDirectory)))
            {
                string xmlFileName = (targetDirectory + Path.DirectorySeparatorChar + fileName + ".xml");

                XElement root = new XElement("SessionSummary");

                XElement elem;

                elem = new XElement("TotalDistanceDriven");
                elem.Value = currentDistance.ToString();
                root.Add(elem);

                elem = new XElement("TotalTime");
                elem.Value = Time.time.ToString();
                root.Add(elem);

                elem = new XElement("HighestFPS");
                elem.Value = fpsCounter.HighestFPS.ToString();
                root.Add(elem);
                elem = new XElement("LowestFPS");
                elem.Value = fpsCounter.LowestFPS.ToString();
                root.Add(elem);

                elem = new XElement("Crashes");
                foreach (var crash in crashes)
                {
                    elem.Add(crash.Serialize());
                }
                root.Add(elem);

                XDocument doc = new XDocument(root);
                doc.Save(xmlFileName);
            }
            else
            {
                Debug.LogError("Unable to save summary; no log directory found!");
            }
        }

        private void CollisionDetection_OnCollisionDetected(Collision col)
        {
            LoggedCrash crash = new LoggedCrash();

            crash.timestamp = Time.time;
            crash.impulse = col.impulse;
            foreach (ContactPoint cp in col.contacts)
            {
                if (cp.thisCollider.gameObject != egoCar.gameObject)
                {
                    crash.collisionObjectName = cp.thisCollider.gameObject.name;
                }
                else
                {
                    crash.impactPoints.Add(cp.point);
                }
                if (cp.otherCollider.gameObject != egoCar.gameObject)
                {
                    crash.collisionObjectName = cp.otherCollider.gameObject.name;
                }
                else
                {
                    crash.impactPoints.Add(cp.point);
                }
            }

            crash.relativeVelocity = col.relativeVelocity;

            crashes.Add(crash);

        }

        void Update()
        {
            if (!(String.IsNullOrEmpty(targetDirectory))) {
                if (StreamLogger == null)
                {
                    this.Log(CreateCsvLine());
                }
                else { StreamLogger.Log(CreateCsvLine()); }
            }
        }

        public void WriteForeignCsvLine(SumoVehicle car)
        {
            
            Vector3 carPos = car.transform.position;
            string line = currentTime.ToString() + delimiter +
                car.name + delimiter +
                car.transform.position.x + "," + car.transform.position.y + "," + car.transform.position.z + delimiter +
                car.transform.rotation.eulerAngles.x + ',' + car.transform.rotation.eulerAngles.y + ',' + car.transform.rotation.eulerAngles.z + delimiter +
                car.velocity +
                "\n";
            foreignCarWriter.Write(line);

            //Debug.Log("Logging foreign car: "+line);
        }

        public string CreateCsvLine()
        {
            Vector3 thisPos = position;
            return currentTime.ToString() + delimiter +
                steeringAngle.ToString() + delimiter +
                pedalPressure.ToString() + delimiter +
                distanceToLine.ToString() + delimiter +
                lateralAcc.ToString() + delimiter +
                longitudinalAcc.ToString() + delimiter +
                currentSpeed.ToString() + delimiter +
                distanceDriven.ToString() + delimiter +
                currentFrameDelay.ToString() + delimiter +
                currentBufferDelay.ToString() + delimiter +
                currentInputDelay.ToString() + delimiter +
                thisPos.x+','+thisPos.y+','+thisPos.z + delimiter +
                eulerAngles.x +','+eulerAngles.y+','+eulerAngles.z+ delimiter +
                Settings.dynamicDelay.ToString()+ delimiter+
                rBody.velocity.x + ',' + rBody.velocity.y + ',' + rBody.velocity.z + delimiter+
                brakePressure+delimiter+ 
                fpsCounter.CurrentFPS.ToString();

        }

        public void Log(string serializedFrame)
        {
            writer.WriteLine(serializedFrame);
        }
    }
}
