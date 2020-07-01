using System.Xml.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Collections.Generic;
using Assets.SUMOConnectionScripts;
using System.Linq;
using Assets.Scripts.SUMOConnectionScripts;
using System;
using System.Globalization;


using System.Collections;

using Assets.Scripts.OsmParser;
using System.IO;
using Assets.Scripts.SUMOConnectionScripts.Maps;

//using Assets.Scripts.SUMOConnectionScripts.Maps.Splines;
namespace SUMOConnectionScripts
{
    public class SumoStreetImporter : MonoBehaviour
    {
        // Used for 2D SUMO coordinates 
        const float defaultRoadHeight = 0.01f;
        const float SCAN_WIDTH = 6f;
        public const float autoMergeRadius = 0.4f;

        public Material roadMaterial;//default
        public Material roadMaterialSingleLane;
        public Material roadMaterialTrippleLane;
        public Material roadMaterialQuadLane;
        public Material junctionMaterial;
        public Material markerMaterial;
        public Material sidewalkMaterial;
        public PhysicMaterial roadPhysicMaterial;
        public PhysicMaterial offRoadPhysicsMaterial;

        const float defaultStreetWidth = 3.5f;
        const float defaultSidewalkWidth = 1.8f; // https://www.geh-recht.de/42-fussverkehrsanlagen/fussverkehrsanlagen/139-fa-gehwege-gehwegbreiten-grundstueckszufahrten-mischungsprinzip.html#Breite
        public float streetWidth = 0;
        public float sidewalkWidth = 0;
        public float chunkwidth;
        public float mergeRange;
        public Transform parent;

        private OsmParser osmParser;

        VehicleClass vc = new VehicleClass();

        private Dictionary<string, List<string>> interlinkedLanes = new Dictionary<string, List<string>>();
        private Dictionary<string, KeyValuePair<string, string>> fromTo = new Dictionary<string, KeyValuePair<string, string>>();
        private Dictionary<string, Dictionary<string, float>> closebyLanes = new Dictionary<string, Dictionary<string, float>>();
        private Dictionary<string, Vector3> laneStartPositions = new Dictionary<string, Vector3>();
        private Dictionary<string, Vector3> laneEndPositions = new Dictionary<string, Vector3>();
        private LinkedList<GraphChunk> chunks = new LinkedList<GraphChunk>();
        private List<StreetNode> rawGraph = new List<StreetNode>();
        private List<StreetNode> optGraph = new List<StreetNode>();

        public List<StreetNode> OptGraph
        {
            get { return optGraph; }

        }

        private Dictionary<string, LaneProperties> propertiesOfEveryLane = new Dictionary<string, LaneProperties>();
        public Dictionary<string, LaneProperties> PropertiesOfEveryLane
        {
            get { return propertiesOfEveryLane; }
        }


        public Dictionary<string, List<LaneSegment>> laneSegments;

        private IntersectionGenerator intersectionGenerator;
        private List<StreetNode> signNodes = new List<StreetNode>();

        public GameObject streetSignPrefab;
        public Dictionary<int, Material> speedMaterials = new Dictionary<int, Material>();




        public SumoStreetImporter(Material roadMaterial, PhysicMaterial roadPhysicMaterial, PhysicMaterial offRoadPhysicMaterial, float chunkwidth, float mergeRange, Transform parent, OsmParser osmParser, XElement root)
        {
            this.chunkwidth = chunkwidth;
            this.mergeRange = mergeRange;
            this.roadMaterial = roadMaterial;
            this.roadPhysicMaterial = roadPhysicMaterial;
            this.intersectionGenerator = new IntersectionGenerator();
            this.osmParser = osmParser;
            this.offRoadPhysicsMaterial = offRoadPhysicMaterial;

            if (osmParser != null)
            {
                UnityEngine.Debug.Log("Amount of ways in OSM Data = " + osmParser.ways.Count);
                UnityEngine.Debug.Log("Amount of nodes in OSM Data = " + osmParser.nodes.Count);
            }


        }
     


        /// <summary>  
        ///  Creates a list of all edges of a streetmap from a sumo .net.xml file
        /// </summary>
        /// <param name="rootElement">Root element of a .net.xml SUMO file</param>
        public IDictionary<string, IList<Vector3>> ImportStreetNet(XElement rootElement, bool internalEdges, bool normalAndConnectorEdges)
        {
            Dictionary<string, IList<Vector3>> streetNet = new Dictionary<string, IList<Vector3>>();

            foreach (XElement edge in rootElement.Elements("edge"))
            {
                if ((edge.Attribute("function") != null && edge.Attribute("function").Value == "internal") ? internalEdges : normalAndConnectorEdges)
                {
                    foreach (XElement lane in edge.Elements("lane"))
                    {
                        string id = lane.Attribute("id").Value;
                        string shape = lane.Attribute("shape").Value;
                        bool generateStreetNodes = !internalEdges && MapRasterizer.Instance.IsEditor && osmParser != null;
                        List<Vector3> positions = GetCoordinates(shape, id, generateStreetNodes);


                        laneStartPositions.Add(id, positions.First());
                        laneEndPositions.Add(id, positions.Last());
                        bool linkFromTo = (edge.Attribute("from") != null && edge.Attribute("to") != null);

                        if (linkFromTo)
                        {
                            fromTo.Add(id, new KeyValuePair<string, string>(edge.Attribute("from").Value, edge.Attribute("to").Value));

                        }

                        // Read the properties of each lane and store them in a Dictionary/struct
                        // The speed is needed to set the street signs
                        LaneProperties laneProperties = new LaneProperties();
                        laneProperties.laneId = lane.Attribute("id").Value;
                        laneProperties.speed = float.Parse(lane.Attribute("speed").Value, CultureInfo.InvariantCulture);
                        laneProperties.parentedge = edge.Attribute("id").Value;




                        // get index
                        laneProperties.laneIndex = Int32.Parse(lane.Attribute("index").Value);
                        // extract vehicle class permissions
                        List<VehicleClasses> lvca = null;// new List<VehicleClasses>(); // List of allowed vehicle classes

                        if (lane.Attribute("allow") != null)
                        {
                            string allowed = lane.Attribute("allow").Value;     // extract all allowed vehicle classes
                            lvca = GetVehiclePermits(allowed);
                        }
                        // extract vehicle class restriction
                        List<VehicleClasses> lvcd = null;// = new List<VehicleClasses>(); // List of disallowed vehicle classes

                        if (lane.Attribute("disallow") != null)
                        {
                            string disallow = lane.Attribute("disallow").Value;     // extract all disallowed vehicle classes
                            lvcd = GetVehiclePermits(disallow);
                        }
                        laneProperties.AllowedVClass = lvca;
                        laneProperties.DisallowedVClass = lvcd;

                        if (lane.Attribute("width") != null) // check if width is set 
                        {
                            laneProperties.width = float.Parse(lane.Attribute("width").Value); // use width from .net.xml
                        }
                        else
                        {
                            if (IsSidewalk(laneProperties.AllowedVClass)) // check 
                            {
                                laneProperties.width = defaultStreetWidth; // use default width, propably leads to minor errors with calculating the position
                            }
                            else
                            {
                                laneProperties.width = defaultStreetWidth;
                            }

                        }
                        propertiesOfEveryLane.Add(lane.Attribute("id").Value, laneProperties);


                        /*
                    StreetNode previous = null;
                    foreach (Vector3 pos in positions)
                    {
                        StreetNode node = new StreetNode(pos, NodeType.OriginalNode);
                        if (previous != null)
                        {
                            node.AddNeighbour(previous);
                        }
                        node.SortToChunk(chunks, chunkwidth);
                        rawGraph.Add(node);
                        previous = node;
                    }
                     */
                        streetNet.Add(lane.Attribute("id").Value, positions);
                    }

                }
            }

            // set sidewalkWidth and streetWidth (for now)
            bool sidewalkSet = false;
            bool streetSet = false;
            foreach (var lane in PropertiesOfEveryLane)
            {
                if (IsSidewalk(lane.Value.AllowedVClass) && !sidewalkSet)  // there is a sidewalk defined
                {
                    this.sidewalkWidth = lane.Value.width;
                }
                else if (!streetSet && !IsSidewalk(lane.Value.AllowedVClass))
                {
                    // this lane does not have a sidewalk
                    this.streetWidth = lane.Value.width;

                }
                if (sidewalkSet && streetSet)
                    break;
            }
            return streetNet;
        }


        /// <summary>
        /// Split up a string with vehicle permissions
        /// </summary>
        /// <param name="permits">string with vehicle permissions in the form "private pedestrian [...]"</param>
        /// <returns>List<VehicleClasses></returns>
        private List<VehicleClasses> GetVehiclePermits(string permits)
        {
            List<VehicleClasses> lvc = new List<VehicleClasses>();
            var words = permits.Split(null);
            foreach (string s in words)
            {
                lvc.Add(vc.ParseVehicleClassEnum(s));

                // add warning for test purposes
                if (lvc.Last().Equals(VehicleClasses.error))
                    UnityEngine.Debug.LogWarning("Error at ParseVehicleClass: Not a known vehicleclass");
            }
            return lvc;
        }

        /// <summary>
        /// Iterate through the fromTo Dictionary with a given laneid to get twoway lanes and return them
        /// </summary>
        /// <param name="currentLaneId"></param>
        /// <returns>List<string> with landid of the twoway lanes</returns>
        public List<string> GetTwoWayLanes(string currentLaneId)
        {
            List<string> twoWayLanes = new List<string>();

            var fromToKeys = fromTo.Keys; // string lane id
            if (!fromToKeys.Contains(currentLaneId))
            {
                UnityEngine.Debug.Log("No key with laneid: " + currentLaneId);
                return twoWayLanes;
            }
            var outerIdVal = fromTo[currentLaneId];


            foreach (string innerId in fromToKeys)
            {

                var innerIdVal = fromTo[innerId];
                if (outerIdVal.Key == innerIdVal.Value && outerIdVal.Value == innerIdVal.Key)
                    twoWayLanes.Add(innerId);

            }
            return twoWayLanes;

        }

        public void MakeJunctionsSeamless(IDictionary<string, IList<Vector3>> externRoadNet, IList<Junction> junctions)
        {
            //turn junction list into junction dict for O(n)=1 access instead of O(n)=n
            Dictionary<string, Junction> junctionDict = new Dictionary<string, Junction>();
            foreach (Junction junc in junctions)
            {
                junctionDict.Add(junc.identifier, junc);
            }
            foreach (KeyValuePair<string, IList<Vector3>> kvp in externRoadNet)
            {
                string roadID = kvp.Key;
                var associatedJunctions = fromTo[roadID];
                Junction associatedJunctionFront = junctionDict[associatedJunctions.Key];
                Junction associatedJunctionEnd = junctionDict[associatedJunctions.Value];
                kvp.Value[0] = new Vector3(kvp.Value[0].x, associatedJunctionFront.AverageHeight, kvp.Value[0].z);
                int lastIndex = kvp.Value.Count - 1;
                kvp.Value[lastIndex] = new Vector3(kvp.Value[lastIndex].x, associatedJunctionEnd.AverageHeight, kvp.Value[lastIndex].z);
            }
        }

        public float DistanceToStreet(Vector2 point)
        {

            int cx = GraphChunk.FullDivision(point.x, chunkwidth);
            int cy = GraphChunk.FullDivision(point.y, chunkwidth);

            float dist = float.MaxValue;

            foreach (GraphChunk c in chunks)
            {
                if (Mathf.Abs(cx - c.x) <= 1 && Mathf.Abs(cy - c.y) <= 1)
                {
                    foreach (StreetNode n in c.nodes)
                    {
                        dist = Mathf.Min(dist, n.DistanceToPoint(point));
                    }
                }
            }

            return dist;

        }

        public void SimplifyGraph()
        {
            LinkLanesByDirection();

            foreach (StreetNode node in rawGraph)
            {
                int cx = GraphChunk.FullDivision(node.position.x, chunkwidth);
                int cy = GraphChunk.FullDivision(node.position.z, chunkwidth);

                List<StreetNode> nearbyNodes = new List<StreetNode>();

                if (this.interlinkedLanes.ContainsKey(node.LaneID))
                {
                    string dbstring = "Linked " + node.LaneID + " with: ";
                    foreach (string id in this.interlinkedLanes[node.LaneID])
                    {
                        dbstring += id + " ,";
                    }
                    //UnityEngine.Debug.LogWarning(dbstring);
                }


                foreach (GraphChunk c in chunks)
                {
                    if (Mathf.Abs(cx - c.x) <= 1 && Mathf.Abs(cy - c.y) <= 1)
                    {
                        foreach (StreetNode n in c.nodes)
                        {
                            //make sure that only lanes that connect the same intersections can be connected. That way you can make sure that 
                            //they belong to the same road
                            if (this.interlinkedLanes.ContainsKey(n.LaneID) && this.interlinkedLanes[n.LaneID].Contains(node.LaneID)
                                && this.interlinkedLanes.ContainsKey(node.LaneID) && this.interlinkedLanes[node.LaneID].Contains(n.LaneID)
                                )
                            {
                                    nearbyNodes.Add(n);
                                
                            }
                        }
                    }
                }


                StreetNode mergedNode = node.PrimitiveMerge(nearbyNodes, this.streetWidth);//use mergeRange for non-primitive merge algos
                //StreetNode mergedNode = node.MultiMerge(mergeRange,nearbyNodes);

                if (mergedNode != null && mergedNode.generate)
                {
                    optGraph.Add(mergedNode);
                }
            }
        }

        public void ReduceNearbyLanesToBestLane()
        {
            foreach (string key in this.closebyLanes.Keys)
            {
                float max = 0;
                string maxstr = "";
                foreach (var kvp in closebyLanes[key])
                {
                    if (max < kvp.Value)
                    {
                        max = kvp.Value;
                        maxstr = kvp.Key;
                    }
                }
                closebyLanes[key].Clear();
                closebyLanes[key].Add(maxstr, max);

            }
        }

        public void HitScanRoadgraph()
        {
            foreach (StreetNode node in this.rawGraph)
            {
                List<StreetNode> nearbyNodes = new List<StreetNode>();
                if (!this.closebyLanes.ContainsKey(node.LaneID))
                {
                    this.closebyLanes.Add(node.LaneID, new Dictionary<string, float>());
                }
                int cx = GraphChunk.FullDivision(node.position.x, chunkwidth);
                int cy = GraphChunk.FullDivision(node.position.z, chunkwidth);
                foreach (GraphChunk c in chunks)
                {
                    if (Mathf.Abs(cx - c.x) <= 1 && Mathf.Abs(cy - c.y) <= 1)
                    {
                        foreach (StreetNode n in c.nodes)
                        {
                            nearbyNodes.Add(n);
                        }
                    }
                }
                List<StreetNode> hitscanResults = node.Hitscan(9, nearbyNodes);
                foreach (StreetNode n in hitscanResults)
                {
                    if (!closebyLanes[node.LaneID].ContainsKey(n.LaneID))
                    {
                        closebyLanes[node.LaneID].Add(n.LaneID, 0);
                    }
                    closebyLanes[node.LaneID][n.LaneID] += Mathf.Min(1 / Vector3.Distance(node.position, n.position), 1f);
                }
            }
        }

        public void LinkLanesByDirection()
        {
            foreach (string lane in fromTo.Keys)
            {
                foreach (string otherlane in fromTo.Keys)
                {
                    if (lane == otherlane)
                    {
                        continue;
                    }
                    if (
                        (fromTo[lane].Key == fromTo[otherlane].Key || fromTo[lane].Key == fromTo[otherlane].Value)
                        && (fromTo[lane].Value == fromTo[otherlane].Key || fromTo[lane].Value == fromTo[otherlane].Value)
                        )
                    {
                        if (!interlinkedLanes.ContainsKey(otherlane))
                        {
                            interlinkedLanes.Add(otherlane, new List<string>());
                        }
                        if (!interlinkedLanes.ContainsKey(lane))
                        {
                            interlinkedLanes.Add(lane, new List<string>());
                        }
                        if (!interlinkedLanes[otherlane].Contains(lane))
                        {
                            interlinkedLanes[otherlane].Add(lane);
                        }
                        if (!interlinkedLanes[lane].Contains(otherlane))
                        {
                            interlinkedLanes[lane].Add(otherlane);
                        }
                    }
                }
            }
        }

        public void LinkLanesByNearbyNodes()
        {
            //as soon as one lane is 20% less important than the most important, don't consider it linked
            //For now don't take more than 2 lanes. 4 laned streets can consist out of 2x2 lanes, 4 didn't work on intersections!

            foreach (string lane in closebyLanes.Keys)
            {
                float secondmax = 0;
                float maxlanecount = 0;
                string firstallowed = "";
                string secondallowed = "";
                foreach (string otherlane in closebyLanes[lane].Keys)
                {
                    if (closebyLanes[lane][otherlane] > maxlanecount)
                    {
                        secondmax = maxlanecount;
                        secondallowed = firstallowed;
                        maxlanecount = closebyLanes[lane][otherlane];
                        firstallowed = otherlane;
                    }
                    else if (closebyLanes[lane][otherlane] > secondmax)
                    {
                        secondmax = closebyLanes[lane][otherlane];
                        secondallowed = otherlane;
                    }
                }

                foreach (string otherlane in closebyLanes[lane].Keys)
                {
                    if (otherlane != firstallowed && otherlane != secondallowed)
                    {
                        continue;
                    }
                    if (closebyLanes[lane][otherlane] > (maxlanecount / 5))
                    {
                        if (!interlinkedLanes.ContainsKey(otherlane))
                        {
                            interlinkedLanes.Add(otherlane, new List<string>());
                        }
                        if (!interlinkedLanes.ContainsKey(lane))
                        {
                            interlinkedLanes.Add(lane, new List<string>());
                        }
                        if (!interlinkedLanes[otherlane].Contains(lane))
                        {
                            interlinkedLanes[otherlane].Add(lane);
                        }
                        if (!interlinkedLanes[lane].Contains(otherlane))
                        {
                            interlinkedLanes[lane].Add(otherlane);
                        }
                    }
                }
            }
        }

        public void ResetRoadGraph()
        {
            chunks.Clear();
            optGraph.Clear();
            rawGraph.Clear();
        }

        public void ScanRoadgraph()
        {
            foreach (StreetNode node in this.rawGraph)
            {
                if (!this.closebyLanes.ContainsKey(node.LaneID))
                {
                    this.closebyLanes.Add(node.LaneID, new Dictionary<string, float>());
                }
                int cx = GraphChunk.FullDivision(node.position.x, chunkwidth);
                int cy = GraphChunk.FullDivision(node.position.z, chunkwidth);
                foreach (GraphChunk c in chunks)
                {
                    if (Mathf.Abs(cx - c.x) <= 1 && Mathf.Abs(cy - c.y) <= 1)
                    {
                        foreach (StreetNode n in c.nodes)
                        {
                            if (Vector3.Distance(n.position, node.position) < SCAN_WIDTH)
                            {
                                if (!closebyLanes[node.LaneID].ContainsKey(n.LaneID))
                                {
                                    closebyLanes[node.LaneID].Add(n.LaneID, 0);
                                }
                                closebyLanes[node.LaneID][n.LaneID] += Mathf.Min(1 / Vector3.Distance(node.position, n.position), 1f);
                            }
                        }
                    }
                }
            }
        }

        public void GenerateJunctions(IList<Junction> junctions)
        {


            foreach (Junction junction in junctions)
            {
                Vector2[] points = new Vector2[junction.positions.Count];
                Vector3 center = Vector3.zero;
                float avrgHeight = junction.AverageHeight;

                int i = 0;
                foreach (Vector3 point in junction.positions)
                {
                    points[i] = new Vector2(point.x, point.z);
                    center += point;

                    if (osmParser != null)
                    {
                        MapRasterizer.Instance.AddMapPoint(new Vector3(point.x, 0, point.z), junction.AverageHeight);
                    }
                    i++;
                }

                center /= junction.positions.Count;

                Mesh m = WrapperTriangulation.createMesh(points);
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                go.isStatic = true;
                MeshFilter mf = go.GetComponent<MeshFilter>();
                mf.mesh = m;
                go.name = "Kreuzung_" + junction.identifier;
                Assets.Scripts.Chunking.ChunkManager.AddRenderer(go.GetComponent<MeshRenderer>(), center, false);

                MeshCollider mc = go.AddComponent<MeshCollider>();


                go.GetComponent<MeshRenderer>().material = junctionMaterial;

                UnityEngine.Debug.Log("Average Height : " + avrgHeight);

                mc.material = PhysicMaterial.Instantiate(roadPhysicMaterial);

                UnityEngine.Debug.Log("Average Height : " + avrgHeight);

                go.transform.position = new Vector3(0, avrgHeight - 0.001f, 0);

                intersectionGenerator.Finalize(go, junction, IdentifyStreetEdges(junction));

            }
        }

        public List<KeyValuePair<int, int>> IdentifyStreetEdges(Junction junction)
        {
            List<KeyValuePair<int, int>> connectedVertexIds = new List<KeyValuePair<int, int>>();
            Vector3 junctionCenter = Vector3.zero;
            foreach (Vector3 vert in junction.positions)
            {
                junctionCenter += vert;
            }
            junctionCenter /= junction.positions.Count;

            foreach (string laneID in junction.connectedLanes)
            {
                if (!laneStartPositions.ContainsKey(laneID) || !laneEndPositions.ContainsKey(laneID))
                {
                    continue;
                }
                //find relevant STREET edge
                Vector3 relevantStreetEdge = laneStartPositions[laneID];

                if (Vector3.Distance(laneStartPositions[laneID], junctionCenter) > Vector3.Distance(laneEndPositions[laneID], junctionCenter))
                {
                    relevantStreetEdge = laneEndPositions[laneID];
                }

                //find closest junction edge
                int closestJunctionID = -1;
                Vector3 closestJunctionPosition = new Vector3(-9999, -9999, -9999);
                int id = 0;
                foreach (Vector3 point in junction.positions)
                {
                    if (Vector3.Distance(point, relevantStreetEdge) < Vector3.Distance(relevantStreetEdge, closestJunctionPosition))
                    {
                        closestJunctionPosition = point;
                        closestJunctionID = id;
                    }
                    id++;
                }
                //find which neighbouring edge
                int nextID = (closestJunctionID + 1) % junction.positions.Count;
                int prevID = (closestJunctionID - 1 + junction.positions.Count) % junction.positions.Count;

                //find comparison point: opposite direction of first point
                Vector3 comparisonPoint = relevantStreetEdge + (closestJunctionPosition - relevantStreetEdge);

                if (Vector3.Distance(junction.positions[nextID], comparisonPoint) > Vector3.Distance(junction.positions[prevID], comparisonPoint))
                {
                    connectedVertexIds.Add(new KeyValuePair<int, int>(closestJunctionID, nextID));
                    connectedVertexIds.Add(new KeyValuePair<int, int>(nextID, closestJunctionID));
                    //DrawLine(junction.positions[closestJunctionID], junction.positions[nextID], Color.green);
                    //DrawLine(junction.positions[closestJunctionID], junction.positions[prevID], Color.red);
                }
                else
                {
                    connectedVertexIds.Add(new KeyValuePair<int, int>(closestJunctionID, prevID));
                    connectedVertexIds.Add(new KeyValuePair<int, int>(prevID, closestJunctionID));
                    //DrawLine(junction.positions[closestJunctionID], junction.positions[prevID], Color.green);
                    //DrawLine(junction.positions[closestJunctionID], junction.positions[nextID], Color.red);
                }

            }
            return connectedVertexIds;
        }

        //Probably not working properly
        public List<KeyValuePair<int, int>> IdentifyStreetEdgesOld(Junction junction)
        {
            List<KeyValuePair<int, int>> connectedVertexIds = new List<KeyValuePair<int, int>>();
            Vector3 junctionCenter = Vector3.zero;
            foreach (Vector3 vert in junction.positions)
            {
                junctionCenter += vert;
            }
            junctionCenter /= junction.positions.Count;

            foreach (string laneID in junction.connectedLanes)
            {
                string parallelLane = "";
                bool foundParallelLane = false;
                foreach (string otherLane in junction.connectedLanes)
                {
                    if (otherLane == laneID)
                    {
                        continue;
                    }
                    if (IsParallelLane(otherLane, laneID))
                    {
                        foundParallelLane = true;
                        parallelLane = otherLane;
                        break;
                    }
                }
                if (!foundParallelLane)
                {
                    continue;
                }

                //find relevant STREET edges now
                Vector3 relevantStreetEdge1 = laneStartPositions[laneID];
                Vector3 relevantStreetEdge2 = laneStartPositions[parallelLane];

                if (Vector3.Distance(laneStartPositions[laneID], junctionCenter) > Vector3.Distance(laneEndPositions[laneID], junctionCenter))
                {
                    relevantStreetEdge1 = laneEndPositions[laneID];
                }
                if (Vector3.Distance(laneStartPositions[parallelLane], junctionCenter) > Vector3.Distance(laneEndPositions[parallelLane], junctionCenter))
                {
                    relevantStreetEdge2 = laneEndPositions[parallelLane];
                }

                //Now find the closest JUNCTION edges to those two street edges. Those are the parts where the street is "connecting" to the junction
                //This is usefull to know where to draw a white line and where not to

                Vector3 closestJunctionEdge1 = new Vector3(-9999, -9999, -9999);
                Vector3 closestJunctionEdge2 = new Vector3(-9999, -9999, -9999);
                int id1 = -1;
                int id2 = -1;

                int id = 0;
                foreach (Vector3 point in junction.positions)
                {
                    if (Vector3.Distance(point, relevantStreetEdge1) < Vector3.Distance(relevantStreetEdge1, closestJunctionEdge1))
                    {
                        closestJunctionEdge1 = point;
                        id1 = id;
                    }
                    if (Vector3.Distance(point, relevantStreetEdge2) < Vector3.Distance(relevantStreetEdge2, closestJunctionEdge2))
                    {
                        closestJunctionEdge2 = point;
                        id2 = id;
                    }

                    id++;
                }
                connectedVertexIds.Add(new KeyValuePair<int, int>(id1, id2));
            }

            return connectedVertexIds;
        }

        public bool IsParallelLane(string laneA, string laneB)
        {
            /*
            if (
                (fromTo[laneA].Key == fromTo[laneB].Key)
                && (fromTo[laneA].Value == fromTo[laneB].Value)
              )
            {
                return true;
            }
            if (
                (fromTo[laneA].Value == fromTo[laneB].Key)
                && (fromTo[laneA].Key == fromTo[laneB].Value)
              )
            {
                return true;
            }
            return false;
            */
            if (interlinkedLanes[laneA].Contains(laneB))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Creates a list of vectors for every junction representing the shape of that junction from a sumo .net.xml file
        /// </summary>
        /// <param name="rootElement"></param>
        /// <returns></returns>
        public IList<Junction> ImportJunctionShapes(XElement rootElement)
        {
            List<Junction> junctions = new List<Junction>();

            foreach (XElement junction in rootElement.Elements("junction"))
            {
                string lanes = junction.Attribute("incLanes").Value;
                List<string> connectedLanes = lanes.Split(' ').ToList();
                if (junction.Attribute("shape") != null)
                {
                    string shape = junction.Attribute("shape").ToString();
                    string id = junction.Attribute("id").Value;
                    Junction junctionObject = new Junction(GetCoordinates(shape, id), id, connectedLanes);
                    junctions.Add(junctionObject);
                    if (osmParser != null)
                    {
                        if (osmParser.nodes.ContainsKey(id))
                        {
                            float junctionHeight = (float)osmParser.nodes[id].height;

                            for (int i = 0; i < junctionObject.positions.Count; i++)
                            {
                                junctionObject.positions[i] = new Vector3(junctionObject.positions[i].x, junctionHeight, junctionObject.positions[i].z);
                            }
                            junctionObject.AverageHeight = junctionHeight;
                            if (StreetNode.DO_DEBUG)
                            {
                                foreach (Vector3 point in junctionObject.positions)
                                {
                                    UnityEngine.Debug.Log(point);
                                }
                            }
                            if (MapRasterizer.Instance.IsEditor)
                            {
                                List<string> idlist = new List<string>();
                                List<string> fullidlist = new List<string>();
                                idlist.Add(id);
                                fullidlist.Add("unique-" + id);
                                Vector3 avrgPoint = Vector3.zero;
                                var coordlist = GetCoordinates(shape, id);
                                foreach (Vector3 pos in coordlist)
                                {
                                    avrgPoint += pos;
                                }
                                avrgPoint /= coordlist.Count;
                                avrgPoint.y = junctionHeight;
                                MapRasterizer.Instance.EditorControl.AddPoint(avrgPoint, Assets.Scripts.EditorScene.EditorPointType.CrossroadPoint, idlist, fullidlist, new List<MapPoint>());
                            }
                        }

                    }


                }


                /*
                lanes = junction.Attribute("intLanes").Value;
                LinkLanes(lanes.Split(' ').ToList());
                */
            }
            return junctions;
        }

        public void LinkLanes(List<string> list)
        {
            List<string> nlist = new List<string>();
            foreach (string id in list)
            {
                string nstr = id.Replace(":", "");
                nstr = id.Split('#')[0];
                nlist.Add(nstr);
            }

            foreach (string laneid in nlist)
            {
                foreach (string otherid in nlist)
                {
                    if (otherid == laneid)
                    {
                        continue;
                    }
                    if (!interlinkedLanes.ContainsKey(otherid))
                    {
                        interlinkedLanes.Add(otherid, new List<string>());
                    }
                    if (!interlinkedLanes.ContainsKey(laneid))
                    {
                        interlinkedLanes.Add(laneid, new List<string>());
                    }
                    if (!interlinkedLanes[otherid].Contains(laneid))
                    {
                        interlinkedLanes[otherid].Add(laneid);
                    }
                    if (!interlinkedLanes[laneid].Contains(otherid))
                    {
                        interlinkedLanes[laneid].Add(otherid);
                    }
                }

            }
        }

        public IDictionary<string, float> ImportLaneLenghts(XElement rootElement, bool internalEdges, bool normalAndConnectorEdges)
        {
            IDictionary<string, float> laneIds = new Dictionary<string, float>();

            foreach (XElement edge in rootElement.Elements("edge"))
            {
                if ((edge.Attribute("function") != null && edge.Attribute("function").Value == "internal") ? internalEdges : normalAndConnectorEdges)
                {
                    foreach (XElement lane in edge.Elements("lane"))
                    {
                        laneIds.Add(lane.Attribute("id").Value, float.Parse(lane.Attribute("length").Value, CultureInfo.InvariantCulture));
                    }
                }
            }
            return laneIds;
        }

        private List<Vector3> GetCoordinates(string shape, string identifier, bool generateStreetNodes = false)
        {
            List<Vector3> coordinates = new List<Vector3>();

            string relevantId = identifier.Replace(":", "").Replace("-", "").Split('_')[0].Split('#')[0];

            int i = 0;
            Match coordinate = Regex.Match(shape, @"(-)?\d+.\d\d,(-)?\d+.\d\d");
            bool osmInfoFound = false;
            while (coordinate.Success)
            {
                Match number = Regex.Match(coordinate.Value, @"(-)?\d+.\d\d");
                Vector3 position = new Vector3(float.Parse(number.Value), defaultRoadHeight, float.Parse(number.NextMatch().Value));



                if (osmParser != null)
                {
                    if (osmParser.ways.ContainsKey(relevantId))
                    {
                        OsmWay way = osmParser.ways[relevantId];
                        if (way != null)
                        {
                            if (way.includedNodes.Count > i)
                            {
                                position.y = (float)way.includedNodes[i].height;
                                string predictedFullid = way.id + "-" + way.includedNodes[i].id;
                                if (osmParser.correctionPositions.ContainsKey(predictedFullid))
                                {
                                    position.y = osmParser.correctionPositions[predictedFullid].y;
                                }

                                if (StreetNode.DO_DEBUG)
                                {
                                    UnityEngine.Debug.Log("Matched Shape id with OSM Data: Shape: " + relevantId + " Node: " + way.includedNodes[i].id + " Height: " + position.y);
                                }
                                osmInfoFound = true;
                                if (generateStreetNodes)
                                {
                                    List<string> fullidList = new List<string>();
                                    List<string> idlist = new List<string>();
                                    idlist.Add(way.includedNodes[i].id);
                                    fullidList.Add(predictedFullid);
                                    MapRasterizer.Instance.EditorControl.AddPoint(position, Assets.Scripts.EditorScene.EditorPointType.StreetPoint, idlist, fullidList, new List<MapPoint>());
                                }
                            }
                        }
                    }
                    else
                    {

                    }
                }


                coordinates.Add(position);
                i++;
                coordinate = coordinate.NextMatch();
            }

            if (!osmInfoFound)
            {
                UnityEngine.Debug.Log("Could not find identifier in OSM Data: " + relevantId);
            }


            return coordinates;

        }

        public void DrawStreetNet(IDictionary<string, IList<Vector3>> net, Transform parent, bool spheres, bool lines, IDictionary<string, IList<Vector3>> unoptimizedNet)
        {
            ResetRoadGraph();

            laneSegments = new Dictionary<string, List<LaneSegment>>();

            foreach (string laneId in net.Keys.ToList())
            {
                laneSegments.Add(laneId, new List<LaneSegment>());
                IList<Vector3> lane = net[laneId];
                bool init = false;
                Vector3 oldpos = new Vector3();
                bool isFirst = true;
                bool isSecond = false;

                OsmWay way = null;
                string relevantId = laneId.Replace(":", "").Replace("-", "").Split('_')[0].Split('#')[0]; ;

                if (osmParser != null && osmParser.ways.ContainsKey(relevantId))
                {
                    way = osmParser.ways[relevantId];
                }

                StreetNode previous = null;
                int index = 0;
                foreach (Vector3 pos in lane)
                {
                    StreetNode node = new StreetNode(pos, NodeType.OriginalNode);
                    node.speed = propertiesOfEveryLane[laneId].speed;
                    bool isLast = (index == lane.Count - 1);

                    if (isSecond)
                    {
                        isSecond = false;
                        previous.unoptimizedNeighbourNode = new StreetNode(unoptimizedNet[laneId][1], NodeType.VirtualNode);

                    }
                    if (isFirst)
                    {
                        isFirst = false;
                        isSecond = true;
                        // Here we determine wheterh the node needs a speed sign or not.
                        // We need to use only the first lane of each edge.
                        // This is for streets that consist out of multiple lanes
                        // The id of the outmost lane in each direction ends with "_0"
                        // This prevents street signs on streets with more than two lanes
                        if (laneId.EndsWith("_0"))
                        {
                            node.needsStreetSign = true;	// Place a street sign whenever a new Lane starts
                        }
                    }
                    if (isLast)
                    {
                        node.unoptimizedNeighbourNode = new StreetNode(unoptimizedNet[laneId][unoptimizedNet[laneId].Count - 2], NodeType.VirtualNode);
                    }
                    node.HopsToEnd = Math.Min(index, lane.Count - index - 1);
                    if (previous != null)
                    {
                        node.AddNeighbour(previous);
                    }
                    node.SortToChunk(chunks, chunkwidth);
                    rawGraph.Add(node);
                    previous = node;
                    node.LaneID = laneId;
                    node.EdgeIndex = index;

                    node.osmIdentifier = relevantId;
                    node.osmWay = way;

                    if (node.osmWay != null && StreetNode.BlacklistedStreetTypes.Contains(node.osmWay.highway))
                    {
                        node.generate = false;
                    }
                    LaneSegment ls = new LaneSegment();
                    ls.laneId = laneId;
                    ls.edgeIndex = index;
                    ls.ownPosition = node.position;
                    if (osmParser != null && osmParser.nodes.ContainsKey(node.osmIdentifier))
                    {
                        ls.ownPosition.y = (float)(osmParser.nodes[node.osmIdentifier].height);
                    }
                    laneSegments[laneId].Add(ls);

                    index++;
                }

                foreach (Vector3 pos in lane)
                {

                    if (spheres)
                    {
                        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        sphere.transform.position = pos;
                        sphere.transform.parent = parent.transform;
                    }
                    if (lines)
                    {
                        if (init)
                        {
                            DrawLine(pos, oldpos, parent.transform);
                        }
                    }



                    if (init)
                    {

                    }
                    else
                    {
                        init = true;
                    }
                    oldpos = pos;
                }
            }

            // Use raw graph to have one lane on each driving direction
            // We place a speed sign whenever a new lane starts
            foreach (StreetNode sn in rawGraph)
            {
                // Work with each node that needs a street sign.
                // It has to be the first node in each lane to make sure it has only one neighbour node
                if (sn.needsStreetSign == true)
                {
                    string speedSignName = DetermineSpeedSignPath(sn);
                    signNodes.Add(sn);

                }
            }


            UnityEngine.Debug.Log("Simplifying Road Graph");
            SimplifyGraph();
            UnityEngine.Debug.Log("Road Graph optimization complete");
            UnityEngine.Debug.Log("" + optGraph.Count + " in opt graph");
            float lastDigits = 0f;
            //using (StreamWriter sw = new StreamWriter(targetpath))
            //{
            // sw.WriteLine("NodeX;NodeY;NodeZ;StreetSize");
            foreach (StreetNode sn in optGraph)
            {

                KeyValuePair<Vector3, Vector3> originEdges = sn.GetMeshEdges(sn.width);

                sn.AddNeighbour(new StreetNode(originEdges.Key, NodeType.VirtualNode));
                sn.AddNeighbour(new StreetNode(originEdges.Value, NodeType.VirtualNode));

                if (osmParser != null)
                {
                    MapRasterizer.Instance.AddMapPoint(new Vector3(originEdges.Key.x, 0, originEdges.Key.z), originEdges.Key.y);
                    MapRasterizer.Instance.AddMapPoint(new Vector3(originEdges.Value.x, 0, originEdges.Value.z), originEdges.Value.y);
                }

                // Write Line
                //sw.WriteLine(sn.position.x + ";" + sn.position.y + ";" + sn.position.z + ";" + sn.width + ";");




                foreach (StreetNode neighbour in sn.neighbours)
                {

                    if ((neighbour.type == NodeType.MergedNode || neighbour.type == NodeType.UnmergedSingleLaneNode) && !sn.neighboursWithMeshes.ContainsKey(neighbour))
                    {
                        KeyValuePair<Vector3, Vector3> neighbourEdges = neighbour.GetMeshEdges(neighbour.width);


                        float originSign = (neighbour.position.x - sn.position.x) * (originEdges.Key.z - sn.position.z) - (neighbour.position.z - sn.position.z) * (originEdges.Key.x - sn.position.x);
                        float neighbourSign = (neighbour.position.x - sn.position.x) * (neighbourEdges.Key.z - sn.position.z) - (neighbour.position.z - sn.position.z) * (neighbourEdges.Key.x - sn.position.x);

                        if (originSign * neighbourSign > 0)
                        {
                            neighbourEdges = new KeyValuePair<Vector3, Vector3>(neighbourEdges.Value, neighbourEdges.Key);
                        }

                        //GameObject streetPlane = GenerateStreetMesh(originEdges.Value, neighbourEdges.Value,originEdges.Key, neighbourEdges.Key, mergeRange);
                        //GameObject streetPlane = DrawRoad(neighbourEdges.Key, neighbourEdges.Value, originEdges.Key, originEdges.Value, parent);
                        List<LaneSegment> lanes = GetSegments(sn, neighbour);


                        //Annahme: Straße ist doppelt so lang wie breit. Erwünschter wert im scale ist 2
                        float roadLength = Vector3.Distance(sn.position, neighbour.position) / (sn.width);



                        roadLength /= 3;
                        GameObject streetPlane = DrawRoad(neighbourEdges.Key, originEdges.Value, originEdges.Key, neighbourEdges.Value, roadLength, lastDigits, parent, sn.LaneID, sn.lanecount);

                        streetPlane.isStatic = true;
                        lastDigits += roadLength - ((int)roadLength);
                        lastDigits = lastDigits - ((int)lastDigits);

                        StreetSegmentMesh seg = streetPlane.AddComponent<StreetSegmentMesh>();
                        seg.lanes = lanes;

                        seg.enabled = true;

                        MeshCollider mc = streetPlane.AddComponent<MeshCollider>();

                        mc.material = PhysicMaterial.Instantiate(roadPhysicMaterial);

                        if (!sn.neighboursWithMeshes.ContainsKey(neighbour))
                        {
                            sn.neighboursWithMeshes.Add(neighbour, seg);
                        }
                        if (!neighbour.neighboursWithMeshes.ContainsKey(sn))
                        {
                            neighbour.neighboursWithMeshes.Add(sn, seg);
                        }


                    }
                    /*else if (neighbour.type==NodeType.VirtualNode) {
                        GameObject testcube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        testcube.transform.position = neighbour.position;
                        testcube.transform.localScale = new Vector3(0.3f,0.3f,0.3f);
                        testcube.GetComponent<MeshRenderer>().material.color = nodeColor;
                        testcube.name = "VirtualNode_" + sn.id;
                    }*/

                }
                if (StreetNode.DO_DEBUG && StreetNode.DEBUG_BOXES)
                {
                    if (sn.type == NodeType.MergedNode)
                    {
                        int neighbourcount = 0;
                        foreach (StreetNode neigh in sn.neighbours)
                        {
                            if (neigh.type == NodeType.MergedNode)
                            {
                                neighbourcount++;
                            }
                        }
                        if (neighbourcount == 2)
                        {
                            GameObject testcube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            testcube.transform.position = sn.position;
                            testcube.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                            testcube.GetComponent<MeshRenderer>().material.color = Color.green;
                            testcube.name = "MergedNode_2Neighbours_" + sn.id;
                        }
                        else if (neighbourcount > 2)
                        {
                            GameObject testcube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            testcube.transform.position = sn.position;
                            testcube.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                            testcube.GetComponent<MeshRenderer>().material.color = Color.blue;
                            testcube.name = "MergedNode_MultiNeighbours_" + sn.id;
                        }
                        else if (neighbourcount < 2)
                        {
                            GameObject testcube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            testcube.transform.position = sn.position;
                            testcube.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                            testcube.GetComponent<MeshRenderer>().material.color = Color.yellow;
                            testcube.name = "MergedNode_InsufficientNeighbours_" + sn.id;
                        }
                    }
                    else if (sn.type == NodeType.UnmergedSingleLaneNode)
                    {
                        GameObject testcube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        testcube.transform.position = sn.position;
                        testcube.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                        testcube.GetComponent<MeshRenderer>().material.color = Color.red;
                        testcube.name = "SingleLaneNode_" + sn.id;
                    }
                    else if (sn.type == NodeType.OriginalNode)
                    {
                        GameObject testcube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        testcube.transform.position = sn.position;
                        testcube.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                        testcube.GetComponent<MeshRenderer>().material.color = Color.magenta;
                        testcube.name = "OriginalNode_" + sn.id;
                    }
                }
                // }
            }
        }

        /* 
         * This function is used to determine the path of the speed signs.
         * First it converts the m/s to km/h.
         * Then it uses the floor function to always use the nearest full decimal value.
         */
        private string DetermineSpeedSignPath(StreetNode sn)
        {
            double km_h = (sn.speed * 18) / 5;
            int speedLimit = Convert.ToInt32(km_h);
            if (StreetNode.DO_DEBUG)
            {
                UnityEngine.Debug.Log("Max Speed = " + speedLimit);
            }
            string pathToSign = speedLimit.ToString();
            return pathToSign;
        }


        public void InstantiateSigns()
        {
            foreach (StreetNode sn in signNodes)
            {
                PlaceSignRelativeToNode(sn, 0, 0, -(sn.width*2));
            }
        }

        /*
         * Go through every street node and work with those who need  street sign
         * First we build a coordinate system around every node that needs a street sign.
         * X: Points in the driving direction
         * Y: Points upwards
         * Z: Points to the right (away from street center)
         * We start with the x-axis and use the Neighbour Node to determine the driving direction.
         * The neighbour node is the next one in each lane, so the first node of each lane has exactly one neighbour.
         * The other coordinate axes will be calculated as orthogonal vectors
         * We might need to correct the axis if they point towards the wrong direction.
         * 
         * We use the raw graph for this.
         * The raw graph consists out of two lanes, one for each driving direction. 
         * 
         * To rotate the sign we also build a coordinate system around the sign
         * Then we determine the difference between both coordinate systems.
         * The sign will be rotated by the difference.
         */
        private void PlaceSignRelativeToNode(StreetNode sn, float xOffset, float yOffset, float zOffset)
        {
            /* Build the coordinate system around each noed */
            // The Center is the position of the node
            Vector3 nodeCoordinateSystem_Center = sn.position;
            // x-Axis will point towards the driving direction
            // The neighbour node is the next node in the driving direction
            Vector3 nodeCoordinateSystem_X = sn.neighbours.First().position - nodeCoordinateSystem_Center;

            // Now we calculate the y and z axis as orthogonal vectors
            // The length of those vectors will be normalized 
            Vector3 nodeCoordinateSystem_Y = Vector3.up;
            Vector3 nodeCoordinateSystem_Z = new Vector3(nodeCoordinateSystem_X.z, 0, -nodeCoordinateSystem_X.x); // 90 degeree vector through replacing x with z and z with -x
            Vector3.OrthoNormalize(ref nodeCoordinateSystem_X, ref nodeCoordinateSystem_Y, ref nodeCoordinateSystem_Z);

            /* Correct the axis direction if needed */
            // Every y Axis shall look upwards
            if (nodeCoordinateSystem_Y.y < 0)
            {
                nodeCoordinateSystem_Y.y *= -1;
            }

            // When the z-axis points to the center of the street we need to invert it
            // The angle between the z and x axis can be used to determine that.
            // It's +/- 90 degree
            float angle_X_Z = Vector3.SignedAngle(nodeCoordinateSystem_X, nodeCoordinateSystem_Z, nodeCoordinateSystem_Center);
            if (angle_X_Z != -90 || angle_X_Z != 90)
            {
                nodeCoordinateSystem_Z.x *= -1;
                nodeCoordinateSystem_Z.z *= -1;
            }

            //~ // Draw the coordinate system    
            //DrawLine(nodeCoordinateSystem_Center, nodeCoordinateSystem_Center+3*nodeCoordinateSystem_X, Color.red);
            //DrawLine(nodeCoordinateSystem_Center, sn.position+3*nodeCoordinateSystem_Y, Color.green);
            //DrawLine(nodeCoordinateSystem_Center, sn.position+3*nodeCoordinateSystem_Z, Color.blue);

            /* Debug. Draw the coordinate system and place cubes for each node with street sign and neighbour */
            // Place a cube at the node position that needs the street sign
            //~ GameObject signNode = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //~ signNode.transform.position = sn.position;


            //~ // Place a cube on the first neighbour
            //~ foreach(StreetNode nb in sn.neighbours)
            //~ {
            //~ GameObject n = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //~ n.transform.position = nb.position;
            //~ }
            /* End Debug */

            // Create the sign
            GameObject sign = Instantiate(streetSignPrefab) as GameObject;

            //UnityEngine.Debug.Log("Sign could not detect terrain under itself to settle");
            sign.transform.position = sn.position;
            sign.transform.position += zOffset * nodeCoordinateSystem_Z;
            sign.transform.position += xOffset * nodeCoordinateSystem_X;

            /* Rotate the sign */
            // Create the sign coordinate system
            Vector3 signCoordinateSystem_Center = sign.transform.position;
            Vector3 signCoordinateSystem_Z = nodeCoordinateSystem_Center - signCoordinateSystem_Center;
            Vector3 signCoordinateSystem_X = new Vector3(signCoordinateSystem_Z.z, 0, -signCoordinateSystem_Z.x);
            Vector3 signCoordinateSystem_Y = new Vector3(0, 1, 0);

            // The length of those vectors will be normalized 
            Vector3.OrthoNormalize(ref signCoordinateSystem_X, ref signCoordinateSystem_Y, ref signCoordinateSystem_Z);

            /* Correct the axis direction if needed */
            // Every y Axis shall look upwards
            if (signCoordinateSystem_Y.y < 0)
            {
                signCoordinateSystem_Y.y *= -1;
            }

            //Debug
            //DrawLine(signCoordinateSystem_Center, signCoordinateSystem_Center+3*signCoordinateSystem_X, Color.red);
            //DrawLine(signCoordinateSystem_Center, signCoordinateSystem_Center + 3*signCoordinateSystem_Y, Color.yellow);
            //DrawLine(signCoordinateSystem_Center, signCoordinateSystem_Center + 3*signCoordinateSystem_Z, Color.blue);

            //let the sign look to the street center 
            sign.transform.LookAt(nodeCoordinateSystem_Center, Vector3.up);
            // Rotate the sign by 90 degrees
            sign.transform.Rotate(signCoordinateSystem_Y, 90);

            /* Set the sign text */
            // Check wheter we need to display the street name
            bool streetNameExists = DisplayStreetNameIfPresent(sn, sign);

            // Check wheter a speed limit is set
            bool speedLimitExists = DisplaySpeedLimitIfPresent(sn, sign);

            DisableSignIfNoInformationIsSet(sign, streetNameExists, speedLimitExists);

            //Delete sign when it is placed at the street or to far away             
            Vector2 distance = new Vector2(signCoordinateSystem_Center.x, signCoordinateSystem_Center.z);
            if (DistanceToStreet(distance) < 0.5f)
            {
                sign.transform.gameObject.SetActive(false);  //delete sign
            }
            /*              
               if (sign.transform.childCount != 0)
               {
                   for (int i = 0; i < sign.transform.childCount; i++)
                   {
                      , ;
                       Assets.Scripts.Chunking.ChunkManager.AddRenderer(sign.transform.GetChild(i).GetComponent<MeshRenderer>(), sign.transform.GetChild(i).position, false);

                   }
               }

            */
            //Adding the signs to ChunkMananger for deactivating them during not beeing in range
            Assets.Scripts.Chunking.ChunkManager.AddRenderer(sign.transform.Find("PolePart1").gameObject.GetComponent<MeshRenderer>(), sign.transform.Find("PolePart1").position, false);
            Assets.Scripts.Chunking.ChunkManager.AddRenderer(sign.transform.Find("GroundPlate").gameObject.GetComponent<MeshRenderer>(), sign.transform.Find("GroundPlate").position, false);
            Assets.Scripts.Chunking.ChunkManager.AddRenderer(sign.transform.Find("PolePart2").gameObject.GetComponent<MeshRenderer>(), sign.transform.Find("PolePart2").position, false);
            Assets.Scripts.Chunking.ChunkManager.AddRenderer(sign.transform.Find("SpeedElement").gameObject.GetComponent<MeshRenderer>(), sign.transform.Find("SpeedElement").position, false);


        }

        private bool DisplayStreetNameIfPresent(StreetNode sn, GameObject sign)
        {
            if (sn.streetName == "")
            {
                sign.transform.Find("StreetNameElement").gameObject.SetActive(false);
                return false;
            }
            else
            {
                GameObject streetNameElement = sign.transform.Find("StreetNameElement").gameObject;
                GameObject streetName = streetNameElement.transform.Find("StreetName").gameObject;
                if (streetName != null)
                {
                    streetName.GetComponent<TextMesh>().text = sn.streetName;
                }

                return true;
            }
        }

        private bool DisplaySpeedLimitIfPresent(StreetNode sn, GameObject sign)
        {
            if (sn.speed == -1)
            {
                sign.transform.Find("PolePart2").gameObject.SetActive(false);
                sign.transform.Find("SpeedElement").gameObject.SetActive(false);

                // If neither information is needed we don't show any sign


                return false;
            }
            else
            {
                GameObject speedElement = sign.transform.Find("SpeedElement").gameObject;

                if (speedElement != null)
                {
                    int speedInKmH = Convert.ToInt32(sn.speed * 3.6);
                    Material mat = null;
                    speedMaterials.TryGetValue(speedInKmH, out mat);
                    if (mat != null)
                    {
                        Material[] mats = speedElement.GetComponent<MeshRenderer>().materials;
                        mats[1] = mat;
                        speedElement.GetComponent<MeshRenderer>().materials = mats;
                    }
                }

                return true;
            }
        }

        private void DisableSignIfNoInformationIsSet(GameObject sign, bool streetNameExists, bool speedLimitExists)
        {
            if (streetNameExists == false && speedLimitExists == false)
            {
                sign.transform.Find("PolePart1").gameObject.SetActive(false);
                sign.transform.Find("GroundPlate").gameObject.SetActive(false);
            }
        }

        private List<LaneSegment> GetSegments(StreetNode n1, StreetNode n2)
        {
            List<LaneSegment> segments = new List<LaneSegment>();

            foreach (StreetNode sn in n1.neighbours)
            {
                if (sn.type != NodeType.MergedNode && sn.type != NodeType.VirtualNode)
                {
                    foreach (StreetNode other in n2.neighbours)
                    {
                        if (other.type != NodeType.MergedNode && other.type != NodeType.VirtualNode)
                        {
                            if (sn.neighbours.Contains(other))
                            {
                                LaneSegment rs = new LaneSegment();
                                rs.edgeIndex = sn.EdgeIndex;
                                rs.laneId = sn.LaneID;
                                segments.Add(rs);
                                rs = new LaneSegment();
                                rs.edgeIndex = other.EdgeIndex;
                                rs.laneId = other.LaneID;
                                segments.Add(rs);
                            }
                        }
                    }
                }
            }

            return segments;
        }

        private void DrawLine(Vector3 start, Vector3 end, Transform parent)
        {
            GameObject line = new GameObject();
            line.isStatic = true;
            line.transform.position = start;
            line.AddComponent<LineRenderer>();
            line.transform.parent = parent;
            line.name = "line";
            LineRenderer lr = line.GetComponent<LineRenderer>();
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0.1f, 0.1f);
            lr.widthCurve = curve;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }

        public static void DrawLine(Vector3 start, Vector3 end, Color cl)
        {
            GameObject line = new GameObject();
            line.isStatic = true;
            line.transform.position = start;
            line.AddComponent<LineRenderer>();
            line.name = "line";
            LineRenderer lr = line.GetComponent<LineRenderer>();
            lr.startColor = cl;
            lr.endColor = cl;
            lr.material.color = cl;
            lr.material.SetColor("_EmissionColor", cl);
            lr.material.EnableKeyword("_EMISSION");
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0.05f, 0.05f);
            lr.widthCurve = curve;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }

        /// <summary>
        /// Instanciates a new Mesh with an appropriate texture (depending on the lane count) of given start and end edges
        /// </summary>
        /// <param name="startA">First edge vector of new road segement mesh start</param>
        /// <param name="startB">Second edge vector of new road segement mesh start</param>
        /// <param name="endA">First edge vector of new road segement mesh end</param>
        /// <param name="endB">Second edge vector of new road segement mesh end</param>
        /// <param name="roadLength">expected Length of new road segment</param>
        /// <param name="textureOffset">Offset of the UV texture coordinates</param>
        /// <param name="parent">Transform that contains the new instance for the street mesh</param>
        /// <param name="customName">Name for the GameObject</param>
        /// <param name="lanecount">Amount of Lanes in the street mesh</param>
        /// <returns></returns>
        GameObject DrawRoad(Vector3 startA, Vector3 startB, Vector3 endA, Vector3 endB, float roadLength, float textureOffset, Transform parent, string customName, int lanecount = 2)
        {
            GameObject roadPath = new GameObject();
            roadPath.name = "road_segment_" + customName;

            roadPath.isStatic = true;


            Mesh mesh;

            //float scaleX = Mathf.Cos(Time.time) * 0.5F + 1;
            //float scaleY = Mathf.Sin(Time.time) * 0.5F + 1;

            Material streetMaterialCopy;
            switch (lanecount)
            {
                case 1:
                    streetMaterialCopy = Material.Instantiate(roadMaterialSingleLane);
                    break;
                case 2:
                    streetMaterialCopy = Material.Instantiate(this.roadMaterial);
                    break;
                case 3:
                    streetMaterialCopy = Material.Instantiate(this.roadMaterialTrippleLane);
                    break;
                case 4:
                    streetMaterialCopy = Material.Instantiate(this.roadMaterialQuadLane);
                    break;
                default:
                    streetMaterialCopy = Material.Instantiate(roadMaterial);
                    break;
            }
            streetMaterialCopy.mainTextureScale = new Vector2(1f, roadLength);
            streetMaterialCopy.mainTextureOffset = new Vector2(0, textureOffset);

            roadPath.AddComponent<MeshFilter>().mesh = mesh = new Mesh();
            roadPath.AddComponent<MeshRenderer>().material = streetMaterialCopy;
            roadPath.transform.parent = parent;

            //float theta = (start.z - end.z) / (start.x - end.x);
            //UnityEngine.Debug.Log(theta);
            Vector3[] vertices = new Vector3[4];


            //Vector3 lot = new Vector3(start.z - end.z, 0, -(start.x - end.x));
            //lot.Normalize();

            /*if (theta < 0 || true)
            {
                vertices[0] = start + lot * (roadWidth / 2);
                vertices[1] = start + lot * (-roadWidth / 2);
                vertices[2] = end + lot * (-roadWidth / 2);
                vertices[3] = end + lot * (roadWidth / 2); ;
                mesh.vertices = vertices;
            }
            else
            {
                vertices[0] = new Vector3(start.x, 0, (start.z - (roadWidth / 2)));
                vertices[1] = new Vector3(start.x, 0, (start.z + (roadWidth / 2)));
                vertices[2] = new Vector3(end.x, 0, (end.z + (roadWidth / 2)));
                vertices[3] = new Vector3(end.x, 0, (end.z - (roadWidth / 2)));
                mesh.vertices = vertices;
            }*/
            vertices[0] = startA;
            vertices[1] = startB;
            vertices[2] = endA;
            vertices[3] = endB;
            mesh.vertices = vertices;

            //Vector4[] tangents = new Vector4[mesh.vertices.Length];
            //Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);

            //Create UVs
            Vector2[] uvs = new Vector2[4];
            //UnityEngine.Debug.Log("UV Amount: "+uvs.Length);

            //uvs[0] = new Vector2(0, 0); //bottom-left
            //uvs[1] = new Vector2(1f, 0); //top-left
            //uvs[2] = new Vector2(1f, 1f); //top-right
            //uvs[3] = new Vector2(0, 1f); //bottom-right

            uvs[0] = new Vector2(0, 1f); //bottom-left
            uvs[1] = new Vector2(0, 0); //top-left
            uvs[2] = new Vector2(1f, 0); //top-right
            uvs[3] = new Vector2(1f, 1f); //bottom-right


            // Create rectangle
            int[] triangles = new int[6];
            triangles[5] = triangles[2] = 0;
            triangles[1] = 1;
            triangles[4] = triangles[0] = 2;
            triangles[3] = 3;
            mesh.triangles = triangles;

            // Assign uvs
            mesh.uv = uvs;

            // Assing tangents
            //mesh.tangents = tangent;

            // Assing triangles
            mesh.triangles = triangles;

            // Recalculate bounds 
            mesh.RecalculateBounds();

            // Recalculate normals
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            Assets.Scripts.Chunking.ChunkManager.AddRenderer(roadPath.GetComponent<MeshRenderer>(), (startA + startB + endA + endB) / 4, false);

            return roadPath;
        }

        public GameObject GenerateStreetMesh(Vector3 startA, Vector3 startB, Vector3 endA, Vector3 endB, float width)
        {
            Vector3 r1 = startA;
            Vector3 l1 = startB;

            Vector3 r2 = endA;
            Vector3 l2 = endB;

            Vector3 origin = l1;

            GameObject streetPlane = GameObject.Instantiate(Resources.Load("PlanePrefab")) as GameObject;
            streetPlane.isStatic = true;
            streetPlane.transform.localScale = new Vector3(1, -1, 1);
            streetPlane.transform.position = origin;

            MeshFilter mf = streetPlane.GetComponent<MeshFilter>();
            Vector3[] verts = mf.mesh.vertices;

            //UnityEngine.Debug.Log("verts length "+verts.Length);

            verts[2] = r1 - origin;
            verts[1] = r2 - origin;
            verts[3] = l1 - origin;
            verts[0] = l2 - origin;
            mf.mesh.vertices = verts;

            Vector3[] normals = mf.mesh.normals;

            // edit the normals in an external array
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = new Vector3(0, 0, -1);
            }
            //mf.mesh.RecalculateNormals();
            //mf.mesh.RecalculateTangents();

            // assign the array of normals to the mesh
            mf.mesh.normals = normals;

            mf.mesh.RecalculateBounds();//This should fix all frustum culling issues!
            //Mesh Collider the car should be able to drive on
            MeshCollider mc = streetPlane.AddComponent<MeshCollider>();
            mc.sharedMesh = mf.mesh;
            return streetPlane;
        }

        /*
        public IDictionary<string, IList<Vector3>> SubdivideStreetNetBSpline(IDictionary<string, IList<Vector3>> streetNet, int loops)
        {
            IDictionary<string, IList<Vector3>> subStreetNet = new Dictionary<string, IList<Vector3>>();

            foreach (string laneId in streetNet.Keys)
            {
                List<Vector3> lane = streetNet[laneId].ToList();
                IList<Vector3> subLane;
                BSpline bezier = new BSpline(loops,lane);
                subLane = bezier.GetSubdividedPoints();
                subStreetNet.Add(laneId,subLane);
            }

            return subStreetNet;
        }
        */

        /// <summary>
        /// Creates smoother curves by a adding aditional points to a given list of vectors using a Catmull-Rom spline for interpolation
        /// </summary>
        /// <param name="streetNet"></param>
        /// <param name="loops">Number of segments to compute</param>
        /// <returns></returns>
        public IDictionary<string, IList<Vector3>> SubdivideStreetNet(IDictionary<string, IList<Vector3>> streetNet, int loops)
        {
            IDictionary<string, IList<Vector3>> subStreetNet = new Dictionary<string, IList<Vector3>>();

            foreach (string laneId in streetNet.Keys.ToList())
            {
                IList<Vector3> lane = streetNet[laneId];
                List<Vector3> subLane = new List<Vector3>();
                if (lane.Count > 2)
                {
                    Vector3 p0 = lane[0];
                    Vector3 p1 = lane[0];
                    Vector3 p2 = lane[1];
                    Vector3 p3 = lane[2];

                    //remove height component so it's not curved, we want linear interpolation for height, centripetal for x and z 
                    Vector3 p0flat = new Vector3(p0.x, 0, p0.z);
                    Vector3 p1flat = new Vector3(p1.x, 0, p1.z);
                    Vector3 p2flat = new Vector3(p2.x, 0, p2.z);
                    Vector3 p3flat = new Vector3(p3.x, 0, p3.z);

                    IList<Vector3> interpolatedPoints = CentripetalCatmullRom(loops, p0flat, p1flat, p2flat, p3flat);
                    //Manipulate height of interpolated points
                    float startingHeight = p1.y;
                    float endHeight = p2.y;
                    float heightdiff = endHeight - startingHeight;
                    float factor = 0;
                    if (StreetNode.DO_DEBUG)
                    {
                        UnityEngine.Debug.Log("Start and End Heights: " + startingHeight + " , " + endHeight);
                    }
                    for (int i = 0; i < interpolatedPoints.Count; i++)
                    {
                        factor = i / ((float)interpolatedPoints.Count - 1);
                        float height = startingHeight + (factor * heightdiff);
                        interpolatedPoints[i] = new Vector3(interpolatedPoints[i].x, height, interpolatedPoints[i].z);
                    }
                    subLane.AddRange(interpolatedPoints);
                    for (int i = 3; i < lane.Count; i++)
                    {
                        p0 = p1;
                        p1 = p2;
                        p2 = p3;
                        p3 = lane[i];

                        p0flat = p1flat;
                        p1flat = p2flat;
                        p2flat = p3flat;
                        p3flat = new Vector3(p3.x, 0, p3.z);

                        interpolatedPoints = CentripetalCatmullRom(loops, p0flat, p1flat, p2flat, p3flat);
                        //Manipulate height of interpolated points
                        startingHeight = p1.y;
                        endHeight = p2.y;
                        heightdiff = endHeight - startingHeight;
                        factor = 0;
                        if (StreetNode.DO_DEBUG)
                        {
                            UnityEngine.Debug.Log("Start and End Heights: " + startingHeight + " , " + endHeight);
                        }
                        for (int j = 0; j < interpolatedPoints.Count; j++)
                        {
                            factor = j / ((float)interpolatedPoints.Count - 1);
                            float height = startingHeight + (factor * heightdiff);
                            interpolatedPoints[j] = new Vector3(interpolatedPoints[j].x, height, interpolatedPoints[j].z);
                        }
                        subLane.AddRange(interpolatedPoints);
                    }
                    interpolatedPoints = CentripetalCatmullRom(loops, p1flat, p2flat, p3flat, p3flat);
                    //Manipulate height of interpolated points
                    startingHeight = p1.y;
                    endHeight = p2.y;
                    heightdiff = endHeight - startingHeight;
                    factor = 0;
                    if (StreetNode.DO_DEBUG)
                    {
                        UnityEngine.Debug.Log("Start and End Heights: " + startingHeight + " , " + endHeight);
                    }
                    for (int j = 0; j < interpolatedPoints.Count; j++)
                    {
                        factor = j / ((float)interpolatedPoints.Count - 1);
                        float height = startingHeight + (factor * heightdiff);
                        interpolatedPoints[j] = new Vector3(interpolatedPoints[j].x, height, interpolatedPoints[j].z);
                    }
                    subLane.AddRange(interpolatedPoints);
                    subLane.Add(p3);
                }
                else
                {
                    subLane.AddRange(lane);
                }
                subStreetNet.Add(laneId, subLane);
            }
            return subStreetNet;
        }

        /// <summary>
        /// Computes a Certripetal Catmull-Rom spline between the points p1 and p2 given the four points p0 to p3.
        /// Compare https://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline#Code_example
        /// </summary>
        /// <param name="loops">Number of segments to compute, at least one</param>
        /// <returns>Returns the new created vectors, including p1 and p2</returns>
        private IList<Vector3> CentripetalCatmullRom(int loops, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            IList<Vector3> newPoints = new List<Vector3>();

            float alpha = 0.5f;
            float t0 = 0.0f;
            float t1 = GetT(alpha, t0, p0, p1);
            float t2 = GetT(alpha, t1, p1, p2);
            float t3 = GetT(alpha, t2, p2, p3);

            if (loops < 1)
            {
                loops = 1;
            }

            float t = t1;
            for (int i = 0; i <= loops - 1; i++)
            {
                // If two points are at the same position, a division by zero is the result, so we have to check this
                Vector3 a1 = p0.Equals(p1) ? p0 : (t1 - t) / (t1 - t0) * p0 + (t - t0) / (t1 - t0) * p1;
                Vector3 a2;
                if (!p1.Equals(p2))
                {
                    a2 = (t2 - t) / (t2 - t1) * p1 + (t - t1) / (t2 - t1) * p2;
                }
                else
                {
                    UnityEngine.Debug.LogWarning("insuficient arguments: p1 equals p2 -> no curve computable");
                    newPoints.Add(p1);
                    return newPoints;
                }
                Vector3 a3 = p2.Equals(p3) ? p2 : (t3 - t) / (t3 - t2) * p2 + (t - t2) / (t3 - t2) * p3;

                Vector3 b1 = (t2 - t) / (t2 - t0) * a1 + (t - t0) / (t2 - t0) * a2;
                Vector3 b2 = (t3 - t) / (t3 - t1) * a2 + (t - t1) / (t3 - t1) * a3;

                Vector3 c = (t2 - t) / (t2 - t1) * b1 + (t - t1) / (t2 - t1) * b2;

                t += ((t2 - t1) / loops);

                newPoints.Add(c);
            }
            return newPoints;
        }

        private float GetT(float alpha, float t, Vector3 p0, Vector3 p1)
        {
            float a = Mathf.Pow((p1.x - p0.x), 2.0f) + Mathf.Pow((p1.y - p0.y), 2.0f) + Mathf.Pow((p1.z - p0.z), 2.0f);
            float b = Mathf.Pow(a, 0.5f);
            float c = Mathf.Pow(b, alpha);

            return (c + t);
        }

        /// <summary>
        /// Checks if a given list with allowed vehicleclasses contains only the pedestrian vehicleclasses
        /// </summary>
        /// <param name="lvc"></param>
        /// <returns></returns>
        public static bool IsSidewalk(List<VehicleClasses> lvc)
        {
            if (lvc == null) // check if the list is null, then we dont have a sidewalk
                return false;

            if (lvc.Count == 1 && lvc[0] == VehicleClasses.pedestrian)
                return true;

            return false;
        }


    }
}

