using Assets.Scripts.EditorScene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.SUMOConnectionScripts.Maps
{
    public class MapRasterizer
    {
        public static MapRasterizer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MapRasterizer();
                }
                return _instance;
            }
        }

        private static MapRasterizer _instance = null;

        public int RelevantNeighbours = 1;
        public bool IsEditor = false;
        public EditorPointControl EditorControl = null;

        //KD-Tree Implementation: https://gist.github.com/ditzel/194ec800053ce7083b73faa1be9101b0
        public KdTree<MapPoint> MapPoints = new KdTree<MapPoint>(true);

        public void Init(bool isEditor)
        {
            IsEditor = isEditor;
            MapPoints.Clear();
        }

        public void FinalizePointCloud()
        {
            MapPoints.UpdatePositions();
        }

        public MapPoint AddMapPoint(Vector3 location, float height)
        {
            GameObject mapPointObject = new GameObject("mapPoint");
            MapPoint mp = (MapPoint)mapPointObject.AddComponent(typeof(MapPoint));
            mp.height = height;
            mp.transform.position = location;
            MapPoints.Add(mp);
            return mp;
        }

        public IEnumerable<MapPoint> GetNeighbours(Vector2 location)
        {
            return GetNeighbours(new Vector3(location.x,0,location.y));
        }

        public IEnumerable<MapPoint> GetNeighbours(Vector3 location)
        {
            return MapPoints.FindClosest(location,RelevantNeighbours);
        }

        public float GetHeightClosest(Vector3 location)
        {
            return MapPoints.FindClosest(location).height;
        }

        public float GetHeight(Vector3 location)
        {
            IEnumerable<MapPoint> mapPoints =  MapPoints.FindClosest(location, RelevantNeighbours);
            Dictionary<MapPoint, float> weights = new Dictionary<MapPoint, float>();
            float totalweight = 0;
            foreach (MapPoint mp in mapPoints)
            {
                float distance = (location - mp.transform.position).sqrMagnitude;
                float weight = 1 / distance;
                weights.Add(mp,weight);
                totalweight += weight;
            }
            float height = 0;
            foreach (KeyValuePair<MapPoint, float> kvp in weights)
            {
                float normalizedWeight = kvp.Value / totalweight;
                height += (kvp.Key.height / normalizedWeight);
            }
            return height;

        }


    }
}
