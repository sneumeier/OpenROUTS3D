using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.OsmParser
{
    // Represents a <way> element in the .osm file
    public class OsmWay
    {
        public string id;
        public List<string> includedIds = new List<string>();
        public List<OsmNode> includedNodes = new List<OsmNode>();

        // Tags
        // To get the original names replace "_" with ":"

        // General tags
        public string surface = "random";
        public int height = -1;
        public int width = -1;
        public int length = -1;
        public string color = "random";
        public string bridge = "random";
        public string amenity = "random";

        // Road tags
        public string street_name = "";
        public string highway = "random";
        public int maxspeed = -1;
        public int lanes = -1;
        public string traffic_sign = "random";	// Original name. Keep the "_" for this one
        public string waterway = "random";

        // Building tags
        public string building = "random";
        public string building_color = "random";
        public int building_levels = -1;
        public string roof_shape = "random";
        public string roof_color = "random";
        public string roof_material = "random";
        public string roof_height = "random";
        public int roof_levels = -1;

        //Other tags
        public Dictionary<string, string> other_tags = new Dictionary<string, string>();
    }
}
