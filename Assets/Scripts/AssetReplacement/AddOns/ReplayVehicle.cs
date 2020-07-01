using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.AssetReplacement.AddOns
{
    public class ReplayVehicle
    {
        public GameObject replayVehicle;
        public float spawnTime;
        public float despawnTime;
        public List<Tuple<float, Vector3, Vector3>> timedPositions = new List<Tuple<float, Vector3, Vector3>>();//Timestamp ; Position ; Rotation

        private float lastRequestedTime = 0;
        private int lastIndex = 0;


        public void ApplyTime(float virtualTime)
        {
            if (virtualTime < spawnTime || virtualTime > despawnTime)
            {
                replayVehicle.SetActive(false);
                return;
            }
            else
            {
                replayVehicle.SetActive(true);
            }
            int startIndex = 1;
            if (lastRequestedTime < virtualTime)
            {
                startIndex = lastIndex;
            }

            int currentIndex = Math.Max(startIndex - 1, 0);

            while (currentIndex < timedPositions.Count)
            {
                float nextTimestamp = timedPositions[currentIndex].Item1;

                if (nextTimestamp > virtualTime)
                {
                    if (currentIndex>=1)
                    {
                        float pastTimestamp = timedPositions[currentIndex-1].Item1;
                        //TODO: Virtual time is between old timestamp and new timestamp. Find out with wich factor the old timestamp has to be weighted and with wich factor the new one has to be weighted
                        float timeDistancePre = virtualTime - pastTimestamp;
                        float timeDistanceFrames = nextTimestamp - pastTimestamp;

                        float weightPost = timeDistancePre / timeDistanceFrames;
                        float weightPre = 1 - weightPost;

                        Quaternion quat = Quaternion.Lerp(Quaternion.Euler(timedPositions[currentIndex - 1].Item3), Quaternion.Euler(timedPositions[currentIndex].Item3), weightPost);
                        //Apply with fractions
                        ApplyState(timedPositions[currentIndex].Item2* weightPost + timedPositions[currentIndex-1].Item2* weightPre,
                            quat
                            );
                        return;
                    }
                    ApplyState(timedPositions[currentIndex].Item2, Quaternion.Euler(timedPositions[currentIndex].Item3));
                    return;
                }

                lastRequestedTime = nextTimestamp;
                lastIndex = currentIndex;
                currentIndex++;
            }
            Debug.Log("End of Logfile");
            return;
        }

        public void ApplyState(Vector3 position, Quaternion rotation)
        {
            replayVehicle.transform.position = position;
            replayVehicle.transform.rotation = rotation;
        }

    }
}
