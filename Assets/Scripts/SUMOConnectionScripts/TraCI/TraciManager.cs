using SUMOConnectionScripts;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Traci
{
    /// <summary>
    /// This class is the entry-point for the communication between the SUMO-Simulation and Unity.
    /// The Traffic Control Interface(TraCI) is used as a standardized platform.
    /// TraCI uses a TCP-based client/server architecture where SUMO acts as a server and the
    /// Unity application acts as a client.
    /// </summary>
    public partial class TraciManager
    {
        // Here are the instances to "the outside world" (as nested classes of the TraciManager)
        public SimulationControl simcontrol = new SimulationControl();
        public Vehicle vehicle = new Vehicle();
        public Lane lane = new Lane();
        public TrafficLights trafficlights = new TrafficLights();
        public Route route = new Route();
        public Route ghostRoute = new Route();
        public long count = 0;
        private static TraciManager _instance = null;
        private TraciSumoConnector connector = TraciSumoConnector.Instance;

        /// <summary>
        /// Private Constructor of this class to prevent instanciation (Singleton)
        /// </summary>
        private TraciManager() { }

        /// <summary>
        /// Accessor of TraciSumoConnector,
        /// </summary>
        public static TraciManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TraciManager();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Start SUMO and connect
        /// </summary>
        /// <param name="path">Path of the selected Map</param>
        public void Start(string path)
        {
            Debug.Log("Starting SUMO");
            connector.Init(path);
        }

        /// <summary>
        /// Close the connecetion and quit SUMO
        /// </summary>
        public void Stop()
        {
            simcontrol.Close();
            connector.Close();  
        }

        /// <summary>
        /// Add an ego vehicle
        /// </summary>
        /// <param name="firstRoadElement"></param>
        public string AddEgoVehicle(string firstRoadElement)
        {
            // Get the edgeId(s) from firstRoadElement
            List<string> edge = lane.GetEdgeId(new List<string> { firstRoadElement });

            // Add a new route with that edge(s)
            route.Add("EgoRoute", edge);

            // Add the EgoVehicle
            //string vehicleId = "EgoVehicle";
            string vehicleId = Settings.userName.Replace(' ','_');
            vehicle.Add(vehicleId, "EgoRoute");
            
            return vehicleId;
        }

        /// <summary>
        /// Add an ego ghost vehicle at a given lane
        /// </summary>
        /// <param name="firstRoadElement"></param>
        public string AddGhostVehicle(string RoadElement, string carCornerPoint)
        {
            // Get the edgeId(s) from firstRoadElement
            List<string> edge = lane.GetEdgeId(new List<string> { RoadElement });
            string routeName = "GhostRoute_" + carCornerPoint + "_"+count;
            // TraCi don't likes multiple routes with the same name
            count++; // just add 1 each time a Ghostrout is created

            // Add a new route with that edge(s)
            route.Add(routeName, edge);

            // use cornerpoint for id
            string vehicleId = carCornerPoint;
            vehicle.Add(vehicleId, routeName);

            return vehicleId;
        }
    }
}