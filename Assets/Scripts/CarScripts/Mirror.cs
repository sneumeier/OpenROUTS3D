using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CarScripts
{
    public class Mirror : MonoBehaviour
    {
        public Transform mirrorCam;
        public Transform casterCam;

        void Start() {
            UpdateRotation();
        }
        
        void UpdateRotation()
        {
            Vector3 dir = (casterCam.position-transform.position).normalized;
            Quaternion quat = Quaternion.LookRotation(dir);

            quat.eulerAngles = transform.eulerAngles - quat.eulerAngles;

            mirrorCam.localRotation = quat;
        }
    }
}