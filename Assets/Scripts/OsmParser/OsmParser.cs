using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;

namespace Assets.Scripts.OsmParser
{
    /* This classs does the .osm parsing.
 * It reads an .osm file that was modified with osmosis and inclues SRTM height information for each node.
 * It contains two Dictionaries. 
 * - One contains the nodes
 * - One contains the ways
 * The Key is the id
 * The Value is the Object with the id
 */
    public class OsmParser
    {
        /* Data */
        public Dictionary<string, OsmNode> nodes = new Dictionary<string, OsmNode>();
        public Dictionary<string, OsmWay> ways = new Dictionary<string, OsmWay>();
        public Dictionary<string, Vector3> correctionPositions = new Dictionary<string, Vector3>();

        public void ParseCorrectionFile(string pathToFile)
        {
            using (XmlReader reader = XmlReader.Create(pathToFile))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            case "Node":
                                ParseCorrectionNode(reader);
                                break;
                            case "TerrainNode":
                                ParseCorrectionNode(reader);
                                break;
                        }
                    }

                }
            }
        }

        public void ParseCorrectionNode(XmlReader reader)
        {
            correctionPositions.Clear();
            string identifier = "";
            string fullId = "";
            double height = 0;
            bool replacementAvailable = false;
            float replacementX = 0;
            float replacementZ = 0;
            XmlReader inner = reader.ReadSubtree();
            while (inner.Read())
            {
                try
                {
                    switch (inner.Name)
                    {
                        case "Id":
                            identifier = inner.ReadString();
                            break;
                        case "FullId":
                            fullId = inner.ReadString();
                            break;
                        case "Height":


                            height = double.Parse(inner.ReadString(), CultureInfo.InvariantCulture);

                            break;
                        case "X":
                            replacementAvailable = true;
                            replacementX = float.Parse(inner.ReadString(), CultureInfo.InvariantCulture);
                            break;
                        case "Z":
                            replacementAvailable = true;
                            replacementZ = float.Parse(inner.ReadString(), CultureInfo.InvariantCulture);
                            break;
                    }
                }
                catch (Exception)
                {
                    Debug.LogWarning("Incorrectly formatted correction info: " + inner.Name + " " + inner.ReadString());
                }
            }

            OsmNode node = null;
            if (nodes.ContainsKey(identifier))
            {
                node = nodes[identifier];
            }
            else
            {
                Debug.LogWarning("Did not find any Nodes with the id "+identifier+", adding a new one instead of correcting an existing one");
                node = new OsmNode();
                node.id = identifier;
                nodes.Add(identifier, node);
            }
            node.height = height;
            if (!String.IsNullOrEmpty(fullId))
            {
                Vector3 correctedPosition = new Vector3(replacementX,(float)height,replacementZ);
                if (correctionPositions.ContainsKey(fullId))
                {
                    Debug.LogWarning("Two Identical Correction IDs were contained in the correction file");
                    correctionPositions[fullId] = correctedPosition;
                }
                else
                {
                    correctionPositions.Add(fullId,correctedPosition);
                }
            }
        }

        /* This method is the one called externally.
         * It looks for <node> and <way> tags and processes them.
         */
        public void ParseOsmFile(string pathToFile)
        {
            using (XmlReader reader = XmlReader.Create(pathToFile))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            case "node":
                                ParseNode(reader);
                                break;
                            case "way":
                                ParseWay(reader);
                                break;
                        }
                    }

                }
            }
        }

        /* When we found a <node> element we extract the information we need.
         * We use ReadSubtree() to access the <tag> containing the height
         */
        private void ParseNode(XmlReader reader)
        {
            //Console.WriteLine("Node found, id: {0} Lat: {1}, Lon: {2}", reader["id"], reader["lat"], reader["lon"]);
            OsmNode node = new OsmNode();
            node.id = reader["id"];
            node.lat = double.Parse(reader["lat"], CultureInfo.InvariantCulture);
            node.lon = double.Parse(reader["lon"], CultureInfo.InvariantCulture);
            nodes.Add(reader["id"], node);
            XmlReader inner = reader.ReadSubtree();
            //UnityEngine.Debug.Log("Parsing Node "+node.id);
            // Access child element
            // Here it's the <tag> element containing the "height" information osmosis added
            while (inner.Read())
            {
                //UnityEngine.Debug.Log("Inner element : " + inner.Name + " , k = " + inner["k"] + " , id = " + inner["id"]);
                if (inner["k"] == "height")
                {
                    //UnityEngine.Debug.Log("Height of Node: "+inner["v"]);
                    node.height = double.Parse(inner["v"], CultureInfo.InvariantCulture);
                }

            }
        }

        /* When we found a <node> element we extract the information we need.
         * We use ReadSuptree() to access all child Elements.
         * Each way consists out of multiple nodes. We store a reference to each node.
         * Also a Way may have multiple <tag> elements containing further description.
         * We will add the needed tags to the way later
         */
        private void ParseWay(XmlReader reader)
        {

            OsmWay way = new OsmWay();
            way.id = reader["id"];
            ways.Add(reader["id"], way);
            XmlReader inner = reader.ReadSubtree();

            // Access the child Elements
            // It's either a <nd> containin the reference to a node the way consists of
            // Or it'S a <tag> containing descriptive elements like number of floors
            while (inner.Read())
            {
                switch (inner.Name)
                {
                    case "nd":
                        Debug.Log("Reference to node" + inner["ref"] + " found");
                        string includedId = inner["ref"];
                        way.includedNodes.Add(nodes[includedId]);
                        way.includedIds.Add(includedId);
                        UnityEngine.Debug.Log("Way " + way.id + " includes node " + includedId + " with Height " + nodes[includedId].height);
						
                        break;
                    case "tag":
                        Debug.Log("Tag found: ");
                        switch (inner.GetAttribute("k"))
                        {
                            case "surface":
                                way.surface = inner.GetAttribute("v");
                                UnityEngine.Debug.Log("Found Surface: " + way.surface);
                                break;
                            case "height":
                                Int32.TryParse(inner.GetAttribute("v"), out way.height); break;
                            case "width":
                                Int32.TryParse(inner["v"], out way.height); break;
                            case "length":
                                Int32.TryParse(inner["v"], out way.height); break;
                            case "color":
                                way.color = inner["v"]; break;
                            case "amenity":
                                way.amenity = inner["v"]; break;
                            case "name":
                                way.street_name = inner["v"]; break;
                            case "bridge":
                                way.bridge = inner["v"]; break;
                            case "highway":
                                way.highway = inner["v"]; break;
                            case "maxspeed":
                                Int32.TryParse(inner["v"], out way.height); break;
                            case "lanes":
                                Int32.TryParse(inner["v"], out way.height); break;
                            case "traffic_sign":
                                way.traffic_sign = inner["v"]; break;
                            case "waterway":
                                way.waterway = inner["v"]; break;
                            case "building":
                                way.building = inner["v"]; break;
                            case "building_color":
                                way.building_color = inner["v"]; break;
                            case "building_levels":
                                Int32.TryParse(inner["v"], out way.height); break;
                            case "roof_shape":
                                way.roof_shape = inner["v"]; break;
                            case "roof_color":
                                way.roof_color = inner["v"]; break;
                            case "roof_material":
                                way.roof_material = inner["v"]; break;
                            case "roof_height":
                                way.roof_height = inner["v"]; break;
                            case "roof_levels":
                                Int32.TryParse(inner["v"], out way.height); break;
                            default:
                                way.other_tags.Add(inner.GetAttribute("k"), inner.GetAttribute("v"));
                                break;
                        }
                        break;
                }
            }
        }

    }

    /* This class parses a .poly.xml file
     * It extracts the information of each <poly> elemet and stores it in a Dictionary.
     * The file contains <location> information with general information
     * We also store this
     */
    public class PolyParser
    {

        /* Data*/
        public Dictionary<string, SumoPoly> polys = new Dictionary<string, SumoPoly>();
        public SumoLocation location = new SumoLocation();

        // Read the file and process either the <location> or the <poly> elements found.
        public void ParsePolyFile(string pathToFile)
        {
            using (XmlReader reader = XmlReader.Create(pathToFile))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            case "poly":
                                ParsePoly(reader);
                                break;
                            case "location":
                                ParseLocation(reader);
                                break;

                        }
                    }

                }
            }
        }

        /* When the <location> tag is detected its information will be stored
         */
        private void ParseLocation(XmlReader reader)
        {
            Debug.Log("Location found, netOffset: " + reader["netOffset"]+
                ", convBoundary: "+ reader["convBoundary"]+", origBoundary: "+ reader["origBoundary"]+", projParameter: "+ reader["projParameter"]);
            location.netOffset = reader["netOffset"];
            location.convBoundary = reader["convBoundary"];
            location.origBoundary = reader["origBoundary"];
            location.projParameter = reader["projParameter"];
        }

        /* Reading the <poly> elements is straight forward. They don't have any child elements
         * We read each line and store the element as an object in the dictionary
         */
        private void ParsePoly(XmlReader reader)
        {
            Debug.Log("Poly found, id: "+ reader["id"] + " Type: "+ reader["type"] + ", Shape: "+reader["shape"]);
            SumoPoly poly = new SumoPoly();
            poly.id = reader["id"];
            poly.type = reader["type"];
            poly.color = reader["color"];
            poly.fill = reader["fill"];
            poly.layer = reader["layer"];
            poly.shape = reader["shape"];
            polys.Add(reader["id"], poly);
        }

    }
}
