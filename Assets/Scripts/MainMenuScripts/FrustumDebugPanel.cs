using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FrustumDebugPanel : MonoBehaviour {

    public GameObject TextPrefab;
    public bool JustRequests;

    public FrustumObjectCollector frustumScanner;
    private float currentHeight = 40;
    public float heightPerStep = 30;

    public List<GameObject> TextObjects = new List<GameObject>();

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        foreach(GameObject go in TextObjects){
            GameObject.Destroy(go);
        }
        TextObjects.Clear();
        currentHeight = 40;
        List<GameObject> scannedObjects = frustumScanner.ObjectsInFrustum;
        if (JustRequests) { return; }
		foreach(GameObject go in scannedObjects)
        {
            GameObject newText = GameObject.Instantiate(TextPrefab);
            TextObjects.Add(newText);
            newText.SetActive(true);
            newText.transform.SetParent(TextPrefab.transform.parent);
            newText.GetComponent<RectTransform>().anchoredPosition = new Vector3(20, -currentHeight);
            newText.GetComponent<Text>().text = go.name;
            currentHeight += heightPerStep;

            
        }
	}
}
