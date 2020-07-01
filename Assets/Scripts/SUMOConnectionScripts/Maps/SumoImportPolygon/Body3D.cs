using SumoImportPolygon;
using System.Collections.Generic;
using UnityEngine;

namespace SumoImportPolygon
{

    public class Body3D
    {

        // variables for building heights
        private static float minHeight = 5.0f;
        private static float maxHeight = 15.0f;
        private static float Height;

        /*
        public static Mesh CreateBodyNew(List<Vector3> verts, bool orientation)
        {
            Height = Random.Range(minHeight, maxHeight);
            Triangulator.TriangulateConcavePolygon(verts);
        }
        */
          
        /// <summary>
        /// This methods creates a Unity mesh from given polygon coordinates
        /// (and their orientation; clockwise or counter-clockwise).
        /// </summary>
        /// <param name="polygons">List of 3D polygons</param>
        /// <param name="orientation">orientation of polygon coordinates</param>
        /// <returns>Unity mesh of given 3D polygon</returns>
        public static Mesh CreateBody(List<Vector3> polygons, bool orientation)
        {

            Height = Random.Range(minHeight, maxHeight);

            Vector3[] vertices3DWallsInputWC = polygons.ToArray();

            //
            List<Vector3> listPolygon = polygons;
            listPolygon.AddRange(polygons.GetRange(0, 1)); // only needed for calculation of bary centre

            // bary centre
            Vector3 baryCentre = AreaCalculations.CalculateCentrePosition3D(listPolygon, Height);

            Vector3[] vertices3DWallOutputWC;

            vertices3DWallOutputWC = new Vector3[vertices3DWallsInputWC.Length * 2 + 1]; // <-- walls with ceiling

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < vertices3DWallsInputWC.Length; j++)
                {

                    if (i == 0)
                    {
                        vertices3DWallOutputWC[j] = vertices3DWallsInputWC[j];
                    }
                    else
                    {
                        vertices3DWallOutputWC[j + vertices3DWallsInputWC.Length] = vertices3DWallsInputWC[j];

                        vertices3DWallOutputWC[j + vertices3DWallsInputWC.Length] += new Vector3(0, Height, 0); // given the wall some height
                    }
                }
            }

            // adding bary centre
            vertices3DWallOutputWC[vertices3DWallOutputWC.Length - 1] = baryCentre;

            // setting trianlges
            int[] trianglesWallWC;

            //
            List<int> listTriangles = new List<int>();

            /*
             * adding walls
             * 
             * a wall is split into two triangles, therefore the left (upper) side triangle has the
             * coordinates in one, two and three
             * whereas the right (lower) triangle has the coordinates on four, five and six
             */
            int one = 0;
            int two = vertices3DWallsInputWC.Length;
            int three = two + 1;
            int four = 0;
            int five = three;
            int six = one + 1;

            //
            for (int polygonLenght = 0; polygonLenght < vertices3DWallsInputWC.Length; polygonLenght++)
            {
                if (polygonLenght == vertices3DWallsInputWC.Length - 1)
                {
                    three = vertices3DWallsInputWC.Length;
                    five = vertices3DWallsInputWC.Length;
                    six = 0;
                }

                listTriangles.Add(one);
                listTriangles.Add(two);
                listTriangles.Add(three);
                listTriangles.Add(four);
                listTriangles.Add(five);
                listTriangles.Add(six);

                one++;
                two++;
                three++;
                four++;
                five++;
                six++;

            }

            // adding ceiling
            int temp = vertices3DWallsInputWC.Length;
            int BARYCENTRE = vertices3DWallOutputWC.Length - 1;
            //
            for (int polygonLenght = 0; polygonLenght < vertices3DWallsInputWC.Length - 1; polygonLenght++)
            {

                listTriangles.Add(temp);
                listTriangles.Add(BARYCENTRE);
                if (polygonLenght == vertices3DWallsInputWC.Length - 1)
                {
                    listTriangles.Add(vertices3DWallsInputWC.Length);
                }
                else
                {
                    listTriangles.Add(temp + 1);
                }

                temp++;

            }

            // adding last indices for ceiling of body
            listTriangles.Add(temp);
            listTriangles.Add(BARYCENTRE);
            listTriangles.Add(vertices3DWallsInputWC.Length);

            //
            trianglesWallWC = listTriangles.ToArray();

            // when wrong orientation of given polygons, changing order of triplets for proper display of walls
            if (!orientation)
            {

                int tempChange = 0;

                for (int index = 0; index < trianglesWallWC.Length; index += 3)
                {

                    tempChange = trianglesWallWC[index];
                    trianglesWallWC[index] = trianglesWallWC[index + 2];
                    trianglesWallWC[index] = tempChange;

                }

            }

            // Create the mesh
            Mesh msh = new Mesh();

            msh.vertices = vertices3DWallOutputWC;
            msh.triangles = trianglesWallWC;

            msh.RecalculateNormals();
            msh.RecalculateBounds();

            return msh;
        }
        
    }

}
