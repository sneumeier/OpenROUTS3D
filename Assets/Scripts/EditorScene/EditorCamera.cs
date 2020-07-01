using CarCameraScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.EditorScene
{
    public class EditorCamera : MonoBehaviour
    {
        public EditorPointControl PointControl;
        public float SpeedFactor;
        public float HeightMovementFactor;
        public MouseCamMover CamRotator;
        private bool terrainOn = true;
        public GameObject Terrain;
        
        public void Start()
        {
            Cursor.visible = true;
            Time.timeScale = 1;
        }

        public void Update()
        {
            float speedModifier = 1;
            if (Input.GetKey(KeyCode.Space))
            {
                speedModifier *= 20;
            }
            else if (Input.GetKey(KeyCode.LeftAlt))
            {
                speedModifier *= 0.1f;
            }

            CamRotator.enabled = Input.GetKey(KeyCode.Mouse1);

            if (Input.GetKeyDown(KeyCode.E))
            {
                terrainOn = !terrainOn;
                Terrain.SetActive(terrainOn);
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                PointControl.MoveAssociatedToggle.isOn = !PointControl.MoveAssociatedToggle.isOn;
            }

            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 500.0f))
                {
                    if (hit.transform != null)
                    {
                        EditorPoint ep = hit.transform.GetComponent<EditorPoint>();
                        if (ep != null)
                        {
                            PointControl.SelectPoint(ep);
                        }
                    }
                }
            }

            
            if (Input.GetKey(KeyCode.UpArrow))
            {
                PointControl.MoveSelected(speedModifier * HeightMovementFactor * Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                PointControl.MoveSelected( -speedModifier * HeightMovementFactor * Time.deltaTime);
            }

            Vector3 direction = Vector3.zero;
            if (Input.GetKey(KeyCode.W))
            {
                direction += transform.forward;
            }
            if (Input.GetKey(KeyCode.S))
            {
                direction -= transform.forward;
            }
            if (Input.GetKey(KeyCode.A))
            {
                direction -= transform.right;
            }
            if (Input.GetKey(KeyCode.D))
            {
                direction += transform.right;
            }
            if (Input.GetKey(KeyCode.LeftShift))
            {
                direction += transform.up;
            }
            if (Input.GetKey(KeyCode.LeftControl))
            {
                direction -= transform.up;
            }

            if (direction != Vector3.zero)
            {
                direction.Normalize();
                direction *= speedModifier * SpeedFactor;
                transform.position += direction * Time.deltaTime;
            }


        }

    }
}
