using CarCameraScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.AssetReplacement.AddOns
{
    public class CamDeletion : MonoBehaviour
    {
        public bool initialized = false;

        // Use this for initialization
        void Update()
        {

            if (!initialized)
            {
                initialized = true;

                AnchorMapping.GetAnchor("CamContainer").GetComponent<MouseCamMover>().enabled = false;
            }
        }
    }
}