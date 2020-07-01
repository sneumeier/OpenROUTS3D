using CarScripts;
using SUMOConnectionScripts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.AssetReplacement.AddOns
{
    public class ReplayScript : MonoBehaviour
    {

        #region Properties and Fields
        public const int TIMESTAMP_INDEX = 0;
        public const int POSITION_INDEX = 11;
        public const int ROTATION_INDEX = 12;
        public const int VELOCITY_INDEX = 6;

        public bool lidarActive;
        public float lidarDelay;
        public float lidarAngle;
        public int lidarColums;
        public int lidarRows;
        public string savePath;
        public string lidarDumpPath;

        public float timeFactor;

        public float virtualTime = 0;

        public bool started = false;
        private bool initialized = false;
        private string[] lines;
        private int lastIndex = 1;
        private float lastLineTime = 0;

        public bool cameraScanActive;

        private FrustumObjectCollector frustumScanner;
        private Transform lidarSource;
        private GameObject vehiclePrefab;
        public GameObject customVehiclePrefab
        {
            get
            {
                var customCar = PrefabProvider.GetCustomCar();
                if (customCar != null)
                {
                    return customCar.gameObject;
                }
                else return null;
            }
        }

        private Dictionary<string, ReplayVehicle> foreignVehicles = new Dictionary<string, ReplayVehicle>();

        public Text displaytext;

        private Rigidbody rig;
        private FileStream stream = null;
        private GameObject _car = null;
        public GameObject car
        {
            get
            {
                if (_car == null)
                {
                    _car = AnchorMapping.GetAnchor("CarPhysics");
                }
                return _car;
            }
        }
        #endregion Properties and Fields

        #region Methods
        public void Awake()
        {
            //Don't start traci during replay. We merely want to read saved car paths
            SumoUnityConnection.startTraci = false;
        }

        public void Start()
        {
            //Delete InputManager
            car.GetComponent<InputManager>().enabled = false;
            car.GetComponent<WheelDrive>().enabled = false;
            rig = car.GetComponent<Rigidbody>();
            rig.useGravity = false;
            timeFactor = 1;
            vehiclePrefab = PrefabInitializer.GetPrefab("SumoVehicle");
        }

        public void OnDestory()
        {
            if (stream != null)
            {
                stream.Close();
            }
        }

        public void Update()
        {

            if (Input.GetKeyDown(KeyCode.Equals))
            {
                timeFactor *= 1.2f;
            }

            if (Input.GetKeyDown(KeyCode.Minus))
            {
                timeFactor /= 1.2f;
            }

            if (started)
            {

                if (!initialized)
                {
                    //First frame is just reading all the csv data
                    lines = System.IO.File.ReadAllLines(this.savePath);
                    initialized = true;
                    //This frame probably caused some lag, influencing deltaTime.

                    string[] lineparts = lines[1].Split(';');
                    virtualTime = float.Parse(lineparts[TIMESTAMP_INDEX]);

                    //File.Create(Path.Combine(lidarDumpPath,"lidarScan.lidar"));
                    frustumScanner = AnchorMapping.GetAnchor("FrustumScanner").GetComponent<FrustumObjectCollector>();
                    frustumScanner.savepath = lidarDumpPath;
					
					if (frustumScanner.MainCam == null)
                    {
                        Debug.Log("rustumScanner.MainCam is null, but it is requiered within the Updatefunction");
                        return;
                    }

                    Debug.Log("Focal Length: "+frustumScanner.MainCam.focalLength);
                    Debug.Log("Sensor Size: " + frustumScanner.MainCam.sensorSize);


                    lidarSource = AnchorMapping.GetAnchor("LIDAR").transform;
                    GenerateForeignVehiclePaths();




                    return;
                }

                float lastTime = virtualTime;
                float delta;
                if (!lidarActive && !cameraScanActive)
                {
                    delta = Time.deltaTime * timeFactor;
                }
                else
                {
                    delta = lidarDelay;

                }
                virtualTime += delta;
                float virtualDelta = virtualTime - lastTime;
                ApplyVehiclePosition(virtualTime);
                ApplyAllForeignCars(virtualTime);
                int secs = (int)(virtualTime % 60);
                int mins = (int)(virtualTime / 60);
                displaytext.text = mins.ToString("00") + ":" + secs.ToString("00");
				if (frustumScanner == null)
                {
                    Debug.Log("frustumScanner is null, but it is requiered within the Updatefunction");
                    return;
                }
                if (cameraScanActive)
                {
                    frustumScanner.MachineLearningDump();
                }
                if (lidarActive)
                {
                    List<Vector3> pointCloud = LidarScan();
                    

                    int frustumFrame = frustumScanner.framecounter;
					Debug.Log("Frame ID = "+ frustumFrame);

                    int pointCloudSize = pointCloud.Count * 3 * sizeof(float);
                    byte[] pointCloudPayload = new byte[sizeof(float) + sizeof(int) + sizeof(int) + pointCloudSize + (sizeof(float) * 16)];

                    Matrix4x4 mat = frustumScanner.MainCam.projectionMatrix * frustumScanner.MainCam.worldToCameraMatrix;

                    //Time Stamp and Point Count
                    System.Buffer.BlockCopy(BitConverter.GetBytes(virtualTime), 0, pointCloudPayload, 0, sizeof(float));
                    System.Buffer.BlockCopy(BitConverter.GetBytes(frustumFrame), 0, pointCloudPayload, sizeof(float), sizeof(int));
                    System.Buffer.BlockCopy(BitConverter.GetBytes(pointCloud.Count), 0, pointCloudPayload, sizeof(float)+ sizeof(int), sizeof(int));
                    int offset = sizeof(float) + sizeof(int)+ sizeof(int);

                    //Transformation mat
                    System.Buffer.BlockCopy(BitConverter.GetBytes(mat.m00), 0, pointCloudPayload, offset,sizeof(float));
                    offset += sizeof(float);
                    System.Buffer.BlockCopy(BitConverter.GetBytes(mat.m01), 0, pointCloudPayload, offset, sizeof(float));
                    offset += sizeof(float);
                    System.Buffer.BlockCopy(BitConverter.GetBytes(mat.m02), 0, pointCloudPayload, offset, sizeof(float));
                    offset += sizeof(float);
                    System.Buffer.BlockCopy(BitConverter.GetBytes(mat.m03), 0, pointCloudPayload, offset, sizeof(float));
                    offset += sizeof(float);
                    System.Buffer.BlockCopy(BitConverter.GetBytes(mat.m10), 0, pointCloudPayload, offset, sizeof(float));
                    offset += sizeof(float);
                    System.Buffer.BlockCopy(BitConverter.GetBytes(mat.m11), 0, pointCloudPayload, offset, sizeof(float));
                    offset += sizeof(float);
                    System.Buffer.BlockCopy(BitConverter.GetBytes(mat.m12), 0, pointCloudPayload, offset, sizeof(float));
                    offset += sizeof(float);
                    System.Buffer.BlockCopy(BitConverter.GetBytes(mat.m13), 0, pointCloudPayload, offset, sizeof(float));
                    offset += sizeof(float);
                    System.Buffer.BlockCopy(BitConverter.GetBytes(mat.m20), 0, pointCloudPayload, offset, sizeof(float));
                    offset += sizeof(float);
                    System.Buffer.BlockCopy(BitConverter.GetBytes(mat.m21), 0, pointCloudPayload, offset, sizeof(float));
                    offset += sizeof(float);
                    System.Buffer.BlockCopy(BitConverter.GetBytes(mat.m22), 0, pointCloudPayload, offset, sizeof(float));
                    offset += sizeof(float);
                    System.Buffer.BlockCopy(BitConverter.GetBytes(mat.m23), 0, pointCloudPayload, offset, sizeof(float));
                    offset += sizeof(float);
                    System.Buffer.BlockCopy(BitConverter.GetBytes(mat.m30), 0, pointCloudPayload, offset, sizeof(float));
                    offset += sizeof(float);
                    System.Buffer.BlockCopy(BitConverter.GetBytes(mat.m31), 0, pointCloudPayload, offset, sizeof(float));
                    offset += sizeof(float);
                    System.Buffer.BlockCopy(BitConverter.GetBytes(mat.m32), 0, pointCloudPayload, offset, sizeof(float));
                    offset += sizeof(float);
                    System.Buffer.BlockCopy(BitConverter.GetBytes(mat.m33), 0, pointCloudPayload, offset, sizeof(float));
                    offset += sizeof(float);

                    foreach (Vector3 v3 in pointCloud)
                    {
                        //Serialize X
                        System.Buffer.BlockCopy(BitConverter.GetBytes(v3.x), 0, pointCloudPayload, offset, sizeof(float));
                        offset += sizeof(float);

                        //Serialize Y
                        System.Buffer.BlockCopy(BitConverter.GetBytes(v3.y), 0, pointCloudPayload, offset, sizeof(float));
                        offset += sizeof(float);

                        //Serialize Z
                        System.Buffer.BlockCopy(BitConverter.GetBytes(v3.z), 0, pointCloudPayload, offset, sizeof(float));
                        offset += sizeof(float);
                    }
                    AppendAllBytes(Path.Combine(lidarDumpPath, "lidarScan.lidar"), pointCloudPayload);
                }
            }
        }

        public void AppendAllBytes(string path, byte[] bytes)
        {

            if (stream == null)
            {
                stream = new FileStream(path, FileMode.Append);
            }
            stream.Write(bytes, 0, bytes.Length);
            
        }

        public void ApplyAllForeignCars(float virtualTime)
        {
            foreach (var replayVehicle in foreignVehicles.Values)
            {
                replayVehicle.ApplyTime(virtualTime);
            }
        }

        public void GenerateForeignVehiclePaths()
        {
            Debug.Log("Loading foreign vehicle Paths");
            string[] foreignCarLines = System.IO.File.ReadAllLines(this.savePath.Replace(".csv","_foreignCars.csv"));

            for (int lineIndex = 1; lineIndex < foreignCarLines.Length; lineIndex++)
            {
                string[] lineParts = foreignCarLines[lineIndex].Split(';');
                if (lineParts.Length < 4)
                {
                    Debug.LogWarning("Line "+(lineIndex+1)+" in foreign cars csv is formatted incorrectly");
                    continue;//Would cause an error, line is clearly not formatted correctly
                }
                float timestamp = float.Parse(lineParts[0]);
                string name = lineParts[1];
                string[] vectorparts = lineParts[2].Split(',');
                string[] rotationparts = lineParts[3].Split(',');
                Vector3 position = new Vector3(float.Parse(vectorparts[0]), float.Parse(vectorparts[1]), float.Parse(vectorparts[2]));
                Vector3 rotation = new Vector3(float.Parse(rotationparts[0]), float.Parse(rotationparts[1]), float.Parse(rotationparts[2]));

                //Check if car is already registered
                if (!foreignVehicles.ContainsKey(name))
                {
                    ReplayVehicle vehicle = new ReplayVehicle();
                    vehicle.spawnTime = timestamp;

                    //Spawn prefab

                    GameObject instantiatedVehicle;
                    GameObject prefab = customVehiclePrefab;
                    if (prefab != null)
                    {
                        //Use custom Prefab
                        instantiatedVehicle = Instantiate(prefab);
                    }
                    else
                    {
                        //Use default prefab
                        instantiatedVehicle = Instantiate(vehiclePrefab);
                    }

                    vehicle.replayVehicle = instantiatedVehicle;
                    vehicle.replayVehicle.SetActive(false);
                    Debug.Log("Spawned Foreign Vehicle");
                    foreignVehicles.Add(name,vehicle);
                }
                foreignVehicles[name].despawnTime = timestamp;//Will be overwritten until the last line of this car has been reached
                foreignVehicles[name].timedPositions.Add(new Tuple<float, Vector3, Vector3>(timestamp,position,rotation));
                
            }
        }


        public void ApplyVehiclePosition(float virtualTime)
        {
            string line = GetLine(virtualTime);
            string linePost = GetLine(virtualTime,true);
            string[] lineparts = line.Split(';');
            string[] linepartsPost = linePost.Split(';');

            float timePre = 0;
            float timePost = 0;
            
            Vector3 posPre = Vector3.zero;
            Vector3 posPost = Vector3.zero;

            Vector3 rotPre = Vector3.zero;
            Vector3 rotPost = Vector3.zero;

            float velocityPre = 0;
            float velocityPost = 0;

            //Read data from time before our virtual time
            if (!(lineparts.Length <= TIMESTAMP_INDEX))
            {
                try
                {
                    timePre = float.Parse(lineparts[TIMESTAMP_INDEX]);
                }
                catch (Exception)
                {
                    Debug.LogWarning("Was not able to read timestamp from csv log, incorrect format: " + lineparts[TIMESTAMP_INDEX]);
                }
            }
            else
            {
                Debug.LogWarning("Was not able to read timestamp from csv log, position not contained in this line: " + line);
            }
            if (!(lineparts.Length <= POSITION_INDEX))
            {
                try
                {
                    posPre = ParseCsvVector(lineparts[POSITION_INDEX]);
                }
                catch (Exception)
                {
                    Debug.LogWarning("Was not able to set vehicle position from csv log, incorrect format: "+lineparts[POSITION_INDEX]);
                }
            }
            else
            {
                Debug.LogWarning("Was not able to set vehicle position from csv log, position not contained in this line: "+line);
            }
            if (!(lineparts.Length <= ROTATION_INDEX))
            {
                try
                {
                    rotPre = ParseCsvVector(lineparts[ROTATION_INDEX]);
                }
                catch (Exception)
                {
                    Debug.LogWarning("Was not able to set vehicle rotation from csv log, incorrect format: " + lineparts[ROTATION_INDEX]);
                }
            }
            else
            {
                Debug.LogWarning("Was not able to set vehicle rotation from csv log, rotation not contained in this line: " + line);
            }
            if (!(lineparts.Length <= VELOCITY_INDEX))
            {
                try
                {
                    velocityPre = float.Parse(lineparts[VELOCITY_INDEX]);
                }
                catch (Exception)
                {
                    Debug.LogWarning("Was not able to set vehicle velocity from csv log, incorrect format: " + lineparts[VELOCITY_INDEX]);
                }
            }
            else
            {
                Debug.LogWarning("Was not able to set vehicle velocity from csv log, rotation not contained in this line: " + line);
            }

            //Read data from logged line that happened after the virtual time
            if (!(linepartsPost.Length <= TIMESTAMP_INDEX))
            {
                try
                {
                    timePost = float.Parse(linepartsPost[TIMESTAMP_INDEX]);
                }
                catch (Exception)
                {
                    Debug.LogWarning("Was not able to read timestamp from csv log, incorrect format: " + linepartsPost[TIMESTAMP_INDEX]);
                }
            }
            else
            {
                Debug.LogWarning("Was not able to read timestamp from csv log, position not contained in this line: " + line);
            }
            if (!(linepartsPost.Length <= POSITION_INDEX))
            {
                try
                {
                    posPost = ParseCsvVector(linepartsPost[POSITION_INDEX]);
                }
                catch (Exception)
                {
                    Debug.LogWarning("Was not able to set vehicle position from csv log, incorrect format: " + linepartsPost[POSITION_INDEX]);
                }
            }
            else
            {
                Debug.LogWarning("Was not able to set vehicle position from csv log, position not contained in this line: " + line);
            }
            if (!(linepartsPost.Length <= ROTATION_INDEX))
            {
                try
                {
                    rotPost = ParseCsvVector(linepartsPost[ROTATION_INDEX]);
                }
                catch (Exception)
                {
                    Debug.LogWarning("Was not able to set vehicle rotation from csv log, incorrect format: " + lineparts[ROTATION_INDEX]);
                }
            }
            else
            {
                Debug.LogWarning("Was not able to set vehicle rotation from csv log, rotation not contained in this line: " + line);
            }
            if (!(linepartsPost.Length <= VELOCITY_INDEX))
            {
                try
                {
                    velocityPost = float.Parse(lineparts[VELOCITY_INDEX]);
                }
                catch (Exception)
                {
                    Debug.LogWarning("Was not able to set vehicle velocity from csv log, incorrect format: " + linepartsPost[VELOCITY_INDEX]);
                }
            }
            else
            {
                Debug.LogWarning("Was not able to set vehicle velocity from csv log, rotation not contained in this line: " + line);
            }

            float timeDistancePre = virtualTime - timePre;
            float timeDistanceFrames = timePost - timePre;

            
            float weightPost = timeDistancePre/timeDistanceFrames;
            float weightPre = 1-weightPost;

            if (timeDistanceFrames == 0)
            {
                weightPre = 1.0f;
                weightPost = 0.0f;
                Debug.Log("Identical timestamps for pre and post");
            }
            else {
                Debug.Log("Weights: Pre: " + weightPre.ToString("0.000") + " Post: " + weightPost.ToString("0.000"));
            }

            car.gameObject.transform.position = posPre * weightPre + posPost * weightPost;
            car.gameObject.transform.rotation = Quaternion.Lerp(Quaternion.Euler(rotPre), Quaternion.Euler(rotPost),weightPost);
            car.GetComponent<StatisticsContainer>().velocity = velocityPost * weightPost + velocityPre * weightPre;
        }

        public List<Vector3> MockLidarScan()
        {
            List<Vector3> pointCloud = new List<Vector3>();
            for (int i = 0; i < 10; i++)
            {
                pointCloud.Add(new Vector3( i, 0, 0));
                pointCloud.Add(new Vector3(-i, 0, 0));
                pointCloud.Add(new Vector3( 0, 0,-i));
                pointCloud.Add(new Vector3( 0, 0, i));
            }
            return pointCloud;
        }

        public List<Vector3> LidarScan()
        {
            List<Vector3> pointCloud = new List<Vector3>();
            for (int row = 0; row < lidarRows; row++)
            {
                float rowAngle = ( row/ (float)lidarRows) * lidarAngle -(lidarAngle/2);
                for (int col = 0; col < lidarColums; col++)
                {
                    float colAngle = (col / (float)lidarColums) * 360;
                    //Cast Ray from lidar Source
                    
                    int layerMask = 1 << 8;

                    //Invert layer mask so we're casting against anything except the non mirror layer
                    layerMask = ~layerMask;

                    RaycastHit hit;
                    // Does the ray intersect any objects excluding the player layer
                    if (Physics.Raycast(lidarSource.position,  Quaternion.Euler(rowAngle,colAngle,0)*Vector3.forward, out hit, 250, layerMask))
                    {
                        pointCloud.Add(hit.point);
                    }
                    else
                    {
                        //Ray did not hit
                    }

                }
            }
            return pointCloud;
        }

        public Vector3 ParseCsvVector(string str)
        {
            string vectorstring = str;
            vectorstring = vectorstring.Replace("(", "");
            vectorstring = vectorstring.Replace(")", "");
            vectorstring = vectorstring.Replace(" ", "");
            string[] splits = vectorstring.Split(',');
            return new Vector3(float.Parse(splits[0]), float.Parse(splits[1]), float.Parse(splits[2]));
        }

        public string GetLine(float virtualTime, bool followingLine = false)
        {
            
            int startIndex = 1;
            if (lastLineTime < virtualTime)
            {
                startIndex = lastIndex;
            }

            int currentIndex = Math.Max(startIndex-1,1);

            while (currentIndex < lines.Length)
            {
                string[] lineparts = lines[currentIndex].Split(';');
                float timestamp = float.Parse(lineparts[TIMESTAMP_INDEX]);
                
                if (timestamp > virtualTime)
                {
                    Debug.Log("CurrentIndex = "+currentIndex+" LastIndex = "+lastIndex);
                    if (followingLine)
                    {
                        return lines[currentIndex];
                    }
                    else return lines[lastIndex];
                }

                lastLineTime = timestamp;
                lastIndex = currentIndex;
                currentIndex++;
            }
            Debug.Log("End of Logfile");
            return lines[lines.Length - 1];
        }
        #endregion Methods
    }
}
