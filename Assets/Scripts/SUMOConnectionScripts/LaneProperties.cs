using SUMOConnectionScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.SUMOConnectionScripts
{
    public struct LaneProperties
    {
        public string laneId;
        public float speed;

        public int laneIndex;
        public float width;
        public string parentedge;
        public List<VehicleClasses> AllowedVClass;
        public List<VehicleClasses> DisallowedVClass;
    }
}
