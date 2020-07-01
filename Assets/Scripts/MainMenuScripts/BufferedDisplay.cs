using CarCameraScripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenuScripts
{
    public class BufferedDisplay : MonoBehaviour
    {
        public float delayTime
        {
            get { return Settings.frameBufferDelay; }
            set
            {
                Settings.frameBufferDelay = value;
                if (value > 0)
                {
                    if (!active)
                    {
                        Activate();
                    }
                    CalculateBuffer();
                }
                else
                {
                    if (active)
                    {
                        Deactivate();
                    }
                }
            }
        }


        public int maxBufferSize = 256;
        private double _scaleFactor = 1;
        public double scaleFactor
        {
            get
            {
                return _scaleFactor;
            }
            set
            {
                Debug.Log("Setting Scale Factor");
                if (_scaleFactor != value)
                {
                    _scaleFactor = Math.Max(0.01, Math.Min(1, value));
                    if (initialized)
                    {
                        InitCamBuffer();
                    }
                    else {
                        Start();
                    }
                }
            }
        }
        private int _currentBufferSize;
        public int currentBufferSize
        {
            get
            {
                if (_currentBufferSize < 0 && scaleFactor < 1)
                {
                    return 2;
                }
                else return _currentBufferSize;
            }
            set { _currentBufferSize = value; }
        }
        public RawImage display;
        public Camera cam;
        public int currentReadIndex = 0;
        public DelayManager delayManager;

        private RenderTexture camTexture;
        private Rect camsize;
        private bool active = true;

        private int currentWriteIndex { get { return (currentReadIndex - 1 + currentBufferSize) % currentBufferSize; } }
        private int currentClearIndex { get { return (currentReadIndex - 2 + currentBufferSize) % currentBufferSize; } }

        private RenderTexture[] renderBuffer;
        private bool initialized = false;

        // Use this for initialization
        void Start()
        {
            renderBuffer = new RenderTexture[maxBufferSize];
            InitCamBuffer();
            initialized = true;
        }

        public void InitCamBuffer()
        {
            Debug.Log("Creating new camera Buffers");
            if (camTexture != null)
            {
                camTexture.DiscardContents();
                camTexture.Release();
            }
            camTexture = new RenderTexture((int)(Display.main.systemWidth * scaleFactor), (int)(Display.main.systemHeight * scaleFactor), 16);
            camTexture.Create();

            camsize = new Rect(0, 0, (int)(Display.main.systemWidth * scaleFactor), (int)(Display.main.systemHeight * scaleFactor));
            for (int i = 0; i < renderBuffer.Length; i++)
            {
                if (renderBuffer[i] != null)
                {
                    renderBuffer[i].DiscardContents();
                    renderBuffer[i].Release();
                }
                renderBuffer[i] = new RenderTexture((int)(Display.main.systemWidth * scaleFactor), (int)(Display.main.systemHeight * scaleFactor), 16);
                renderBuffer[i].Create();

            }
            display.texture = camTexture;
            currentReadIndex = 0;
        }

        public void Deactivate()
        {
            Debug.Log("Deactivating buffer");
            this.active = false;
            cam.gameObject.SetActive(false);
            display.enabled = false;
        }

        public void Activate()
        {
            Debug.Log("Activating buffer");
            this.active = true;
            cam.gameObject.SetActive(true);
            display.enabled = true;
        }

        public void CalculateBuffer()
        {
            int delayedFrames = (int)(60 * delayTime);//Assuming constant 60
            currentBufferSize = delayedFrames;
        }


        // Update is called once per frame
        void Update()
        {

            if (currentBufferSize > 0)
            {
                //display.texture = renderBuffer[currentReadIndex];
                if (!delayManager.isDelayed)
                {
                    Graphics.CopyTexture(renderBuffer[currentReadIndex], camTexture);
                }

                cam.targetTexture = renderBuffer[currentWriteIndex];

                currentReadIndex = (currentReadIndex + 1) % currentBufferSize;
                //Debug.Log("Next Index: "+currentReadIndex);
            }
        }


    }
}