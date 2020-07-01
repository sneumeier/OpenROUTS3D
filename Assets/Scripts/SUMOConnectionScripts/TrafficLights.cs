using System.Collections.Generic;
using System.Linq;

namespace SUMOConnectionScripts
{
    public class TrafficLights
    {
        private Traci.TraciManager traci = Traci.TraciManager.Instance;
        private List<string> trafficLightIds;
        private List<TrafficLightIntersection> trafficLightsList = new List<TrafficLightIntersection>();
        private HashSet<string> trafficLightsLanesSet = new HashSet<string>();

        public static readonly Dictionary<char, TrafficLightState> states = new Dictionary<char, TrafficLightState>
        {
            { 'r', TrafficLightState.RED },
            { 'y', TrafficLightState.YELLOW },
            { 'g', TrafficLightState.GREEN },
            { 'G', TrafficLightState.GREEN_PRIORITY },
            { 'u', TrafficLightState.RED_YELLOW },
            { 'o', TrafficLightState.OFF_BLINKING },
            { 'O', TrafficLightState.OFF }
        };

        public List<TrafficLightIntersection> Generate()
        {
            List<string> trafficLightLanes;
            trafficLightIds = traci.trafficlights.GetIdList();

            for (int i = trafficLightIds.Count - 1; i >= 0; i--)
            {
                trafficLightLanes = traci.trafficlights.GetControlledLanes(new List<string> { trafficLightIds[i] });

                for (int j = 0; j < trafficLightLanes.Count; j++)
                {
                    if (trafficLightsLanesSet.Contains(trafficLightLanes[j]) == false)
                    {
                        trafficLightsLanesSet.Add(trafficLightLanes[j]);

                        TrafficLightIntersection directionalTrafficLight = new TrafficLightIntersection
                        {
                            trafficLightId = trafficLightIds[i],
                            laneId = trafficLightLanes[j],
                            linkId = j
                        };

                        trafficLightsList.Add(directionalTrafficLight);
                    }
                }
            }
            return trafficLightsList;
        }

        public void UpdateTrafficLights()
        {
            // Index of trafficLightIds
            int listIndex = 0;

            // Get the current states of all traffic lights from SUMO
            List<string> trafficLightStates = traci.trafficlights.GetState(trafficLightIds);
            trafficLightStates.Reverse();

            // Iterate over all traffic lights (one string is responsible for a complete crossing)
            foreach (var state in trafficLightStates)
            {
                // Map the states of the link indices (chars of trafficLightStates) to the corresponding traffic lights
                for (int linkIndex = 0; linkIndex < state.Length; linkIndex++)
                {
                    //TrafficLightIntersection tli = trafficLightsList[linkIndex];
                    if (trafficLightsList[listIndex].linkId == linkIndex)
                    {
                        // Do a mapping of the SUMO state (chars) to the unity enum
                        states.TryGetValue(state[linkIndex], out trafficLightsList[listIndex].state);

                        if (listIndex < trafficLightsList.Count - 1)
                        {
                            listIndex++;
                        }
                    }
                }

                if (listIndex < trafficLightIds.Count)
                {
                    listIndex++;
                }
            }

            foreach(var t1 in trafficLightsList)
            {
                t1.SetState(t1.state);
            }

            // Debug
            //foreach (var tl in trafficLightsList)
            //{
            //    UnityEngine.Debug.Log("Kreuzung: " + tl.trafficLightId);
            //    UnityEngine.Debug.Log("Lane: " + tl.laneId);
            //    UnityEngine.Debug.Log("State: " + tl.state);
            //    UnityEngine.Debug.Log("");
            //}
        }
    }
}