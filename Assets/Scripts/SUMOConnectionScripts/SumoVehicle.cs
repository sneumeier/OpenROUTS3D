using Assets.Scripts.AssetReplacement;
using Assets.Scripts.CarScripts;
using System.Collections.Generic;
using UnityEngine;

namespace SUMOConnectionScripts
{
    public class SumoVehicle : MonoBehaviour
    {
        public float wheelbase = 3;
        /// <summary>
        /// wheel diameter in meter
        /// </summary>
        public float wheelDiameter = 0.71f;

        public GameObject frontLeftWheel;
        public GameObject frontRightWheel;
        public GameObject rearLeftWheel;
        public GameObject rearRightWheel;


        public bool light = false; // toggle the light
        public bool indicatorLeft = false; // toggle the left indicator
        public bool indicatorRight = false; // toggle the right indicator
        public bool brakelight = false;

        private StatisticsLogger statLogger;
        private float lastTimestamp = 0;
        private float circumference;
        public float velocity = 0;

        private void Awake()
        {
            circumference = wheelDiameter * Mathf.PI;
        }

        void Start()
        {
            statLogger = AnchorMapping.GetAnchor("CarPhysics").GetComponent<StatisticsLogger>();

        }



        /// <summary>
        /// Sets the vehicle to the specified position, the orientation is computed internally considering the last position and the wheelbase. The tires are rotated according to the distance covered.
        /// </summary>
        /// <param name="position"></param>
        public void SetPosition(Vector3 position)
        {



            Quaternion rotation = new Quaternion();

            Vector3 oldPos = transform.position;
            Vector3 forward = transform.forward;
            Vector3 rearwheel = oldPos + forward * -wheelbase;

            Vector3 rearwheelPosition = rearwheel + forward * Vector3.Distance(position, oldPos);

            rotation.SetLookRotation(position - rearwheelPosition);

            transform.SetPositionAndRotation(position, rotation);

            float wheelRotation = (Vector3.Distance(position, oldPos) / circumference) * 360;

            if (frontLeftWheel != null)
            {
                frontLeftWheel.transform.Rotate(Quaternion.Euler(wheelRotation, 0, 0).eulerAngles);
            }
            if (frontRightWheel != null)
            {
                frontRightWheel.transform.Rotate(Quaternion.Euler(wheelRotation, 0, 0).eulerAngles);
            }
            if (rearLeftWheel != null)
            {
                rearLeftWheel.transform.Rotate(Quaternion.Euler(wheelRotation, 0, 0).eulerAngles);
            }
            if (rearRightWheel != null)
            {
                rearRightWheel.transform.Rotate(Quaternion.Euler(wheelRotation, 0, 0).eulerAngles);
            }

            if (lastTimestamp != 0)
            {
                float passedTime = Time.time - lastTimestamp;
                velocity = Vector3.Distance(position, oldPos) * passedTime;
            }

            lastTimestamp = Time.time;

            if (statLogger != null)
            {
                statLogger.WriteForeignCsvLine(this);
            }


        }

        /// <summary>
        /// Checks if the needed light information are contained in a list and thus set the bools according to the value
        /// If the information is not in the list, the light is off, else it is on.
        /// </summary>
        /// <param name="decodedLights">List of Traci.LightSignalStates</param>
        public void UpdateSumoCarLights(int undecodedLight)//Dictionary<Traci.LightSignalStates, bool> decodedLights)
        {
            // set bools 


            this.light = Traci.TraciManager.Vehicle.DecodeSingleLightSignal(Traci.LightSignalStates.VEH_SIGNAL_FRONTLIGHT, undecodedLight);
            this.indicatorLeft = Traci.TraciManager.Vehicle.DecodeSingleLightSignal(Traci.LightSignalStates.VEH_SIGNAL_BLINKER_LEFT, undecodedLight);
            this.indicatorRight = Traci.TraciManager.Vehicle.DecodeSingleLightSignal(Traci.LightSignalStates.VEH_SIGNAL_BLINKER_RIGHT, undecodedLight);
            this.brakelight = Traci.TraciManager.Vehicle.DecodeSingleLightSignal(Traci.LightSignalStates.VEH_SIGNAL_BRAKELIGHT, undecodedLight);
            /*
            this.light = decodedLights[Traci.LightSignalStates.VEH_SIGNAL_FRONTLIGHT];
            this.indicatorLeft = decodedLights[Traci.LightSignalStates.VEH_SIGNAL_BLINKER_LEFT];
            this.indicatorRight = decodedLights[Traci.LightSignalStates.VEH_SIGNAL_BLINKER_RIGHT];
            this.brakelight = decodedLights[Traci.LightSignalStates.VEH_SIGNAL_BRAKELIGHT];
            */
            /*
            if (decodedLights.Contains(Traci.LightSignalStates.VEH_SIGNAL_FRONTLIGHT))
            {
                this.light = true;
            }
            else
            {
                this.light = false;
            }
            if (decodedLights.Contains(Traci.LightSignalStates.VEH_SIGNAL_BLINKER_LEFT))
            {
                this.indicatorLeft = true;
            }
            else
            {
                this.indicatorLeft = false;
            }
            if (decodedLights.Contains(Traci.LightSignalStates.VEH_SIGNAL_BLINKER_RIGHT))
            {
                this.indicatorRight = true;
            }
            else
            {
                this.indicatorRight = false;
            }
            if(decodedLights.Contains(Traci.LightSignalStates.VEH_SIGNAL_BRAKELIGHT))
            {
                this.brakelight = true;
            }
            else
            {
                this.brakelight = false;
            }
            */

        }
    }
}