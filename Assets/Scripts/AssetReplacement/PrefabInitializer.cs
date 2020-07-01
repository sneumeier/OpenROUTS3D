using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AssetReplacement
{
    public class PrefabInitializer : MonoBehaviour
    {

        public static Dictionary<string, GameObject> StaticPrefabs = new Dictionary<string, GameObject>();
        public static Dictionary<string, Material> StaticMaterials = new Dictionary<string, Material>();

        public string[] PrefabIdentifiers;
        public GameObject[] Prefabs;

        public string[] MaterialIdentifiers;
        public Material[] Materials;

        // Use this for initialization
        void Awake()
        {
            int size = Math.Min(Prefabs.Length, PrefabIdentifiers.Length);
            int i = 0;
            while (i < size)
            {
                StaticPrefabs.Add(PrefabIdentifiers[i], Prefabs[i]);
                i++;
            }


            size = Math.Min(Materials.Length, MaterialIdentifiers.Length);
            i = 0;
            while (i < size)
            {
                StaticMaterials.Add(MaterialIdentifiers[i], Materials[i]);
                i++;
            }
        }

        public static GameObject GetPrefab(string identifier)
        {
            if (StaticPrefabs.ContainsKey(identifier))
            {
                return StaticPrefabs[identifier];
            }
            else
            {
                return null;
            }
        }

        public static Material GetMaterial(string identifier)
        {
            if (PrefabProvider.customTextures.ContainsKey(identifier))
            {
                return PrefabProvider.customTextures[identifier];
            }
            else if (StaticMaterials.ContainsKey(identifier))
            {
                return StaticMaterials[identifier];
            }
            else
            {
                return null;
            }
        }

    }
}
