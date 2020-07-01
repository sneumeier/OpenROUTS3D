using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.CarScripts
{
    public class ColorCar:MonoBehaviour
    {
        public bool instantApply;
        public MeshRenderer[] surfaceRenderers;

        public void Start()
        {
            if (instantApply)
            {
                Apply(new Color(Settings.multiplayerColorRed,Settings.multiplayerColorGreen,Settings.multiplayerColorBlue));
            }
        }

        public void Apply(Color col)
        { 
            foreach(MeshRenderer mr in surfaceRenderers)
            {
                mr.material.color = col;
            }
        }
    }
}
