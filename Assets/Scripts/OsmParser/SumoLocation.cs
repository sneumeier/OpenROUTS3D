using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.OsmParser
{
    // Represents the <location> element in the .poly.xml file
    public class SumoLocation
    {
        public string netOffset;
        public string convBoundary;
        public string origBoundary;
        public string projParameter;
    }
}
