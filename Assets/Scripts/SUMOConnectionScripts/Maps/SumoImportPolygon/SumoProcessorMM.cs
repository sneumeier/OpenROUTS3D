using Assets.Scripts.SUMOConnectionScripts.Maps;
using Assets.Scripts.SUMOConnectionScripts.Maps.SumoImportPolygon;
using SUMOConnectionScripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;

namespace SumoImportPolygon
{

    /// <summary>
    /// Starting point for all actions to read polygon data and create meshes
    /// for display in Unity (buildings etc.)
    /// </summary>
    public class SumoProcessorMM : MonoBehaviour
    {
        //public GameObject defaultTerrain;

        public GameObject prefabPlane;
        public GameObject prefabBuilding;
        public GameObject prefabTree;
        public SumoUnityConnection connection;

        private SceneAreaDimension sad;

        public Material flatRoofMaterial;
        public Material wallMaterial;
        public Material pointedRoofMaterial;
        public Material grassMaterial;
        public Material cityMaterial;

        // Initialization
        void Start()
        {
            //set DelaunayGenerator initially to false
            connection.HeightGenerator.gameObject.SetActive(false);

            if (!connection.IsInit)
            {
                connection.Init();
            }
            this.sad = new SceneAreaDimension();

            string filepath = Settings.polyPath;
            if (!File.Exists(filepath))
            {
                return;
            }
            XElement rootElement = XElement.Load(filepath);
            //
            List<PolygonMM> polygons = ImportPolygons(rootElement);

            Building3D.pointedRoofMaterial = pointedRoofMaterial;
            Building3D.wallMaterial = wallMaterial;
            Building3D.flatRoofMaterial = flatRoofMaterial;

            PolyDrawerMM pd = new PolyDrawerMM(connection.importer, prefabTree);
            pd.grassMaterial = grassMaterial;
            pd.cityMaterial = cityMaterial;
            pd.DrawAllPolygons(polygons, this, connection.osmParser);

            if (connection.osmParser!=null)
            { 
                MapRasterizer.Instance.FinalizePointCloud();
                connection.HeightGenerator.GenerateTerrain();
                connection.HeightGenerator.GetComponent<MeshRenderer>().material = cityMaterial;
                // If there is height information in the map activate delaunay
                connection.HeightGenerator.gameObject.SetActive(true);
                
            }
            connection.importer.InstantiateSigns();
            if(MapRasterizer.Instance.IsEditor)
            {
                //We required the MeshCollider to be active for Street Signs to be instantiated, but we should be able to select nodes through the terrain now
                connection.HeightGenerator.GetComponent<MeshCollider>().enabled = false;
                
            }
        }

        /// <summary>
        /// Imports all polygons from given xml file into a list.
        /// </summary>
        /// <param name="rootElement">root element of xml to be processed</param>
        /// <returns>List of 2D polygons</returns>
        private List<PolygonMM> ImportPolygons(XElement rootElement)
        {
            List<PolygonMM> polygons = new List<PolygonMM>();

            foreach (XElement poly in rootElement.Elements("poly"))
            {
                //
                //Polygon polygon = new Polygon();
                string type = "";
                //
                Color color;
                string[] rgb;
                float layer;
                //
                List<Vector2> listPolygonPoints = new List<Vector2>();
                string[] vectors;

                //
                type = poly.Attribute("type").Value;

                rgb = poly.Attribute("color").Value.Split(',');
                try
                {
                    color = new Color(float.Parse(rgb[0]), float.Parse(rgb[1]), float.Parse(rgb[2]), 1.0f);
                }
                catch (Exception)
                {
                    color = UnityEngine.Color.white;
                    Debug.Log("Error while parsing following string: " + poly.ToString());
                }
                layer = float.Parse(poly.Attribute("layer").Value);

                vectors = poly.Attribute("shape").Value.Split(' ');

                string id = poly.Attribute("id").Value.Split('_')[0].Split('#')[0];
                foreach (string vector in vectors)
                {
                    string[] xy = vector.Split(',');
                    Vector2 vector2 = new Vector2(float.Parse(xy[0]), float.Parse(xy[1]));
                    //
                    this.sad.CheckVector2D(vector2);
                    //
                    listPolygonPoints.Add(vector2);
                }

                polygons.Add(new PolygonMM(type, color, layer, listPolygonPoints, id));
            }

            //
            return polygons;
        }

        /// <summary>
        /// Giving back an obejct with x- and y-dimensions of generated scene.
        /// </summary>
        /// <returns>SceneAreaDimension with all x and y min-/max-values</returns>
        public SceneAreaDimension GetSceneAreaDimension()
        {
            return this.sad;
        }

    }
}