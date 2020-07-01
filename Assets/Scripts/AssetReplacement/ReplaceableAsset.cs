using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.AssetReplacement
{
    public class ReplaceableAsset : MonoBehaviour
    {


        public string identifier;

        public bool alreadyReplaced = false;

        public void Start()
        {
            AnchorMapping.SetMapping(identifier, this.gameObject);
        }


    }
}
