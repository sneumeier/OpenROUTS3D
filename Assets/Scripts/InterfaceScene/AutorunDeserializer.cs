using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.InterfaceScene
{
    public static class AutorunDeserializer
    {
        public static List<AutomatedScene> scenes = new List<AutomatedScene>();
        public static int index = 0;
        public static bool automated = false;

        public static Dictionary<string, List<string>> answers = new Dictionary<string, List<string>>(); 

        public static AutomatedScene GetScene()
        {
            return scenes[index];
        }

        public static AutomatedScene NextScene()
        {
            index++;
            if (index >= scenes.Count)
            {
                return null;
            }
            return scenes[index];
        }

        public static void SaveAnswers()
        {
            string path = Path.Combine(Settings.lastLogPath,Settings.userName+"_Answers_"+DateTime.Now.ToString("dd_MM_yyyy__hh_mm_ss")+".xml");
            XElement root = new XElement("Answers");
            
            foreach (var key in answers.Keys)
            {
                XElement elem = new XElement("Question");
                root.Add(elem);
                XElement identifier = new XElement("Identifier");
                identifier.Value = key;
                elem.Add(identifier);
                foreach (string ans in answers[key])
                {
                    XElement answerElem = new XElement("Answer");
                    answerElem.Value = ans;
                    elem.Add(answerElem);
                }
            }

            XDocument doc = new XDocument(root);
            doc.Save(path);
            Debug.Log("Saved Answers to \""+path+"\"");

        }


        public static void Deserialize(string path)
        {
            scenes.Clear();
            index = 0;
            XElement root = XDocument.Load(path).Root;
            foreach (XElement elem in root.Elements())
            {
                switch (elem.Name.ToString())
                { 
                    case "Scenario":
                        scenes.Add(new ScenarioScene(elem));
                        break;

                    case "Questions":
                        scenes.Add(new InterfaceScene(elem));
                        break;
                }
            }
        }

        public static void LoadScene()
        {
            if (index >= scenes.Count)
            {
                AutorunDeserializer.SaveAnswers();
                SceneManager.LoadScene(0);
            }
            else if (scenes[index] is ScenarioScene)
            {
                Settings.displayTimeDelay = ((ScenarioScene)scenes[index]).interframeDelay;
                Settings.frameBufferDelay = ((ScenarioScene)scenes[index]).outputDelay;
                Settings.inputTimeDelay = ((ScenarioScene)scenes[index]).inputDelay;
                Settings.mapPath = ((ScenarioScene)scenes[index]).mapPath;
                Settings.polyPath = Settings.mapPath.Replace("net.xml", "poly.xml");
                Settings.scenarioPath = Settings.mapPath.Replace("net.xml", "scenario.xml");
                Settings.skippableScenario = ((ScenarioScene)scenes[index]).skippable;
                Settings.scenarioTime = ((ScenarioScene)scenes[index]).time;
                Settings.screenFactor = ((ScenarioScene)scenes[index]).resolutionFactor;
                Settings.enableScenario = true;
                SceneManager.LoadScene(1);
            }
            else
            {
                SceneManager.LoadScene(2);
                Debug.Log("Loading Interface for questions");
            }

        }
    }
}
