using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AssetReplacement
{
    public static class AnchorMapping
    {
        private static Dictionary<string, GameObject> anchorDict = new Dictionary<string, GameObject>();

        public static void SetMapping(string identifier, GameObject anchor)
        {
            if (anchorDict.ContainsKey(identifier))
            {
                anchorDict[identifier] = anchor;
            }
            else {
                anchorDict.Add(identifier,anchor);
            }
        }

        public static GameObject GetAnchor(string identifier)
        {
            if (anchorDict.ContainsKey(identifier))
            {
                return anchorDict[identifier];
            }
            return null;
        }

    }
}
