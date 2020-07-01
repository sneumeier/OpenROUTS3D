
namespace SUMOConnectionScripts
{
    /// <summary>
    /// This enum is used to differ between the different states internally.
    /// A mapping is done in TraffiLights.cs
    /// </summary>
    public enum TrafficLightState
    {
        OFF,
        OFF_BLINKING,
        GREEN_PRIORITY, // vehicle may pass with priority
        GREEN, // vehicle may pass the junction if there is no oncoming traffic
        YELLOW,
        RED_YELLOW,
        RED
    }
}