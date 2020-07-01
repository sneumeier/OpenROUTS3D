using System.Collections.Generic;
using UnityEngine;
public enum DriveType
{
    RearWheelDrive,
    FrontWheelDrive,
    AllWheelDrive
}
public struct LogData
{
    public float timestamp;
    public int numberSteeringMovements;
    public float avgSteeringMovement;
    public float minSteeringMovement;
    public float maxSteeringMovement;
    public int numberOfThrottleMovements;
    public float avgThrottleMovement;
    public float minThrottleMovement;
    public float maxThrottleMovement;
    public int numberOfBrakeMovements;
    public float avgBrakeMovement;
    public float minBrakeMovement;
    public float maxBrakeMovement;
    public int numberOfTrackLeavings;
    public float avgTrackLeavingDistance;
    public float minTrackLeavingDistance;
    public float maxTrackLeavingDistance;
    public Vector3 carPosition;

}

public class StatisticsContainer : MonoBehaviour
{
    [Header("Inputs")]
    public float angle; // -1 to 1
    public float pedal; // 0 to 1
    public float brake; // -1 to 0
    public bool handBrake = false; // 0 or 1
    public bool reverse = false; // 0 or 1
    public bool light = false; // toggle the light
    public bool indicatorLeft = false; // toggle the left indicator
    public bool indicatorRight = false; // toggle the right indicator

    [Header("Dynamic Statistics")]
    public float wheelAngle;
    public float velocity;
    public float torque;
    public float handBrakeTorque;
    public Vector3 carPosition;
    public float stiffness;
    public float backWheelStiffnessFactor;

    [Header("Static Statistics")]
    public int maxVelocityBackwards;
    public int maxVelocity;
    public int maxAngle;
    public int maxTorque;
    public int maxBrakeTorque;
    public DriveType driveType;

    [Header("Collision")]
    public int collisionTerrain = 0;
    public int collisionTrack = 0;
    public double CrashDeltaTime = 0.5;
    public double CrashImpact = 2;

    [Header("Log")]
    public List<LogData> log = new List<LogData>();
    public float logInterval = 1000;
}
