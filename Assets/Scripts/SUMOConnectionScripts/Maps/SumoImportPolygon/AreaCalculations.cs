using SumoImportPolygon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SumoImportPolygon
{

    /// <summary>
    /// This class is used for area calculations. Main puprose is to get the geometric centre
    /// of any polygon. Addiotnally it serves for determining the orientation of polygon coordinates
    /// (clockwise, or counter-clockwise).
    /// </summary>
    public class AreaCalculations {

        /// <summary>
        /// calculating centre of gravity of any given polygon (provided it is for 2D)
        /// (works ONLY with non-self-intersecting closed polygon defined by n vertices)
        /// 
        /// according: https://en.wikipedia.org/wiki/Centroid#Centroid_of_a_polygon                
        /// </summary>
        /// <param name="polygon">List of 2D polygons</param>
        /// <returns>centroid position as Vector2</returns>
        public static Vector3 CalculateCentrePosition2D(PolygonMM polygon)
        {

        Vector3 centrePosition = new Vector3(0, 0, 0);
        float xCentre = 0.0f;
        float yCentre = 0.0f;

        if (polygon.ListPolygonPoints == null || polygon.ListPolygonPoints.Count < 3)
        {
            return centrePosition;
        }

        float area = CalculatePolygonArea2D(polygon.ListPolygonPoints);

        for (int i = 0; i + 1 < polygon.ListPolygonPoints.Count; i++)
        {
            Vector2 vector = polygon.ListPolygonPoints[i];
            Vector2 vectorNext = polygon.ListPolygonPoints[i + 1];

            xCentre += (vector.x + vectorNext.x) * (vector.x * vectorNext.y - vectorNext.x * vector.y);
            yCentre += (vector.y + vectorNext.y) * (vector.x * vectorNext.y - vectorNext.x * vector.y);
        }

        //
        xCentre = xCentre / (6.0f * area);
        yCentre = yCentre / (6.0f * area);

        //
        centrePosition.x = Mathf.Abs(xCentre); // Mathf.Abs securing that all positions have absolute (positive) value and buildings are correctly positioned
        centrePosition.z = Mathf.Abs(yCentre);
        centrePosition.y = 1.0f;

        //
        return centrePosition;
    }

        /// <summary>
        /// calculating centre of gravity of any given polygon (provided it is 3D)
        /// (works ONLY with non-self-intersecting closed polygon defined by n vertices)
        /// 
        /// according:  <see cref="https://en.wikipedia.org/wiki/Centroid#Centroid_of_a_polygon">
        /// </summary>
        /// <param name="listPolygon">List of 3D polygons</param>
        /// <param name="Height">height of building</param>
        /// <returns>centroid position as Vector3</returns>
        public static Vector3 CalculateCentrePosition3D(List<Vector3> listPolygon, float Height)
        {

            Vector3 centrePosition = new Vector3(0, 0, 0);
            float xCentre = 0.0f;
            //float yCentre = 0.0f;
            float zCentre = 0.0f;

            if (listPolygon == null || listPolygon.Count < 3)
            {
                return centrePosition;
            }
            
            float area = GetArea3D(listPolygon);

            for (int i = 0; i + 1 < listPolygon.Count; i++)
            {
                Vector3 vector = listPolygon[i];
                Vector3 vectorNext = listPolygon[i + 1];

                //xCentre += (vector.x + vectorNext.x) * (vector.x * vectorNext.y - vectorNext.x * vector.y);
                //yCentre += (vector.y + vectorNext.y) * (vector.x * vectorNext.y - vectorNext.x * vector.y);
                xCentre += (vector.x + vectorNext.x) * (vector.x * vectorNext.z - vectorNext.x * vector.z);
                zCentre += (vector.z + vectorNext.z) * (vector.x * vectorNext.z - vectorNext.x * vector.z);
            }

            //
            xCentre = xCentre / (6.0f * area);
            //yCentre = yCentre / (6.0f * area);
            zCentre = zCentre / (6.0f * area);

            //
            centrePosition.x = Mathf.Abs(xCentre); // Mathf.Abs securing that all positions have absolute (positive) value and buildings are correctly positioned
                                                   //centrePosition.z = Mathf.Abs(yCentre);
                                                   //centrePosition.y = 1.0f;
            centrePosition.z = Mathf.Abs(zCentre);
            centrePosition.y = Mathf.Abs(Height);

            //
            return centrePosition;
        }

        /// <summary>
        /// calculating centre of gravity of any given polygon
        /// (works ONLY with non-self-intersecting closed polygon defined by n vertices)
        /// 
        /// according:  <see cref="https://en.wikipedia.org/wiki/Centroid#Centroid_of_a_polygon">
        /// </summary>
        /// <param name="listPolygonPoints">List of 2D polygons</param>
        /// <returns>polygon orientation as boolean</returns>
        public static float CalculatePolygonArea2D(List<Vector2> listPolygonPoints)
        {
            if (listPolygonPoints == null || listPolygonPoints.Count < 3)
            {
                return 0.0f;
            }

            float area = 0.0f;

            int count = listPolygonPoints.Count;

            for (int i = 0; i + 1 < listPolygonPoints.Count; i++)
            {
                Vector2 vector = listPolygonPoints[i];
                Vector2 vectorNext = listPolygonPoints[i + 1];

                area += vector.x * vectorNext.y - vectorNext.x * vector.y;
            }
            return Mathf.Abs(area / 2.0f);
        }

        /// <summary>
        /// Getting area of 3D object (with Vector3 coordinates)
        /// </summary>
        /// <param name="listPolygonPoints">List of 3D polygons</param>
        /// <returns>Area as float</returns>
        private static float GetArea3D(List<Vector3> listPolygonPoints)
        {
            return Mathf.Abs(CalculatePolygonArea3D(listPolygonPoints) / 2.0f);
        }

        /// <summary>
        /// calculating orientation of polygon coordinates
        /// 
        /// false = clockwise, area bigger than 0.0
        ///         polygon order has to be reversed
        /// true  = counter clockwise (needed for creating building bodies of polygons)
        ///         area smaller than 0.0
        ///         no changes to polygon order
        /// 
        /// according: <see cref="https://de.wikipedia.org/wiki/Gau%C3%9Fsche_Trapezformel#Beispiel">
        ///        or: <see cref="https://math.stackexchange.com/a/340860">
        ///
        /// </summary>
        /// <param name="listPolygonPoints">List of 3D polygons</param>
        /// <returns>polygon orientation as boolean</returns>
        public static bool CalculatePolygonOrientation3D(List<Vector3> listPolygonPoints)
        {
            return ((CalculatePolygonArea3D(listPolygonPoints) < 0.0f) ? false : true);
        }

        /// <summary>
        /// calculating centre of gravity of any given polygon
        /// (works ONLY with non-self-intersecting closed polygon defined by n vertices)
        /// 
        /// according:  <see cref="https://en.wikipedia.org/wiki/Centroid#Centroid_of_a_polygon">
        /// </summary>
        /// <param name="listPolygonPoints">List of 3D polygons</param>
        /// <returns>polygon area as float</returns>
        private static float CalculatePolygonArea3D(List<Vector3> listPolygonPoints)
        {
            if (listPolygonPoints == null || listPolygonPoints.Count < 3)
            {
                return 0.0f;
            }

            float area = 0.0f;

            int count = listPolygonPoints.Count;

            for (int i = 0; i + 1 < listPolygonPoints.Count; i++)
            {
                Vector3 vector = listPolygonPoints[i];
                Vector3 vectorNext = listPolygonPoints[i + 1];

                area += vector.x * vectorNext.z - vectorNext.x * vector.z;
            }
            return area;
        }

    }

}
