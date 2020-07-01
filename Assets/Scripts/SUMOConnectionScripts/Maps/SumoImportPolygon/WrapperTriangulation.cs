using Assets.Scripts.SUMOConnectionScripts.Maps.SumoImportPolygon;
using SumoImportPolygon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    /// <summary>
    /// Class with stativ method to convert 2D coordinates into Unity 3D
    /// coordinates and giving back a correspoding mesh.
    /// </summary>
public class WrapperTriangulation
{

    public static float textureSizeFactor = 10;

    /// <summary>
    /// Creating a Unity Mesh from given vertices with 2D coordintes
    /// </summary>
    /// <param name="vertices2D">List of 2D polygons</param>
    /// <returns>Unity mesh of given 2D vertices</returns>
    public static Mesh createMesh(Vector2[] vertices2D)
    {

        // Use the triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(vertices2D);
        int[] indices = tr.Triangulate();

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[vertices2D.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices2D[i].x, 0, vertices2D[i].y);
        }

        Vector2[] uvs = new Vector2[vertices.Length];

        int j = 0;
        foreach (Vector3 vert in vertices)
        {
            uvs[j] = new Vector2(vert.x / textureSizeFactor, vert.z / textureSizeFactor);
            j++;
        }

        // Create the mesh
        Mesh msh = new Mesh
        {
            vertices = vertices,
            triangles = indices,
            uv = uvs
        };
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        //
        return msh;
    }


    /// <summary>
    /// Creating a Unity Mesh from given vertices with 2D coordintes
    /// </summary>
    /// <param name="vertices2D">List of 2D polygons</param>
    /// <returns>Unity mesh of given 2D vertices</returns>
    public static Mesh createMeshNew(Vector2[] vertices2D)
    {

        // Create the Vector3 vertices
        List<Vector3> vertices = new List<Vector3>();
        for (int i = 0; i < vertices2D.Length; i++)
        {
            vertices.Add(new Vector3(vertices2D[i].x, 0, vertices2D[i].y));
        }

        Dictionary<Vertex, int> vertexDict = new Dictionary<Vertex, int>();

        List<Triangle> tris = Triangulator.TriangulateConcavePolygon(vertices);
        List<Vector3> vertPositions = new List<Vector3>();
        int[] triangleIndices = new int[3 * tris.Count];
        int index = 0;

        bool even = false;

        foreach (Triangle tri in tris)
        {
            foreach (Vertex vert in tri.verts)
            {
                if (vertexDict.ContainsKey(vert))
                {
                    triangleIndices[index] = vertexDict[vert];
                }
                else
                {
                    triangleIndices[index] = vertPositions.Count;
                    vertexDict.Add(vert, vertPositions.Count);
                    vertPositions.Add(vert.vector);
                    
                }
                index++;
            }

            float x1 = tri.verts[0].vector.x;
            float x2 = tri.verts[1].vector.x;
            float x3 = tri.verts[2].vector.x;

            float y1 = tri.verts[0].vector.z;
            float y2 = tri.verts[1].vector.z;
            float y3 = tri.verts[2].vector.z;
            
            bool rightHanded = (( (x2 - x1)*(y2 + y1) + (x3 - x2)*(y3 + y2) )> 0);

            if (even ^ rightHanded)
            {
                var temp = triangleIndices[index - 1];
                triangleIndices[index - 1] = triangleIndices[index - 3];
                triangleIndices[index - 3] = temp;
            }
            even = !even;
            
            if (tri.orientation == TriangleOrientation.Colinear)
            {
                Debug.Log("Colinear Triangle!");
            }
        }

        Debug.Log("Creating Polygon with "+tris.Count+" Triangles");
        Mesh msh = new Mesh
        {
            vertices = vertPositions.ToArray(),
            triangles = triangleIndices
        };
        


        msh.RecalculateNormals();
        msh.RecalculateBounds();


        //
        return msh;
    }

}