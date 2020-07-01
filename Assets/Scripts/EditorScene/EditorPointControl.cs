using Assets.Scripts.SUMOConnectionScripts.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.EditorScene
{
    public class EditorPointControl:MonoBehaviour
    {
        public EditorPoint StreetPointPrefab;
        public EditorPoint BuildingPointPrefab;
        public EditorPoint CrossroadPointPrefab;
        public EditorPoint TerrainPointPrefab;

        public List<EditorPoint> Points = new List<EditorPoint>();
        public EditorPoint SelectedPoint = null;
        public GameObject SelectionArrow;
        public InputField HeightInput;
        public Text AffectedNodes;
        public Material LineMaterial;
        public Toggle MoveAssociatedToggle;
        public Dictionary<string, List<EditorPoint>> SameNodes = new Dictionary<string, List<EditorPoint>>();
        private List<LineRenderer> lines = new List<LineRenderer>();

        public void Awake()
        {
            MapRasterizer.Instance.EditorControl = this;
            Points.Clear();
            SameNodes.Clear();
        }

        public void MoveSelected(float delta)
        {
            if(SelectedPoint!=null)
            {
                SelectedPoint.MoveTo(SelectedPoint.transform.position + new Vector3(0, delta, 0), MoveAssociatedToggle.isOn);
                HeightInput.text = SelectedPoint.transform.position.y.ToString();
                HeightInput.GetComponent<Image>().color = Color.white;
            }
        }

        public void Save()
        {
            XElement root = new XElement("root");
            List<XElement> contents = new List<XElement>();
            foreach (var ep in Points)
            {
                ep.GenerateTags(contents);
            }
            foreach (var elem in contents)
            {
                root.Add(elem);
            }
            XDocument doc = new XDocument(root);
            doc.Save(Settings.correctionPath);
        }

        public void UpdateInputField()
        {
            float heightValue;
            try
            {
                heightValue = float.Parse(HeightInput.text);
                HeightInput.GetComponent<Image>().color = Color.white;
                if (SelectedPoint != null)
                {
                    Vector3 newpos = SelectedPoint.transform.position;
                    newpos.y = heightValue;
                    SelectedPoint.MoveTo(newpos, MoveAssociatedToggle.isOn);
                    SelectionArrow.transform.position = SelectedPoint.transform.position;
                }
            }
            catch (Exception)
            {
                HeightInput.GetComponent<Image>().color = Color.red;
            }
        }

        public void SelectPoint(EditorPoint point)
        {
            foreach (LineRenderer renderer in lines)
            {
                GameObject.Destroy(renderer.gameObject);
            }
            lines.Clear();
            if (point == null)
            {
                HeightInput.text = "0";
                HeightInput.GetComponent<Image>().color = Color.white;
                AffectedNodes.text = "<none>";
                SelectionArrow.SetActive(false);
                
            }
            else
            {
                SelectedPoint = point;
                HeightInput.text = SelectedPoint.transform.position.y.ToString();
                HeightInput.GetComponent<Image>().color = Color.white;
                SelectionArrow.SetActive(true);
                SelectionArrow.transform.position = SelectedPoint.transform.position;
                string nodeText = "";
                int i = 0;
                foreach (string nodeid in SelectedPoint.OsmIds)
                {
                    nodeText += nodeid+ "  [ "+SelectedPoint.FullIds[i]+" ]" + "\n";
                    i++;
                }
                AffectedNodes.text = nodeText;
                foreach (EditorPoint ep in point.AssociatedPoints)
                {
                    GameObject lineObject = new GameObject();
                    LineRenderer renderer = lineObject.AddComponent<LineRenderer>();
                    renderer.startColor = Color.green;
                    renderer.endColor = Color.cyan;
                    renderer.material = LineMaterial;
                    renderer.SetPositions(new Vector3[] { point.transform.position,ep.transform.position });
                    renderer.startWidth = 0.2f;
                    renderer.endWidth = 0.2f;
                    lines.Add(renderer);
                }
            }
        }

        public void Associate(EditorPoint newpoint, List<EditorPoint> otherPoints)
        {
            foreach (EditorPoint other in otherPoints)
            {
                newpoint.AssociatedPoints.Add(other);
                other.AssociatedPoints.Add(newpoint);
            }
            otherPoints.Add(newpoint);
        }

        public void AddPoint(Vector3 location, EditorPointType type, List<string> ids, List<string> fullIds, List<MapPoint> associatedPoints)
        {
            EditorPoint point = null;
            bool needsAssociation = false;
            switch (type)
            {
                case EditorPointType.TerrainPoint:
                case EditorPointType.AdditionalPoint:
                    point = GameObject.Instantiate(TerrainPointPrefab);
                    break;
                case EditorPointType.StreetPoint:
                    point = GameObject.Instantiate(StreetPointPrefab);
                    break;
                case EditorPointType.BuildingPoint:
                    point = GameObject.Instantiate(BuildingPointPrefab);
                    break;
                case EditorPointType.CrossroadPoint:
                    point = GameObject.Instantiate(CrossroadPointPrefab);
                    break;
            }
            if (ids.Count > 0)
            {
                if (SameNodes.ContainsKey(ids.First()))
                {
                    needsAssociation = true;
                }
                else
                {
                    var newlist = new List<EditorPoint>();
                    newlist.Add(point);
                    SameNodes.Add(ids.First(), newlist);
                }
            }
            point.transform.position = location;
            point.OsmIds = ids;
            point.MapPoints = associatedPoints;
            point.FullIds = fullIds;
            if(needsAssociation)
            { 
                Associate(point, SameNodes[ids.First()]);
            }
            
            Points.Add(point);
            

        }

    }
}
