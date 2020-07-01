using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.SUMOConnectionScripts
{
    public class Junction
    {
        public List<Vector3> positions;
        public string identifier;
        public List<string> connectedLanes;
        public float AverageHeight;

        public Junction(List<Vector3> positions, string identifier, List<string> connectedLanes)
        {
            this.positions = positions;
            this.identifier = identifier;
            this.connectedLanes = connectedLanes;
            
        }

        public void CalculateAverageHeight()
        {
            float avrgHeight = 0;

            int i = 0;
            foreach (Vector3 point in this.positions)
            {
                avrgHeight += point.y;
                i++;
            }
            AverageHeight = avrgHeight / i;
        }
    }
}
