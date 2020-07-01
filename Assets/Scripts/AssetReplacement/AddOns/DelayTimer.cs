using CarScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.AssetReplacement.AddOns
{
    public class DelayTimer : MonoBehaviour
    {
        public Image img;
        public Text text;

        void Update()
        {
            float totaldelay = (Settings.frameBufferDelay + Settings.inputTimeDelay);

            if (totaldelay == 0)
            {
                text.text = "Keine Verzögerung";
                img.color = Color.cyan;
            }
            else if (totaldelay < 0.2) { 
                text.text = "Geringe Verzögerung";
                img.color = Color.green;
            }
            else if (totaldelay < 0.4)
            {
                text.text = "Mittlere Verzögerung";
                img.color = Color.yellow;
            }
            else if (totaldelay >= 0.4)
            {
                text.text = "Hohe Verzögerung";
                img.color = Color.red;
            }
            
        }
    }
}
