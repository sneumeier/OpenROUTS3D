using UnityEngine;
using System;
using System.Linq;
using System.Collections;

namespace CarScripts
{
    [RequireComponent(typeof(AudioSource))]
    public class WheelDrive : MonoBehaviour
    {
        // StatisticsContainer for data exchange
        public StatisticsContainer carStatistic;

        // Rigidbody needed for velocity
        public Rigidbody rb;

        // Wheel collider Array where the four Wheel collider are stored
        public WheelCollider[] wheelColliders;
        public Transform[] wheelMeshes;

        // Downforce per m/s
        public float downforceMultiplier;

        // Animation Curve Steering
        public AnimationCurve steeringSpeed;
        public AnimationCurve steeringAngleLimit;

        // Animation Curve acceleration
        public AnimationCurve accelCurve;

        // Animation Curve acceleration backwards
        public AnimationCurve backAccelCurve;

        // Animation Curve braking
        public AnimationCurve brakeCurve;

        // Acceleration for multilayer
        private Vector3 oldMomentum = Vector3.zero;
        public Vector3 acceleration;

        // center of mass
        public GameObject centerOfMass;

        private bool engineStarted;

        public AudioSource engineSound;

        public AudioSource ignitionSound;

        void Start()
        {
            rb.centerOfMass = centerOfMass.GetComponent<Transform>().localPosition;
        }

        void FixedUpdate()
        {
            CalculateAcceleration();

            CalculateVelocity();

            CalculateSteering();

            CalculateTorque();

            if (!engineStarted && carStatistic.pedal != 0)
            {
                engineStarted = true;
                ignitionSound.Play();
                StartCoroutine(StartFade(ignitionSound, engineSound, 1.3f));
            }

            if (!ignitionSound.isPlaying)
            {
                foreach (var wheel in wheelColliders)
                {
                    Steering(wheel);

                    Acceleration(wheel);

                    Braking(wheel);

                    ApplyWeatherEffects(wheel);

                    DetectGroundMaterial(wheel);
                }
            }

            AddDownforce();
        }

        void ApplyWeatherEffects(WheelCollider wheel)
        {
            WheelFrictionCurve wheelFrictionCurve = wheel.forwardFriction;
            wheelFrictionCurve.stiffness = carStatistic.stiffness;
            wheel.forwardFriction = wheelFrictionCurve;
            wheelFrictionCurve = wheel.sidewaysFriction;
            if (wheel.name.Equals("Front_Left") || wheel.name.Equals("Front_Right"))
            {
                wheelFrictionCurve.stiffness = carStatistic.stiffness;
            }
            else
            {
                // back wheels
                wheelFrictionCurve.stiffness = carStatistic.stiffness * carStatistic.backWheelStiffnessFactor;
                ;
            }
            wheel.sidewaysFriction = wheelFrictionCurve;
        }

        void DetectGroundMaterial(WheelCollider wheel)
        {
            if (wheel.GetGroundHit(out WheelHit hit))
            {
                if (hit.collider.material != null && (hit.collider.material.name.StartsWith("Road") || hit.collider.material.name.StartsWith("Offroad")))
                {
                    WheelFrictionCurve fFriction = wheel.forwardFriction;
                    fFriction.stiffness *= carStatistic.velocity == 0 ? hit.collider.material.staticFriction : hit.collider.material.dynamicFriction;
                    wheel.forwardFriction = fFriction;
                    WheelFrictionCurve sFriction = wheel.sidewaysFriction;
                    sFriction.stiffness *= carStatistic.velocity == 0 ? hit.collider.material.staticFriction : hit.collider.material.dynamicFriction;
                    wheel.sidewaysFriction = sFriction;
                }
            }
        }

        void Update()
        {
            // Rotate the wheels
            for (int i = 0; i < wheelMeshes.Length; i++)
            {
                RefreshVisuals(wheelColliders.ElementAt(i), wheelMeshes.ElementAt(i));
            }

            if (engineStarted)
            {
                EngineSound();
            }
        }

        void CalculateAcceleration()
        {
            acceleration = rb.velocity - oldMomentum;
            oldMomentum = rb.velocity;
        }
        void CalculateVelocity()
        {
            // Get velocity (in km/h) for GUI
            carStatistic.velocity = (int)rb.velocity.magnitude * 3.6f;
        }
        void CalculateSteering()
        {
            carStatistic.angle = carStatistic.maxAngle * carStatistic.angle;
            float steeringSpeed = this.steeringSpeed.Evaluate(carStatistic.velocity);
            carStatistic.wheelAngle = carStatistic.angle + (carStatistic.angle * steeringSpeed);
            float sgn = Mathf.Sign(carStatistic.wheelAngle);
            float steerLimit = steeringAngleLimit.Evaluate(carStatistic.velocity);
            carStatistic.wheelAngle = Mathf.Min(Math.Abs(carStatistic.wheelAngle), steerLimit) * sgn;
            carStatistic.angle = carStatistic.wheelAngle;
        }
        void CalculateTorque()
        {
            carStatistic.torque = carStatistic.pedal * carStatistic.maxTorque;
            if (!carStatistic.reverse && carStatistic.velocity >= carStatistic.maxVelocity || carStatistic.reverse && carStatistic.velocity >= carStatistic.maxVelocityBackwards)
            {
                carStatistic.torque = 0;
            }
        }
        void Steering(WheelCollider wheel)
        {
            if (wheel.name.Equals("Front_Left") || wheel.name.Equals("Front_Right"))
            {
                wheel.steerAngle = carStatistic.wheelAngle;
            }
        }

        void Acceleration(WheelCollider wheel)
        {
            float accel = carStatistic.reverse ? backAccelCurve.Evaluate(carStatistic.velocity / carStatistic.maxVelocityBackwards) : accelCurve.Evaluate(carStatistic.velocity / carStatistic.maxVelocity);

            if (carStatistic.reverse)
            {
                wheel.motorTorque = carStatistic.torque * -1 * accel;
            }
            else
            {
                wheel.motorTorque = carStatistic.torque * accel;
            }
        }

        void Braking(WheelCollider wheel)
        {
            float brake = brakeCurve.Evaluate(carStatistic.velocity / carStatistic.maxVelocity);

            wheel.brakeTorque = carStatistic.brake * carStatistic.maxBrakeTorque * -1 * brake;

            if (carStatistic.handBrake)
            {
                wheel.brakeTorque += carStatistic.handBrakeTorque;
            }
        }
        void AddDownforce()
        {
            rb.AddForce(-transform.up * downforceMultiplier * carStatistic.velocity);
        }
        void RefreshVisuals(WheelCollider wheel, Transform visualWheel)
        {
            Quaternion quat;
            Vector3 position;
            wheel.GetWorldPose(out position, out quat);
            visualWheel.transform.position = position;
            visualWheel.transform.rotation = quat;
        }
        void EngineSound()
        {
            var min_pitch = 0.4f;
            var delta = 1.0f;
            var pitch = carStatistic.velocity <= (carStatistic.maxVelocity - delta) ? (1 - min_pitch) / (carStatistic.maxVelocity - delta) * carStatistic.velocity + min_pitch : 1;
            engineSound.pitch = pitch;
        }

        public static IEnumerator StartFade(AudioSource first, AudioSource second, float startTime)
        {
            float currentTime = 0;
            float dt = first.clip.length - startTime;
            float dvFirst = first.volume;
            float dvSecond = second.volume;
            second.volume = 0;
            second.Play();
            while (first.isPlaying)
            {
                if (first.time >= startTime)
                {
                    currentTime += Time.fixedDeltaTime;
                    first.volume = Mathf.Max(Mathf.Lerp(dvFirst, 0, currentTime / dt), 0);
                    second.volume = Mathf.Min(Mathf.Lerp(0, dvSecond, currentTime / dt), dvSecond);
                }
                yield return null;
            }
            first.volume = 0;
            second.volume = dvSecond;
            yield break;
        }

    }
}