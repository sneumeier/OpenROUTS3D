using Assets.Scripts.CarScripts;
using Assets.Scripts.ScenarioEditor;
using MainMenuScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityInputManager;
namespace CarScripts
{

    public class InputManager : MonoBehaviour
    {
        public StatisticsContainer statistics;
        public Vector3 spawnPos = Vector3.zero;

        public float delay = 0.0f;

        public BufferedDisplay bufferedDisplay;

        //public List<KeyCode> scannedInputs = new List<KeyCode>();
        //public List<string> scannedAxes = new List<string>();
        //public List<string> scannedButtons = new List<string>();

        //public List<KeyOrder> delayedOrders = new List<KeyOrder>();
		public List<InputAction.CallbackContext> delayedAction = new List<InputAction.CallbackContext>();

        public List<ScenarioDelayBeacon> delayBeacons = new List<ScenarioDelayBeacon>();

        private Dictionary<string, float> lastAxisValues = new Dictionary<string, float>();

        /*
             This cannot be replaced with a Dictonary<Vector2,float>

             Reason for this is, that every time a drive actions ends, the list gets a entry to reset
             the direction. If a user stopps an action twice within the span of the Delay, a error will
             occure, because we cant add the same Input/Key twice to a Dictionary
        */
        private List<Tuple<Vector2, float>> ml_inputdirection_timestamp = new List<Tuple<Vector2, float>>();

        //private Plane playerMovementPlane;

        private UnityInputManager playerControls;

        private void Drive(InputAction.CallbackContext context)
        {
            ml_inputdirection_timestamp.Add(new Tuple<Vector2, float>(context.ReadValue<Vector2>(), Time.fixedTime));
        }

        private void StopDrive(InputAction.CallbackContext context)
        {
            ml_inputdirection_timestamp.Add(new Tuple<Vector2, float>(context.ReadValue<Vector2>(), Time.fixedTime));
        }

        private void Pause(InputAction.CallbackContext context)
        {
            Settings.paused = !Settings.paused;
        }

        private void Reverse(InputAction.CallbackContext context)
        {
            if (statistics.velocity == 0)
            {
                statistics.reverse = !statistics.reverse;
            }
        }

        private void Handbrake(InputAction.CallbackContext context)
        {
            statistics.handBrake = !statistics.handBrake;
        }

        private void Respawn(InputAction.CallbackContext context)
        {
            transform.localRotation = Quaternion.Euler(new Vector3(0, transform.localRotation.eulerAngles.y, 0));
            transform.position = spawnPos;
        }

        private void ToggleLight(InputAction.CallbackContext context)
        {
            statistics.light = !statistics.light;
        }

        private void TurnSignalsLeft(InputAction.CallbackContext context)
        {
            statistics.indicatorLeft = !statistics.indicatorLeft;
        }

        private void TurnSignalsRight(InputAction.CallbackContext context)
        {
            statistics.indicatorRight = !statistics.indicatorRight;
        }

        public void SetMovement(List<InputAction.CallbackContext> l_callbackContexts)
        {
            foreach (InputAction.CallbackContext cur_context in l_callbackContexts)
            {
                var direction = cur_context.ReadValue<Vector2>();
                // Horizontal
                statistics.angle = direction.x;
                    
                // Vertical
                float vertical = direction.y;
                if (vertical < 0)
                {
                    statistics.pedal = 0;
                    statistics.brake = vertical;
                }
                else
                {
                    statistics.brake = 0;
                    statistics.pedal = vertical;
                }
            }
        }
        
        void Start()
        {
			//playerMovementPlane = new Plane(transform.up, transform.position + transform.up);
           /*
           	scannedAxes.Add("Horizontal");
            scannedAxes.Add("Vertical");
            scannedButtons.Add("HandBrake");
            scannedButtons.Add("Reverse");
            scannedButtons.Add("Respawn");
            scannedButtons.Add("Light");
            scannedButtons.Add("IndicatorLeft");
            scannedButtons.Add("IndicatorRight");
            */
        }
        
        private void Awake()
        {
            playerControls = new UnityInputManager();
        }

        public bool KeyListContains(List<KeyOrder> list, KeyCode kc, PressMode mode)
        {
            foreach (KeyOrder ko in list)
            {
                if (ko.code == kc && ko.mode == mode) { return true; }
            }
            return false;
        }

        public bool KeyListContains(List<KeyOrder> list, string buttonName, PressMode mode)
        {
            foreach (KeyOrder ko in list)
            {
                if (ko.description == buttonName && ko.mode == mode) { return true; }
            }
            return false;
        }

        public float GetAxisInput(List<KeyOrder> list, string axisName)
        {
            foreach (KeyOrder ko in list)
            {
                if (ko.description == axisName)
                {
                    if (lastAxisValues.ContainsKey(axisName))
                    {
                        lastAxisValues[axisName] = ko.axisValue;
                    }
                    else
                    {
                        lastAxisValues.Add(axisName, ko.axisValue);
                    }
                    return ko.axisValue;
                }
            }

            if (lastAxisValues.ContainsKey(axisName))
            {
                return lastAxisValues[axisName];
            }
            return 0;
        }

        // Update is called once per frame
        void Update()
        {
            #region Key Delay
            float frameDelay = 0;
            float inputDelay = 0;
            float bufferDelay = 0;
            
            //This is used to unpause the game through the Resume button of the pause menu
            if (Settings.paused)
            {
                if(playerControls.CarInputs.Drive.enabled)
                    playerControls.CarInputs.Drive.Disable();
            }
            else
            {
                if (!playerControls.CarInputs.Drive.enabled)
                    playerControls.CarInputs.Drive.Enable();
            }

            if (delayBeacons.Count == 0)
            {
                frameDelay = Settings.displayTimeDelay;
                inputDelay = Settings.inputTimeDelay;
                bufferDelay = bufferedDisplay.delayTime;
            }
            else
            {
                frameDelay = float.MaxValue;
                inputDelay = float.MaxValue;
                bufferDelay = float.MaxValue;
                bool hasFrameValue = false;
                bool hasInputValue = false;
                bool hasBufferValue = false;
                foreach (ScenarioDelayBeacon beacon in delayBeacons)
                {
                    float frameDelayBeacon = 0;
                    float inputDelayBeacon = 0;
                    float bufferDelayBeacon = 0;

                    float distance = Vector3.Distance(beacon.transform.position, transform.position);
                    float lastDist = 0;
                    foreach (KeyValuePair<float, float> kvp in beacon.frameDelayZones)
                    {
                        if (kvp.Key < distance)
                        {
                            if (!hasFrameValue)
                            {
                                frameDelayBeacon = kvp.Value;
                                hasFrameValue = true;
                            }
                            else if (kvp.Key > lastDist)
                            {
                                frameDelayBeacon = kvp.Value;
                            }
                        }
                    }
                    lastDist = 0;
                    foreach (KeyValuePair<float, float> kvp in beacon.inputDelayZones)
                    {
                        if (kvp.Key < distance)
                        {
                            if (!hasInputValue)
                            {
                                inputDelayBeacon = kvp.Value;
                                hasInputValue = true;
                            }
                            else if (kvp.Key > lastDist)
                            {
                                inputDelayBeacon = kvp.Value;
                            }
                        }
                    }
                    lastDist = 0;
                    foreach (KeyValuePair<float, float> kvp in beacon.bufferDelayZones)
                    {
                        if (kvp.Key < distance)
                        {
                            if (!hasBufferValue)
                            {
                                bufferDelayBeacon = kvp.Value;
                                hasBufferValue = true;
                            }
                            else if (kvp.Key > lastDist)
                            {
                                bufferDelayBeacon = kvp.Value;
                            }
                        }
                    }

                    frameDelay = Mathf.Min(frameDelay, frameDelayBeacon);
                    inputDelay = Mathf.Min(inputDelay, inputDelayBeacon);
                    bufferDelay = Mathf.Min(bufferDelay, bufferDelayBeacon);
                }
            }

            Settings.displayTimeDelay = frameDelay;
            Settings.inputTimeDelay = inputDelay;
            Settings.frameBufferDelay = bufferDelay;
        
            bufferedDisplay.delayTime = bufferDelay;
            
            delay = inputDelay;

            // Prepare collections for input delay
            List<KeyOrder> currentInputs = new List<KeyOrder>();
            float timestamp = Time.fixedTime;

            List<Tuple<Vector2,float>> l_actions_performed = new List<Tuple<Vector2,float>>();
            foreach (var entry in ml_inputdirection_timestamp)
            {
                if (entry.Item2 + delay == timestamp)
                {
                    l_actions_performed.Add(new Tuple<Vector2, float>(entry.Item1, entry.Item2));
                    statistics.angle = entry.Item1.x;

                    // Vertical
                    float vertical = entry.Item1.y;
                    if (vertical < 0)
                    {
                        statistics.pedal = 0;
                        statistics.brake = vertical;
                    }
                    else
                    {
                        statistics.brake = 0;
                        statistics.pedal = vertical;
                    }
                }
            }
            foreach (var entry in l_actions_performed)
            {
                ml_inputdirection_timestamp.Remove(entry);
            }

            #endregion
            
        }
         private void OnEnable()
        {
            playerControls.CarInputs.Drive.performed += Drive;
            playerControls.CarInputs.Drive.canceled += StopDrive;

            playerControls.CarInputs.Pause.performed += Pause;
            playerControls.CarInputs.Reverse.performed += Reverse;
            playerControls.CarInputs.Handbrake.performed += Handbrake;
            playerControls.CarInputs.Respawn.performed += Respawn;
            playerControls.CarInputs.SwitchLights.performed += ToggleLight;
            playerControls.CarInputs.TurnSignalLeft.performed += TurnSignalsLeft;
            playerControls.CarInputs.TurnSignalRight.performed += TurnSignalsRight;
            playerControls.CarInputs.Enable();
        }

        private void OnDisable()
        {
            playerControls.CarInputs.Drive.performed -= Drive;
            playerControls.CarInputs.Drive.canceled -= StopDrive;

            playerControls.CarInputs.Pause.performed -= Pause;
            playerControls.CarInputs.Reverse.performed -= Reverse;
            playerControls.CarInputs.Handbrake.performed -= Handbrake;
            playerControls.CarInputs.Respawn.performed -= Respawn;
            playerControls.CarInputs.SwitchLights.performed -= ToggleLight;
            playerControls.CarInputs.TurnSignalLeft.performed -= TurnSignalsLeft;
            playerControls.CarInputs.TurnSignalRight.performed -= TurnSignalsRight;
            playerControls.CarInputs.Disable();
        }
    }
}
