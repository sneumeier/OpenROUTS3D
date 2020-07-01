using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SUMOConnectionScripts
{
    /// <summary>
    /// A class to simply control the appearance of one single traffic light by switching states, no logic in here.
    /// </summary>
    public class TrafficLightController : MonoBehaviour
    {
        TrafficLightState state;

        public Material redOn;
        public Material redOff;

        public Material yellowOn;
        public Material yellowOff;

        public Material greenOn;
        public Material greenOff;

        public MeshRenderer[] greenLamps;
        public MeshRenderer[] yellowLamps;
        public MeshRenderer[] redLamps;


        void Start()
        {
            SetState(TrafficLightState.OFF);
        }

        public void SetState(TrafficLightState state)
        {
            this.state = state;

            switch (state)
            {
                // we ignore the difference between GREEN_PRIORITY and GREEN for now
                case TrafficLightState.GREEN_PRIORITY:
                    SetRed(false);
                    SetYellow(false);
                    SetGreen(true);
                    break;
                case TrafficLightState.GREEN:
                    SetRed(false);
                    SetYellow(false);
                    SetGreen(true);
                    break;
                case TrafficLightState.YELLOW:
                    SetRed(false);
                    SetYellow(true);
                    SetGreen(false);
                    break;
                case TrafficLightState.RED:
                    SetRed(true);
                    SetYellow(false);
                    SetGreen(false);
                    break;
                case TrafficLightState.RED_YELLOW:
                    SetRed(true);
                    SetYellow(true);
                    SetGreen(false);
                    break;
                case TrafficLightState.OFF_BLINKING:
                    SetRed(false);
                    SetYellow(true);
                    SetGreen(false);
                    StartCoroutine(Blinking());
                    break;
                case TrafficLightState.OFF:
                    SetRed(false);
                    SetYellow(false);
                    SetGreen(false);
                    break;
            }
        }

        private IEnumerator Blinking()
        {
            bool active = true;
            while (state == TrafficLightState.OFF_BLINKING)
            {
                SetYellow(active);
                active = !active;
                yield return new WaitForSeconds(0.5f);
            }
        }

        private void SetRed(bool on)
        {
            foreach (MeshRenderer mr in redLamps)
            {
                if (on)
                {
                    mr.material = redOn;
                }
                else
                {
                    mr.material = redOff;
                }
            }
        }

        private void SetYellow(bool on)
        {
            foreach (MeshRenderer mr in yellowLamps)
            {
                if (on)
                {
                    mr.material = yellowOn;
                }
                else
                {
                    mr.material = yellowOff;
                }
            }
        }

        private void SetGreen(bool on)
        {
            foreach (MeshRenderer mr in greenLamps)
            {
                if (on)
                {
                    mr.material = greenOn;
                }
                else
                {
                    mr.material = greenOff;
                }
            }
        }
    }
}
