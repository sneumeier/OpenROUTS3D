using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.SUMOConnectionScripts.Maps.SumoImportPolygon
{
    public class Triangle
    {
        public Vertex[] verts = new Vertex[3];

        public Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            verts[0] = new Vertex(a);
            verts[1] = new Vertex(b);
            verts[2] = new Vertex(c);

            verts[0].next = verts[1];
            verts[0].prev = verts[2];

            
            verts[1].next = verts[2];
            verts[1].prev = verts[0];

                
            verts[2].next = verts[0];
            verts[2].prev = verts[1];
        }

        public Triangle(Vertex a, Vertex b, Vertex c)
        {
            verts[0] = (a);
            verts[1] = (b);
            verts[2] = (c);
        }

        public TriangleOrientation orientation
        {
            get 
            {
                float val = (verts[1].vector.y - verts[0].vector.y) * (verts[2].vector.x - verts[1].vector.x) -
                  (verts[1].vector.x - verts[0].vector.x) * (verts[2].vector.y - verts[1].vector.y);

                if (val == 0) return TriangleOrientation.Colinear;  // colinear 

                // clock or counterclock wise 
                return (val > 0) ? TriangleOrientation.Clockwise : TriangleOrientation.Counterclockwise;  
            }
        }

    }

    public enum TriangleOrientation { 
        Colinear,
        Clockwise,
        Counterclockwise
    }
}
