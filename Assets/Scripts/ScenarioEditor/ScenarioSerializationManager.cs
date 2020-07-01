using Assets.Scripts.WeatherScripts;
using CarScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace Assets.Scripts.ScenarioEditor
{
    public class ScenarioSerializationManager:MonoBehaviour
    {

        public GameObject car;

        public GameObject pylonPrefab;
        public GameObject staticCarPrefab;
        public GameObject delayBeaconPrefab;
        public GameObject scenarioTriggerPrefab;
        public GameObject weatherTriggerPrefab;

        private string savePath;
        public string loadPath { get { return Settings.scenarioPath; } }

        public void Start() {
            
            savePath = loadPath;
            //Load scenario XML
            //deserialize all elements
            if (!Settings.enableScenario)
            {
                Debug.Log("Loading scenario is disabled");
                return;
            }

            XDocument doc = XDocument.Load(loadPath);
            XElement root = doc.Root;

            Debug.Log("Loading scenario...");
            Settings.dynamicDelay = false;
            foreach(XElement elem in root.Elements())
            {
                GameObject go;
                switch (elem.Name.ToString())
                {
                    case "ScenarioObject":
                        Debug.Log("Untagged Scenario Object detected");
                        break;
                    case "ScenarioDelayBeacon":
                        go = GameObject.Instantiate(delayBeaconPrefab);
                        ScenarioDelayBeacon sdb = go.GetComponent<ScenarioDelayBeacon>();
                        sdb.Deserialize(elem);
                        car.GetComponent<InputManager>().delayBeacons.Add(sdb);
                        Settings.dynamicDelay = true;
                        Debug.Log("Activating Dynamic Delay");
                        break;
                    case "ScenarioPylon":
                        go = GameObject.Instantiate(pylonPrefab);
                        ScenarioPylon sp = go.GetComponent<ScenarioPylon>();
                        sp.Deserialize(elem);
                        break;
                    case "ScenarioStaticCar":
                        go = GameObject.Instantiate(staticCarPrefab);
                        ScenarioStaticCar ssc = go.GetComponent<ScenarioStaticCar>();
                        ssc.Deserialize(elem);
                        break;
                    case "ScenarioSpawnPoint":
                        ScenarioSpawnPoint spawn = car.GetComponent<ScenarioSpawnPoint>();
                        spawn.Deserialize(elem);
                        break;
                    case "ScenarioTrigger":
                        go = GameObject.Instantiate(scenarioTriggerPrefab);
                        ScenarioTrigger st = go.GetComponent<ScenarioTrigger>();
                        st.Deserialize(elem);
                        go.GetComponent<ScenarioTriggerMonoBehaviour>().Init(st);
                        break;
                    case "WeatherTrigger":
                        go = GameObject.Instantiate(weatherTriggerPrefab);
                        WeatherTrigger wt = go.GetComponent<WeatherTrigger>();
                        wt.Deserialize(elem);
                        break;
                }

            }

        }


        //TODO check if get component returns derivates
        public void Save() {
            Debug.Log("Serializing Scenario XML");
            
            XElement root = new XElement("root");


            GameObject[] allObjs = UnityEngine.Object.FindObjectsOfType<GameObject>();

            foreach (GameObject obj in allObjs)
            {
                
                ScenarioObject so = obj.GetComponent<ScenarioObject>();
                if (so == null) { so = obj.GetComponent<ScenarioDelayBeacon>(); }
                if (so == null) { so = obj.GetComponent<ScenarioPylon>(); }
                if (so == null) { so = obj.GetComponent<ScenarioStaticCar>(); }
                if (so == null) { so = obj.GetComponent<ScenarioSpawnPoint>(); }
                if (so == null) { so = obj.GetComponent<ScenarioTrigger>(); }

                if (so != null)
                {
                    Debug.Log("Serializing "+so.gameObject.name);
                    XElement elem = so.Serialize();
                    root.Add(elem);
                }
            }
            XDocument doc = new XDocument(root);
            doc.Save(savePath);
        }

    }
}
