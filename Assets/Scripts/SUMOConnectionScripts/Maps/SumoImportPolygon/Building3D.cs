using Assets.Scripts.AssetReplacement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Scripts.SUMOConnectionScripts.Maps.SumoImportPolygon
{
    public static class Building3D
    {
        public static int currentBuilding = 0;
        public static float heightPerLevel = 3.0f;
        public static float surroundFactor = 3.0f;
        public static float textureSizeFactor = 10;
        public static float roofTilingFactor = 0.2f;
        public static float pointedRoofOverlap = 0.5f;
        public static float windowFrontOffset = 0.1f;
        public static Material wallMaterial;
        public static Material flatRoofMaterial;
        public static Material pointedRoofMaterial;
        public static Material[] randomizableWallMaterials;
        public static Material[] randomizableRoofMaterials;
        public static GameObject windowPrefab;
        public static GameObject[] randomizableWindowPrefabs;

        public static float windowOffset = 1.5f;

        public static GameObject CreateBody(List<Vector3> listVertices3D, bool orientation, int levels, GameObject go, float height = 0)
        {
            ShuffleRoofMaterial();
            ShuffleWallMaterial();
            ShuffleWindow();

            GameObject basePlate;
            if (go == null)
            {
                basePlate = GameObject.CreatePrimitive(PrimitiveType.Quad);
            }
            else
            {
                basePlate = go;
            }

            MeshFilter baseMf = basePlate.GetComponent<MeshFilter>();
            basePlate.name = "Building_" + (currentBuilding);

            Vector3 center = Vector3.zero;
            foreach (Vector3 v3 in listVertices3D)
            {
                center += v3;
            }
            center /= listVertices3D.Count;
            center += new Vector3(0, height, 0);
            basePlate.transform.position = center;

            List<Vector2> fullRelativeEdges = new List<Vector2>();
            List<Vector2> addaptedRelativeEdges = new List<Vector2>();
            foreach (Vector3 v3 in listVertices3D)
            {
                //treshhold to detect a wrong genereated building (quick and dirty)
                if(!(v3.x - center.x>50|| v3.x - center.x < -50))
                {
                    addaptedRelativeEdges.Add(new Vector2(v3.x - center.x, v3.z - center.z));
                    fullRelativeEdges.Add(new Vector2(v3.x - center.x, v3.z - center.z));
                }
            }
            fullRelativeEdges.Add(fullRelativeEdges.First());

            List<Vector2> addaptedOuterEdges = new List<Vector2>();
            int id = 0;
            foreach (Vector2 v2 in addaptedRelativeEdges)
            {
                int prevID = (id - 1 + addaptedRelativeEdges.Count) % addaptedRelativeEdges.Count;
                int nextID = (id + 1) % addaptedRelativeEdges.Count;

                Vector2 directional = addaptedRelativeEdges[nextID] - addaptedRelativeEdges[prevID];

                Vector2 perpA = new Vector2(-directional.y, directional.x).normalized * pointedRoofOverlap;
                Vector2 perpB = new Vector2(directional.y, -directional.x).normalized * pointedRoofOverlap;

                if (Vector2.Distance(v2 + perpA, Vector2.zero) > Vector3.Distance(v2 + perpB, Vector2.zero))
                {
                    addaptedOuterEdges.Add(v2 + perpA);
                    //Debug.LogWarning("perpA = " + perpA.magnitude);
                }
                else
                {
                    addaptedOuterEdges.Add(v2 + perpB);
                    //Debug.LogWarning("perpB = " + perpB.magnitude);
                }

                id++;
            }
            List<Vector2> fullOuterEdges = addaptedOuterEdges.ToList();
            fullOuterEdges.Add(fullOuterEdges.First());

            baseMf.mesh = WrapperTriangulation.createMesh(addaptedRelativeEdges.ToArray());
            baseMf.mesh.RecalculateBounds();
            baseMf.mesh.RecalculateNormals();
            baseMf.mesh.RecalculateTangents();

            CreateWall(basePlate, fullRelativeEdges, levels);

            bool inside = IsInside(addaptedRelativeEdges, Vector2.zero);

            if (inside)
            {
                foreach (Vector2 v2 in addaptedRelativeEdges)
                {
                    Vector2 between = (v2) / 2;
                    if (!IsInside(addaptedRelativeEdges, between))
                    {
                        inside = false;
                        break;
                    }
                }
            }

            if (inside)
            {
                CreatePointedRoof(basePlate, fullOuterEdges, levels);
                CreateFlatRoof(basePlate, addaptedOuterEdges, levels, true);
            }
            else
            {
                CreateFlatRoof(basePlate, addaptedRelativeEdges, levels);
            }


            //Create Ceiling
            Assets.Scripts.Chunking.ChunkManager.AddRenderer(basePlate.GetComponent<MeshRenderer>(), basePlate.transform.position, false);

            currentBuilding++;
            return basePlate;
        }

        public static GameObject CreateFlatRoof(GameObject basePlate, List<Vector2> addaptedRelativeEdges, int levels, bool invertTris = false)
        {
            GameObject roofObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            MeshFilter roofFilter = roofObject.GetComponent<MeshFilter>();
            roofObject.name = "BuildingFlatRoof_" + (currentBuilding);

            roofFilter.mesh = WrapperTriangulation.createMesh(addaptedRelativeEdges.ToArray());
            Vector2[] uvs = new Vector2[roofFilter.mesh.vertices.Length];

            int i = 0;
            foreach (Vector3 vert in roofFilter.mesh.vertices)
            {
                uvs[i] = new Vector2(vert.x / textureSizeFactor, vert.z / textureSizeFactor);
                i++;
            }

            if (invertTris)
            {
                roofFilter.mesh.triangles = roofFilter.mesh.triangles.Reverse().ToArray();
            }

            roofFilter.mesh.uv = uvs;

            roofFilter.mesh.RecalculateBounds();
            roofFilter.mesh.RecalculateNormals();
            roofFilter.mesh.RecalculateTangents();


            roofObject.transform.SetParent(basePlate.transform);
            roofObject.transform.localPosition = new Vector3(0, levels * heightPerLevel, 0);

            MeshRenderer mr = roofObject.GetComponent<MeshRenderer>();
            mr.material = flatRoofMaterial;

            MeshCollider mc = roofObject.GetComponent<MeshCollider>();
            mc.sharedMesh = roofFilter.mesh;

            Assets.Scripts.Chunking.ChunkManager.AddRenderer(roofObject.GetComponent<MeshRenderer>(), roofObject.transform.position, false);

            return roofObject;
        }

        public static GameObject CreatePointedRoof(GameObject basePlate, List<Vector2> addaptedRelativeEdges, int levels)
        {
            GameObject roofObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            MeshFilter roofFilter = roofObject.GetComponent<MeshFilter>();
            roofObject.name = "BuildingRoof_" + (currentBuilding);

            Mesh tempmesh = roofFilter.mesh;

            Vector3 bary = Vector3.zero;
            List<Vector3> realVerts = new List<Vector3>();
            List<Vector2> realUVs = new List<Vector2>();
            List<int> tris = new List<int>();
            int current = 0;
            bool isFirst = true;
            foreach (Vector2 v2 in addaptedRelativeEdges)
            {

                if (!isFirst)
                {
                    bary += new Vector3(v2.x, heightPerLevel, v2.y);

                }
                isFirst = false;
                current++;
            }

            bary /= addaptedRelativeEdges.Count;
            isFirst = true;
            current = 0;
            int currentVertexID = 0;
            float currentDistance = 0;
            float previousDistance = 0;
            int lastVertexID = 0;

            foreach (Vector2 v2 in addaptedRelativeEdges)
            {
                if (!isFirst)
                {
                    currentDistance += Vector3.Distance(v2, addaptedRelativeEdges[current - 1]);
                }
                realVerts.Add(new Vector3(v2.x, 0, v2.y));
                realUVs.Add(new Vector2(currentDistance * roofTilingFactor, 0));
                int currentMainID = currentVertexID;
                currentVertexID++;


                if (!isFirst)
                {
                    float distanceBaryToCenter = Vector3.Distance(bary, (addaptedRelativeEdges[current - 1] + addaptedRelativeEdges[current]) / 2);
                    realVerts.Add(bary);
                    realUVs.Add(new Vector2(((currentDistance + previousDistance) / 2) * roofTilingFactor, distanceBaryToCenter * roofTilingFactor));
                    currentVertexID++;
                    tris.Add(currentVertexID - 1);
                    tris.Add(currentVertexID - 2);
                    tris.Add(lastVertexID);
                }
                lastVertexID = currentMainID;
                isFirst = false;
                previousDistance = currentDistance;
                current++;
            }


            tempmesh.vertices = realVerts.ToArray();
            tempmesh.triangles = tris.ToArray();
            tempmesh.uv = realUVs.ToArray();


            roofFilter.mesh = tempmesh;
            roofFilter.mesh.RecalculateBounds();
            roofFilter.mesh.RecalculateNormals();

            roofObject.transform.SetParent(basePlate.transform);
            roofObject.transform.localPosition = new Vector3(0, levels * heightPerLevel, 0);



            MeshRenderer mr = roofObject.GetComponent<MeshRenderer>();
            mr.material = pointedRoofMaterial;

            MeshCollider mc = roofObject.GetComponent<MeshCollider>();
            mc.sharedMesh = roofFilter.mesh;

            Assets.Scripts.Chunking.ChunkManager.AddRenderer(roofObject.GetComponent<MeshRenderer>(), roofObject.transform.position, false);

            return roofObject;
        }

        public static bool IsInside(List<Vector2> addaptedRelativeEdges, Vector2 point)
        {
            //Inspired by https://codereview.stackexchange.com/questions/108857/point-inside-polygon-check
            //Quote: "Removing all white space doesn't make code faster"
            int j = addaptedRelativeEdges.Count - 1;
            bool c = false;
            for (int i = 0; i < addaptedRelativeEdges.Count; j = i++) c ^= addaptedRelativeEdges[i].y > point.y ^ addaptedRelativeEdges[j].y > point.y && point.x < (addaptedRelativeEdges[j].x - addaptedRelativeEdges[i].x) * (point.y - addaptedRelativeEdges[i].y) / (addaptedRelativeEdges[j].y - addaptedRelativeEdges[i].y) + addaptedRelativeEdges[i].x;
            return c;
        }

        public static void ShuffleWallMaterial()
        {
            //Custom materials
            wallMaterial = WeightedRandoms.Shuffle<Material>(PrefabProvider.customFassades);

            if (wallMaterial == null)
            {
                //Default materials
                wallMaterial = randomizableWallMaterials[WeightedRandoms.Rand.Next(0, randomizableWallMaterials.Count())];
            }
        }

        public static void ShuffleWindow()
        {
            windowPrefab = randomizableWindowPrefabs[WeightedRandoms.Rand.Next(0, randomizableWindowPrefabs.Count())];
        }

        public static void ShuffleRoofMaterial()
        {
            pointedRoofMaterial = randomizableRoofMaterials[WeightedRandoms.Rand.Next(0, randomizableRoofMaterials.Count())];
        }

        public static GameObject CreateWall(GameObject basePlate, List<Vector2> addaptedRelativeEdges, int levels)
        {
            GameObject wallObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            MeshFilter wallFilter = wallObject.GetComponent<MeshFilter>();
            wallObject.name = "BuildingWall_" + (currentBuilding);

            wallObject.transform.SetParent(basePlate.transform);
            wallObject.transform.localPosition = new Vector3(0, 0, 0);

            List<Vector3> totalVerts = new List<Vector3>();
            List<Vector2> totalUVs = new List<Vector2>();
            List<int> totalTris = new List<int>();
            int vertsPerLevel = addaptedRelativeEdges.Count;

            float surroundDistance = 0f;
            Vector2 last = Vector2.zero;
            bool first = true;
            foreach (Vector2 v2 in addaptedRelativeEdges)
            {
                if (!first)
                {
                    surroundDistance += Vector2.Distance(v2, last);
                }
                first = false;
                last = v2;
            }
            int plannedSurrounds = Mathf.RoundToInt(surroundDistance / surroundFactor);
            float unitToUvFactor = plannedSurrounds / surroundDistance;

            for (int lvl = 0; lvl < levels; lvl++)
            {
                int run = 0;
                float currentSurround = 0;
                if (lvl == 0)
                {

                    foreach (Vector2 flatVector in addaptedRelativeEdges)
                    {
                        totalVerts.Add(new Vector3(flatVector.x, (lvl) * heightPerLevel, flatVector.y));
                        float distance = Vector2.Distance(addaptedRelativeEdges[run], addaptedRelativeEdges[(run + 1) % addaptedRelativeEdges.Count]);
                        totalUVs.Add(new Vector2(currentSurround, lvl * heightPerLevel * unitToUvFactor));
                        currentSurround += distance * unitToUvFactor;

                        run++;
                    }
                }
                run = 0;
                currentSurround = 0;

                foreach (Vector2 flatVector in addaptedRelativeEdges)
                {
                    Vector3 newvert = new Vector3(flatVector.x, (lvl + 1) * heightPerLevel, flatVector.y);
                    totalVerts.Add(newvert);

                    float distance = Vector2.Distance(addaptedRelativeEdges[run], addaptedRelativeEdges[(run + 1) % addaptedRelativeEdges.Count]);
                    totalUVs.Add(new Vector2(currentSurround, (lvl + 1) * heightPerLevel * unitToUvFactor));
                    currentSurround += distance * unitToUvFactor;

                    if (run > 0)
                    {
                        totalTris.Add((vertsPerLevel * lvl) + run);         // :. <-lower Right
                        totalTris.Add((vertsPerLevel * lvl) + run - 1);           // :. <-lower Left
                        totalTris.Add((vertsPerLevel * (lvl + 1)) + run - 1);   // :. <-upper Left

                        totalTris.Add((vertsPerLevel * lvl) + run);             // ': <-lower Right
                        totalTris.Add((vertsPerLevel * (lvl + 1)) + run - 1);   // ': <-upper Left
                        totalTris.Add((vertsPerLevel * (lvl + 1)) + run);       // ': <-upper Right

                    }

                    //Place Windows:
                    int windowcount = (int)(distance / windowOffset);
                    float actualWindowDistance = distance / windowcount;

                    for (int windowIndex = 0; windowIndex < windowcount - 1; windowIndex++)
                    {
                        Vector2 windowPosition2d = (windowIndex + 1) * actualWindowDistance * (addaptedRelativeEdges[(run + 1 + addaptedRelativeEdges.Count) % addaptedRelativeEdges.Count] - addaptedRelativeEdges[run]).normalized;
                        Vector3 windowPosition = new Vector3(windowPosition2d.x, heightPerLevel * (-0.5f), windowPosition2d.y) + newvert;

                        Vector3 directional = new Vector3(windowPosition2d.x, 0, windowPosition2d.y);


                        Vector3 perpA = new Vector3(-directional.z, 0, directional.x).normalized * windowFrontOffset;
                        Vector3 perpB = new Vector3(directional.z, 0, -directional.x).normalized * windowFrontOffset;

                        GameObject window = GameObject.Instantiate(windowPrefab);
                        window.transform.SetParent(wallObject.transform);

                        Vector3 offsetter = Vector3.zero;

                        if (Vector2.Distance(windowPosition + perpA, Vector2.zero) > Vector3.Distance(windowPosition + perpB, Vector2.zero))
                        {
                            window.transform.localPosition = windowPosition + perpA;
                            offsetter = perpA;
                        }
                        else
                        {
                            window.transform.localPosition = windowPosition + perpB;
                            offsetter = perpB;
                        }
                        window.transform.localRotation = Quaternion.Euler(new Vector3(0, Mathf.Atan2(offsetter.x, offsetter.z) * Mathf.Rad2Deg + 180, 0));


                        Chunking.ChunkManager.AddRenderer(window.GetComponent<MeshRenderer>(), window.transform.position);
                    }



                    run++;
                }
                /*
                //Set last tri that connects last wall with first wall
                totalTris.Add((vertsPerLevel * lvl));         // :. <-lower Right
                totalTris.Add((vertsPerLevel * (lvl+1))- 1);           // :. <-lower Left
                totalTris.Add((vertsPerLevel * (lvl + 2)) - 1);   // :. <-upper Left

                totalTris.Add((vertsPerLevel * lvl));             // ': <-lower Right
                totalTris.Add((vertsPerLevel * (lvl + 2)) - 1);   // ': <-upper Left
                totalTris.Add((vertsPerLevel * (lvl + 1)));       // ': <-upper Right
                 * */

            }

            wallFilter.mesh.vertices = totalVerts.ToArray();
            wallFilter.mesh.triangles = totalTris.ToArray();
            wallFilter.mesh.uv = totalUVs.ToArray();

            wallFilter.mesh.RecalculateNormals();
            wallFilter.mesh.RecalculateBounds();
            wallFilter.mesh.RecalculateTangents();

            MeshRenderer mr = wallObject.GetComponent<MeshRenderer>();
            mr.material = wallMaterial;

            MeshCollider mc = wallObject.GetComponent<MeshCollider>();
            mc.sharedMesh = wallFilter.mesh;

            Assets.Scripts.Chunking.ChunkManager.AddRenderer(mr, wallObject.transform.position, false);

            return wallObject;
        }

    }
}
