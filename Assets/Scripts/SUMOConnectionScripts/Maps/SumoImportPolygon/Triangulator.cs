using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.SUMOConnectionScripts.Maps.SumoImportPolygon;

    /// <summary>
    /// source code of this class taken from:
    ///  <see cref="http://wiki.unity3d.com/index.php?title=Triangulator#C.23-_Triangulator.cs">
    /// </summary>
public class Triangulator
{
    private List<Vector2> m_points = new List<Vector2>();

    public Triangulator(Vector2[] points)
    {
        m_points = new List<Vector2>(points);
    }

    public int[] Triangulate()
    {
        List<int> indices = new List<int>();

        int n = m_points.Count;
        if (n < 3)
            return indices.ToArray();

        int[] V = new int[n];
        if (Area() > 0)
        {
            for (int v = 0; v < n; v++)
                V[v] = v;
        }
        else
        {
            for (int v = 0; v < n; v++)
                V[v] = (n - 1) - v;
        }

        int nv = n;
        int count = 2 * nv;
        for (int m = 0, v = nv - 1; nv > 2;)
        {
            if ((count--) <= 0)
                return indices.ToArray();

            int u = v;
            if (nv <= u)
                u = 0;
            v = u + 1;
            if (nv <= v)
                v = 0;
            int w = v + 1;
            if (nv <= w)
                w = 0;

            if (Snip(u, v, w, nv, V))
            {
                int a, b, c, s, t;
                a = V[u];
                b = V[v];
                c = V[w];
                indices.Add(a);
                indices.Add(b);
                indices.Add(c);
                m++;
                for (s = v, t = v + 1; t < nv; s++, t++)
                    V[s] = V[t];
                nv--;
                count = 2 * nv;
            }
        }

        indices.Reverse();
        return indices.ToArray();
    }

    private float Area()
    {
        int n = m_points.Count;
        float A = 0.0f;
        for (int p = n - 1, q = 0; q < n; p = q++)
        {
            Vector2 pval = m_points[p];
            Vector2 qval = m_points[q];
            A += pval.x * qval.y - qval.x * pval.y;
        }
        return (A * 0.5f);
    }

    private bool Snip(int u, int v, int w, int n, int[] V)
    {
        int p;
        Vector2 A = m_points[V[u]];
        Vector2 B = m_points[V[v]];
        Vector2 C = m_points[V[w]];
        if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
            return false;
        for (p = 0; p < n; p++)
        {
            if ((p == u) || (p == v) || (p == w))
                continue;
            Vector2 P = m_points[V[p]];
            if (InsideTriangle(A, B, C, P))
                return false;
        }
        return true;
    }

    private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
        float cCROSSap, bCROSScp, aCROSSbp;

        ax = C.x - B.x; ay = C.y - B.y;
        bx = A.x - C.x; by = A.y - C.y;
        cx = B.x - A.x; cy = B.y - A.y;
        apx = P.x - A.x; apy = P.y - A.y;
        bpx = P.x - B.x; bpy = P.y - B.y;
        cpx = P.x - C.x; cpy = P.y - C.y;

        aCROSSbp = ax * bpy - ay * bpx;
        cCROSSap = cx * apy - cy * apx;
        bCROSScp = bx * cpy - by * cpx;

        return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
    }


    public static List<Triangle> TriangulateConcavePolygon(List<Vector3> points)
    {
        //The list with triangles the method returns
        List<Triangle> triangles = new List<Triangle>();

        //If we just have three points, then we dont have to do all calculations
        if (points.Count == 3)
        {
            triangles.Add(new Triangle(points[0], points[1], points[2]));

            return triangles;
        }



        //Step 1. Store the vertices in a list and we also need to know the next and prev vertex
        List<Vertex> vertices = new List<Vertex>();

        for (int i = 0; i < points.Count; i++)
        {
            vertices.Add(new Vertex(points[i]));
        }

        //Find the next and previous vertex
        for (int i = 0; i < vertices.Count; i++)
        {
            int nextPos = ((i + 1) % vertices.Count);

            int prevPos = ((i - 1 + vertices.Count) % vertices.Count);

            vertices[i].prev = vertices[prevPos];

            vertices[i].next = vertices[nextPos];
        }



        //Step 2. Find the reflex (concave) and convex vertices, and ear vertices
        for (int i = 0; i < vertices.Count; i++)
        {
            CheckIfReflexOrConvex(vertices[i]);
        }

        //Have to find the ears after we have found if the vertex is reflex or convex
        List<Vertex> earVertices = new List<Vertex>();

        for (int i = 0; i < vertices.Count; i++)
        {
            IsVertexEar(vertices[i], vertices, earVertices);
        }



        //Step 3. Triangulate!
        while (true)
        {
            //This means we have just one triangle left
            if (vertices.Count == 3)
            {
                //The final triangle
                triangles.Add(new Triangle(vertices[0], vertices[1], vertices[2]));

                break;
            }

            //Make a triangle of the first ear
            Vertex earVertex = earVertices[0];

            Vertex earVertexPrev = earVertex.prev;
            Vertex earVertexNext = earVertex.next;

            Triangle newTriangle = new Triangle(earVertex, earVertexPrev, earVertexNext);

            triangles.Add(newTriangle);

            //Remove the vertex from the lists
            earVertices.Remove(earVertex);

            vertices.Remove(earVertex);

            //Update the previous vertex and next vertex
            earVertexPrev.prev = earVertexNext;
            earVertexNext.next = earVertexPrev;

            //...see if we have found a new ear by investigating the two vertices that was part of the ear
            CheckIfReflexOrConvex(earVertexPrev);
            CheckIfReflexOrConvex(earVertexNext);

            earVertices.Remove(earVertexPrev);
            earVertices.Remove(earVertexNext);

            IsVertexEar(earVertexPrev, vertices, earVertices);
            IsVertexEar(earVertexNext, vertices, earVertices);
        }

        //Debug.Log(triangles.Count);

        return triangles;
    }



    //Check if a vertex if reflex or convex, and add to appropriate list
    private static void CheckIfReflexOrConvex(Vertex v)
    {
        v.isReflex = false;
        v.isConvex = false;

      

        if (new Triangle(v.prev,v,v.next).orientation == TriangleOrientation.Clockwise)
        {
            v.isReflex = true;
        }
        else
        {
            v.isConvex = true;
        }
    }



    //Check if a vertex is an ear
    private static void IsVertexEar(Vertex v, List<Vertex> vertices, List<Vertex> earVertices)
    {
        //A reflex vertex cant be an ear!
        if (v.isReflex)
        {
            return;
        }

        //This triangle to check point in triangle
        Vector2 a = v.prev.GetPos2D_XZ();
        Vector2 b = v.GetPos2D_XZ();
        Vector2 c = v.next.GetPos2D_XZ();

        bool hasPointInside = false;

        for (int i = 0; i < vertices.Count; i++)
        {
            //We only need to check if a reflex vertex is inside of the triangle
            if (vertices[i].isReflex)
            {
                Vector2 p = vertices[i].GetPos2D_XZ();

                //This means inside and not on the hull
                if (IsPointInTriangle(a, b, c, p))
                {
                    hasPointInside = true;

                    break;
                }
            }
        }

        if (!hasPointInside)
        {
            earVertices.Add(v);
        }
    }

    public static bool IsPointInTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
    {
        bool b1, b2, b3;

        b1 = sign(p, a, b) < 0.0f;
        b2 = sign(p, b, c) < 0.0f;
        b3 = sign(p, c, a) < 0.0f;

        return ((b1 == b2) && (b2 == b3));
    }

    public static float sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

}