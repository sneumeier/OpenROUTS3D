using Assets.Scripts.AssetReplacement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddonPanelScript : MonoBehaviour {

    public GameObject prefabButton;
    public int distancePerButton = 180;
    public CanvasScaler canvasScaler;
    public RectTransform viewport; 

	// Use this for initialization
	void Start () {
        AssetReplacement.LoadAssetPacks();
        
        gameObject.SetActive((PrefabProvider.loadedAssetPacks.Count > 0));

        int loadedPacks = 0;
        foreach (var assetPack in PrefabProvider.loadedAssetPacks)
        {
            GameObject addonButtonObject = GameObject.Instantiate(prefabButton);
            addonButtonObject.transform.SetParent(prefabButton.transform.parent);
            addonButtonObject.SetActive(true);
            AddonButton addonButton = addonButtonObject.GetComponent<AddonButton>();
            addonButton.GetComponent<RectTransform>().localPosition = new Vector2(prefabButton.GetComponent<RectTransform>().localPosition.x, prefabButton.GetComponent<RectTransform>().localPosition.y - (addonButton.GetComponent<RectTransform>().rect.height + 20) * (loadedPacks));

            viewport.sizeDelta = new Vector2(viewport.sizeDelta.x, (addonButton.GetComponent<RectTransform>().rect.height + 20) * (loadedPacks+0.5f) + 100);
            loadedPacks++;
            addonButton.icon.sprite = assetPack.icon;
            addonButton.pack = assetPack;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
