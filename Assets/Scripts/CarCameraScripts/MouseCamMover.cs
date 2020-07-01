using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CarCameraScripts
{

    public class MouseCamMover : MonoBehaviour
    {
        public float speedH = 2.0f;
        public float speedV = 2.0f;

        private float yaw = 0;
        private float pitch = 0.0f;

        public Camera centerCam;
        public bool createAdditionalCams;

 		public UnityInputManager CameraPlayerControls;
        public void Start()
        {
            int screencount = Display.displays.Length;
            int i = 1;
            float camFoV = centerCam.fieldOfView * ((float)(Display.displays[0].renderingHeight) / Display.displays[0].renderingWidth);
            float currentOffset = camFoV;
            bool rightHanded = true;
            Cursor.visible = false;
            Time.timeScale = 1;
            while (i < screencount && createAdditionalCams)
            {
                Display.displays[i].Activate();
                GameObject newcamObject = GameObject.Instantiate(centerCam.gameObject);
                newcamObject.transform.SetParent(this.transform);
                newcamObject.transform.localPosition = Vector3.zero;
                if (rightHanded)
                {
                    newcamObject.transform.rotation = Quaternion.Euler(0, currentOffset, 0);
                }
                else
                {
                    newcamObject.transform.rotation = Quaternion.Euler(0, -currentOffset, 0);
                }
                Camera newcam = newcamObject.GetComponent<Camera>();
                newcam.targetDisplay = i;
                newcam.enabled = true;

                rightHanded = !rightHanded;
                if (rightHanded)
                {
                    currentOffset += camFoV;
                }
                i++;
            }
        }

        // Update is called once per frame
        private void CameraMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (!Settings.paused)
            {
                var directon = context.ReadValue<Vector2>();
                
                yaw += directon.x / 100;
                pitch -= directon.y / 100;
                transform.localEulerAngles = new Vector3(pitch, yaw, 0.0f);
            }
        }        
		private void Awake()
        {
            CameraPlayerControls = new UnityInputManager();
        }

        // Update is called once per frame
        void Update()
        {
           
        }

        private void OnEnable()
        {
            CameraPlayerControls.Enable();
            CameraPlayerControls.CarCamera.Look.performed += CameraMove;
        }

        private void OnDisable()
        {
            CameraPlayerControls.CarCamera.Look.performed -= CameraMove;
            CameraPlayerControls.Disable();
        }
    }
}