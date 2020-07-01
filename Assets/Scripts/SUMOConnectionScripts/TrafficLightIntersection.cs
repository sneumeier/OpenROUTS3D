using UnityEngine;

namespace SUMOConnectionScripts
{
    /// <summary>
    /// A single TrafficLightIntersection object depicts one traffic light station,
    /// responsible for one lane at one side of the crossing.
    /// 
    /// Note: Links are the directions a vehicle can drive, so there are up to three links per lane.
    /// It is not wise to generate an own traffic light station for every link, so for the moment, we always 
    /// choose the first valid link for the specific lane.
    /// </summary>
    public class TrafficLightIntersection
    {
        public string trafficLightId;    // Same ID as used in SUMO
        public string laneId;            // Lane ID for the specific intersection
        public int linkId;               // (char) Index for the mapped link, necessary for updating
        public TrafficLightState state; // enum defined state

        public TrafficLightController trafficLight;

        public void SetState(TrafficLightState state)
        {
            trafficLight.SetState(state);
        }
    }
}