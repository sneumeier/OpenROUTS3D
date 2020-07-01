using Assets.Scripts.SUMOConnectionScripts.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace Assets.Scripts.EditorScene
{
    public class EditorPoint:MonoBehaviour
    {
        public EditorPointType Type;
        public List<string> OsmIds = new List<string>();
        public List<string> FullIds = new List<string>();
        public List<MapPoint> MapPoints = new List<MapPoint>();
        public List<EditorPoint> AssociatedPoints = new List<EditorPoint>();
        public bool Deactivated;
        public GameObject DeactivationModel;

        public bool CanMoveHorizontally
        {
            get { return Type == EditorPointType.TerrainPoint; }
        }

        public void MoveTo(Vector3 target, bool moveAssociated = false)
        {
            if (!CanMoveHorizontally)
            {
                transform.position = new Vector3(transform.position.x,target.y, transform.position.z);
            }
            else
            { 
                transform.position = target;
            }
            foreach (MapPoint mp in MapPoints)
            {
                mp.transform.position = transform.position;
            }
            if (moveAssociated)
            {
                foreach (EditorPoint otherPoint in AssociatedPoints)
                {
                    otherPoint.MoveTo(new Vector3(otherPoint.transform.position.x, target.y, otherPoint.transform.position.z), false);
                }
            }
        }

        public void GenerateTags(List<XElement> elementList)
        {
            int i = 0;
            foreach(string fullid in FullIds)
            { 
                float height = transform.position.y;
                XElement elem = new XElement("Node");
                if (CanMoveHorizontally)
                {
                    elem.Name = "TerrainNode";
                }

                XElement subelemId = new XElement("Id");
                subelemId.Value = OsmIds[i];
                elem.Add(subelemId);

                XElement fullidElem = new XElement("FullId");
                fullidElem.Value = fullid;
                elem.Add(fullidElem);

                XElement heightElement = new XElement("Height");
                heightElement.Value = height.ToString(System.Globalization.CultureInfo.InvariantCulture);
                elem.Add(heightElement);

                if(CanMoveHorizontally)
                { 
                    XElement elementX = new XElement("X");
                    elementX.Value = transform.position.x.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    elem.Add(elementX);

                    XElement elementZ = new XElement("Z");
                    elementZ.Value = transform.position.z.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    elem.Add(elementZ);

                    XElement elementDeactivated = new XElement("Deactivated");
                    elementDeactivated.Value = Deactivated.ToString();
                    elem.Add(elementDeactivated);
                }

                elementList.Add(elem);
                i++;
            }
        }

    }

    public enum EditorPointType
    {
        StreetPoint,
        CrossroadPoint,
        BuildingPoint,
        TerrainPoint,
        AdditionalPoint,
    }
}
