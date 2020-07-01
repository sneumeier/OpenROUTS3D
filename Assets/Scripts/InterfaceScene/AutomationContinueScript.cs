using Assets.Scripts.CarScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.InterfaceScene
{
    public class AutomationContinueScript : MonoBehaviour
    {
        public StatisticsLogger logger;

        public float expectedPresstime;

        private float presstime = 0;
        
        private UnityInputManager uiControls;
        private bool mb_skipScene;


        // Update is called once per frame
        void Update() {
            if(!mb_skipScene)
            {
                presstime = 0;
            }
            mb_skipScene = false;
            if (presstime > expectedPresstime)
            {
                NextScene();
            }
	    }
        //SkipScene
        public void NextScene()
        {
            logger.WriteXML();
            AutorunDeserializer.NextScene();
            AutorunDeserializer.LoadScene();
        }

        private void SceneAction(InputAction.CallbackContext context)
        {
            mb_skipScene = true;
            presstime += Time.deltaTime;
            Debug.Log("preapring skip");
        }

        private void Awake()
        {
            uiControls = new UnityInputManager();
        }

        private void OnEnable()
        {
            uiControls.UI.Scene.Enable();
            uiControls.UI.Scene.performed += SceneAction;
        }

        private void OnDisable()
        {
            uiControls.UI.Scene.Disable();
            uiControls.UI.Scene.performed -= SceneAction;
        }
    }
}