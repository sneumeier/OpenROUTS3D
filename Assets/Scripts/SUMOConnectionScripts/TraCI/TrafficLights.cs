using System.Collections.Generic;
using System.Linq;

namespace Traci
{
    public partial class TraciManager
    {
        /// <summary>
        /// Implementation of traffic lights related commands
        /// </summary>
        /// <see cref="="http://sumo.dlr.de/wiki/TraCI/Traffic_Lights_Value_Retrieval"/>
        public class TrafficLights : TraciTransceiver
        {
            /// <summary>
            /// Returns a list of ids of´all traffic lights within the scenario
            /// </summary>
            public List<string> GetIdList()
            {
                return getUniversal<string>(TraciConstants.CMD_GET_TL_VARIABLE, null, TraciConstants.ID_LIST, TraciConstants.RESPONSE_GET_TL_VARIABLE);
            }

            /// <summary>
            /// Returns the number of traffic lights within the scenario
            /// </summary>
            public int GetCount()
            {
                return getUniversal<int>(TraciConstants.CMD_GET_TL_VARIABLE, null, TraciConstants.ID_COUNT, TraciConstants.RESPONSE_GET_TL_VARIABLE).First();
            }

            /// <summary>
            /// Returns the named traffic lights state as a tuple of light definitions
            /// </summary>
            /// <param name="id">List of traffic light IDs </param>
            /// <returns></returns>
            public List<string> GetState(List<string> ids)
            {
                return getUniversal<string>(TraciConstants.CMD_GET_TL_VARIABLE, ids, TraciConstants.TL_RED_YELLOW_GREEN_STATE, TraciConstants.RESPONSE_GET_TL_VARIABLE);
            }

            /// <summary>
            /// Returns the list of lanes which are controlled by the named traffic light
            /// </summary>
            /// <param name="id">List of traffic light IDs </param>
            /// <returns></returns>
            public List<string> GetControlledLanes(List<string> ids)
            {
                return getUniversal<string>(TraciConstants.CMD_GET_TL_VARIABLE, ids, TraciConstants.TL_CONTROLLED_LANES, TraciConstants.RESPONSE_GET_TL_VARIABLE);
            }
        }
    }
}