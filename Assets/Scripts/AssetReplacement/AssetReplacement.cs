using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using Assets.Scripts.InterfaceScene;
using Assets.Scripts.AssetReplacement.AddOns;
using System.Collections;
using UnityEngine.UI;

namespace Assets.Scripts.AssetReplacement
{
    public class AssetReplacement : MonoBehaviour
    {

        public GameObject loadingScreen;
        public Slider slider;
        public Text progressText;

        public static bool alreadyLoaded = false;
        public static Dictionary<AssetPack, string> assetNames = new Dictionary<AssetPack, string>();
        public static List<string> loadedAssetNames
        {
            get
            {
                List<string> rList = new List<string>();
                foreach (AssetPack key in assetNames.Keys)
                {
                    if (key.requestLoad)
                    {
                        rList.Add(assetNames[key]);
                    }
                }
                return rList;
            }
        }

        public void Clear()
        {
            PrefabProvider.additionalAssets.Clear();
            PrefabProvider.replaceablePrefabs.Clear();
            PrefabProvider.replacement.Clear();
            PrefabProvider.customCars.Clear();
            PrefabProvider.customTextures.Clear();
            PrefabProvider.customFassades.Clear();
        }

        public static void LoadAssetPacks()
        {
            string assetBundleSearchPath = Path.Combine(Application.streamingAssetsPath, "AssetBundles");
            if (alreadyLoaded)
            {
                Debug.Log("Already Loaded Asset Bundles");
                return;
            }

            Debug.Log("Current search path for Asset Bundles: " + Path.GetFullPath(assetBundleSearchPath));
            if (!Directory.Exists(Path.GetFullPath(assetBundleSearchPath)))
            {
                Debug.LogWarning("Asset Bundle Search Directory does not exist: " + Path.GetFullPath(assetBundleSearchPath));
                return;
            }

            assetNames.Clear();
            List<string> paths = Directory.GetFiles(Path.GetFullPath(assetBundleSearchPath)).ToList();
            alreadyLoaded = true;
            foreach (string path in paths)
            {
                if (!Path.GetExtension(path).Contains("assetpack"))
                {
                    //Debug.LogWarning(path + " is not an assetpack");
                    continue;
                }
                try
                {

                    AssetBundle bundle = AssetBundle.LoadFromFile(path);
                    if (bundle != null)
                    {
                        GameObject assetObject = bundle.LoadAsset<GameObject>("AssetPack");
                        if (assetObject != null)
                        {
                            AssetPack pack = assetObject.GetComponent<AssetPack>();
                            if (pack != null)
                            {
                                PrefabProvider.loadedAssetPacks.Add(pack);
                                Debug.Log("Loaded Asset Pack: " + path);
                                assetNames.Add(pack, Path.GetFileNameWithoutExtension(path));
                            }
                            else
                            {
                                Debug.LogWarning("Loaded Asset Pack does not contain an AssetPack script");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Could not find AssetPack Object containing replacement information in Asset Bundle");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Loading Asset Pack " + path + " was unsuccessfull");
                    Debug.LogWarning(e.Message);
                    Debug.LogWarning(e.StackTrace);
                }
            }
        }

        public void EndMenuScene(int sceneIndex)
        {
            LoadForeignObjects();
            Assets.Scripts.SUMOConnectionScripts.Maps.DelaunayMapGenerator.TotalTimeStopwatch.Start();
            if (Settings.automatedPlay)
            {
                AutorunDeserializer.Deserialize(Settings.autorunXmlPath);
                AutorunDeserializer.LoadScene();
            }
            else
            {
                StartCoroutine(LoadAsynchronously(sceneIndex));
            }
        }

        private IEnumerator LoadAsynchronously(int sceneIndex)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

            while (!operation.isDone)
            {
                float progress = Mathf.Clamp01(operation.progress / .9f);

                slider.value = progress;
                progressText.text = progress * 100f + " %";

                if (operation.progress >= 0.9f)
                {
                    operation.allowSceneActivation = true;
                }
                yield return null;
            }
        }

        public void LoadForeignObjects()
        {
            Clear();
            foreach (AssetPack pack in PrefabProvider.loadedAssetPacks)
            {
                if (pack.requestLoad == false)
                {
                    continue;
                }
                for (int i = 0; i < pack.transform.childCount; i++)
                {
                    GameObject go = pack.transform.GetChild(i).gameObject;
                    ReplacementGameObject rgo = go.GetComponent<ReplacementGameObject>();
                    if (rgo != null)
                    {
                        PrefabProvider.replacement.Add(rgo);
                    }
                    else
                    {
                        AdditionalAsset aa = go.GetComponent<AdditionalAsset>();
                        if (aa != null)
                        {
                            PrefabProvider.additionalAssets.Add(aa);
                        }
                    }
                    IAssetPackPreInit[] inits = go.GetComponents<IAssetPackPreInit>();
                    foreach (IAssetPackPreInit init in inits)
                    {
                        init.PreInit();
                    }
                }
            }

            List<ReplacementGameObject> obsoleteReplacements = new List<ReplacementGameObject>();
            foreach (ReplacementGameObject rgo in PrefabProvider.replacement)
            {
                foreach (ReplacementGameObject other in PrefabProvider.replacement)
                {
                    if (other == rgo)
                    {
                        continue;
                    }
                    if (rgo.identifier == other.identifier)
                    {
                        if (rgo.priority < other.priority)
                        {
                            obsoleteReplacements.Add(rgo);
                        }
                    }
                }
            }

            foreach (ReplacementGameObject rgo in obsoleteReplacements)
            {
                PrefabProvider.replacement.Remove(rgo);
            }

        }

    }
}
