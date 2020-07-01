using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AssetReplacement.AddOns
{
    public class CustomWallTextures : MonoBehaviour, IAssetPackPreInit
    {

        public List<Material> customWalls = new List<Material>();
        public List<double> weights = new List<double>();

        public void PreInit()
        {
            Debug.Log("Adding custom wall materials");
            int i = 0;
            foreach(Material mat in customWalls)
            {
                double weight = 1;
                if (i < weights.Count)
                { 
                    weight = weights[i];
                }
                PrefabProvider.customFassades.Add(mat,weight);
                i++;
            }
        }
    }
}
