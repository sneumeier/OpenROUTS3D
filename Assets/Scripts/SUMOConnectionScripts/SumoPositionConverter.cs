using Assets.Scripts.OsmParser;
using Assets.Scripts.SUMOConnectionScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SUMOConnectionScripts
{
    public class SumoPositionConverter
    {
        private const float bezierFormFactor = 0.33f;

        /// <summary>
        /// Returns the position as percentage of the total lenght of the lane
        /// </summary>
        /// <param name="lanePosition"></param>
        /// <param name="lane"></param>
        /// <returns></returns>
        public float GetPercentage(float lanePosition, IList<Vector3> lane)
        {
            float laneLenght = 0;
            for (int i = 0; i < lane.Count - 1; i++)
            {
                laneLenght += Vector3.Magnitude(lane[i + 1] - lane[i]);
            }
            return lanePosition / laneLenght;
        }

        /// <summary>
        /// Sets the vehicle to a position on a given lane based on traveled distance as percentage.
        /// Values beyond 1 are treatended as 1, values below 0 as 0. The lane has to consist of at least two points.
        /// </summary>
        /// <param name="percentage"></param>
        /// <param name="lane"></param>
        /// <param name="vehicle"></param>
        public void ComputePositionOnLane(float percentage, IList<Vector3> lane, SumoVehicle vehicle, List<LaneSegment> laneSegments)
        {
            Vector3 position;

            // Clamp percentage to range 0..1
            percentage = (percentage <= 0) ? 0 : (percentage >= 1) ? 1 : percentage;

            // If lane has only two points, there is not much to do:
            if (lane.Count == 2)
            {
                position = Vector3.Lerp(lane[0], lane[1], percentage);
                position.y = laneSegments[0].GetVehicleHeight(percentage,laneSegments[1].ownPosition);
            }
            else
            {
                // We need to know the total length of the lane first
                float totalLaneLength = 0;

                for (int i = 0; i < lane.Count - 1; i++)
                {
                    totalLaneLength += Vector3.Magnitude(lane[i + 1] - lane[i]);
                }

                // Distance we have to travel along the lane
                float distance = totalLaneLength * percentage;

                // Find the index of the next vertice not reached yet
                int v = 0;
                float computeDist = 0;
                do
                {
                    computeDist += Vector3.Magnitude(lane[v + 1] - lane[v]);
                    v++;
                } while (computeDist < distance);

                LaneSegment endSegment = laneSegments[v];
                LaneSegment startSegment = laneSegments[v-1];

                // Now we can compute the exact position
                computeDist -= Vector3.Magnitude(lane[v] - lane[v - 1]);

                float restDist = distance - computeDist;
                float edgeDist = Vector3.Magnitude(lane[v] - lane[v - 1]);
                float edgePercentage = restDist / edgeDist;

                float osmHeight = startSegment.GetVehicleHeight(edgePercentage,endSegment.ownPosition);

                Vector3 p0, p1, p2, p3;

                if (v > 1) // 2..3..4..
                {
                    p0 = lane[v - 2];
                    p1 = lane[v - 1];
                    p2 = lane[v];
                }
                else // 1
                {
                    p0 = lane[0];
                    p1 = lane[0];
                    p2 = lane[1];
                }
                if (v < lane.Count - 1) // ..count-3..count-2
                {
                    p3 = lane[v + 1];
                }
                else // count-1
                {
                    p3 = lane[v];
                }

                position = InterpolateToCubicBezier(p0, p1, p2, p3, edgePercentage);
                position.y = osmHeight;
            }

            vehicle.SetPosition(position);
        }

        /// <summary>
        /// Gives the position of point t between b and c by estimating a bezier curve through the points a to d.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="t"></param>
        private Vector3 InterpolateToCubicBezier(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            Vector3 origin = new Vector3();

            Vector3 ac = c - a;
            Vector3 bd = b - d;

            float lengthBC = Vector3.Magnitude(c - b);

            Vector3 p1 = b + Vector3.Lerp(origin, ac, (lengthBC / Vector3.Magnitude(ac)) * bezierFormFactor);
            Vector3 p2 = c + Vector3.Lerp(origin, bd, (lengthBC / Vector3.Magnitude(bd)) * bezierFormFactor);

            return CubicDeCasteljau(b, p1, p2, c, t);
        }

        /// <summary>
        /// Implementation of De Casteljau's algorithm to compute a cubic bezier curve.
        /// </summary>
        private Vector3 CubicDeCasteljau(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float oneMinusT = 1f - t;

            Vector3 p02 = oneMinusT * (oneMinusT * p0 + t * p1) + t * (oneMinusT * p1 + t * p2);
            Vector3 p12 = oneMinusT * (oneMinusT * p1 + t * p2) + t * (oneMinusT * p2 + t * p3);

            return oneMinusT * p02 + t * p12;
        }

        /// <summary>
        /// returns percentage between begin and end 
        /// https://www.wolframalpha.com/input/?i=solve(+%7Bt+*E_1,+t*E_2,+t*E_3%7D+.+%7BP_1-(B_1%2Bt*E_1),+P_2-(B_2%2Bt*E_2),+P_3-(B_3%2Bt*E_3)%7D+%3D+0,+t)
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public float FindT(Vector3 begin, Vector3 end, Vector3 point)
        {
            Vector3 vector = end - begin;

            if (vector.x * vector.x + vector.y * vector.y + vector.z * vector.z != 0f)
            {
                float dividend = -1 * (begin.x * vector.x) - (begin.y * vector.y) - (begin.z * vector.z) + (vector.x * point.x) + (vector.y * point.y) + (vector.z * point.z);
                float divisor = vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;

                return dividend / divisor;
            }
            else
            {
                return 0;
            }
        }
    }
}

