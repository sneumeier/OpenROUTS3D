using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.SUMOConnectionScripts.Maps.Splines
{
    public class BSplineControlPoint : MonoBehaviour
    {

        public Color color = Color.red;

        public Vector3 cachedPosition;

        void Start()
        {
            cachedPosition = transform.position;
        }

        void OnDrawGizmos()
        {

            cachedPosition = transform.position;

            // Draw control point
            Gizmos.color = color;
            Gizmos.DrawSphere(cachedPosition, 0.1f);

        }

    }
}
