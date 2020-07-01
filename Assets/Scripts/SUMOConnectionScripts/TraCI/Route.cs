using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Traci
{
    public partial class TraciManager
    {
        /// <summary>
        /// Implementation of route related commands
        /// </summary>
        /// <see cref="http://sumo.dlr.de/wiki/TraCI/Route_Value_Retrieval"/>
        /// <see cref="http://sumo.dlr.de/wiki/TraCI/Change_Route_State"/>
        public class Route : TraciTransceiver
        {
            /// <summary>
            /// Returns the ids of all edges in the given routes
            /// </summary>
            /// <param name="ids">List of Route IDs</param>
            /// <returns></returns>
            public List<string> GetEdges(List<string> ids)
            {
                return getUniversal<string>(TraciConstants.CMD_GET_ROUTE_VARIABLE, ids, TraciConstants.VAR_EDGES, TraciConstants.RESPONSE_GET_ROUTE_VARIABLE);
            }

            /// <summary>
            /// Adds a new route. The route gets the given route ID and follows the given edges
            /// </summary>
            /// <param name="routeId">Name of the new route</param>
            /// <param name="edges">List of edges the new route consists of</param>
            public void Add(string routeId, List<string> edges)
            {
                List<byte> contentList = new List<byte>();
                contentList.AddRange(new List<byte> { TraciConstants.ADD });             // [byte]   traci variable
                contentList.AddRange(BitConverter.GetBytes(routeId.Length).Reverse());   // [int]    length of route ID
                contentList.AddRange(Encoding.ASCII.GetBytes(routeId));                  // [string] route ID
                contentList.AddRange(new List<byte> { TraciConstants.TYPE_STRINGLIST }); // [byte]   value type string list
                contentList.AddRange(BitConverter.GetBytes(edges.Count).Reverse());      // [int]    string count
                foreach(var e in edges)
                {
                    contentList.AddRange(BitConverter.GetBytes(e.Length).Reverse());     // [int]    length of edge ID
                    contentList.AddRange(Encoding.ASCII.GetBytes(e));                    // [string] edge ID
                }

                var command = new TraciCommand
                {
                    Identifier = TraciConstants.CMD_SET_ROUTE_VARIABLE,
                    Contents = contentList.ToArray()
                };

                SendMessage(command);
            }
        }
    }
}