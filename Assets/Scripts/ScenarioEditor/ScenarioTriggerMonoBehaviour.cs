using Assets.Scripts.CarScripts;
using Assets.Scripts.InterfaceScene;
using CarScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.ScenarioEditor
{
    public class ScenarioTriggerMonoBehaviour:MonoBehaviour
    {
        public BoxCollider BoxCollider;

        public void Init(ScenarioTrigger trigger)
        {
            BoxCollider.size = new Vector3(trigger.BoxX,trigger.BoxY, trigger.BoxZ);
        }

        private void OnTriggerEnter(Collider other)
        {

            if (other.attachedRigidbody.GetComponent<WheelDrive>() != null)
            {
                other.attachedRigidbody.GetComponent<StatisticsLogger>().WriteXML();
                AutorunDeserializer.NextScene();
                AutorunDeserializer.LoadScene();
            }
        }



    }
}
