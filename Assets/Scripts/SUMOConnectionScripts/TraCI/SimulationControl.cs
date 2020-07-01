using Assets.Scripts.SUMOConnectionScripts.Maps;
using SUMOConnectionScripts;
using System;
using System.Linq;
using System.Text;


namespace Traci
{
    public partial class TraciManager
    {
        /// <summary>
        /// Implementation of control related commands
        /// </summary>
        /// <see cref="http://sumo.dlr.de/wiki/TraCI/Control-related_commands"/>
        public partial class SimulationControl : TraciTransceiver
        {
            /// <summary>
            /// Instruct SUMO to execute a single simulation step
            /// </summary>
            public void SimStep()
            {


                if (!SumoUnityConnection.startTraci || MapRasterizer.Instance.IsEditor)
                {
                    return;
                }
                //UnityEngine.Debug.Log("SimStep");
                var command = new TraciCommand
                {
                    Identifier = TraciConstants.CMD_SIMSTEP,
                    Contents = BitConverter.GetBytes(0.0)  // double since version 1.0
                };

                var response = SendMessage(command);
                if (response == null)
                {
                    throw new Exception("execution of SimStep() was not possible");
                }
            }

            /// <summary>
            /// Instruct SUMO to stop the simulation
            /// </summary>
            public void Close()
            {
                var command = new TraciCommand
                {
                    Identifier = TraciConstants.CMD_CLOSE,
                    Contents = null
                };

                var response = SendMessage(command);
                if (response == null)
                {
                    throw new Exception("stopping SUMO was not possible");
                }
            }

            /// <summary>
            /// Gets a user friendly string describing the version of SUMO
            /// </summary>
            public string GetVersionString()
            {
                var command = new TraciCommand
                {
                    Identifier = TraciConstants.CMD_GETVERSION,
                    Contents = null
                };

                var response = SendMessage(command);
                if (response.Length == 2)
                {
                    var strlen = response[1].Response.Skip(4).Take(4).Reverse().ToArray();
                    var idl = BitConverter.ToInt32(strlen, 0);
                    var ver = Encoding.ASCII.GetString(response[1].Response, 8, idl);

                    UnityEngine.Debug.Log("SUMO Version: " + ver);

                    return ver;
                }
                return null;
            }
        }
    }
}