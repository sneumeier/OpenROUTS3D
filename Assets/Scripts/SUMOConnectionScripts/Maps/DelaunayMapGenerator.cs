using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using UnityEngine;

namespace Assets.Scripts.SUMOConnectionScripts.Maps
{
    public class DelaunayMapGenerator : MonoBehaviour,HeightTerrainGenerator
    {
        public static Stopwatch TotalTimeStopwatch = new Stopwatch();

        public void GenerateTerrain()
        {
            MapRasterizer rasterizer = MapRasterizer.Instance;

            float minX = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxZ = float.MinValue;

            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;

            List<Vertex> points = new List<Vertex>();
            foreach (MapPoint point in rasterizer.MapPoints)
            {
                points.Add(new Vertex(point.transform.position.x, point.transform.position.z, point.height));

                minX = Mathf.Min(minX, point.transform.position.x);
                minZ = Mathf.Min(minZ, point.transform.position.z);
                minHeight = Mathf.Min(minHeight, point.height);

                maxX = Mathf.Max(maxX, point.transform.position.x);
                maxZ = Mathf.Max(maxZ, point.transform.position.z);
                maxHeight = Mathf.Max(maxHeight, point.height);
            }
            Rectf rect = new Rectf(minX,minZ,maxX-minX,maxZ-minZ);
            

            Stopwatch sw2 = new Stopwatch();
            sw2.Start();
            Mesh tempMesh = TriangulateDelaunay(points, rect);
            this.GetComponent<MeshFilter>().mesh = tempMesh;
            this.GetComponent<MeshFilter>().sharedMesh = tempMesh;
            sw2.Stop();
            UnityEngine.Debug.Log("Triangulation process: " + sw2.Elapsed);
            
            this.transform.position = new Vector3(0,-0.05f,0);
            this.GetComponent<MeshCollider>().sharedMesh = tempMesh;

            /*
            UnityEngine.Debug.LogWarning("Debugging Mesh");
            foreach (Vector3 vec in tempMesh.vertices)
            {
                GameObject testGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                testGo.transform.position = vec;
                testGo.GetComponent<MeshRenderer>().material.color = Color.blue;
                UnityEngine.Debug.LogWarning("Placed Debug Cube");
            }
            */

            TotalTimeStopwatch.Stop();
            UnityEngine.Debug.Log("Total Load: " + TotalTimeStopwatch.Elapsed);
        }


        public Mesh TriangulateDelaunay(List<Vertex> points,Rectf rect)
        {

            Vector3[] vertices = new Vector3[points.Count];
            List<int> tris = new List<int>();
            Vector2[] uvs = new Vector2[points.Count];

            TriangleNet.Geometry.Polygon polygon = new Polygon(points.Count);
            foreach (Vertex point in points)
            {
                polygon.Add(point);
            }
            IMesh triangleNetMesh = polygon.Triangulate();

            foreach (Vertex point in triangleNetMesh.Vertices)
            {
                vertices[point.id] = new Vector3((float)point.x,point.height, (float)point.y);
                uvs[point.id] = new Vector2((float)point.x,(float)point.y);
            }

            foreach (var tri in triangleNetMesh.Triangles)
            {
                tris.Add(tri.vertices[1].id);
                tris.Add(tri.vertices[0].id);
                tris.Add(tri.vertices[2].id);
            }

            Mesh newmesh = new Mesh();
            newmesh.vertices = vertices.ToArray();
            newmesh.triangles = tris.ToArray();
            newmesh.uv = uvs;

            newmesh.RecalculateBounds();
            newmesh.RecalculateNormals();
            newmesh.RecalculateTangents();
                
            return newmesh;
        }


        public Vector3[,] Rasterize(Vector3 start, float distance, Vector3 end)
        {

            MapRasterizer rasterizer = MapRasterizer.Instance;
            int amountX = (int)((end.x - start.x)/distance);
            int amountZ = (int)((end.z - start.z) / distance);
            Vector3[,] rasterizedPoints = new Vector3[amountX,amountZ];
            for (int x = 0; x < amountX; x++)
            {
                for (int z = 0; z < amountZ; z++)
                {
                    Vector3 pos = new Vector3(start.x + x * distance, 0, start.z + z * distance);
                    float height = (rasterizer.GetHeightClosest(pos));
                    pos.y = height;
                    rasterizedPoints[x,z] = pos;
                }
            }
            return rasterizedPoints;
        }

    }
}
