using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.CarScripts
{
    public struct KeyOrder   //struct containing a timestamp aswell as Key Code and press mode for processing outside the unity input manager
    {
        public KeyCode code;
        public PressMode mode;
        public float timestamp;
        public string description;
        public float axisValue;

        public KeyOrder(KeyCode code, PressMode mode, float timestamp)
        {
            this.code = code;
            this.mode = mode;
            this.timestamp = timestamp;
            axisValue = 0;
            description = null;
        }

        public KeyOrder(string buttonName, PressMode mode, float timestamp)
        {
            this.code = KeyCode.None;
            this.description = buttonName;
            this.mode = mode;
            this.timestamp = timestamp;
            axisValue = 0;
        }

        public KeyOrder(string axisName, float axisValue, float timestamp) {
            this.timestamp = timestamp;
            this.description = axisName;
            this.axisValue = axisValue;
            this.mode = PressMode.Axis;
            this.code = KeyCode.None;
        }

    }
}
