using UnityEngine;

namespace CarCameraScripts
{
    public class DelayManager : MonoBehaviour
    {
        public Camera Cam;
        private float nextFrame = 0;
        public bool isDelayed = false;
        private System.Random DelayRandom = new System.Random(0);

        private CameraClearFlags bufferedClearFlags;    //Saving initial clear flags to restore them later
        private int bufferedCullingMask;                //Culling mask is basically just an int32 where every bit is a flag

        void Start()
        {
            //Backing up initial values. Therefore the values are not hardcoded and if someone changes the camera data, they carry over after freezing and unfreezing the camera
            bufferedClearFlags = Cam.clearFlags;
            bufferedCullingMask = Cam.cullingMask;
            //Cam.useOcclusionCulling = false;  //has no bigger affect
        }

        void Update()
        {
            if (Settings.displayTimeDelay > 0 || Settings.packetLoss > 0)
            {
                if (!Settings.paused)
                {
                    bool renderFrame = false;
                    if (nextFrame < Time.fixedTime)
                    {
                        nextFrame = Mathf.Max(Settings.displayTimeDelay + nextFrame, Time.fixedTime + Settings.displayTimeDelay);
                        if (DelayRandom.NextDouble()>Settings.packetLoss)
                        {
                            //Unfreeze Camera
                            Cam.clearFlags = bufferedClearFlags;
                            Cam.cullingMask = bufferedCullingMask;
                            isDelayed = false;
                            renderFrame = true;
                        }
                        else
                        {
                            renderFrame = false;
                        }
                    }
                    if(!renderFrame)
                    {
                        //Freeze Camera
                        //Camera Freeze code adapted from the Unity Forum ( http://answers.unity3d.com/questions/909394/how-do-i-freeze-the-camera-frame.html )
                        Cam.clearFlags = CameraClearFlags.Nothing;
                        Cam.cullingMask = 0;
                        isDelayed = true;
                    }

                }
                else
                {
                    Cam.clearFlags = bufferedClearFlags;
                    Cam.cullingMask = bufferedCullingMask;
                    isDelayed = false;
                }
            }
        }

    }
}
