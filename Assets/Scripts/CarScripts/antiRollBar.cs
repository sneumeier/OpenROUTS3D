using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CarScripts
{

    public class antiRollBar : MonoBehaviour
    {

        public WheelCollider wheelR;
        public WheelCollider wheelL;
        public float AntiRoll;
        public Rigidbody rigbody;
        private WheelHit hit;
        
        void FixedUpdate()
        {
            float travelL = 1.0f;
            float travelR = 1.0f;


            var groundedR = wheelL.GetGroundHit(out hit);
            if (groundedR)
            {
                travelR = (-wheelL.transform.InverseTransformPoint(hit.point).y - wheelL.radius) / wheelL.suspensionDistance;

            }


            var groundedL = wheelR.GetGroundHit(out hit);
            if (groundedL)
            {
                travelL = (-wheelR.transform.InverseTransformPoint(hit.point).y - wheelR.radius) / wheelR.suspensionDistance;

            }


            var antiRollForce = (travelL - travelR) * AntiRoll;

            if (groundedL)
                rigbody.AddForceAtPosition(wheelL.transform.up * -antiRollForce,
                       wheelL.transform.position);
            if (groundedR)
                rigbody.AddForceAtPosition(wheelR.transform.up * antiRollForce,
                       wheelR.transform.position);  



        }
    }
}