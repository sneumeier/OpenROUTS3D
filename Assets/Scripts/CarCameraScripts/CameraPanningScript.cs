using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.CarCameraScripts
{
    public class CameraPanningScript:MonoBehaviour
    {

        public bool rotateHorizontally;
        private Vector3 initialRotation;
        public float intensity;
        public float timeFactor;

        public void Start()
        {
            initialRotation = transform.rotation.eulerAngles;
        }


        public void Update()
        {
            float now = Time.time*timeFactor;
            if (rotateHorizontally)
            {
                transform.rotation = Quaternion.Euler(initialRotation.x, initialRotation.y + (intensity * Mathf.Sin(now)), initialRotation.z);
            }
            else {
                transform.rotation = Quaternion.Euler(initialRotation.x+ (intensity * Mathf.Sin(now)), initialRotation.y, initialRotation.z);

            }
        }
    }
}
