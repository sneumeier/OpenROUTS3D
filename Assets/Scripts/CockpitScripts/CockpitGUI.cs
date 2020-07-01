using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//using UnityEngine;

namespace CockpitScripts
{

    public class CockpitGUI : MonoBehaviour
    {
        public StatisticsContainer carPhysics;
        public bool reverseIndicationState;
        public string velocityText;
        public string avgSteeringMovementText;
        public string gearText;
        public string avgBrakeMovementText;
        public TextMesh reverseIndication;
        public TextMesh reverseIndicationCockpit;
        public TextMesh velocity;
        public TextMesh avgSteeringMovement;
        public TextMesh gear;
        public TextMesh avgBrakeMovement;
        // Use this for initialization
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            display();
            //nText = "LIKE";
        }

        void display()
        {
			reverseIndicationState = carPhysics.reverse;
			if (reverseIndicationState == true)
			{
				reverseIndication.color = Color.red;
				reverseIndicationCockpit.color = Color.red;
			}else
			{
				reverseIndication.color = Color.grey;
				reverseIndicationCockpit.color = Color.grey;
			}

            
            velocityText = "" + System.Math.Round(carPhysics.velocity, 0);//get display text
            velocity.text = velocityText; //update the textmesh

            avgSteeringMovementText = "" + System.Math.Round(carPhysics.angle, 2);//get display text
            avgSteeringMovement.text = avgSteeringMovementText;
        }
    }
}
