using System.Collections.Generic;
using System.Linq;


namespace Traci
{
    public partial class TraciManager
    {
        /// <summary>
        /// Implementation of lane related commands
        /// </summary>
        /// <see cref="http://sumo.dlr.de/wiki/TraCI/Lane_Value_Retrieval"/>
        public class Lane : TraciTransceiver
        {
            /// <summary>
            /// Returns the length of the named lanes [m] 
            /// </summary>
            /// <param name="ids">List of Lane IDs</param>
            /// <returns></returns>
            public List<double> GetLength(List<string> ids)
            {
                return getUniversal<double>(TraciConstants.CMD_GET_LANE_VARIABLE, ids, TraciConstants.VAR_LENGTH, TraciConstants.RESPONSE_GET_LANE_VARIABLE);
            }

            /// <summary>
            /// Returns the ids of the edges the lanes belong to 
            /// </summary>
            /// <param name="ids">List of Lane IDs</param>
            /// <returns></returns>
            public List<string> GetEdgeId(List<string> ids)
            {
                return getUniversal<string>(TraciConstants.CMD_GET_LANE_VARIABLE, ids, TraciConstants.LANE_EDGE_ID, TraciConstants.RESPONSE_GET_LANE_VARIABLE);
            }
        }
    }
}