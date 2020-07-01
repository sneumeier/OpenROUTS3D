using SUMOConnectionScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AssetReplacement
{
    public static class PrefabProvider
    {
        public static int debugInt = 0;

        public static List<ReplaceableAsset> replaceablePrefabs = new List<ReplaceableAsset>();
        public static List<AdditionalAsset> additionalAssets = new List<AdditionalAsset>();
        public static List<List<AdditionalAsset>> randomizedAdditionalPrefabs = new List<List<AdditionalAsset>>();
        public static Dictionary<SumoVehicle, double> customCars = new Dictionary<SumoVehicle, double>();
        public static Dictionary<Material, double> customFassades = new Dictionary<Material, double>();
        public static Dictionary<string, Material> customTextures = new Dictionary<string, Material>();

        public static List<ReplacementGameObject> replacement = new List<ReplacementGameObject>();

        public static List<AssetPack> loadedAssetPacks = new List<AssetPack>();

        public static List<GameObject> ToBeDestroyed = new List<GameObject>();

        public static GameObject LoadPrefab(string identifier)
        {
            foreach (ReplacementGameObject rgo in replacement)
            {
                if (rgo.identifier == identifier)
                {
                    return GameObject.Instantiate(rgo.gameObject);
                }
            }

            foreach (ReplaceableAsset rp in replaceablePrefabs)
            {
                if (rp.identifier == identifier)
                {
                    return GameObject.Instantiate(rp.gameObject);
                }
            }

            return null;
        }

        public static SumoVehicle GetCustomCar()
        {
            if (customCars.Count == 0)
            {
                return null;
            }
            else
            {
                return (SumoVehicle)WeightedRandoms.Shuffle<SumoVehicle>(customCars);
            }
        }

        public static void ReplaceSceneAssets(List<GameObject> sceeneRoots)
        {
            foreach (GameObject go in sceeneRoots)
            {
                ReplaceAsset(go);
            }
            DestroyObsoloeteSceneObjects();
            AddAdditionalAssets();
        }

        public static void AddAdditionalAssets()
        {
            foreach (AdditionalAsset aa in additionalAssets)
            {
                GameObject newobject = GameObject.Instantiate(aa.gameObject);
                newobject.transform.localPosition = aa.transform.localPosition;
            }
        }

        public static void DestroyObsoloeteSceneObjects()
        { 
            foreach(GameObject go in ToBeDestroyed)
            {
                GameObject.Destroy(go);
            }
        }

        public static void ReplaceAsset(GameObject go)
        {
            debugInt++;
            ReplaceableAsset ra = go.GetComponent<ReplaceableAsset>();

            if (ra != null)
            {
                if(ra.alreadyReplaced)
                {
                    return;
                }
                foreach (ReplacementGameObject rgo in replacement)
                {
                    if (rgo.identifier == ra.identifier)
                    {
                        GameObject newobject = GameObject.Instantiate(rgo.gameObject);
                        newobject.transform.SetParent(ra.transform.parent);
                        newobject.transform.localPosition = ra.transform.localPosition;
                        ra.alreadyReplaced = true;
                        ToBeDestroyed.Add(ra.gameObject);

                        Debug.Log("Replacing "+ra.identifier);

                        return;
                    }
                }
            }
            else
            {
                for (int i = 0; i < go.transform.childCount; i++)
                {
                    GameObject sub = go.transform.GetChild(i).gameObject;
                    ReplaceAsset(sub);
                }
            }

        }

    }
}
