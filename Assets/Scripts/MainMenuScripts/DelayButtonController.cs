using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenuScripts
{
    public class DelayButtonController : MonoBehaviour
    {
        public Text delayInputFieldText;

        public void SetDelay()
        {
            string delayString = delayInputFieldText.text;
            Settings.displayTimeDelay = float.Parse(delayString);
        }

    }
}
