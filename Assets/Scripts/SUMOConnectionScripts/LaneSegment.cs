using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.SUMOConnectionScripts
{
    /// <summary>
    /// Representing one single segment of a lane of the subdivided roadnet
    /// <para>laneId: sumo-id of the corresponding subdivided lane</para>
    /// <para>edgeIndex: index of the segment on the subdivided lane</para>
    /// </summary>
    public struct LaneSegment
    {
        public string laneId;
        public int edgeIndex;

        public Vector3 ownPosition;

        public float GetVehicleHeight(Vector3 position,Vector3 otherPosition)
        {
            Vector2 pos2d = new Vector2(position.x, position.z);
            Vector2 start2d = new Vector2(ownPosition.x, ownPosition.z);
            Vector2 end2d = new Vector2(otherPosition.x, otherPosition.z);
            float distToStart = Vector2.Distance(start2d, pos2d);
            float distToEnd = Vector2.Distance(end2d, pos2d);
            float totalDist = distToStart + distToEnd;
            float normalizedStartDist = distToStart / totalDist;
            float normalizedEndDist = distToEnd / totalDist;
            float height = normalizedStartDist * ownPosition.y + normalizedEndDist * otherPosition.y;
            return height;
        }

        public float GetVehicleHeight(float fraction, Vector3 otherPosition)
        {
            float height = (1 - fraction) * ownPosition.y + fraction * otherPosition.y;
            return height;
        }
    }
}
