using SumoImportPolygon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SumoImportPolygon
{

    public class Body3Dv3
    {

        // variables for building heights
        private static float minHeight = 5.0f;
        private static float maxHeight = 15.0f;
        private static float Height;

        public static Mesh CreateBody(List<Vector3> polygons, bool orientation)
        {

            Height = Random.Range(minHeight, maxHeight);

            //Debug.Log("polygon count = " + polygons.Count);

            //
            List<Vector3> listPolygon = new List<Vector3>(polygons);

            Debug.Log("polygon count = " + listPolygon.Count);

            // calculating bary centre
            Vector3 baryCentre = AreaCalculations.CalculateCentrePosition3D(listPolygon, Height);

            /*
             * setting vertices for walls and ceiling
             */

            Vector3[] arrayVertices3DBuilding;
            List<Vector3> listVertices3DBuilding = new List<Vector3>();
            List<Vector3> tmpListVertices = new List<Vector3>();

            tmpListVertices = listVertices3DBuilding;

            // adding ground for the first time
            foreach (Vector3 v3 in listPolygon)
            {
                listVertices3DBuilding.Add(v3);
            }

            // adding ceiling for the first time
            foreach (Vector3 v3 in listPolygon)
            {
                Vector3 tempV3 = v3;

                tempV3 += new Vector3(0, Height, 0);

                listVertices3DBuilding.Add(tempV3);
            }

            int tmpList = listVertices3DBuilding.Count;

            //Debug.Log("VerticesArrayList_size 1 = " + listVertices3DBuilding.Count);

            for (int i = 0; i < tmpList; i++)
            {
                // doubling all vertices for using UVs later on
                Vector3 tmp = listVertices3DBuilding[i];
                Vector3 v = new Vector3(tmp.x, tmp.y, tmp.z);
                tmpListVertices.Add(v);
            }

            //Debug.Log("VerticesArrayList_size 2 = " + tmpListVertices.Count);

            // adding ceiling for the second time, this time for using for creating ceiling mesh
            foreach (Vector3 v3 in listPolygon)
            {
                Vector3 tempV3 = v3;

                tempV3 += new Vector3(0, Height, 0);

                listVertices3DBuilding.Add(tempV3);
            }

            Debug.Log("vertices" + listVertices3DBuilding.Count);

            // adding bary centre for ceiling
            listVertices3DBuilding.Add(baryCentre);

            Debug.Log("vertices" + listVertices3DBuilding.Count);

            //
            arrayVertices3DBuilding = listVertices3DBuilding.ToArray();

            /*
             * setting trianlges
             */

            //Debug.Log("polygon count = " + polygons.Count);

            // calculating offset for generating indices
            int offset = 2 * polygons.Count + 1;

            Debug.Log("offset = " + offset);

            int[] trianglesWallWC;

            //
            List<int> listTriangles = new List<int>();

            /*
             * adding walls
             * 
             * a wall is split into two triangles, therefore the left (upper) side triangle has the
             * coordinates in one, two and three
             * whereas the right (lower) triangle has the coordinates on four, five and six
             * 
             */
             
            // first wall
            int wall01_left_down = 0;
            int wall01_left_up = polygons.Count;
            int wall01_right_up = wall01_left_up + 1;
            int wall01_right_down = wall01_left_down + 1;

            //Debug.Log("wall 1 = " + wall01_left_down);
            //Debug.Log("wall 1 = " + wall01_left_up);
            //Debug.Log("wall 1 = " + wall01_right_up);
            //Debug.Log("wall 1 = " + wall01_right_down);

            //Debug.Log("0 + 1 = " + (0 + 1));

            // second wall
            int wall02_left_down = offset;
            int wall02_left_up = wall02_left_down + polygons.Count;
            int wall02_right_up = wall02_left_up + 1;
            int wall02_right_down = wall02_left_down + 1;

            //Debug.Log("wall 2 = " + wall02_left_down);
            //Debug.Log("wall 2 = " + wall02_left_up);
            //Debug.Log("wall 2 = " + wall02_right_up);
            //Debug.Log("wall 2 = " + wall02_right_down);

            // toggle
            int toggle = 1; // modulo 2 produces '1' or '0'

            //Debug.Log("toggle = " + toggle % 2);

            // adding indices for walls
            for (int polygonLenght = 0; polygonLenght < polygons.Count; polygonLenght++)
            {

                // first wall
                if ((toggle % 2) == 1)
                {

                    // first triplet of wall
                    listTriangles.Add(wall01_left_down);
                    listTriangles.Add(wall01_left_up);
                    listTriangles.Add(wall01_right_up);

                    // second triplet of wall
                    listTriangles.Add(wall01_left_down);
                    listTriangles.Add(wall01_right_up);
                    listTriangles.Add(wall01_right_down);
                    
                    wall01_left_down = wall01_left_down + 2;
                    wall01_left_up = wall01_left_up + 2;
                    wall01_right_up = wall01_right_up + 2;
                    wall01_right_down = wall01_right_down + 2;

                    //Debug.Log("wall 1 = " + wall01_left_down);
                    //Debug.Log("wall 1 = " + wall01_left_up);
                    //Debug.Log("wall 1 = " + wall01_right_up);
                    //Debug.Log("wall 1 = " + wall01_right_down);

                    //
                    toggle++;

                    // second wall
                }
                else if ((toggle % 2) == 0)
                {
                    // first triplet of wall
                    listTriangles.Add(wall02_left_down);
                    listTriangles.Add(wall02_left_up);
                    listTriangles.Add(wall02_right_up);

                    // second triplet of wall
                    listTriangles.Add(wall02_left_down);
                    listTriangles.Add(wall02_right_up);
                    listTriangles.Add(wall02_right_down);
                    
                    wall02_left_down = wall02_left_down + 2;
                    wall02_left_up = wall02_left_up + 2;
                    wall02_right_up = wall02_right_up + 2;
                    wall02_right_down = wall02_right_down + 2;

                    //Debug.Log("wall 2 = " + wall02_left_down);
                    //Debug.Log("wall 2 = " + wall02_left_up);
                    //Debug.Log("wall 2 = " + wall02_right_up);
                    //Debug.Log("wall 2 = " + wall02_right_down);

                    toggle++;

                }
            }
            
            // adding ceiling
            int temp = polygons.Count * 4;
            int BARYCENTRE = arrayVertices3DBuilding.Length - 1;
            //
            for (int polygonLenght = 0; polygonLenght < polygons.Count - 1; polygonLenght++)
            {

                listTriangles.Add(temp);
                listTriangles.Add(BARYCENTRE);
                if (polygonLenght == polygons.Count - 1)
                {
                    listTriangles.Add(polygons.Count);
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
            listTriangles.Add(polygons.Count);

            //
            trianglesWallWC = listTriangles.ToArray();

            //Create UVs
            Vector2[] uvs = new Vector2[tmpListVertices.Count];

            // first wall
            wall01_left_down = 0;
            wall01_left_up = polygons.Count;
            wall01_right_up = wall01_left_up + 1;
            wall01_right_down = wall01_left_down + 1;

            // second wall
            wall02_left_down = offset;
            wall02_left_up = wall02_left_down + polygons.Count;
            wall02_right_up = wall02_left_up + 1;
            wall02_right_down = wall02_left_down + 1;

            // toggle
            toggle = 1; // modulo 2 produces '1' or '0'

            //Debug.Log("Polygons_Count = " + polygons.Count);

            for (int wall_nr = 0; wall_nr < polygons.Count; wall_nr++)
            {
                // first wall
                if ((toggle % 2) == 1)
                {

                    uvs[wall01_left_down] = new Vector2(0, 0); //bottom-left
                    Debug.Log("wall 1 = " + wall01_left_down);

                    uvs[wall01_left_up] = new Vector2(0, 1); //top-left
                    Debug.Log("wall 1 = " + wall01_left_up);

                    uvs[wall01_right_up] = new Vector2(1, 1); //top-right
                    Debug.Log("wall 1 = " + wall01_right_up);

                    uvs[wall01_right_down] = new Vector2(1, 0); //bottom-right
                    Debug.Log("wall 1 = " + wall01_right_down);


                    wall01_left_down = wall01_left_down + 2;
                    wall01_left_up = wall01_left_up + 2;
                    wall01_right_up = wall01_right_up + 2;
                    wall01_right_down = wall01_right_down + 2;

                    toggle++;
                    Debug.Log("ToggleVal = " + toggle);

                }
                else if ((toggle % 2) == 0)
                {
                    // second wall
                    uvs[wall02_left_down] = new Vector2(0, 0); //bottom-left
                    Debug.Log("wall 2 = " + wall02_left_down);

                    uvs[wall02_left_up] = new Vector2(0, 1); //top-left
                    Debug.Log("wall 2 = " + wall02_left_up);

                    uvs[wall02_right_up] = new Vector2(1, 1); //top-right
                    Debug.Log("wall 2 = " + wall02_right_up);

                    uvs[wall02_right_down] = new Vector2(1, 0); //bottom-right
                    Debug.Log("wall 2 = " + wall02_right_down);


                    wall02_left_down = wall02_left_down + 2;
                    wall02_left_up = wall02_left_up + 2;
                    wall02_right_up = wall02_right_up + 2;
                    wall02_right_down = wall02_right_down + 2;

                    //
                    toggle++;
                }

            }

            //// adding last triplet for closing wall
            //if (toggle % 2 == 1)
            //{

            //    //uvs[wall02_left_down - 1] = new Vector2(0, 0); //bottom-left
            //    //uvs[wall02_left_up - 1] = new Vector2(0, 1); //top-left
            //    //uvs[0] = new Vector2(1, 1); //top-right
            //    //uvs[polygons.Count] = new Vector2(1, 0); //bottom-right

            //    //// first triplet of wall
            //    //listTriangles.Add(wall01_left_down - 1);
            //    //listTriangles.Add(wall01_left_up - 1);
            //    //listTriangles.Add(polygons.Count);
            //    //listTriangles.Add(wall01_left_up - 1);
            //    //// second triplet of wall
            //    //listTriangles.Add(wall01_left_down - 1);
            //    //listTriangles.Add(polygons.Count);
            //    //listTriangles.Add(polygons.Count * 2);

            //}
            //else if (toggle % 2 == 0)
            //{
            //    ////    uvs[wall02_left_down - 1] = new Vector2(0, 0); //bottom-left
            //    ////    uvs[wall02_left_up - 1] = new Vector2(0, 1); //top-left
            //    //uvs[polygons.Count * 3] = new Vector2(1, 1); //top-right
            //    //uvs[polygons.Count * 2] = new Vector2(1, 0); //bottom-right
            //}

            Debug.Log("uvs = " + uvs.Length);

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

            msh.vertices = arrayVertices3DBuilding;
            msh.triangles = trianglesWallWC;

            msh.uv = uvs;

            msh.RecalculateNormals();
            msh.RecalculateBounds();

            return msh;
        }

    }

}