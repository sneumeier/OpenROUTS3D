using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.MultiplayerMessages
{
    public class NetworkVehicle:MonoBehaviour
    {
        public Rigidbody rig;
        public Transform trans;
        public float lastPhysicsUpdate;    //Note that apply counts as update too, not just update
        public Vector3 acceleration;
        public Color color;

        public ulong highestId = 0;

        public string id;

        public void Start()
        {
            lastPhysicsUpdate = Time.time;
            
        }

        public void Apply(PlayerCarMessage pcm)
        {
            if (pcm.messageNr < highestId)
            {
                return;
            }
            acceleration = pcm.acceleration;
            rig.velocity = pcm.momentum;
            trans.position = pcm.position;
            trans.rotation = Quaternion.Euler(pcm.rotation);
            rig.angularVelocity = pcm.momentum;
            lastPhysicsUpdate = Time.time;
            highestId = pcm.messageNr;
        }

        public void Update()
        {
            float dt = Time.time - lastPhysicsUpdate;
            rig.velocity += acceleration * dt;
        }

    }
}
