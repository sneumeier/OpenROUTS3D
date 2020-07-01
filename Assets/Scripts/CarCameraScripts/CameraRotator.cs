using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CarCameraScripts
{

    public class CameraRotator : MonoBehaviour
    {

        public GameObject Target;
        public float RotationFactor;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            //Rotate around target
            transform.RotateAround(Target.transform.position, Vector3.up, RotationFactor * Time.fixedDeltaTime);
            //Face Target
            transform.LookAt(Target.transform.position);
        }
    }
}