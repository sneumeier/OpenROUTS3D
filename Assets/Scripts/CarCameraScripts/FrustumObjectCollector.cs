using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Assets.Scripts.SUMOConnectionScripts;
using System.IO;
using Google.Protobuf;
using SUMOConnectionScripts;
using UnityEditor;

public class FrustumObjectCollector : MonoBehaviour
{

    private const bool DO_DEBUG = false;

    public bool WriteScanLog;
    public Camera MainCam;
    public string savepath;
    private int counter = 0;
    public int framecounter = 0;
    private RenderTexture texture;   //generate this texture based on cam
    public bool filterRoads;
    public bool onlyCars;
    public List<GameObject> ObjectsInFrustum
    {
        get
        {
            if (_objectsInFrustum.Count == 0)
            {
                scanObjects();
            }
            return _objectsInFrustum.ToList();
        }
    }
    private List<GameObject> _objectsInFrustum = new List<GameObject>();


    // Update is called once per frame
    void Update()
    {
        _objectsInFrustum.Clear();

        if (WriteScanLog)
        {
            MachineLearningDump();
        }

    }

    public Bounds GetTotalBounds(GameObject frustumObject,out bool visible)
    {
        Bounds bound = new Bounds();
        bool init = false;
        visible = false;
        //Cars consist out of multiple parts. Those have to be encapsulated to be treated as single instance
        foreach (MeshRenderer childRenderer in frustumObject.GetComponentsInChildren<MeshRenderer>())
        {
            if (childRenderer.isVisible)
            {
                visible = true;
            }
            if (!init)
            {
                init = true;
                bound = childRenderer.bounds;
                
            }
            else { 
                bound.Encapsulate(childRenderer.bounds);
            }

        }
        return bound;
    }

    public void MachineLearningDump()
    {
        if (ObjectsInFrustum.Count() > 0)
        {
            counter = 0;
            List<ObjectList.Types.IdentifiedObject> objlist = new List<ObjectList.Types.IdentifiedObject>();
            //MainCam.pixelRect = new Rect(0,0,3840,2160);
            foreach (GameObject objectInFrustum in ObjectsInFrustum)
            {
                Bounds bound;
                if (onlyCars)
                {
                    bool vis = false;
                    bound = GetTotalBounds(objectInFrustum,out vis);
                }
                else
                {
                    bound = objectInFrustum.GetComponent<Renderer>().bounds;
                }

                Vector3 center = MainCam.WorldToViewportPoint(bound.center);
                Vector3 edgeTop = MainCam.WorldToViewportPoint(bound.center + bound.size / 2);
                Vector3 edgeBot = MainCam.WorldToViewportPoint(bound.center - bound.size / 2);

                Debug.DrawLine(edgeTop, edgeBot, Color.red);

                ObjectList.Types.IdentifiedObject identifiedObject = new ObjectList.Types.IdentifiedObject();

                identifiedObject.BottomX = edgeBot.x;
                identifiedObject.BottomY = edgeBot.y;
                identifiedObject.BottomZ = edgeBot.z;
                identifiedObject.CenterX = center.x;
                identifiedObject.CenterY = center.y;
                identifiedObject.CenterZ = center.z;
                identifiedObject.TopX = edgeTop.x;
                identifiedObject.TopY = edgeTop.y;
                identifiedObject.TopZ = edgeTop.z;
                identifiedObject.Confidence = 1;
                identifiedObject.Uid = (uint)(objectInFrustum.GetInstanceID()) + int.MaxValue;
                identifiedObject.Type = objectInFrustum.name;

                objlist.Add(identifiedObject);

                counter++;
            }

            foreach (var identifiedObject in objlist)
            {
                string directoryPath = System.IO.Path.Combine(savepath, framecounter.ToString());
                Directory.CreateDirectory(directoryPath);
                using (var output = File.Create(Path.Combine(directoryPath, identifiedObject.Uid + "_" + framecounter + ".dat")))
                {
                    identifiedObject.WriteTo(output);
                    Debug.Log("Written to " + output);
                }
            }

            //Rendering image:
            int resHeight = (int)MainCam.pixelRect.height;
            int resWidth = (int)MainCam.pixelRect.width;
            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            MainCam.targetTexture = rt;
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            MainCam.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            MainCam.targetTexture = texture;
            RenderTexture.active = null;
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            System.IO.File.WriteAllBytes(Path.Combine(savepath, framecounter.ToString(), "image" + ".png"), bytes);

        }
        framecounter++;
    }

    public void OnWillRenderObject()
    {

    }

    private void scanObjects()
    {
        GameObject[] allObjs = UnityEngine.Object.FindObjectsOfType<GameObject>();
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(MainCam);

        foreach (GameObject obj in allObjs)
        {
            if (!obj.activeSelf || !obj.activeInHierarchy)
            {
                continue;
            }
            MeshRenderer mr = obj.GetComponent<MeshRenderer>();
            if ((!onlyCars && mr != null) || (onlyCars && obj.GetComponent<SumoVehicle>() != null))
            {
                if (filterRoads && obj.GetComponent<StreetSegmentMesh>() != null)
                {
                    continue;
                }
                Bounds bounds;
                bool visible = false;
                if (onlyCars)
                {
                    bounds = GetTotalBounds(obj,out visible);
                }
                else
                {
                    bounds = mr.bounds;
                    visible = mr.isVisible;
                }
                if (GeometryUtility.TestPlanesAABB(frustumPlanes, bounds))
                {

                    //Mesh Renderer is inside Frustum Planes
                    if (visible)
                    {
                        // Object is visible at all (in any camera)
                        _objectsInFrustum.Add(obj);
                        if (DO_DEBUG)
                        {
                            mr.material.color = Color.green;
                        }
                    }
                    else
                    {
                        if (DO_DEBUG)
                        {
                            mr.material.color = Color.yellow;
                        }
                    }
                }
                else
                {
                    if (DO_DEBUG)
                    {
                        mr.material.color = Color.red;
                    }
                }
            }

        }
    }
}
