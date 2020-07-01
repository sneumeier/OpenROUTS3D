using Assets.Scripts.AssetReplacement;
using SUMOConnectionScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.SUMOConnectionScripts
{
    public class IntersectionGenerator
    {
        public float uvFactor = 3.0f;
        public float lineWidth = 0.1f;
        public float lineOffset = 0.25f;
        public float markerWidth = 0.05f;
        public float markerHeight = 0.003f;

        public void Finalize(GameObject go, Junction junction, List<KeyValuePair<int,int>> streetEndingVertices)
        {
            Vector3 junctionCenter = Vector3.zero;
            foreach (Vector3 vert in junction.positions)
            {
                junctionCenter += vert;
            }
            junctionCenter /= junction.positions.Count;

            //Debug.Log("Street Ending vert size: "+streetEndingVertices.Count);
            

            List<List<Vector3>> markerLines = new List<List<Vector3>>();
            bool isFirst = true;
            List<Vector3> currentMarkerLine = null;
            bool firstIsFine = false;

            int id = 0;
            foreach (Vector3 junctionPoint in junction.positions)
            { 
                int lastID = (id + junction.positions.Count - 1) % junction.positions.Count;
                Vector3 lastJunctionPosition = junction.positions[lastID];
                int nextID = (id + 1) % junction.positions.Count;

                //If first, check if it's at the start of a street connection or not. If it's not, later connect the last line and the first line, since they belong together
                if (isFirst)
                {
                    bool lastOccupied = false;
                    foreach (var kvp in streetEndingVertices)
                    {
                        if (kvp.Key == lastID && kvp.Value == id)
                        {
                            lastOccupied = true;
                            break;
                        }
                    }
                    firstIsFine = lastOccupied;
                    if (!lastOccupied)
                    {
                        currentMarkerLine = new List<Vector3>();
                    }
                }
                isFirst = false;

                if(currentMarkerLine == null)
                {
                    
                    


                    //Check if there can even be a line created to the next junctionPoint
                    
                    bool isOccupied = false;
                    foreach (var kvp in streetEndingVertices)
                    {
                        if (kvp.Key == id && kvp.Value == nextID)
                        {
                            isOccupied = true;
                            break;
                        }
                    }
                    if (isOccupied)
                    {
                        id++;
                        //Debug.Log("Next id is occupied too");
                        continue;
                    }
                    currentMarkerLine = new List<Vector3>();//create new marker line

                        //create point exactly at junction point
                        //currentMarkerLine.Add(junctionPoint);
                        //Debug.Log("Creating anchor point for new line of " + junction.identifier);

                    
                    //Calculate inwards position
                    //perpendicular vector towards connection line between this and the last junction edge
                    Vector3 directional = lastJunctionPosition - junctionPoint;
                    Vector3 scaledDirectional = directional.normalized * lineOffset;
                    Vector3 perpA = -scaledDirectional;
                    Vector3 perpB = scaledDirectional;
                    //Debug.Log("Creating anchor point for new line of " + junction.identifier);
                    if (Vector3.Distance(junctionPoint + perpA, junctionCenter) < Vector3.Distance(junctionPoint + perpB, junctionCenter))
                    {
                        currentMarkerLine.Add(junctionPoint+perpA);
                    }
                    else {
                        currentMarkerLine.Add(junctionPoint + perpB);
                    }
                    
                }
                else {
                    //Check if current segement is part of street connection, if yes, finalize the line
                    bool thisOccupied = false;
                    foreach (var kvp in streetEndingVertices)
                    {
                        if (kvp.Key == id && kvp.Value == nextID)
                        {
                            thisOccupied = true;
                            break;
                        }
                    }
                    Vector3 nextJunctionPosition = junction.positions[nextID];
                    if (!thisOccupied) { 
                    //Calculate inwards position
                    //perpendicular vector towards junction edge
                    //two possibilities, the once closer to junction center is the correct one!
                    
                    Vector3 directional = lastJunctionPosition - nextJunctionPosition;
                    Vector3 scaledDirectional = directional.normalized * lineOffset;
                    Vector3 perpA = new Vector3(-scaledDirectional.z, 0, scaledDirectional.x);
                    Vector3 perpB = new Vector3(scaledDirectional.z, 0, -scaledDirectional.x);
                    //Debug.Log("Creating regular point for line of " + junction.identifier);
                    if (Vector3.Distance(junctionPoint + perpA, junctionCenter) < Vector3.Distance(junctionPoint + perpB, junctionCenter))
                    {
                        currentMarkerLine.Add(junctionPoint + perpA);
                    }
                    else
                    {
                        currentMarkerLine.Add(junctionPoint + perpB);
                    }
                    
                    }
                    else
                    {
                        
                        //Calculate inwards position
                        //perpendicular vector towards connection line between this and the next junction edge
                        
                        Vector3 directional = nextJunctionPosition - junctionPoint;
                        Vector3 scaledDirectional = directional.normalized * lineOffset;
                        Vector3 perpA = -scaledDirectional;
                        Vector3 perpB = scaledDirectional;
                        //Debug.Log("Creating last anchor point for line of " + junction.identifier);
                        if (Vector3.Distance(junctionPoint + perpA, junctionCenter) < Vector3.Distance(junctionPoint + perpB, junctionCenter))
                        {
                            currentMarkerLine.Add(junctionPoint + perpA);
                        }
                        else
                        {
                            currentMarkerLine.Add(junctionPoint + perpB);
                        }
                        
                        //currentMarkerLine.Add(junctionPoint);
                        markerLines.Add(currentMarkerLine);
                        currentMarkerLine = null;
                    }
                }
                id++;
            }
            if (currentMarkerLine != null)
            {
                markerLines.Add(currentMarkerLine);
            }
            if (!firstIsFine&&currentMarkerLine!=null&& markerLines.Count>1)
            {
                List<Vector3> first = markerLines[0];
                markerLines.Remove(first);
                List<Vector3> last = markerLines[markerLines.Count-1];
                foreach (Vector3 v3 in first)
                {
                    last.Add(v3);
                }
            }

            DrawLines(go,markerLines);
        }

        public void DrawMarker(GameObject origin, List<Vector3> points)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
            marker.transform.SetParent(origin.transform);
            marker.transform.localPosition = new Vector3(0,0,0);

            bool firstPair = true;
            List<Vector3> verts = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            int index = 0;
            int vertcount = 0;
            float distance = 0;
            while (index < points.Count)
            {

                KeyValuePair<Vector3, Vector3> edges = GetMeshEdges(markerWidth,index, points);
                verts.Add(new Vector3(edges.Key.x, markerHeight, edges.Key.z));
                vertcount++;
                verts.Add(new Vector3(edges.Value.x, markerHeight, edges.Value.z));
                vertcount++;

                if (!firstPair)
                {
                    tris.Add(vertcount - 4);
                    tris.Add(vertcount - 1);
                    tris.Add(vertcount - 3);

                    tris.Add(vertcount - 1);
                    tris.Add(vertcount - 4);
                    tris.Add(vertcount - 2);
                }

                uvs.Add(new Vector2(0, distance / markerWidth));
                uvs.Add(new Vector2(1, distance / markerWidth));
                firstPair = false;

                index++;
            }

            MeshFilter mf = marker.GetComponent<MeshFilter>();

            mf.mesh.vertices = verts.ToArray();
            mf.mesh.triangles = tris.ToArray();
            mf.mesh.uv = uvs.ToArray();

            mf.mesh.RecalculateBounds();
            mf.mesh.RecalculateNormals();
            mf.mesh.RecalculateTangents();

            marker.GetComponent<MeshCollider>().enabled = false;

            MeshRenderer mr = marker.GetComponent<MeshRenderer>();
            mr.material = PrefabInitializer.GetMaterial("MarkerMaterial");
        }

        public KeyValuePair<Vector3, Vector3> GetMeshEdges(float width, int index, List<Vector3> points)
        {
            
            if (index == 0)
            {
                Vector3 virtualTangentDirection = points[index]-points[index+1];
                Vector2 perpendicularStart = new Vector2(points[index].x, points[index].z) + (new Vector2(virtualTangentDirection.z, -virtualTangentDirection.x).normalized * width);
                Vector2 perpendicularEnd = new Vector2(points[index].x, points[index].z) + (new Vector2(-virtualTangentDirection.z, virtualTangentDirection.x).normalized * width);

                return new KeyValuePair<Vector3, Vector3>(new Vector3(perpendicularStart.x, 0, perpendicularStart.y), new Vector3(perpendicularEnd.x, points[index].y, perpendicularEnd.y));
            }
            else if (index == points.Count-1)
            {
                Vector3 virtualTangentDirection = points[index-1] - points[index];
                Vector2 perpendicularStart = new Vector2(points[index].x, points[index].z) + (new Vector2(virtualTangentDirection.z, -virtualTangentDirection.x).normalized * width);
                Vector2 perpendicularEnd = new Vector2(points[index].x, points[index].z) + (new Vector2(-virtualTangentDirection.z, virtualTangentDirection.x).normalized * width);

                return new KeyValuePair<Vector3, Vector3>(new Vector3(perpendicularStart.x, 0, perpendicularStart.y), new Vector3(perpendicularEnd.x, points[index].y, perpendicularEnd.y));
            }
            else
            {
                Vector3 virtualTangentDirection = points[index - 1] - points[index+1];
                Vector2 perpendicularStart = new Vector2(points[index].x, points[index].z) + (new Vector2(virtualTangentDirection.z, -virtualTangentDirection.x).normalized * width);
                Vector2 perpendicularEnd = new Vector2(points[index].x, points[index].z) + (new Vector2(-virtualTangentDirection.z, virtualTangentDirection.x).normalized * width);

                return new KeyValuePair<Vector3, Vector3>(new Vector3(perpendicularStart.x, 0, perpendicularStart.y), new Vector3(perpendicularEnd.x, points[index].y, perpendicularEnd.y));
            }
        }
        
        public void DrawLines(GameObject origin, List<List<Vector3>> lines)
        {
            foreach (List<Vector3> line in lines)
            {
                DrawMarker(origin,line);
            }

            /*
            GameObject markers = GameObject.CreatePrimitive(PrimitiveType.Quad);

            MeshFilter markerFilter = markers.GetComponent<MeshFilter>();
            Mesh markerMesh = markerFilter.mesh;

            List<int> tris = new List<int>();
            List<Vector3> verts = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
             * */
        }

        private void DrawPolyLine(List<Vector3> positions, Transform parent)
        {
            List<Vector3> adjustedPositions = new List<Vector3>();
            foreach (var v3 in positions)
            {
                adjustedPositions.Add(v3+Vector3.up*0.02f);
            }

            GameObject line = new GameObject();
            line.isStatic = true;
            line.transform.position = positions.First();
            line.AddComponent<LineRenderer>();
            line.transform.parent = parent;
            line.name = "line"+positions.Count;
            Color cl = Color.white;
            LineRenderer lr = line.GetComponent<LineRenderer>();
            lr.startColor = cl;
            lr.endColor = cl;
            lr.material.color = cl;
            lr.material.SetColor("_EmissionColor", cl);
            lr.material.EnableKeyword("_EMISSION");
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0.1f, 0.1f);
            lr.widthCurve = curve;
            lr.positionCount=positions.Count;
            lr.SetPositions(adjustedPositions.ToArray());

        }

    }
}
