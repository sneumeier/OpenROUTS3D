using Assets.Scripts.AssetReplacement;
using Assets.Scripts.MultiplayerMessages;
using Assets.Scripts.OsmParser;
using Assets.Scripts.SUMOConnectionScripts;
using Assets.Scripts.SUMOConnectionScripts.Maps;
using Assets.Scripts.SUMOConnectionScripts.Maps.SumoImportPolygon;
using CarScripts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using UnityEngine;

namespace SUMOConnectionScripts
{

    public enum CarPositions
    {
        Frontleft,
        Frontright,
        Backleft,
        Backright
    }

    public class SumoUnityConnection : MonoBehaviour
    {


        public static bool startTraci = true;

        public bool IsEditor;
        public MultiplayerManager Multiplayer;
        public DelaunayMapGenerator HeightGenerator;
        public StatisticsContainer statistics;
        public Material roadMaterial;
        public Material roadMaterial1;
        public Material roadMaterial3;
        public Material roadMaterial4;
        public PhysicMaterial roadPhysicMaterial;
        public PhysicMaterial offRoadPhysicMaterial;
        public Material junctionMaterial;
        public Material markerMaterial;
        public Material sidewalkMaterial;

        public Material[] randomWallMaterials;
        public Material[] randomRoofMaterials;
        public GameObject[] windowPrefabs;
        public GameObject EgoCar;
        public Vector3 egoCarSize; // store the size of the egoCar
        public SumoVehicle vehiclePrefab;
        public SumoVehicle customVehiclePrefab
        {
            get
            {
                return PrefabProvider.GetCustomCar();
            }
        }
        public GameObject streetSignPrefab;
        public int[] speeds;
        public Material[] speedSignMaterials;
        public Dictionary<int, Material> speedMaterials = new Dictionary<int, Material>();

        public TrafficLightController trafficLightPrefab;
        public bool IsInit = false;

        const int subdivisionFactor = 6;
        private float streetWidth = 0;  // store to access later on

        public Vector3 startLocation = Vector3.zero;
        public Quaternion startRotation = Quaternion.identity;

        public SumoStreetImporter importer;
        private IDictionary<string, IList<Vector3>> roadNet;
        private IDictionary<string, IList<Vector3>> subdividedRoadNet;
        private GameObject roadParent;

        private Traci.TraciManager traci = Traci.TraciManager.Instance;
        private SumoPositionConverter converter = new SumoPositionConverter();
        private IDictionary<string, SumoVehicle> vehicles;
        private int vehicleNumber = 0;
        private GameObject vehicleParent;
        private static int frameCount = 0;
        private TrafficLights trafficlights = new TrafficLights();
        private string egoVehicleId;

        public Dictionary<string, KeyValuePair<string, float>> LaneCarPointPosition; // carcornerpoint, <laneid, distance>
        public Dictionary<string, float> CurrentLaneDistance = new Dictionary<string, float>();
        private readonly List<string> ghostVehicleId = new List<string>()
        {
            CarPositions.Frontleft.ToString(),
            CarPositions.Frontright.ToString(),
            CarPositions.Backleft.ToString(),
            CarPositions.Backright.ToString()
        };
        private bool validMap = false;
        public OsmParser osmParser = null;

        void Start()
        {
            if (!IsInit)
            {
                Init();
            }

        }

        // Initialization
        public void Init()
        {
            MapRasterizer.Instance.Init(IsEditor);
            EgoCar = AnchorMapping.GetAnchor("CarPhysics");
            CultureInfo ci = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
            IsInit = true;
            Debug.Log("Init SumoUnityConnection");

            //generate propper dict
            int dictSize = System.Math.Min(speeds.Length, speedSignMaterials.Length);
            for (int i = 0; i < dictSize; i++)
            {
                speedMaterials.Add(speeds[i], speedSignMaterials[i]);
            }

            // 0. Import OSM
            bool osmExists = File.Exists(Settings.osmPath);

            if (osmExists)
            {
                osmParser = new OsmParser();
                osmParser.ParseOsmFile(Settings.osmPath);
                if (File.Exists(Settings.correctionPath))
                {
                    osmParser.ParseCorrectionFile(Settings.correctionPath);
                }
            }

            // 1. Import the roadnet

            Debug.Log("Trying to load: " + Settings.mapPath);

            // Check if there is a Map
            if (!File.Exists(Settings.mapPath))
            {
                Debug.LogError("SumoUnityConnection - No sumonet found");
                return;
            }

            XElement rootElement = XElement.Load(Settings.mapPath);

            roadParent = new GameObject("roadnet");
            roadParent.transform.parent = this.transform;

            Building3D.randomizableWallMaterials = randomWallMaterials;
            Building3D.randomizableRoofMaterials = randomRoofMaterials;
            Building3D.randomizableWindowPrefabs = windowPrefabs;


            importer = new SumoStreetImporter(roadMaterial, roadPhysicMaterial, offRoadPhysicMaterial, 35, 5.5f, roadParent.transform, osmParser, rootElement);
            importer.roadMaterialSingleLane = roadMaterial1;
            importer.roadMaterialQuadLane = roadMaterial4;
            importer.roadMaterialTrippleLane = roadMaterial3;
            // TODO add material for sidewalks in simulator scene
            importer.junctionMaterial = junctionMaterial;
            importer.markerMaterial = markerMaterial;
            importer.streetSignPrefab = streetSignPrefab;
            importer.speedMaterials = speedMaterials;
            importer.sidewalkMaterial = sidewalkMaterial;
            

            // Load Roadnet
            IDictionary<string, IList<Vector3>> internRoadNet = importer.ImportStreetNet(rootElement, true, false);
            IDictionary<string, IList<Vector3>> externRoadNet = importer.ImportStreetNet(rootElement, false, true);

            //Adapt heights of extern road net so they match up with junctions
            IList<Junction> junctions = importer.ImportJunctionShapes(rootElement);
            importer.MakeJunctionsSeamless(externRoadNet, junctions);

            Debug.Log("External Size: " + externRoadNet.Keys.Count);
            Debug.Log("Internal Size: " + internRoadNet.Keys.Count);

            IDictionary<string, IList<Vector3>> subdividedInternRoadNet = importer.SubdivideStreetNet(internRoadNet, subdivisionFactor);
            IDictionary<string, IList<Vector3>> subdividedExternRoadNet = importer.SubdivideStreetNet(externRoadNet, subdivisionFactor);

            //IDictionary<string, IList<Vector3>> subdividedInternRoadNet = internRoadNet;
            //IDictionary<string, IList<Vector3>> subdividedExternRoadNet = externRoadNet;

            roadNet = internRoadNet.Concat(externRoadNet).ToDictionary(x => x.Key, x => x.Value);
            subdividedRoadNet = subdividedInternRoadNet.Concat(subdividedExternRoadNet).ToDictionary(x => x.Key, x => x.Value);


            // Generate Roadnet in Unity

            importer.DrawStreetNet(subdividedExternRoadNet, roadParent.transform, false, false, roadNet);
            importer.GenerateJunctions(junctions);

            // Setup EgoVehicle
            string egoVehicleStartLane = externRoadNet.First().Key;
            Vector3 egoVehicleStartPos = externRoadNet.First().Value[0];
            egoVehicleStartPos.y += 1;
            GameObject egoVehicle = GameObject.Find("carRoot");
            Quaternion egoVehicleStartRotation = new Quaternion();
            egoVehicleStartRotation.SetLookRotation(externRoadNet.First().Value[1] - externRoadNet.First().Value[0]);
            if (egoVehicle != null)
            {
                egoVehicle.transform.SetPositionAndRotation(egoVehicleStartPos, egoVehicleStartRotation);
                egoVehicle.GetComponent<InputManager>().spawnPos = egoVehicleStartPos;
                this.GetEgoCarSize();
            }

            startRotation.SetLookRotation(externRoadNet.First().Value[1] - externRoadNet.First().Value[0]);
            startLocation = externRoadNet.First().Value[0];

            // init Dictionary to track carpointpositions
            // start with the first road element and the distance of Max float
            LaneCarPointPosition = new Dictionary<string, KeyValuePair<string, float>>();
            LaneCarPointPosition.Add(CarPositions.Frontleft.ToString(), new KeyValuePair<string, float>(roadNet.First().Key, float.MaxValue));
            LaneCarPointPosition.Add(CarPositions.Frontright.ToString(), new KeyValuePair<string, float>(roadNet.First().Key, float.MaxValue));
            LaneCarPointPosition.Add(CarPositions.Backleft.ToString(), new KeyValuePair<string, float>(roadNet.First().Key, float.MaxValue));
            LaneCarPointPosition.Add(CarPositions.Backright.ToString(), new KeyValuePair<string, float>(roadNet.First().Key, float.MaxValue));


            // 2. Start SUMO
            if (startTraci && !IsEditor)
            {
                Debug.Log("Starting TraCI");
                // Check if there is a sumoconfig

                if (!File.Exists(Settings.mapPath.Replace("net.xml", "sumocfg")))
                {
                    Debug.LogWarning("No sumoconfig found");
                    return;
                }

                if (!vehiclePrefab)
                {
                    Debug.LogError("Sumo vehicle prefab is missing");
                    return;
                }

                vehicles = new Dictionary<string, SumoVehicle>();
                vehicleParent = new GameObject("vehicles");
                vehicleParent.transform.parent = this.transform;
                converter = new SumoPositionConverter();
                traci.Start(Settings.mapPath.Replace("net.xml", "sumocfg"));
                egoVehicleId = traci.AddEgoVehicle(egoVehicleStartLane);
                // Setup traffic lights
                Transform trafficLightsParent = new GameObject("traffic_lights").transform;
                trafficLightsParent.parent = this.transform;

                List<TrafficLightIntersection> trafficLights = trafficlights.Generate();
                foreach (TrafficLightIntersection trafficLight in trafficLights)
                {
                    IList<Vector3> lane;
                    if (externRoadNet.TryGetValue(trafficLight.laneId, out lane))
                    {
                        Quaternion trafficLightRotation = new Quaternion();
                        trafficLightRotation.SetLookRotation(lane[lane.Count - 2] - lane[lane.Count - 1]);

                        trafficLight.trafficLight = Instantiate(trafficLightPrefab, new Vector3(lane.Last().x, 3.5f, lane.Last().z), trafficLightRotation, trafficLightsParent);
                    }
                }
            }

            // safe streetWidth local
            if (this.streetWidth == 0)
                streetWidth = importer.streetWidth;

            validMap = true;

        }

        // Always update with the same rate
        private void FixedUpdate()
        {

            if (validMap && !Settings.paused && startTraci && !IsEditor)
            {
                if (Settings.isSumoServer)
                {
                    traci.simcontrol.SimStep();
                }

                List<string> currentVehicles = traci.vehicle.GetIdList();
                if (currentVehicles.Contains(egoVehicleId))
                {
                    currentVehicles.Remove(egoVehicleId);
                }
                foreach (string ghostId in ghostVehicleId)
                {
                    if (currentVehicles.Contains(ghostId))
                        currentVehicles.Remove(ghostId);
                }

                if (Settings.isSumoServer)
                {
                    // There are things we don't need to do EVERY frame

                    frameCount++;
                    if (frameCount >= 5)
                    {
                        frameCount = 0;
                        trafficlights.UpdateTrafficLights();
                    }
                    

                    // Delete vehicles that left the simulation and create new ones if new ones arrived
                    this.AdaptNumberOfExistingVehicles(currentVehicles);
                    if (currentVehicles.Count != vehicles.Count)
                    {
                        Debug.LogError("Current Vehicles are: " + currentVehicles.Count + ", but vehicles in unity are: " + vehicles.Count);
                    }

                    List<string> currentLaneIds = traci.vehicle.GetLaneId(currentVehicles);
                    List<float> currentLanePositions = traci.vehicle.GetLanePosition(currentVehicles).Select(i => (float)i).ToList();
                    List<int>lightSignals = traci.vehicle.GetLightSignalState(currentVehicles);
                    for (int i = 0; i < vehicles.Count; i++)
                    {
                        float percentage = converter.GetPercentage(currentLanePositions[i], roadNet[currentLaneIds[i]]);
                        try
                        {

                            List<LaneSegment> segments = importer.laneSegments[currentLaneIds[i]];
                            if (lightSignals.Count != 0)
                            {
                                vehicles[currentVehicles[i]].UpdateSumoCarLights(lightSignals[i]);
                            }
                            converter.ComputePositionOnLane(percentage, subdividedRoadNet[currentLaneIds[i]], vehicles[currentVehicles[i]], segments);

                            if (Settings.isMPServer)
                            {
                                string id = currentVehicles[i];
                                Multiplayer.SendVehiclePosition(id, vehicles[id]);
                            }

                            // Caution! This debug log can consume up to 10% CPU-performace on an i5-4570 @3.20GHz
                            // Debug.Log("Vehicle with id '" + currentVehicles[i] + "' is on lane '" + currentLaneIds[i] + "' which has index " +  laneIndex + " (" + currentLanePositions[i] + " of " + LaneLength + "m)");
                            // Debug.Log("Vehicle with id '" + currentVehicles[i] + "' is on lane '" + currentLaneIds[i]);
                        }
                        catch (Exception e)
                        {
                            //Error only at the editor
                        }
                    }

                    // Scheint sehr gut zu funktionieren...
                    // UpdateEgoVehicleInSumo kann man sich eventuell sparen?

                    // nach einer bestimmten Simulationszeit ist die Antwortliste leer - TraCI entfernt "idle" Fahrzeuge
                    // wenn man zu weit von der Karte entfernt ist, wird man ebenfalls entfernt
                    string currentLaneIdEgoVehicle = "";
                    try
                    {
                        currentLaneIdEgoVehicle = traci.vehicle.GetLaneId(new List<string> { egoVehicleId })[0];
                    }catch(ArgumentOutOfRangeException e)
                    {
                        Debug.LogError("EgoVehicle was removed from Simulation. Returning to MainMenu.\n" + e);
                        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
                        return; // jump out of function
                    }
                    int currentLaneIndexEgoVehicle = traci.vehicle.GetLaneIndex(new List<string> { egoVehicleId })[0];
                    string currentEdgeEgoVehicle = traci.lane.GetEdgeId(new List<string> { currentLaneIdEgoVehicle })[0];
                    Dictionary<string, IList<Vector3>> lanecoordinates = CorrectSumoReturns(currentLaneIdEgoVehicle, currentEdgeEgoVehicle);

                    //Debug.Log("Vehicle with id '" + egoVehicleId + "' is on lane '" + currentLaneIdEgoVehicle + " Position X: "+ (EgoCar.transform.position.x) + " Postition Z: " + (EgoCar.transform.position.z));
                    traci.vehicle.MoveToXY(egoVehicleId, "", importer.PropertiesOfEveryLane[currentLaneIdEgoVehicle].laneIndex, EgoCar.transform.position.x, EgoCar.transform.position.z, EgoCar.transform.rotation.eulerAngles.y);
                    CorrectEgoVehilceInSumo(currentLaneIdEgoVehicle, lanecoordinates);


                }
                else
                {
                    foreach (string id in currentVehicles)
                    {
                        ForeignCarMessage fcm = traci.vehicle.LatestMessages[id];
                        SumoVehicle sv = vehicles[id];
                        sv.velocity = fcm.velocity;
                        sv.transform.position = fcm.position;
                        sv.transform.rotation = Quaternion.Euler(fcm.rotation);

                    }
                }
            }
        }

        private Dictionary<string, IList<Vector3>> CorrectSumoReturns(string currentLaneIdEgoVehicleSumo, string currentEdgeEgoVehicleSumo)
        {
            Dictionary<string, LaneProperties>.KeyCollection lanePropertiesKeys;
            Dictionary<string, IList<Vector3>> lanecoordinates = new Dictionary<string, IList<Vector3>>();

            try
            {
                lanePropertiesKeys = importer.PropertiesOfEveryLane.Keys;
            }
            catch (Exception e)
            {
                Debug.LogError("Error in CheckVehicleToStreetPosition: Could not retrieve PropertiesOfEveryLane\n " + e.Message);
                // got an error, no use for going further
                return lanecoordinates;
            }

            foreach (string key in lanePropertiesKeys)
            {
                if (currentEdgeEgoVehicleSumo == importer.PropertiesOfEveryLane[key].parentedge)
                {
                    lanecoordinates.Add(key, subdividedRoadNet[key]);
                }
            }

            List<string> twoWay = importer.GetTwoWayLanes(currentLaneIdEgoVehicleSumo);
            //Fügt lanes des Gegenverkehr hinzu
            if (twoWay.Count >= 1)
            {
                foreach (string key in twoWay)
                {
                    lanecoordinates.Add(key, subdividedRoadNet[key]);
                }
            }
            /*
            float d = 0;
            foreach (string key in lanecoordinates.Keys)
            {
                d = CheckLaneIntersection(EgoCar.transform.position.x, EgoCar.transform.position.z, lanecoordinates[key], streetWidth);
                this.CurrentLaneDistance.Add(key, d);
            }
            correctlaneID = this.CurrentLaneDistance.OrderBy(k => k.Value).First().Key; // sort by value, smallest on top
            this.CurrentLaneDistance.Clear();
            */
            return lanecoordinates;
        }

        public void AddForeignCarMessage(ForeignCarMessage fcm)
        {
            if (!traci.vehicle.VehiclesFromHost.Contains(fcm.identifier))
            {
                traci.vehicle.LatestMessages.Add(fcm.identifier, fcm);
                traci.vehicle.VehiclesFromHost.Add(fcm.identifier);
                vehicles.Add(fcm.identifier, CreateNewVehicle(vehicleParent.transform, fcm.identifier));
            }

            traci.vehicle.LatestMessages[fcm.identifier] = fcm;

        }

        public void DeleteSumoVehicle(string id)
        {
            traci.vehicle.LatestMessages.Remove(id);
            traci.vehicle.VehiclesFromHost.Remove(id);
            if (vehicles.ContainsKey(id))
            {
                GameObject go = vehicles[id].gameObject;
                GameObject.Destroy(go);
                vehicles.Remove(id);
            }
        }

        // Close the connection and quit SUMO
        private void OnDestroy()
        {
            if (validMap)
            {
                // Stop SUMO
                traci.Stop();
            }
        }

        private void AdaptNumberOfExistingVehicles(IList<string> currentVehicles)
        {
            // Removing old ones
            List<string> keys = new List<string>(vehicles.Keys);

            foreach (string key in keys)
            {
                if (!currentVehicles.Contains(key))
                {
                    // Putting them back into a pool may be better than destroying
                    Destroy(vehicles[key].gameObject);
                    vehicles.Remove(key);
                    if (Settings.isMPServer) // change sumo server to mp server
                    {
                        Multiplayer.SendRemoveVehicle(key);
                    }
                }
            }

            // Adding new ones
            foreach (string item in currentVehicles)
            {
                if (!vehicles.ContainsKey(item))
                {
                    vehicles.Add(item, CreateNewVehicle(vehicleParent.transform, item));
                }
            }
        }

        private SumoVehicle CreateNewVehicle(Transform parent, string name)
        {
            SumoVehicle vehicle;
            SumoVehicle prefab = customVehiclePrefab;
            if (prefab != null)
            {
                //Use custom Prefab
                vehicle = Instantiate(prefab);
            }
            else
            {
                //Use default prefab
                vehicle = Instantiate(vehiclePrefab);
            }
            vehicle.name = "Vehicle_" + vehicleNumber++ + "_" + name;
            vehicle.transform.parent = parent;
            return vehicle;
        }

        /// <summary>
        /// Get the size of the egoCar and store it a local vector
        /// </summary>
        public void GetEgoCarSize()
        {
            MeshCollider[] meshcollid; // use meshcolliders (they're defined, hopefully)
            this.egoCarSize = new Vector3(0, 0, 0); // create empty Vector
            // collect meshColliders from limousine_collider_split_export
            try
            {
                if ((meshcollid = this.EgoCar.GetComponentsInChildren<MeshCollider>()) != null)
                {
                    // sum up the length of the meshcolliders
                    this.egoCarSize.x = meshcollid[0].bounds.size.x + meshcollid[1].bounds.size.x + meshcollid[2].bounds.size.x;
                    // width should be the same in all colliders, use it once
                    this.egoCarSize.z = meshcollid[0].bounds.size.z;
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error in GetEgoCarSize: " + e.Message);
            }
        }

        /// <summary>
        /// Creates a dict with 4 cornerpoints of a vehicle. Ignoring the Y coordinate
        /// </summary>
        /// <param name="root">Vector3 carroot set to the left middle of the vehicle</param>
        /// <param name="size">Vector3 size of the car</param>
        /// <param name="rotation">Quaternion with the rotation of the car</param>
        /// <returns>Dictionary with key "directionside" e.g. "frontright" and the vector at this position</returns>
        public Dictionary<string, Vector3> GetCornerPointsVehicle3D(Vector3 root, Vector3 size, Quaternion rotation)
        {
            // we only need 2d values x & z to create the corner points of the car box
            // so set y to 0
            // we expect root to be in the middle of the car
            float correction = 0.0f; // stay on the inner side of the box
            Quaternion cq = new Quaternion();
            cq = Quaternion.Euler(0, 90, 0); // default is -90 degree on y


            // create vectors with the offset from root to the cornerpoints
            Vector3 offsetFrontright = new Vector3(-size.x / 2 + correction, 0, size.z / 2 - correction);
            Vector3 offsetFrontleft = new Vector3(-size.x / 2 + correction, 0, -size.z / 2 + correction);
            Vector3 offsetBackright = new Vector3(size.x / 2 - correction, 0, +size.z / 2 - correction);
            Vector3 offsetBackleft = new Vector3(size.x / 2 - correction, 0, -size.z / 2 + correction);


            Dictionary<string, Vector3> cornerpointlist = new Dictionary<string, Vector3>();
            // right front 
            cornerpointlist.Add(CarPositions.Frontright.ToString(), (new Vector3(root.x, 0, root.z) + cq * rotation * offsetFrontright)); //new Vector3(root.x + size.x / 2 - correction, 0, root.z + size.z - correction));
            // left front
            cornerpointlist.Add(CarPositions.Frontleft.ToString(), (new Vector3(root.x, 0, root.z) + cq * rotation * offsetFrontleft));//new Vector3(root.x + size.x / 2 - correction, 0, root.z + correction));
            // right back
            cornerpointlist.Add(CarPositions.Backright.ToString(), (new Vector3(root.x, 0, root.z) + cq * rotation * offsetBackright));//new Vector3(root.x - size.x / 2 + correction, 0, root.z + size.z - correction));
            // left back
            cornerpointlist.Add(CarPositions.Backleft.ToString(), (new Vector3(root.x, 0, root.z) + cq * rotation * offsetBackleft));//new Vector3(root.x - size.x / 2 + correction, 0, root.z + correction));

            return cornerpointlist;
        }

        /// <summary>
        /// Checks if the egovehicle is on the current lane, if not sets invisible cars for sumo to simulate intelligent two way traffic
        /// </summary>
        /// <param name="currentLaneIdEgoVehicle"></param>
        /// <param name="lanecoordinates"></param>
        public void CorrectEgoVehilceInSumo(string currentLaneIdEgoVehicle, Dictionary<string, IList<Vector3>> lanecoordinates)
        {

            Dictionary<string, Vector3> carpoints = this.GetCornerPointsVehicle3D(new Vector3(this.EgoCar.transform.position.x, this.EgoCar.transform.position.y, this.EgoCar.transform.position.z), egoCarSize, this.EgoCar.transform.rotation);
            float streetWidth = importer.streetWidth;

            float d = 0;
            // For each key point defined, calculate the shorets distance to the shape given in sumocfg and return the distance and the laneid
            foreach (string keycarpoints in carpoints.Keys)
            {
                foreach (string key in lanecoordinates.Keys)
                {
                    d = GetDistanceToLanenode(carpoints[keycarpoints].x, carpoints[keycarpoints].z, lanecoordinates[key], streetWidth);
                    this.CurrentLaneDistance.Add(key, d);
                }
                var curLaneDis = this.CurrentLaneDistance.OrderBy(k => k.Value).First(); // sort by value, smallest on top
                LaneCarPointPosition[keycarpoints] = new KeyValuePair<string, float>(curLaneDis.Key, curLaneDis.Value);
                this.CurrentLaneDistance.Clear();
            }

            List<string> currentVehicles = traci.vehicle.GetIdList();

            int count = 0;
            string tmpid = "";
            // LaneCarPostition.Key == Keycarpoint
            // LaneCarPostition.Value.Key == laneId
            // LaneCarPostition.Value.Value == distance
            foreach (var position in LaneCarPointPosition)
            {

                if (position.Value.Key != currentLaneIdEgoVehicle)
                {
                    tmpid = position.Value.Key;
                    count++;
                }


                // wrong lane and no ghostcar added
                if (position.Value.Key != currentLaneIdEgoVehicle && !currentVehicles.Contains(position.Key))
                {
                    // create new ghostcar
                    traci.AddGhostVehicle(position.Value.Key, position.Key);

                    // move to position of cornerpoint
                    var tmp = GetEgoGhostcarposition(subdividedRoadNet[currentLaneIdEgoVehicle], EgoCar.transform.position, streetWidth, IsRightSideOffEgoCar(currentLaneIdEgoVehicle, position.Value.Key));
                    traci.vehicle.MoveToXY(position.Key, "", importer.PropertiesOfEveryLane[position.Value.Key].laneIndex, tmp.x, tmp.z, EgoCar.transform.rotation.eulerAngles.y);

                }
                // wrong lane and ghostcar already added
                else if (position.Value.Key != currentLaneIdEgoVehicle && currentVehicles.Contains(position.Key))
                {
                    var tmp = GetEgoGhostcarposition(subdividedRoadNet[currentLaneIdEgoVehicle], EgoCar.transform.position, streetWidth, IsRightSideOffEgoCar(currentLaneIdEgoVehicle, position.Value.Key));
                    traci.vehicle.MoveToXY(position.Key, "", importer.PropertiesOfEveryLane[position.Value.Key].laneIndex, tmp.x, tmp.z, EgoCar.transform.rotation.eulerAngles.y);
                }
                else // right line
                {
                    // look if ghostcar exits
                    if (currentVehicles.Contains(position.Key))
                    {
                        // move to position of egoVehicle
                        traci.vehicle.MoveToXY(position.Key, "", importer.PropertiesOfEveryLane[position.Value.Key].laneIndex, EgoCar.transform.position.x, EgoCar.transform.position.z, EgoCar.transform.rotation.eulerAngles.y);
                    }
                    // else do nothing
                }
            }

            // if 3 or more keypoints are on one side, move the egoCar as well, TraCI MoveToXY has some flaws so we change the positon of the egoCar
            if (count >= 3)
            {
                var tmp = GetEgoGhostcarposition(subdividedRoadNet[currentLaneIdEgoVehicle], EgoCar.transform.position, streetWidth, IsRightSideOffEgoCar(currentLaneIdEgoVehicle, tmpid));
                traci.vehicle.MoveToXY(egoVehicleId, "", importer.PropertiesOfEveryLane[tmpid].laneIndex, tmp.x, tmp.z, EgoCar.transform.rotation.eulerAngles.y);
            }
            else
            {
                traci.vehicle.MoveToXY(egoVehicleId, "", importer.PropertiesOfEveryLane[currentLaneIdEgoVehicle].laneIndex, EgoCar.transform.position.x, EgoCar.transform.position.z, EgoCar.transform.rotation.eulerAngles.y);
            }
            count = 0;
        }





        /// <summary>
        /// Get the distance from a given point to the shape from the lane
        /// </summary>
        /// <param name="xcarpoint">X-Coordinate of the Carpoint</param>
        /// <param name="zcarpoint">Z-Coordinate of the Carpoint</param>
        /// <param name="lanenodes">Coordinates of the lanenodes; lanenodes should be the center of the lane</param>
        /// <param name="lanewidth">Width of the lane</param>
        /// <returns>float distance</returns>
        private float GetDistanceToLanenode(float xcarpoint, float zcarpoint, IList<Vector3> lanenodes, float lanewidth)
        {
            // Calculate the distane of the Carpoint and Center of the Lane
            float x = 0.0f, z = 0.0f, m1 = 0.0f, m2 = 0.0f, n1 = 0.0f, n2 = 0.0f, d = 0.0f;
            if (lanenodes[0].x == lanenodes[1].x) //horizontal Lane 
                d = Math.Abs(xcarpoint - lanenodes[0].x);
            else if (lanenodes[0].z == lanenodes[1].z) //vertical Lane
                d = Math.Abs(zcarpoint - lanenodes[0].z);
            else // skew Lane
            {
                m1 = (lanenodes[0].z - lanenodes[1].z) / (lanenodes[0].x - lanenodes[1].x);
                n1 = lanenodes[0].z - m1 * lanenodes[0].x;
                m2 = -1 / m1;
                n2 = zcarpoint - m2 * xcarpoint;
                x = (n2 - n1) / (m1 - m2);
                z = m1 * x + n1;
                d = (float)Math.Sqrt((xcarpoint - x) * (xcarpoint - x) + (zcarpoint - z) * (zcarpoint - z));
            }

            return (float)Math.Round(d, 5, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Calculate an offset from the egoCar to the ghostcar
        /// </summary>
        /// <param name="lanenodesEgoCar"></param>
        /// <param name="EgoCarpos"></param>
        /// <param name="streetwidth"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private Vector3 GetEgoGhostcarposition(IList<Vector3> lanenodesEgoCar, Vector3 EgoCarpos, float streetwidth, Boolean right)
        {
            Vector3 Ghostcarposition = lanenodesEgoCar[1] - lanenodesEgoCar[0];
            Quaternion oq;
            if (right)
                oq = Quaternion.Euler(0, 90, 0);
            else
                oq = Quaternion.Euler(0, -90, 0);
            Ghostcarposition = oq * Ghostcarposition;
            Ghostcarposition = Ghostcarposition.normalized;
            Ghostcarposition.Scale(new Vector3(streetwidth, 0, streetwidth));
            Ghostcarposition.x = (float)Math.Round(Ghostcarposition.x, 5, MidpointRounding.AwayFromZero);
            Ghostcarposition.z = (float)Math.Round(Ghostcarposition.z, 5, MidpointRounding.AwayFromZero);
            //Debug.Log("X-Abweichung: " + Ghostcarposition.x + " Z-Abweichung: " + Ghostcarposition.z);
            Ghostcarposition += EgoCarpos;

            return Ghostcarposition;
        }

        /// <summary>
        /// Returns bool if laneId is left or right of egoCar
        /// </summary>
        /// <param name="egoCarLaneId"></param>
        /// <param name="ghostCarLaneId"></param>
        /// <returns>bool false if left, true if right</returns>
        public bool IsRightSideOffEgoCar(string egoCarLaneId, string ghostCarLaneId)
        {

            Vector3 egoLane = subdividedRoadNet[egoCarLaneId][1] - subdividedRoadNet[egoCarLaneId][0];
            Vector3 ghostLane = subdividedRoadNet[ghostCarLaneId][1] - subdividedRoadNet[ghostCarLaneId][0];

            if (egoLane.normalized == ghostLane.normalized) // same direction
            {
                if (importer.PropertiesOfEveryLane[egoCarLaneId].laneIndex > importer.PropertiesOfEveryLane[ghostCarLaneId].laneIndex)
                {
                    return true;
                }
            }
            // else we are on the left side
            return false;
        }


    }
}
