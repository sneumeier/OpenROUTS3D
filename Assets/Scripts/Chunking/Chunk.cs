using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Chunking
{
    public class Chunk
    {
        public int x;
        public int y;
        public List<MeshRenderer> detailRenderers = new List<MeshRenderer>();
        public List<MeshRenderer> commonRenderers = new List<MeshRenderer>();

        public bool detailIsOn = true;
        public bool commonIsOn = true;

        public static float chunkwidth = 75;

        public Chunk(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public void TurnOffDetails()
        {
            foreach (MeshRenderer mr in detailRenderers)
            {   
                if (mr != null)
                    mr.enabled = false;
            }
            detailIsOn = false;
        }

        public void TurnOnDetails()
        {
            foreach (MeshRenderer mr in detailRenderers)
            {
                if(mr != null)
                    mr.enabled = true;
            }
            detailIsOn = true;
        }

        public void TurnOffCommons()
        {
            foreach (MeshRenderer mr in commonRenderers)
            {
                if(mr != null)
                    mr.enabled = false;
            }
            commonIsOn = false;
        }

        public void TurnOnCommons()
        {
            foreach (MeshRenderer mr in commonRenderers)
            {
                if(mr != null)
                    mr.enabled = true;
            }
            commonIsOn = true;
        }

        //Helper Function for Chunk Logic
        public static int FullDivision(float a, float b)
        {
            if (a >= 0) { return (int)(a / b); } else return (int)(a / b) - 1;
        }

    }
}
