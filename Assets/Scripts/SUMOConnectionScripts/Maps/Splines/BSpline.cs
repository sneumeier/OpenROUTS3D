using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.SUMOConnectionScripts.Maps.Splines
{
    public class BSpline
    {
        public int n = 2; // Degree of the curve

        public Vector3[] controlPoints; // The control points.

        private int[] nV; // Node vector

        public BSpline(int degree, List<Vector3> controlPoints)
        {
            this.controlPoints = controlPoints.ToArray();
            n = degree;

            nV = new int[this.controlPoints.Length + 5];

            createNodeVector();

        }

        public List<Vector3> GetSubdividedPoints()
        {
            List<Vector3> subdiviedPoints = new List<Vector3>() ;

            Vector3 start = controlPoints[0];
            Vector3 end = Vector3.zero;

            subdiviedPoints.Add(start);

            for (float i = 0.0f; i < nV[n + controlPoints.Length]; i += 0.1f)
            {

                for (int j = 0; j < controlPoints.Length; j++)
                {
                    if (i >= j)
                    {
                        end = deBoor(n, j, i);
                    }
                }
                subdiviedPoints.Add(end);
                //Gizmos.DrawLine(start, end);
                start = end;

            }
            subdiviedPoints.Add(controlPoints.Last());

            return subdiviedPoints;
        }

        // Recursive deBoor algorithm.
        public Vector3 deBoor(int r, int i, float u)
        {

            if (r == 0)
            {
                return controlPoints[i];
            }
            else
            {

                float pre = (u - nV[i + r]) / (nV[i + n + 1] - nV[i + r]); // Precalculation
                return ((deBoor(r - 1, i, u) * (1 - pre)) + (deBoor(r - 1, i + 1, u) * (pre)));


            }

        }

        public void createNodeVector()
        {
            int knoten = 0;

            for (int i = 0; i < (n + controlPoints.Length + 1); i++) // n+m+1 = nr of nodes
            {
                if (i > n)
                {
                    if (i <= controlPoints.Length)
                    {

                        nV[i] = ++knoten;
                    }
                    else
                    {
                        nV[i] = knoten;
                    }
                }
                else
                {
                    nV[i] = knoten;
                }
            }
        }


       
        }
}
