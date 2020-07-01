using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CockpitScripts
{

    public class SteeringWheelRotation : MonoBehaviour
    {
        public StatisticsContainer sC;
        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            //Magic numbers are needed because of different number Types. The sC.angle returns a range of -1,1 in between 0.xx values.
            //That's why we do some multiplication with those magic numbers here.
            transform.localEulerAngles = new Vector3(-(280 * sC.angle), -90, 90);
        }
    }
}