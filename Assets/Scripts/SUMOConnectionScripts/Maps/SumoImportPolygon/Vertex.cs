using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.SUMOConnectionScripts.Maps.SumoImportPolygon
{
    public class Vertex
    {
        public Vector3 vector;
        public Vertex prev = null;
        public Vertex next = null;

        public bool isReflex;
        public bool isConvex;
        public bool isEar;

        public Vertex(Vector3 v3)
        {
            vector = v3;
        }

        public Vector2 GetPos2D_XZ()
        {
            Vector2 pos_2d_xz = new Vector2(vector.x, vector.z);

            return pos_2d_xz;
        }
    }
}
