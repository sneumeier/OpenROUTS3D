using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AssetReplacement.AddOns
{
    public class CustomGroundTextures : MonoBehaviour, IAssetPackPreInit
    {
        public List<Material> customMaterials = new List<Material>();
        public List<string> descriptions = new List<string>();

        public void PreInit()
        {
            Debug.Log("Adding additional ground textures for OSM data");
            for (int i = 0; i<Math.Min(customMaterials.Count,descriptions.Count);i++)
            {
                PrefabProvider.customTextures.Add(descriptions[i],customMaterials[i]);
            }
        }
    }
}
