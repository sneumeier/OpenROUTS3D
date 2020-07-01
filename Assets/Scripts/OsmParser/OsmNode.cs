using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.OsmParser
{
    // Represents a <node> element in the .osm file
    public class OsmNode
    {
        public string id;
        public double lat;
        public double lon;
        public double height = -1;
    }
}
