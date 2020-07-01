using Assets.Scripts.CarScripts;
using Assets.Scripts.SUMOConnectionScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CarScripts
{

    public class CollisionDetection : MonoBehaviour
    {
        public delegate void CollisionDetectedEvent(Collision col);
        public event CollisionDetectedEvent OnCollisionDetected;

        public AudioClip crash;
        private float deltaTime;
        public StatisticsContainer container;
        void Start()
        {
            GetComponents<AudioSource>()[1].playOnAwake = false;
            GetComponents<AudioSource>()[1].clip = crash;
        }
        void OnCollisionEnter(Collision col)
        {
            var time = Time.time;
            float impact = col.impulse.magnitude;
            if (time - deltaTime > container.CrashDeltaTime && impact > container.CrashImpact)
            {
                Debug.Log("Impact = " + impact);
                float volumeModifer = 1.0f;
                float impactSqrt = Mathf.Sqrt(impact);
                if (impactSqrt < 25) {
                    volumeModifer = impactSqrt / 25;
                }
                GetComponents<AudioSource>()[1].volume = volumeModifer;
                GetComponents<AudioSource>()[1].Play();
                deltaTime = Time.time;
                OnCollisionDetected?.Invoke(col);
            }
            
            StreetSegmentMesh ssm = col.gameObject.GetComponent<StreetSegmentMesh>();
            if (ssm != null) {
                gameObject.GetComponent<SumoEgoCar>().RoadSegments=ssm.lanes;
                Debug.Log("Entered a new Street Segment");
            }
        }
    }
}