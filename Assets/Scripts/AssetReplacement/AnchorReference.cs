using Assets.Scripts.AssetReplacement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.AssetReplacement
{ 
    public class AnchorReference : MonoBehaviour {

        public string identifier;

	    // Use this for initialization
	    void Awake ()
        {
            AnchorMapping.SetMapping(identifier, this.gameObject);
	    }
	
    }
}