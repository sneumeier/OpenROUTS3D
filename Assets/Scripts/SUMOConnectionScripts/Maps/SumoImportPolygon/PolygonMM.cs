using SumoImportPolygon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SumoImportPolygon
{


    /// <summary>
    /// This class holds all relevant information about one polygon
    /// (retrieved from *.poly.xml).
    /// </summary>
    public class PolygonMM
    {
        public string osmIdentifier;

        private string type;

        private Color color;

        private float layer;

        private List<Vector2> listPolygonPoints;

        public PolygonMM(string type, Color color, float layer, List<Vector2> listPolygonPoints, string id)
        {
            this.type = type;
            this.color = color;
            this.layer = layer;

            if (listPolygonPoints != null)
            {
                this.listPolygonPoints = listPolygonPoints;
            }
            else
            {
                this.listPolygonPoints = new List<Vector2>();
            }
            this.osmIdentifier = id;
        }

        public List<Vector2> ListPolygonPoints
        {
            get
            {
                return listPolygonPoints;
            }

            set
            {
                listPolygonPoints = value;
            }
        }

        public string Type
        {
            get
            {
                return type;
            }

            set
            {
                type = value;
            }
        }

        public Color Color
        {
            get
            {
                return color;
            }

            set
            {
                color = value;
            }
        }

        public float Layer
        {
            get
            {
                return layer;
            }

            set
            {
                layer = value;
            }
        }

        public List<Vector2> GetListPolygonPoints()
        {
            return listPolygonPoints;
        }

        public void SetListPolygonPoints(List<Vector2> value)
        {
            listPolygonPoints = value;
        }

        public override bool Equals(object obj)
        {
            var polygon = obj as PolygonMM;
            return polygon != null &&
                   base.Equals(obj) &&
                   type == polygon.type &&
                   EqualityComparer<Color>.Default.Equals(color, polygon.color) &&
                   EqualityComparer<List<Vector2>>.Default.Equals(listPolygonPoints, polygon.listPolygonPoints);
        }

        public override int GetHashCode()
        {
            var hashCode = -418496005;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(type);
            hashCode = hashCode * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(color);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<Vector2>>.Default.GetHashCode(listPolygonPoints);
            return hashCode;
        }

    }

}
