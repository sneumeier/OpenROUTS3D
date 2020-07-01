using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.OsmParser
{
    // Represents a <poly> element in the .poly.xml file 
    public class SumoPoly
    {
        public string id;
        public string type;
        public string color;
        public string fill;
        public string shape;
        public string layer;
    }
}
