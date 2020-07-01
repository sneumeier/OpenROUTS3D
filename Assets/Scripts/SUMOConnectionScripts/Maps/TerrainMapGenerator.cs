using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.SUMOConnectionScripts.Maps
{
    public class TerrainMapGenerator:MonoBehaviour,HeightTerrainGenerator
    {
        public Terrain Terrain;
        private List<Terrain> UsedTerrains = new List<Terrain>();
        public float segmentSize;
        public int heightmapSize;
        public float heightmapScale;
        public float heightmapHeight;

        public void GenerateTerrain()
        {
            MapRasterizer rasterizer = MapRasterizer.Instance;

            float minX = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxZ = float.MinValue;

            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;

            foreach (MapPoint mp in rasterizer.MapPoints)
            {
                Vector3 vec = mp.transform.position;

                minX = Mathf.Min(minX, vec.x);
                minZ = Mathf.Min(minZ, vec.z);
                minHeight = Mathf.Min(minHeight, mp.height);

                maxX = Mathf.Max(maxX, vec.x);
                maxZ = Mathf.Max(maxZ, vec.z);
                maxHeight = Mathf.Max(maxHeight, mp.height);
            }
            float yOffset = minHeight;
            heightmapHeight = maxHeight - minHeight;

            heightmapScale = segmentSize / heightmapSize;

            float distX = (maxX - minX);
            float distZ = (maxZ - minZ);

            for (int indexX = 0; (indexX * segmentSize) - (1*heightmapScale) < distX; indexX++)
            {
                for (int indexZ = 0; (indexZ * segmentSize) - (1 * heightmapScale) < distZ; indexZ++)
                {
                    Vector3 offset = new Vector3();
                    offset.y = yOffset;
                    offset.x = (minX + (segmentSize*indexX));
                    offset.z = (minZ + (segmentSize * indexZ));

                    Terrain newTerrain = GameObject.Instantiate(Terrain.gameObject,this.transform).GetComponent<Terrain>();
                    newTerrain.terrainData = new TerrainData();

                    

                    Debug.Log("Heightmap Scale: "+heightmapScale+" Heightmap Height: "+heightmapHeight+" heightmapSize: "+heightmapSize);

                    newTerrain.terrainData.heightmapResolution = heightmapSize + 1;
                    

                    newTerrain.terrainData.size = new Vector3(segmentSize, heightmapHeight, segmentSize);

                    float[,] heights = GenerateHeights(offset,heightmapScale);
                    newTerrain.terrainData.SetHeights(0,0, heights);
                    newTerrain.transform.position = offset;
                    //newTerrain.transform.localScale = new Vector3(heightmapScale, 1, heightmapScale);
                    UsedTerrains.Add(newTerrain);

                    //Debugging terrain heights:
                    
                    for (int hx = 0; hx < heights.GetLength(0); hx+=10)
                    {
                        for (int hz = 0; hz < heights.GetLength(1); hz+=10)
                        {
                            float coordX = hx * heightmapScale + offset.x;
                            float coordZ = hz * heightmapScale + offset.z;
                            float coordY = heights[hx, hz] * heightmapHeight + offset.y;

                            GameObject testGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            testGo.transform.position = new Vector3(coordX,coordY,coordZ);
                            testGo.GetComponent<MeshRenderer>().material.color = Color.blue;
                        }
                    }
                    
                }
            }

            Terrain.gameObject.SetActive(false);
            //Debugging actual map points
            int mpIndex = 0;
            foreach (MapPoint mp in rasterizer.MapPoints)
            {
                mpIndex++;
                if (mpIndex % 10 == 0)
                {
                    GameObject testGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    testGo.GetComponent<MeshRenderer>().material.color = Color.green;
                    testGo.transform.SetParent(mp.transform,false);
                    testGo.transform.localPosition = new Vector3(0,mp.height,0);
                }
            }
        }



        public float[,] GenerateHeights(Vector3 offset,float scale)
        {
            
            MapRasterizer rasterizer = MapRasterizer.Instance;
            float xOffset = offset.x;
            float yOffset = offset.y;
            float zOffset = offset.z;
            Debug.Log("Generating Heights");
            float[,] heights = new float[heightmapSize+1, heightmapSize+1];
            for (int x = 0; x < heightmapSize+1; x++)
            {
                for (int z = 0; z < heightmapSize+1; z++)
                {
                    heights[x, z] = (rasterizer.GetHeightClosest(new Vector3(xOffset+x* scale, 0,zOffset+z* scale)) - yOffset) / heightmapHeight;//  / heightmapHeight
                }
            }
            return heights;
        }
    }
}
