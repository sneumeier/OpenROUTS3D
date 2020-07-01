using Assets.Scripts.OsmParser;
using Assets.Scripts.SUMOConnectionScripts;
using Assets.Scripts.SUMOConnectionScripts.Maps;
using SUMOConnectionScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.SUMOConnectionScripts
{
    public class StreetNode
    {
        public const bool DO_DEBUG = false;
        public const bool DEBUG_LINES = false;
        public const bool DEBUG_BOXES = false;
        public static List<string> BlacklistedStreetTypes = new List<string> { "footway", "cycleway" };

        public static int Next_ID = 0;

        public int HopsToEnd = 0;

        public int lanecount = 1;
        public int id;
        public bool generate = true;

        public string osmIdentifier;
        public OsmWay osmWay = null;

        public List<StreetNode> neighbours = new List<StreetNode>();
        public Dictionary<StreetNode, StreetSegmentMesh> neighboursWithMeshes = new Dictionary<StreetNode, StreetSegmentMesh>();
        public List<int> neighbourIDs = new List<int>();
        public Vector3 position;
        public NodeType type;
        public bool hasMerged = false;
        public bool hasModel = false;
        public float width = 1.75f;

        public bool needsStreetSign;
        public float speed = -1;
        public string streetName = "";

        public StreetNode unoptimizedNeighbourNode = null;

        public int EdgeIndex;
        public string LaneID;

        public StreetNode(Vector3 position, NodeType type)
        {
            this.position = position;
            this.type = type;
            id = Next_ID++;
        }

        public List<KeyValuePair<Vector2, StreetNode>> GetEdges2D()
        {
            List<KeyValuePair<Vector2, StreetNode>> edges = new List<KeyValuePair<Vector2, StreetNode>>();
            int ncount = 0;//If only one neighbour, mirror the edge to the other side, so colliders work even at lane edges
            foreach (StreetNode neighbour in neighbours)
            {
                if (neighbour.type == NodeType.OriginalNode || neighbour.type == NodeType.OriginalCrossroadNode || neighbour.type == NodeType.UnmergedSingleLaneNode)
                {
                    Vector3 v = neighbour.position - position;
                    edges.Add(new KeyValuePair<Vector2, StreetNode>(new Vector2(v.x, v.z), neighbour));
                    ncount++;
                }
            }
            if (ncount == 1)
            {
                KeyValuePair<Vector2, StreetNode> pair = new KeyValuePair<Vector2, StreetNode>(edges.First().Key.normalized, edges.First().Value);

                edges.Add(new KeyValuePair<Vector2, StreetNode>(-0.4f * pair.Key, pair.Value));
            }
            return edges;
        }

        public StreetNode PrimitiveMerge(List<StreetNode> nearbyNodes, float sumoStreetwidth)
        {
            if (this.hasMerged) { return null; }
            Dictionary<string, StreetNode> relevantMergepoints = new Dictionary<string, StreetNode>();
            relevantMergepoints.Add(this.LaneID, this);
            foreach (StreetNode sn in nearbyNodes)
            {
                if (sn == this || sn.type != NodeType.OriginalNode || sn.hasMerged)
                {
                    continue;
                }
                //One has an OSM way, the other doesn't
                if ((sn.osmWay != null && this.osmWay == null) || (sn.osmWay == null && this.osmWay != null))
                {
                    continue;
                }
                //Make sure you have the correct street to merge
                if (sn.osmWay != null && (sn.osmWay.street_name != osmWay.street_name || sn.osmWay.highway != osmWay.highway))
                {
                    continue;
                }


                if (sn.HopsToEnd == this.HopsToEnd)
                {
                    if (relevantMergepoints.ContainsKey(sn.LaneID))
                    {
                        if (Vector3.Distance(relevantMergepoints[sn.LaneID].position, this.position) > Vector3.Distance(sn.position, this.position))
                        {
                            relevantMergepoints[sn.LaneID] = sn;
                        }
                    }
                    else
                    {
                        relevantMergepoints.Add(sn.LaneID, sn);
                    }

                }
            }

            StreetNode unifiedNode = new StreetNode(Vector3.zero, NodeType.MergedNode);
            unifiedNode.generate = this.generate;
            unifiedNode.LaneID = this.LaneID;
            unifiedNode.osmIdentifier = this.osmIdentifier;
            Vector3 location = Vector3.zero;
            int amount = 0;
            unifiedNode.width = 0;
            float lanewidth = 0;
            bool hasUnsmoothedNeighbour = false;
            Vector3 unsmoothedNeighbourLocation = Vector3.zero;
            int unsmoothedNeighboursAmount = 0;
            foreach (var kvp in relevantMergepoints)
            {
                kvp.Value.hasMerged = true;
                location += kvp.Value.position;
                amount++;
                if (kvp.Value.unoptimizedNeighbourNode != null)
                {
                    hasUnsmoothedNeighbour = true;
                    unsmoothedNeighbourLocation += kvp.Value.unoptimizedNeighbourNode.position;
                    unsmoothedNeighboursAmount++;
                }
                unifiedNode.AddNeighbour(kvp.Value);
                
                //unifiedNode.width += kvp.Value.width;
                foreach (var kvp2 in relevantMergepoints)
                {
                    lanewidth = Mathf.Max(lanewidth, Vector3.Distance(kvp.Value.position, kvp2.Value.position));
                }
            }
            // shapes are defined as the middle of a lane, so add one more streetwidth for the whole size
            unifiedNode.width = (lanewidth + sumoStreetwidth) / 2;
            foreach (StreetNode neigh in this.neighbours)
            {
                //Add neighbours of Original Nodes that are merged nodes to your own neighbours
                if (neigh.type != NodeType.OriginalNode)
                {
                    continue;
                }
                foreach (StreetNode nextNeigh in neigh.neighbours)
                {
                    if (!unifiedNode.neighbours.Contains(nextNeigh) && nextNeigh.type == NodeType.MergedNode && neigh != unifiedNode)
                    {
                        unifiedNode.AddNeighbour(nextNeigh);
                    }
                }
            }

            unifiedNode.lanecount = amount;
            location /= amount;
            unifiedNode.position = location;

            

            if (DO_DEBUG && DEBUG_LINES && UnityEngine.Random.Range(0, 10) == 1)
            {
                foreach (StreetNode neigh in this.neighbours)
                {
                    Color col = Color.white;
                    switch (neigh.type)
                    { 
                        case NodeType.MergedNode:
                            col = Color.cyan;
                            break;
                        case NodeType.OriginalNode:
                            col = Color.yellow;
                            break;
                        case NodeType.OriginalCrossroadNode:
                            col = new Color(1,0.6f,0f);
                            break;
                        case NodeType.VirtualNode:
                            continue;
                        case NodeType.UnmergedSingleLaneNode:
                            col = Color.red;
                            break;


                    }

                    SumoStreetImporter.DrawLine(neigh.position, location, col);
                }
                foreach (var kvp in relevantMergepoints)
                {
                    SumoStreetImporter.DrawLine(kvp.Value.position, location, Color.magenta);
                }
            }
            
            if (hasUnsmoothedNeighbour)
            {
                unifiedNode.unoptimizedNeighbourNode = new StreetNode(unsmoothedNeighbourLocation / unsmoothedNeighboursAmount, NodeType.VirtualNode);
            }


            if (DO_DEBUG && DEBUG_BOXES)
            {
                Debug.LogWarning(amount.ToString() + " points merged to " + location);

                foreach (StreetNode n in unifiedNode.neighbours)
                {
                    GameObject dbcube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    dbcube.GetComponent<MeshRenderer>().material.color = Color.cyan;
                    dbcube.transform.position = n.position;
                    dbcube.name = n.id + "_neigh";
                    dbcube.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                }

                GameObject debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                debugCube.GetComponent<MeshRenderer>().material.color = Color.gray;
                debugCube.transform.position = location;
                debugCube.name = unifiedNode.id + "_merged";
                debugCube.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            }

            this.hasMerged = true;
            unifiedNode.hasMerged = true;

            return unifiedNode;

        }

        public float DistanceToPoint(Vector2 point)
        {
            float dist = float.MaxValue;
            foreach (StreetNode sn in neighbours)
            {
                if (sn.type == NodeType.OriginalNode)
                {
                    Vector2 start = new Vector2(sn.position.x, sn.position.z);
                    Vector2 end = new Vector2(this.position.x, this.position.z);
                    Vector2 line = (end - start);
                    Vector2 projection = Vector3.Project((point - start), line.normalized);
                    Vector2 projectionPoint = projection + start;
                    dist = Mathf.Min(Vector2.Distance(projectionPoint, point), dist);
                    dist = Mathf.Min(dist, Vector2.Distance(start, point), Vector2.Distance(end, point));

                }
            }

            return dist;

        }

        public List<StreetNode> Hitscan(float distance, List<StreetNode> nearbyNodes)
        {
            List<StreetNode> hitNodes = new List<StreetNode>();

            int nCount = 0;
            StreetNode neighA = null;
            StreetNode neighB = null;

            //Debug.LogWarning("Nearby Nodes: " + nearbyNodes.Count);

            foreach (StreetNode neighbour in neighbours)
            {
                if (neighbour.type == NodeType.OriginalNode || neighbour.type == NodeType.OriginalCrossroadNode || neighbour.type == NodeType.UnmergedSingleLaneNode)
                {
                    nCount++;
                    if (neighA == null)
                    {
                        neighA = neighbour;
                    }
                    else
                    {
                        neighB = neighbour;
                    }
                }
            }

            if (nCount != 2 && nCount != 1)
            {
                Debug.Log("nCount not 2 or 1");
                hasMerged = true;
                return null;
            }

            Vector2 tangentStart = Vector2.zero;
            Vector2 tangentEnd = Vector2.zero;
            Vector2 tangentDirection = Vector2.zero;

            Vector2 perpendicularStart = Vector2.zero;
            Vector2 perpendicularEnd = Vector2.zero;

            if (nCount == 2)//nCount is either 1 or 2
            {
                tangentStart = new Vector2(neighA.position.x, neighA.position.z);
                tangentEnd = new Vector2(neighB.position.x, neighB.position.z);
                tangentDirection = tangentEnd - tangentStart;

                tangentDirection.Normalize();

                perpendicularStart = new Vector2(position.x, position.z) + (new Vector2(tangentDirection.y, -tangentDirection.x).normalized * distance);
                perpendicularEnd = new Vector2(position.x, position.z) + (new Vector2(-tangentDirection.y, tangentDirection.x).normalized * distance);
            }
            else//nCount is 1
            {
                tangentStart = new Vector2(this.position.x, this.position.z);
                tangentEnd = new Vector2(neighA.position.x, neighA.position.z);
                tangentDirection = tangentEnd - tangentStart;

                tangentDirection.Normalize();

                perpendicularStart = new Vector2(position.x, position.z) + (new Vector2(tangentDirection.y, -tangentDirection.x).normalized * distance);
                perpendicularEnd = new Vector2(position.x, position.z) + (new Vector2(-tangentDirection.y, tangentDirection.x).normalized * distance);
            }
            foreach (StreetNode node in nearbyNodes)
            {
                if ((node.position - this.position).magnitude < SumoStreetImporter.autoMergeRadius)
                {
                    continue;

                }
                if (this.neighbours.Contains(node) || node == this)
                {
                    continue;//Don't wanna intersect our own borders!
                }
                Vector2 otherPosition = new Vector2(node.position.x, node.position.z);
                foreach (KeyValuePair<Vector2, StreetNode> pair in node.GetEdges2D())
                {

                    //Debug.Log("Normal: "+perpendicularStart+" to "+perpendicularEnd+"\nOther Line: "+otherPosition+ " to "+(otherPosition+pair.Key));
                    if (IsIntersecting(perpendicularStart, perpendicularEnd, otherPosition, otherPosition + pair.Key))
                    {
                        StreetNode other;
                        Debug.Log("Found intersection");
                        //Partners found, now take the node that's closer:
                        if ((pair.Value.position - position).magnitude < (node.position - position).magnitude)
                        {
                            other = pair.Value;
                        }
                        else
                        {
                            other = node;
                        }

                        hitNodes.Add(other);
                    }

                }
            }

            return hitNodes;
        }


        public KeyValuePair<Vector3, Vector3> GetMeshEdges(float width)
        {
            int nCount = 0;
            StreetNode neighA = null;
            StreetNode neighB = null;
            foreach (StreetNode neighbour in neighbours)
            {
                if (neighbour.type == NodeType.UnmergedSingleLaneNode || neighbour.type == NodeType.MergedNode)
                {
                    nCount++;
                    if (neighA == null)
                    {
                        neighA = neighbour;
                    }
                    else
                    {
                        neighB = neighbour;
                    }
                }
            }
            if (nCount == 0)
            {
                return new KeyValuePair<Vector3, Vector3>(position, position);
            }
            else if (nCount == 1)
            {
                Vector3 virtualTangentDirection = neighA.position - position;
                if (unoptimizedNeighbourNode != null)
                {
                    virtualTangentDirection = unoptimizedNeighbourNode.position - position;
                    //Debug.Log("using original location");
                }
                Vector2 perpendicularStart = new Vector2(position.x, position.z) + (new Vector2(virtualTangentDirection.z, -virtualTangentDirection.x).normalized * width);
                Vector2 perpendicularEnd = new Vector2(position.x, position.z) + (new Vector2(-virtualTangentDirection.z, virtualTangentDirection.x).normalized * width);

                return new KeyValuePair<Vector3, Vector3>(new Vector3(perpendicularStart.x, position.y, perpendicularStart.y), new Vector3(perpendicularEnd.x, position.y, perpendicularEnd.y));
            }
            else
            {
                //Check if both neighbours are on the same location. That indicates an error in the Street Net!
                //Sometimes happens with manually created street nets
                if (neighA.position == neighB.position)
                {
                    Debug.LogError("Both neighbours are on the same location! "+this.streetName);

                }

                Vector2 tangentStart = new Vector2(neighA.position.x, neighA.position.z);
                Vector2 tangentEnd = new Vector2(neighB.position.x, neighB.position.z);
                Vector2 tangentDirection = tangentEnd - tangentStart;

                Vector2 perpendicularStart = new Vector2(position.x, position.z) + (new Vector2(tangentDirection.y, -tangentDirection.x).normalized * width);
                Vector2 perpendicularEnd = new Vector2(position.x, position.z) + (new Vector2(-tangentDirection.y, tangentDirection.x).normalized * width);

                //Vector2 perpendicularStart = new Vector2(position.x, position.z) + tangentDirection.normalized * width;
                //Vector2 perpendicularEnd = new Vector2(position.x, position.z) - tangentDirection.normalized * width;

                return new KeyValuePair<Vector3, Vector3>(new Vector3(perpendicularStart.x, position.y, perpendicularStart.y), new Vector3(perpendicularEnd.x, position.y, perpendicularEnd.y));

            }
        }

        public void SortToChunk(LinkedList<GraphChunk> chunklist, float chunkwidth)
        {
            int cx = GraphChunk.FullDivision(position.x, chunkwidth);
            int cy = GraphChunk.FullDivision(position.z, chunkwidth);
            GraphChunk relevantChunk = null;
            foreach (GraphChunk c in chunklist)
            {
                if (c.x == cx && c.y == cy)
                {
                    relevantChunk = c;
                    break;
                }
            }
            if (relevantChunk == null)
            {
                relevantChunk = new GraphChunk(cx, cy);
            }
            else
            {
                chunklist.Remove(relevantChunk);//most recently used shall be on top!
            }
            chunklist.AddFirst(relevantChunk);

            relevantChunk.nodes.Add(this);
        }

        public void AddNeighbour(StreetNode n)
        {
            if (!neighbours.Contains(n))
            {
                neighbours.Add(n);
                neighbourIDs.Add(n.id);
            }
            if (!n.neighbours.Contains(this))
            {
                n.neighbours.Add(this);
                n.neighbourIDs.Add(this.id);
            }
        }

        //http://thirdpartyninjas.com/blog/2008/10/07/line-segment-intersection/
        bool IsIntersecting(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            bool isIntersecting = false;

            float denominator = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);

            //Make sure the denominator is > 0, if so the lines are parallel
            if (denominator != 0)
            {
                float u_a = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / denominator;
                float u_b = ((p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x)) / denominator;

                //Is intersecting if u_a and u_b are between 0 and 1
                if (u_a >= 0 && u_a <= 1 && u_b >= 0 && u_b <= 1)
                {
                    isIntersecting = true;
                }
            }

            return isIntersecting;
        }

        //Line Seg. Helper function from the interwebz: http://www.stefanbader.ch/faster-line-segment-intersection-for-unity3dc/
        public static bool FasterLineSegmentIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {

            Vector2 a = p2 - p1;
            Vector2 b = p3 - p4;
            Vector2 c = p1 - p3;

            float alphaNumerator = b.y * c.x - b.x * c.y;
            float alphaDenominator = a.y * b.x - a.x * b.y;
            float betaNumerator = a.x * c.y - a.y * c.x;
            float betaDenominator = a.y * b.x - a.x * b.y;

            bool doIntersect = true;

            if (alphaDenominator == 0 || betaDenominator == 0)
            {
                doIntersect = false;
            }
            else
            {

                if (alphaDenominator > 0)
                {
                    if (alphaNumerator < 0 || alphaNumerator > alphaDenominator)
                    {
                        doIntersect = false;

                    }
                }
                else if (alphaNumerator > 0 || alphaNumerator < alphaDenominator)
                {
                    doIntersect = false;
                }

                if (doIntersect && betaDenominator > 0)
                {
                    if (betaNumerator < 0 || betaNumerator > betaDenominator)
                    {
                        doIntersect = false;
                    }
                }
                else if (betaNumerator > 0 || betaNumerator < betaDenominator)
                {
                    doIntersect = false;
                }
            }

            return doIntersect;
        }

    }

    public enum NodeType
    {
        OriginalNode,
        OriginalCrossroadNode,
        MergedNode,
        UnmergedSingleLaneNode,//Merging process has finished, but hasn't found a partner, Node becomes a regular street node at it's original position
        VirtualNode,
    }
}
