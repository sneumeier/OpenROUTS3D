using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.SUMOConnectionScripts
{
    public class GraphChunk
    {
        public int x;
        public int y;
        public List<StreetNode> nodes = new List<StreetNode>();

        public GraphChunk(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        //Helper Function for Chunk Logic
        public static int FullDivision(float a, float b)
        {
            if (a >= 0) { return (int)(a / b); } else return (int)(a / b) - 1;
        }
    }
}
