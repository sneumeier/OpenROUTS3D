using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CockpitScripts
{
    public class SpeedometerFunction : MonoBehaviour
    {
        //[Range(0f, 180f)]
        public StatisticsContainer sC;
        public float speedometerRotation = 0f;
        public float minAngle;
        public float maxAngle;
        
        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            float calcedAngle = Mathf.Lerp(minAngle, maxAngle, Mathf.InverseLerp(0, sC.maxVelocity, sC.velocity));
            transform.localEulerAngles = new Vector3(-calcedAngle-80,-90, 90);
        }
    }
}