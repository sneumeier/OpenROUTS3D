using Assets.Scripts.MultiplayerMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Traci
{
    public partial class TraciManager
    {
        /// <summary>
        /// Implementation of vehicle related commands
        /// </summary>
        /// <see cref="http://sumo.dlr.de/wiki/TraCI/Vehicle_Value_Retrieval"/>
        /// <see cref="http://sumo.dlr.de/wiki/TraCI/Change_Vehicle_State"/>
        public class Vehicle : TraciTransceiver
        {
            /// <summary>
            /// Vehicles that were recieved from the host application
            /// </summary>
            public List<string> VehiclesFromHost = new List<string>();
            public Dictionary<string, ForeignCarMessage> LatestMessages = new Dictionary<string, ForeignCarMessage>();
            /// <summary>
            /// Returns a list of ids of all vehicles currently running within the scenario
            /// </summary>
            public List<string> GetIdList()
            {
                if (Settings.isSumoServer)
                {
                    return getUniversal<string>(TraciConstants.CMD_GET_VEHICLE_VARIABLE, null, TraciConstants.ID_LIST, TraciConstants.RESPONSE_GET_VEHICLE_VARIABLE);
                }
                else
                {
                    return VehiclesFromHost.ToList();
                }
            }

            /// <summary>
            /// Returns the number of vehicles currently running within the scenario
            /// </summary>
            public int GetCount()
            {
                if (Settings.isSumoServer)
                {
                    return getUniversal<int>(TraciConstants.CMD_GET_VEHICLE_VARIABLE, null, TraciConstants.ID_COUNT, TraciConstants.RESPONSE_GET_VEHICLE_VARIABLE).First();
                }
                else
                {
                    return VehiclesFromHost.Count;
                }
            }

            /// <summary>
            /// Returns the 2D-positions(two doubles) of the named vehicles (center of the front bumper) within the last step [m,m]
            /// </summary>
            /// <param name="id">List of Vehicle IDs </param>
            /// <returns></returns>
            public List<TraciPosition2D> GetPosition2D(List<string> ids)
            {

                return getUniversal<TraciPosition2D>(TraciConstants.CMD_GET_VEHICLE_VARIABLE, ids, TraciConstants.VAR_POSITION, TraciConstants.RESPONSE_GET_VEHICLE_VARIABLE);

            }

            /// <summary>
            /// Returns the 3D-positions(three doubles) of the named vehicles (center of the front bumper) within the last step [m,m,m]
            /// </summary>
            /// <param name="id">List of Vehicle IDs </param>
            /// <returns></returns>
            public List<TraciPosition3D> GetPosition3D(List<string> ids)
            {
                return getUniversal<TraciPosition3D>(TraciConstants.CMD_GET_VEHICLE_VARIABLE, ids, TraciConstants.VAR_POSITION3D, TraciConstants.RESPONSE_GET_VEHICLE_VARIABLE);
            }

            /// <summary>
            /// Returns the id of the lane the named vehicle was at within the last step
            /// </summary>
            /// <param name="ids">List of Vehicle IDs</param>
            /// <returns></returns>
            public List<string> GetLaneId(List<string> ids)
            {
                return getUniversal<string>(TraciConstants.CMD_GET_VEHICLE_VARIABLE, ids, TraciConstants.VAR_LANE_ID, TraciConstants.RESPONSE_GET_VEHICLE_VARIABLE);
            }

            /// <summary>
            /// Returns the index of the lane the named vehicle was at within the last step
            /// </summary>
            /// <param name="ids">List of Vehicle IDs</param>
            /// <returns></returns>
            public List<int> GetLaneIndex(List<string> ids)
            {
                return getUniversal<int>(TraciConstants.CMD_GET_VEHICLE_VARIABLE, ids, TraciConstants.VAR_LANE_INDEX, TraciConstants.RESPONSE_GET_VEHICLE_VARIABLE);
            }

            /// <summary>
            /// Returns the position of the vehicle along the lane (the distance from the front bumper to the start of the lane in [m])
            /// </summary>
            /// <param name="ids">List of Vehicle IDs</param>
            /// <returns></returns>
            public List<double> GetLanePosition(List<string> ids)
            {
                return getUniversal<double>(TraciConstants.CMD_GET_VEHICLE_VARIABLE, ids, TraciConstants.VAR_LANEPOSITION, TraciConstants.RESPONSE_GET_VEHICLE_VARIABLE);
            }

            /// <summary>
            /// Returns the distance, the vehicle has already driven [m]
            /// </summary>
            /// <param name="ids">List of Vehicle IDs</param>
            /// <returns></returns>
            public List<double> GetDistance(List<string> ids)
            {
                return getUniversal<double>(TraciConstants.CMD_GET_VEHICLE_VARIABLE, ids, TraciConstants.VAR_DISTANCE, TraciConstants.RESPONSE_GET_VEHICLE_VARIABLE);
            }

            /// <summary>
            /// Adds the defined vehicle
            /// </summary>
            /// <param name="vehicleId">ID of the new vehicle</param>
            /// <param name="routeId">ID of the route the vehicle should drive along</param>
            /// <param name="vehicleType">Vehicle type ID</param>
            /// <param name="departTime">Departure time</param>
            /// <param name="position">Departure position along lane</param>
            /// <param name="speed">Departure speed</param>
            /// <param name="lane">Departure lane </param>
            public void Add(string vehicleId, string routeId, string vehicleType = "DEFAULT_VEHTYPE", int departTime = TraciConstants.DEPARTFLAG_NOW, double position = 0.0, double speed = 0.0, byte lane = TraciConstants.DEPARTFLAG_LANE_FIRST_ALLOWED)
            {
                List<byte> contentList = new List<byte>();
                contentList.AddRange(new List<byte> { TraciConstants.ADD });               // [byte]   traci variable
                contentList.AddRange(BitConverter.GetBytes(vehicleId.Length).Reverse());   // [int]    length of vehicle ID
                contentList.AddRange(Encoding.ASCII.GetBytes(vehicleId));                  // [string] vehicle ID
                contentList.AddRange(new List<byte> { TraciConstants.TYPE_COMPOUND });     // [byte]   value type compound 
                contentList.AddRange(new List<byte> { 0, 0, 0, 6 });                       // [int]    item number (always=6) 
                contentList.AddRange(new List<byte> { TraciConstants.TYPE_STRING });       // [byte]   value type string
                contentList.AddRange(BitConverter.GetBytes(vehicleType.Length).Reverse()); // [int]    length of vehicle type
                contentList.AddRange(Encoding.ASCII.GetBytes(vehicleType));                // [string] vehicle type
                contentList.AddRange(new List<byte> { TraciConstants.TYPE_STRING });       // [byte]   value type string
                contentList.AddRange(BitConverter.GetBytes(routeId.Length).Reverse());     // [int]    length of route ID
                contentList.AddRange(Encoding.ASCII.GetBytes(routeId));                    // [string] route ID
                contentList.AddRange(new List<byte> { TraciConstants.TYPE_INTEGER });      // [byte]   value type integer
                contentList.AddRange(BitConverter.GetBytes(departTime).Reverse());         // [int]    depart time
                contentList.AddRange(new List<byte> { TraciConstants.TYPE_DOUBLE });       // [byte]   value type double
                contentList.AddRange(BitConverter.GetBytes(position).Reverse());           // [double] depart position along lane
                contentList.AddRange(new List<byte> { TraciConstants.TYPE_DOUBLE });       // [byte]   value type double
                contentList.AddRange(BitConverter.GetBytes(speed).Reverse());              // [double] depart speed along lane
                contentList.AddRange(new List<byte> { TraciConstants.TYPE_BYTE });         // [byte]   value type byte
                contentList.AddRange(new List<byte> { lane });                             // [byte]   lane

                var command = new TraciCommand
                {
                    Identifier = TraciConstants.CMD_SET_VEHICLE_VARIABLE,
                    Contents = contentList.ToArray()
                };

                SendMessage(command);
            }

            /// <summary>
            /// Moves the vehicle to a new position along the current route.
            /// 
            /// The vehicle will be removed from its lane and moved to the given position on the given lane. 
            /// No collision checks are done, this means that moving the vehicle may cause a collisions or a situations leading to collision. 
            /// The vehicle keeps its speed - in the next time step it is at given position + speed. 
            /// Note that the lane must be a part of the following route, this means it must be either a part of the edge the vehicle is currently on 
            /// or a part of an edge the vehicle will pass in future.
            /// </summary>
            /// <param name="vehicleId">ID of the vehicle to move</param>
            /// <param name="laneId">Lane ID the vehicle is currently on or will be next</param>
            /// <param name="position">Position along the lane [0.0 - LaneLength]</param>
            public void MoveTo(string vehicleId, string laneId, double position)
            {
                List<byte> contentList = new List<byte>();
                contentList.AddRange(new List<byte> { TraciConstants.VAR_MOVE_TO });     // [byte]   traci variable
                contentList.AddRange(BitConverter.GetBytes(vehicleId.Length).Reverse()); // [int]    length of vehicle ID
                contentList.AddRange(Encoding.ASCII.GetBytes(vehicleId));                // [string] vehicle ID
                contentList.AddRange(new List<byte> { TraciConstants.TYPE_COMPOUND });   // [byte]   value type compound 
                contentList.AddRange(new List<byte> { 0, 0, 0, 2 });                     // [int]    item number (always=2) 
                contentList.AddRange(new List<byte> { TraciConstants.TYPE_STRING });     // [byte]   value type string
                contentList.AddRange(BitConverter.GetBytes(laneId.Length).Reverse());    // [int]    length of lane ID
                contentList.AddRange(Encoding.ASCII.GetBytes(laneId));                   // [string] lane ID
                contentList.AddRange(new List<byte> { TraciConstants.TYPE_DOUBLE });     // [byte]   value type double
                contentList.AddRange(BitConverter.GetBytes(position).Reverse());         // [double] position along lane

                var command = new TraciCommand
                {
                    Identifier = TraciConstants.CMD_SET_VEHICLE_VARIABLE,
                    Contents = contentList.ToArray()
                };

                SendMessage(command);
            }



            /// <summary>
            /// Moves the vehicle to the network position that best matches the given x,y network coordinates.
            /// 
            /// </summary>
            /// <param name="vehicleId">ID of the vehicle to move</param>
            /// <param name="edgeId">ID of the edge the vehicle shall move to</param>
            /// <param name="laneIndex">Index of the lane the vehicle shall move to</param>
            /// <param name="x">X-Position (SUMO coordinates)</param>
            /// <param name="y">Y-Position (SUMO coordinates)</param>
            /// 
            /// <param name="angle">  
            /// If the angle is set to INVALID_DOUBLE_VALUE, the vehicle assumes the
            /// natural angle of the edge on which it is driving. 
            /// </param>
            /// 
            /// <param name="keepRoute">
            /// There are three valid option:
            /// <list type="bullet">
            /// <item> <description> keepRoute = 0: The vehicle may move to any edge in the network but it's route then only consists of that edge. </description> </item>
            /// <item> <description> keepRoute = 1: The closest position within the existing route is taken. </description> </item>
            /// <item> <description> keepRoute = 2: The vehicle has all the freedom of keepRoute=0, but in addition to that may even move outside the road network. </description> </item>
            /// </list>
            /// </param>
            public void MoveToXY(string vehicleId, string edgeId, int laneIndex, double x, double y, double angle, byte keepRoute = 0)
            {
                List<byte> contentList = new List<byte>();
                contentList.AddRange(new List<byte> { TraciConstants.MOVE_TO_XY });      // [byte]   traci variable
                contentList.AddRange(BitConverter.GetBytes(vehicleId.Length).Reverse()); // [int]    length of vehicle ID
                contentList.AddRange(Encoding.ASCII.GetBytes(vehicleId));                // [string] vehicle ID
                contentList.AddRange(new List<byte> { TraciConstants.TYPE_COMPOUND });   // [byte]   value type compound 
                contentList.AddRange(new List<byte> { 0, 0, 0, 6 });                     // [int]    item number (always=6) 
                contentList.AddRange(new List<byte> { TraciConstants.TYPE_STRING });     // [byte]   value type string
                contentList.AddRange(BitConverter.GetBytes(edgeId.Length).Reverse());    // [int]    length of edge ID
                contentList.AddRange(Encoding.ASCII.GetBytes(edgeId));                   // [string] edge ID
                contentList.AddRange(new List<byte> { TraciConstants.TYPE_INTEGER });    // [byte]   value type integer
                contentList.AddRange(BitConverter.GetBytes(laneIndex).Reverse());        // [double] lane index
                contentList.AddRange(new List<byte> { TraciConstants.TYPE_DOUBLE });     // [byte]   value type double
                contentList.AddRange(BitConverter.GetBytes(x).Reverse());                // [double] x Position (network coordinates)
                contentList.AddRange(new List<byte> { TraciConstants.TYPE_DOUBLE });     // [byte]   value type double
                contentList.AddRange(BitConverter.GetBytes(y).Reverse());                // [double] y Position (network coordinates)
                contentList.AddRange(new List<byte> { TraciConstants.TYPE_DOUBLE });     // [byte]   value type double
                contentList.AddRange(BitConverter.GetBytes(angle).Reverse());            // [double] angle
                contentList.AddRange(new List<byte> { TraciConstants.TYPE_BYTE });       // [byte]   value type byte
                contentList.AddRange(new List<byte> { keepRoute });                      // [byte]   keepRoute

                var command = new TraciCommand
                {
                    Identifier = TraciConstants.CMD_SET_VEHICLE_VARIABLE,
                    Contents = contentList.ToArray()
                };

                SendMessage(command);
            }

            /// <summary>
            /// Sets the vehicle speed to the given value.  
            /// </summary>
            /// <param name="vehicleId"></param>
            /// <param name="speed">Speed measured in m/s</param>
            public void SetSpeed(string vehicleId, double speed)
            {
                List<byte> contentList = new List<byte>();
                contentList.AddRange(new List<byte> { TraciConstants.VAR_SPEED });       // [byte]   traci variable
                contentList.AddRange(BitConverter.GetBytes(vehicleId.Length).Reverse()); // [int]    length of vehicle ID
                contentList.AddRange(Encoding.ASCII.GetBytes(vehicleId));                // [string] vehicle ID
                contentList.AddRange(new List<byte> { TraciConstants.TYPE_DOUBLE });     // [byte]   value type double
                contentList.AddRange(BitConverter.GetBytes(speed).Reverse());            // [double] speed

                var command = new TraciCommand
                {
                    Identifier = TraciConstants.CMD_SET_VEHICLE_VARIABLE,
                    Contents = contentList.ToArray()
                };

                SendMessage(command);
            }

            /// <summary>
            /// Removes vehicle from traci
            /// </summary>
            /// <param name="vehicleId"></param>
            /// <param name="reason"></param>
            public void Remove(string vehicleId, byte reason = TraciConstants.REMOVE_VAPORIZED)
            {
                List<byte> contentList = new List<byte>();
                contentList.AddRange(new List<byte> { TraciConstants.REMOVE });
                contentList.AddRange(BitConverter.GetBytes(vehicleId.Length).Reverse());            // [int] length vehicle id
                contentList.AddRange(Encoding.ASCII.GetBytes(vehicleId));                           // [string] vehicle id
                contentList.AddRange(new List<byte> { TraciConstants.TYPE_BYTE });                  // [byte]   value type compound 
                contentList.AddRange(new List<byte> { reason });                                    // [byte]reason for removal

                var command = new TraciCommand
                {
                    Identifier = TraciConstants.CMD_SET_VEHICLE_VARIABLE,
                    Contents = contentList.ToArray()
                };
                SendMessage(command);
            }

            /// <summary>
            /// Returns a list of signal of TraCI
            /// </summary>
            /// <param name="ids"></param>
            /// <returns></returns>
            public List<int> GetLightSignalState(List<string> ids)
            {
                return getUniversal<int>(TraciConstants.CMD_GET_VEHICLE_VARIABLE, ids, TraciConstants.VAR_SIGNALS, TraciConstants.RESPONSE_GET_VEHICLE_VARIABLE);
            }

            /// <summary>
            /// Converts a List of int in a List of LightSignalStates
            /// </summary>
            /// <param name="lsignals"></param>
            /// <returns></returns>
            public List<Dictionary<LightSignalStates,bool>> DecodeLightSignalState(List<int> lsignals)
            {
                List<Dictionary<LightSignalStates, bool>> decodedList = new List<Dictionary<LightSignalStates, bool>>();
                int sig;
                if (lsignals.Count == 0) // nothing to decode
                    return decodedList;

                // go through each answer for each id
                foreach (int i in lsignals)
                {
                    sig = i;
                    Dictionary<LightSignalStates,bool> dli = new Dictionary<LightSignalStates, bool>();

                    foreach (LightSignalStates lss in Enum.GetValues(typeof(LightSignalStates)).Cast<LightSignalStates>().ToList())
                    {
                        if ((sig & (1 << ((int)lss))) != 0) 
                        {
                            // sig == lss 
                            dli.Add(lss, true);
                            sig = sig - (int)lss - 1;
                        }
                        else
                        {
                            // sig != lss
                            dli.Add(lss, false);
                        }
                        if (sig == 0)
                            break;

                    }
                    decodedList.Add(dli);
                }
                return decodedList;
            }

            /// <summary>
            /// Return if a given Lightsignalstate is set in a given int
            /// </summary>
            /// <param name="lss"></param>
            /// <param name="val"></param>
            /// <returns></returns>
            public static bool DecodeSingleLightSignal(LightSignalStates lss, int val)
            {
                if ((val & (1 << ((int)lss))) != 0)
                {
                    return true;
                }
                return false;
            }
        }
    }
}
