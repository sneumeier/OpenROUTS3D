using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace Assets.Scripts.MultiplayerMessages
{
    public class ScenarioSettingsMessage:NetworkMessage
    {
        public string mapPath;
        public string scenarioPath; //empty string if no scenario is loaded
        public Vector3 spawnPosition;
        public Vector3 spawnRotation;   //euler
        public string mapChecksum;
        public string scenarioChecksum;

        public List<PlayerJoinedMessage> playerCars = new List<PlayerJoinedMessage>();



        public override XElement Serialize()
        {
            XElement root = new XElement(typeof(ScenarioSettingsMessage).ToString());

            XElement map = new XElement("mapPath");
            map.Value = mapPath;
            root.Add(map);

            XElement scen = new XElement("scenarioPath");
            scen.Value = scenarioPath;
            root.Add(scen);

            XElement cs = new XElement("mapChecksum");
            cs.Value = mapChecksum;
            root.Add(cs);

            XElement sc = new XElement("scenarioChecksum");
            sc.Value = scenarioChecksum;
            root.Add(sc);

            XElement spRot = new XElement("spawnRotation");
            spRot.Value = NetworkMessage.SerializeVector(spawnRotation);
            root.Add(spRot);

            XElement spPos = new XElement("spawnPosition");
            spPos.Value = NetworkMessage.SerializeVector(spawnPosition);
            root.Add(spPos);

            XElement pc = new XElement("playerCars");
            foreach (PlayerJoinedMessage pjm in playerCars)
            {
                pc.Add(pjm.Serialize());
            }
            root.Add(pc);

            return root;
        }

        public static ScenarioSettingsMessage Deserialize(XElement root)
        {
            ScenarioSettingsMessage message = new ScenarioSettingsMessage();
            Debug.Log("Deserializing Scenario Settings");
            foreach (XElement elem in root.Elements())
            {
                switch (elem.Name.ToString())
                {
                    case "mapPath":
                        Debug.Log("mapPath");
                        message.mapPath = elem.Value;
                        break;
                    case "scenarioPath":
                        Debug.Log("scenarioPath");
                        message.scenarioPath = elem.Value;
                        break;
                    case "mapChecksum":
                        Debug.Log("mapChecksum");
                        message.mapChecksum = elem.Value;
                        break;
                    case "scenarioChecksum":
                        Debug.Log("scenarioChecksum");
                        message.scenarioChecksum = elem.Value;
                        break;

                    case "spawnRotation":
                        Debug.Log("spawnRotation");
                        message.spawnRotation = NetworkMessage.DeserializeVector(elem.Value);
                        break;
                    case "spawnPosition":
                        Debug.Log("spawnPosition");
                        message.spawnPosition = NetworkMessage.DeserializeVector(elem.Value);
                        break;
                    case "playerCars":
                        Debug.Log("playerCars");
                        foreach (XElement subelem in elem.Elements())
                        {
                            message.playerCars.Add(PlayerJoinedMessage.Deserialize(subelem));
                        }
                        break;

                }
            }
            Debug.Log("Finished ScenarioSettingsMessage Deserialization");
            return message;
        }


    }
}
