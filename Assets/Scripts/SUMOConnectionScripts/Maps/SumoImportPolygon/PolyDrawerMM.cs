using SumoImportPolygon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using SUMOConnectionScripts;
using Assets.SUMOConnectionScripts;
using Assets.Scripts.SUMOConnectionScripts.Maps.SumoImportPolygon;
using Assets.Scripts.OsmParser;
using Assets.Scripts.AssetReplacement;
using Assets.Scripts.SUMOConnectionScripts.Maps;

namespace SumoImportPolygon
{

    /// <summary>
    /// This class is responsibel to draw all given polygons into a Unity scene.
    /// </summary>
    public class PolyDrawerMM
    {
        //variable is used to avoid trees on street
        private const float MIN_TREE_DISTANCE = 4;
        private const float TERRAIN_HEIGHT_OFFSET = 0.15f;
        private const float BUILDING_HEIGHT_OFFSET = 0.20f;
        private const float BUILDING_SCALE_FACTOR = 0.6f;
        // Unity Plane with set material ('no colours')
        private Plane prefab;
        // Tree Object to place in scene
        private GameObject treeObject;

        // Temporary mesh for instantiatin from polygons
        private Mesh tempMesh;

        public Material cityMaterial;
        public Material grassMaterial;

        private SumoProcessorMM sp;
        private SumoStreetImporter importer;

        public PolyDrawerMM(SUMOConnectionScripts.SumoStreetImporter sumoStreetImporter, GameObject treeObject)
        {
            this.importer = sumoStreetImporter;
            this.treeObject = treeObject;
        }

        /// <summary>
        /// This method reads all given polygons, and decides, if it is a 2D
        /// ground area, or a 3D object (building).
        /// </summary>
        /// <param name="polygons">List of 2D polygons</param>
        /// <param name="sp">SumoProcessorMM with GameObject</param>
        public void DrawAllPolygons(List<PolygonMM> polygons, SumoProcessorMM sp, OsmParser parser)
        {

            // Sumo processor
            this.sp = sp;

            Vector2[] vertices2D;

            // seed for ramdom building heights
            UnityEngine.Random.InitState(123456789);

            GameObject go;
            MeshFilter mf;
            Renderer renderer;

            go = GameObject.Instantiate(this.sp.prefabPlane) as GameObject;
            mf = (MeshFilter)go.GetComponent(typeof(MeshFilter));
            this.tempMesh = WrapperTriangulation.createMesh(this.sp.GetSceneAreaDimension().CreateQuad());

            /*
             * to avoid a rigid body can go through a generated plane or building
             * 
             * added mesh collider according: https://www.youtube.com/watch?v=Q3JFtUEUM2M
             * see code at 8:30min
             */
            (go.AddComponent(typeof(MeshCollider)) as MeshCollider).sharedMesh = this.tempMesh;

            mf.mesh = this.CreateBaseTerrain(this.tempMesh);


            /*
             * Generating meshes for areas, buildings and tree areas.
             */
            foreach (PolygonMM polygon in polygons)
            {
                vertices2D = polygon.ListPolygonPoints.GetRange(0, polygon.ListPolygonPoints.Count - 1).ToArray(); // <-- last index position/ccordinate is identical to first index/coordinate and therefore Unity doesn't show polygon at all

                string customTextureName = null;
                OsmWay way = null;

                if (parser != null)
                {
                    way = parser.ways[polygon.osmIdentifier];
                    if (way.other_tags.ContainsKey("natural"))
                    {
                        customTextureName = "OSM_" + way.other_tags["natural"];
                        Debug.Log("Custom Surface name: " + customTextureName);
                    }

                }

                /*
                 * idea from:
                 * https://stackoverflow.com/a/26752701
                 * also documented here:
                 * https://stackoverflow.com/a/19383955
                 */
                Vector3 positionPlane = new Vector3(0, 0, 0);
                Quaternion rotationPlane = new Quaternion(0, 0, 0, 0);

                go = GameObject.Instantiate(this.sp.prefabPlane, positionPlane, rotationPlane) as GameObject;
                go.isStatic = true;
                mf = (MeshFilter)go.GetComponent(typeof(MeshFilter));

                if (!polygon.Type.Contains("building"))
                {

                    /*
                     * to avoid a rigid body can go through a generated plane or building
                     * 
                     * added mesh collider according: https://www.youtube.com/watch?v=Q3JFtUEUM2M
                     * see code at 8:30min
                     */
                    (go.AddComponent(typeof(MeshCollider)) as MeshCollider).sharedMesh = this.tempMesh;

                    /*
                     * following for old 2D coordinates
                     */
                    // Create the mesh
                    if (parser != null)
                    {
                        if (way != null)
                        {
                            float avrg = 0;
                            for (int index = 0; index < way.includedNodes.Count; index++)
                            {
                                if (vertices2D.Length <= index)
                                {
                                    break;
                                }
                                float standardHeight = (float)way.includedNodes[index].height;
                                MapPoint mp = MapRasterizer.Instance.AddMapPoint(new Vector3(vertices2D[index].x, 0, vertices2D[index].y), standardHeight - TERRAIN_HEIGHT_OFFSET);
                                if (MapRasterizer.Instance.IsEditor)
                                {
                                    Vector3 pos = new Vector3(vertices2D[index].x, standardHeight, vertices2D[index].y);
                                    pos.y = standardHeight;
                                    var mplist = new List<MapPoint>();
                                    mplist.Add(mp);
                                    List<string> idlist = new List<string>();
                                    List<string> fullidlist = new List<string>();
                                    idlist.Add(way.includedNodes[index].id);
                                    fullidlist.Add(way.id + "-" + way.includedNodes[index].id);
                                    MapRasterizer.Instance.EditorControl.AddPoint(pos, Assets.Scripts.EditorScene.EditorPointType.TerrainPoint, idlist, fullidlist, mplist);
                                }
                            }



                        }
                        else
                        {
                            Debug.Log("No OSM Way available");
                        }
                    }
                    else
                    {
                        this.tempMesh = WrapperTriangulation.createMesh(vertices2D);


                        this.tempMesh = this.recalculateHeights(this.tempMesh, polygon.Type);
                    }

                    /*
                     * Adding plane with given polygon to scene 
                     */
                    bool customTextureFound = false;
                    if (customTextureName != null)
                    {
                        Material mat = PrefabInitializer.GetMaterial(customTextureName);
                        if (mat != null)
                        {
                            go.GetComponent<MeshRenderer>().material = mat;
                            Debug.Log("Found Custom Texture: " + customTextureName);
                            customTextureFound = true;
                        }

                    }
                    if (!customTextureFound)
                    {
                        go.GetComponent<MeshRenderer>().material = cityMaterial;
                        go.GetComponent<MeshRenderer>().material.color = polygon.Color * (1 / 255.0f);
                    }

                    go.transform.position += new Vector3(0, UnityEngine.Random.Range(0, 0.01f), 0);

                }


                /*
                 * securing that only buildings are rendered with building model (and no forests etc.)
                 */
                if (polygon.Type.Contains("building"))
                {

                    /*
                     * following the new 3D coordinates for three-dimensional bodies
                     */
                    List<Vector3> templistVertices3D = new List<Vector3>();


                    //Scaling the plane around the avrg point

                    foreach (Vector2 v2 in vertices2D)
                    {
                        templistVertices3D.Add(new Vector3(v2.x, 0, v2.y));
                    }
                    List<Vector3> listVertices3D = new List<Vector3>();
                    listVertices3D = ScaleVector3Plane(templistVertices3D, BUILDING_SCALE_FACTOR);



                    //Debug.Log("polgyon: " + (counter));

                    // preparation for area calculation with all necessary data
                    List<Vector3> tempV3 = new List<Vector3>();
                    tempV3.AddRange(listVertices3D);
                    tempV3.AddRange(listVertices3D.GetRange(0, 1));

                    bool orientation = AreaCalculations.CalculatePolygonOrientation3D(tempV3);

                    //reversing order of polygon coordinates(SUMO delivers data in the wrong direction for vertices and trinangles
                    if (!orientation)
                    {
                        //Debug.Log("reversed");
                        listVertices3D.Reverse();
                    }

                    // setting shader and colour for generated buildings
                    Material material = new Material(Shader.Find("Vertex Colored"))
                    {
                        name = "Automatic Material",
                        color = new Color32(255, 142, 142, 128)
                    };
                    renderer = go.GetComponent<Renderer>();
                    renderer.material = material;

                    /*
                     * to avoid a rigid body can go through a generated plane or building
                     * 
                     * added mesh collider according: https://www.youtube.com/watch?v=Q3JFtUEUM2M
                     * see code at 8:30min
                     */
                    //this.tempMesh = Body3D.CreateBody(listVertices3D, orientation);

                    //Get height
                    float height = 0;
                    if (parser != null)
                    {
                        if (way != null)
                        {
                            float avrg = 0;
                            foreach (var node in way.includedNodes)
                            {
                                avrg += (float)node.height;
                            }
                            height = avrg / way.includedNodes.Count;
                            height -= BUILDING_HEIGHT_OFFSET;

                            List<MapPoint> mapPointList = new List<MapPoint>();


                            Vector3 averagePoint = Vector3.zero;

                            foreach (Vector3 v3 in listVertices3D)
                            {
                                MapPoint mp = MapRasterizer.Instance.AddMapPoint(new Vector3(v3.x, 0, v3.z), height);
                                mapPointList.Add(mp);
                                averagePoint += v3;
                            }
                            averagePoint /= listVertices3D.Count;

                            averagePoint.y = height;
                            if (MapRasterizer.Instance.IsEditor)
                            {
                                List<string> idlist = parser.ways[polygon.osmIdentifier].includedIds;
                                List<string> fullidlist = new List<string>();
                                foreach (string nodeid in idlist)
                                {
                                    fullidlist.Add(parser.ways[polygon.osmIdentifier].id + "-" + nodeid);
                                }
                                MapRasterizer.Instance.EditorControl.AddPoint(averagePoint, Assets.Scripts.EditorScene.EditorPointType.BuildingPoint, idlist, fullidlist, mapPointList);
                            }
                        }
                        else
                        {
                            Debug.Log("No OSM Way available");
                        }
                    }

                    GameObject building = Building3D.CreateBody(listVertices3D, orientation, (int)((WeightedRandoms.Rand.NextDouble() * 2.3) + 1), go, height);
                    this.tempMesh = building.GetComponent<MeshFilter>().mesh;

                    //(go.AddComponent(typeof(MeshCollider)) as MeshCollider).sharedMesh = this.tempMesh;


                    //this.tempMesh = Body3Dv3.CreateBody(listVertices3D, orientation);

                }
                /* 
                 * add trees for map
                 */
                if (polygon.Type.Contains("forest") || polygon.Type.Equals("natural.wood"))
                {
                    List<Vector2> polygonPointsList = polygon.GetListPolygonPoints();

                    PlaceTreesOnPolygonPoints(polygonPointsList, treeObject);
                    PlaceTreesOnRandomPointsInPolygon(polygonPointsList, treeObject);

                    go.GetComponent<MeshRenderer>().material = grassMaterial;
                }

                // adding generated mesh to scene
                mf.mesh = this.tempMesh;

            }

        }
        /*
         * check if point is in the area of polygon
         */

        private List<Vector3> ScaleVector3Plane(List<Vector3> list, float scaleValue)
        {
            List<Vector3> tempList = new List<Vector3>();

            Vector3 geometricalCenter = new Vector3(0, 0, 0);
            Vector3 tempVector3;

            //calculate the geometrical center

            foreach (Vector3 v3 in list)
            {
                geometricalCenter += v3;
            }
            geometricalCenter /= list.Count;

            // move the gemetrical center to the origin an scale the polygon, afterwards move it back

            foreach (Vector3 v3 in list)
            {
                tempVector3 = ((v3 - geometricalCenter) * scaleValue) + geometricalCenter;
                tempList.Add(tempVector3);
            }
            Debug.Log("ScaleVector3Plane worked");
            return tempList;


        }
        public bool ContainsPoint(Vector2[] polygon, Vector2 p)
        {
            int j = polygon.Length - 1;
            bool inside = false;
            for (int i = 0; i < polygon.Length; j = i++)
            {
                if (((polygon[i].y <= p.y && p.y < polygon[j].y) || (polygon[j].y <= p.y && p.y < polygon[i].y)) &&
                   (p.x < (polygon[j].x - polygon[i].x) * (p.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
                    inside = !inside;
            }
            return inside;

        }
        private void PlaceTreesOnPolygonPoints(List<Vector2> polygonPointsList, UnityEngine.Object treeObject)
        {
            // place trees on polygonPoints     
            foreach (Vector2 vector in polygonPointsList)
            {
                Vector3 v3 = new Vector3(vector.x, 0, vector.y);
                float dist = importer.DistanceToStreet(vector);
                if (dist > MIN_TREE_DISTANCE)
                {
                    // Create a new GameObject tree and place tree
                    GameObject tree = (GameObject)GameObject.Instantiate(treeObject, v3, Quaternion.identity); 

                    tree.AddComponent<BoxCollider>(); // Add BoxCollider to the GameObject tree
                    BoxCollider bc = tree.GetComponent<BoxCollider>(); // Get the BoxColliderComponent from GameObject tree
                    bc.center = new Vector3(0, 2.5f, 0); // Set a center to the BoxCollider Component
                    bc.size = new Vector3(0.5f, 5, 0.5f); // Set a size to the BoxCollider Component

                    Assets.Scripts.Chunking.ChunkManager.AddRenderer(tree.GetComponent<MeshRenderer>(), tree.transform.position, false);
                }
            }

        }
        private void PlaceTreesOnRandomPointsInPolygon(List<Vector2> polygonPointsList, UnityEngine.Object treeObject)
        {
            int numberOfRandomTreePositions;
            int treeNumberScaleFactor = 500;
            // values for random range
            float maxX, minX, maxY, minY;
            //intialize with values of first vector in polygonPointsList
            maxX = polygonPointsList.First().x;
            minX = polygonPointsList.First().x;
            maxY = polygonPointsList.First().y;
            minY = polygonPointsList.First().y;
            //get min/max values from the polygonPointsList
            foreach (Vector2 vector in polygonPointsList)
            {
                if (minX > vector.x)
                {
                    minX = vector.x;
                }
                if (maxX < vector.x)
                {
                    maxX = vector.x;
                }
                if (minY > vector.y)
                {
                    minY = vector.y;
                }
                if (maxY < vector.y)
                {
                    maxY = vector.y;
                }
            }
            numberOfRandomTreePositions = (int)AreaCalculations.CalculatePolygonArea2D(polygonPointsList) / treeNumberScaleFactor;
            Vector2[] polygonPointsarray = polygonPointsList.ToArray(); // ContainsPoint needs an Array
            while (numberOfRandomTreePositions != 0)
            {
                Vector2 point = new Vector2(UnityEngine.Random.Range(minX, maxX), UnityEngine.Random.Range(minY, maxY));
                if (ContainsPoint(polygonPointsarray, point))
                {
                    Vector3 v3 = new Vector3(point.x, 0, point.y);
                    float dist = importer.DistanceToStreet(point);
                    //check if distance is big enough between street and tree
                    if (dist > MIN_TREE_DISTANCE)
                    {
                        // Create a new GameObject tree and place tree
                        GameObject tree = (GameObject)GameObject.Instantiate(treeObject, v3, Quaternion.identity); 

                        tree.AddComponent<BoxCollider>(); // Add BoxCollider to the GameObject tree
                        BoxCollider bc = tree.GetComponent<BoxCollider>(); // Get the BoxColliderComponent from GameObject tree
                        bc.center = new Vector3(0, 2.5f, 0); // Set a center to the BoxCollider Component
                        bc.size = new Vector3(0.5f, 5, 0.5f); // Set a size to the BoxCollider Component

                        Assets.Scripts.Chunking.ChunkManager.AddRenderer(tree.GetComponent<MeshRenderer>(), tree.transform.position, false);
                        numberOfRandomTreePositions--;
                    }
                    //is used for debugging
                    //places red cubes where distance between tree and street was too small
                    else if (StreetNode.DO_DEBUG)
                    {
                        GameObject debugcube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        debugcube.GetComponent<MeshRenderer>().material.color = Color.red;
                        debugcube.name = "Invalid Tree Location";
                        debugcube.transform.position = v3;
                    }

                }
            }


        }

        /// <summary>
        /// This method recalculates the given meshes, because they have to be
        /// given a certain height according type parameter.
        /// Otherwise, without giving a mesh a new heigth, it causes clipping
        /// errors/flickering with created and coloured ares at runtime, when
        /// Unity is rendering the scene.
        /// Therefore all area meshes have to be given a different height, to
        /// avoid this behaviour.
        /// </summary>
        /// <param name="tempMesh">Mesh of area</param>
        /// <param name="type">Type of area (forest, water etc.)</param>
        /// <returns>a modified Mesh</returns>
        private Mesh recalculateHeights(Mesh tempMesh, string type)
        {
            Vector3[] vertices = tempMesh.vertices;

            Vector3 addHeight = this.getHeight(type);
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] += addHeight;
            }

            tempMesh.vertices = vertices;

            tempMesh.RecalculateNormals();
            tempMesh.RecalculateBounds();

            return tempMesh;
        }

        /// <summary>
        /// Creating a 'correction' Vector3 to add this to a given Vector3, and
        /// therefore give an area a new height.
        /// </summary>
        /// <param name="type">Type of area (forest, water etc.)</param>
        /// <returns>a Vector3 with a certain height correction (negative value)</returns>
        private Vector3 getHeight(string type)
        {

            Vector3 vector;

            switch (type)
            {

                case "amenity":
                    vector = new Vector3(0, -0.01f, 0);
                    break;
                case "building":
                    vector = new Vector3(0, -0.00f, 0);
                    break;
                case "commercial":
                    vector = new Vector3(0, -0.02f, 0);
                    break;
                case "forest":
                    vector = new Vector3(0, -0.03f, 0);
                    break;
                case "historic":
                    vector = new Vector3(0, -0.04f, 0);
                    break;
                case "industrial":
                    vector = new Vector3(0, -0.05f, 0);
                    break;
                case "landuse":
                    vector = new Vector3(0, -0.06f, 0);
                    break;
                case "leisure":
                    vector = new Vector3(0, -0.07f, 0);
                    break;
                case "natural":
                    vector = new Vector3(0, -0.08f, 0);
                    break;
                case "parking":
                    vector = new Vector3(0, -0.09f, 0);
                    break;
                case "residential":
                    vector = new Vector3(0, -0.10f, 0);
                    break;
                case "shop":
                    vector = new Vector3(0, -0.11f, 0);
                    break;
                case "sport":
                    vector = new Vector3(0, -0.12f, 0);
                    break;
                case "tourism":
                    vector = new Vector3(0, -0.13f, 0);
                    break;
                case "water":
                    vector = new Vector3(0, -0.14f, 0);
                    break;
                default:
                    vector = new Vector3(0, -0.15f, 0);
                    break;

            }

            return vector;
        }


        /// <summary>
        /// Creating the mesh for base terran, so no obejct can fall through any
        // hole within generated areas.
        /// </summary>
        /// <param name="tempMesh">Temporary mesh of generated terrain</param>
        /// <returns>A mesh with height below all other generated meshes</returns>
        private Mesh CreateBaseTerrain(Mesh tempMesh)
        {

            Vector3[] vertices = tempMesh.vertices;

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] += new Vector3(0, -0.16f, 0);
            }

            tempMesh.vertices = vertices;

            tempMesh.RecalculateNormals();
            tempMesh.RecalculateBounds();

            return this.tempMesh;

        }

    }

}
