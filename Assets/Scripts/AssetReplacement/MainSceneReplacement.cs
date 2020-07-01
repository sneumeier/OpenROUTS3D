using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AssetReplacement
{
    public class MainSceneReplacement:MonoBehaviour
    {

        public void Start()
        {
            PrefabProvider.ReplaceSceneAssets(UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().ToList());
            Debug.Log("Amount of objects in scene: "+PrefabProvider.debugInt);
        }

    }
}
