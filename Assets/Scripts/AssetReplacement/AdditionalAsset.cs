using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.AssetReplacement
{
    public class AdditionalAsset : MonoBehaviour
    {
        public string identifier;
        public string anchorIdentifier;

        // Use this for initialization
        public void Start()
        {
            transform.SetParent(AnchorMapping.GetAnchor(anchorIdentifier).transform, false);
        }

    }
}