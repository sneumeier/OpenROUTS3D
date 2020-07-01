using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace Traci
{
    /// <summary>
    /// This script contains all methods to start and stop a connection to SUMO
    /// </summary>
    public class TraciSumoConnector
    {
        private const String IP_ADRESS = "127.0.0.1"; // Use localhost, SUMO-Server and "Unity-Client" will run on the same machine
        public static int PORT = 8811;                // Use a free TCP-Port on your local machine
        private const int NUM_RETRIES = 10;           // Number of retries to connect with TraCI-Server
        private const double SIMSTEP = 0.02;          // Length of one SUMO Simulation Step 
        private string startSumoCommand = "sumo";     // Start SUMO via comand line with this comand (either sumo or sumo-gui)

        private static TraciSumoConnector _instance = null;
        private static TcpClient _client = null;
        private static NetworkStream _stream = null;

        /// <summary>
        /// Private Constructor of this class to prevent instanciation (Singleton)
        /// </summary>
        private TraciSumoConnector() { }

        /// <summary>
        /// Accessor of TraciSumoConnector,
        /// </summary>
        public static TraciSumoConnector Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TraciSumoConnector();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Accessor of the TCP-Stream
        /// </summary>
        public NetworkStream Stream
        {
            get
            {
                if (_client.Connected)
                {
                    return _stream;
                }
                UnityEngine.Debug.Log("No Connection");
                return null;
            }
        }

        /// <summary>
        /// Method to initialize the connection to SUMO
        /// </summary>
        public void Init(string sumocfg)
        {
            if (Settings.sumoGui && Settings.isSumoServer)
            {
                startSumoCommand = "sumo-gui";
            } 
            else
            {
                startSumoCommand = "sumo";
            }
            try
            {
                if (Settings.isSumoServer)
                { 
                    // 1. Start SUMO as a server
                    StartSumo(sumocfg);
                    // 2. Connect to SUMO
                    Connect();
                }

                
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex);
            }
        }

        /// <summary>
        /// Method to close and clean up the connection
        /// </summary>
        public void Close()
        {
            _stream.Close();
            _client.Close();

            foreach (Process process in Process.GetProcessesByName(startSumoCommand))
            {
                process.Kill();
            }
        }

        /// <summary>
        /// Method to start sumo-gui application and do all relevant checks before
        /// </summary>
        private void StartSumo(string sumocfg)
        {
            // Check if port is already in use
            try     { PortInUse(PORT); }
            catch   { throw; }

            // Check if sumo-gui-processes exist and kill instances if so
            try     { SumoGuiInUse(); }
            catch   { throw;  }

            // Check if sumocfg-file is available
            try     { SumocfgAvailable(sumocfg); }
            catch   { throw; }

            if(Settings.isSumoServer)
            {
                // Finally start sumo-gui as a server
                try     { StartSumoGui(sumocfg); }
                catch   { throw; }

            }
        }

        /// <summary>
        /// Method to check if designated port is already in use
        /// </summary>
        private void PortInUse(int port)
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();

            foreach (IPEndPoint endPoint in ipEndPoints)
            {
                if (endPoint.Port == port)
                {
                    throw new Exception("Port: " + port + " is already in use");
               }
            }
        }

        /// <summary>
        /// Method to check if sumo-gui-processes exist and kill instances if so
        /// </summary>
        private void SumoGuiInUse()
        {
            Process[] processes = Process.GetProcessesByName(startSumoCommand);

            foreach (Process proc in processes)
                proc.CloseMainWindow();
        }

        /// <summary>
        /// Method to check if sumocfg-file is available.
        /// </summary>
        private void SumocfgAvailable(string sumocfg)
        {
            if (sumocfg == null || sumocfg.Length == 0)
            {
                throw new ArgumentNullException("No sumocfg-File was given.");
            }

            FileInfo fInfo = new FileInfo(sumocfg);

            if (fInfo.Exists == false)
            {
                throw new FileNotFoundException("The file: " + sumocfg + " was not found.");
            }
        }

        /// <summary>
        /// Method to start sumo-gui as a server
        /// </summary>
        private void StartSumoGui(string sumocfg)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = startSumoCommand;
                startInfo.Arguments = "--configuration-file \"" + sumocfg + "\" " +
                              "--remote-port " + PORT + " " +
                              "--step-length " + SIMSTEP + " " +
                              "--start";
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;

                Process.Start(startInfo);
            }
            catch(Exception e)
            {
                throw new Exception("starting " + startSumoCommand + " was not possible", e);
            }
        }

        /// <summary>
        /// Method to connect to sumo-gui-server
        /// </summary>
        private void Connect()
        {
            UnityEngine.Debug.Log("Connecting SUMO");
            try
            {
                int sleepDuration = 0; // milliseconds
                _client = new TcpClient();
                
                for (int i = 0; i < NUM_RETRIES; i++)
                {
                    _client.Connect(IP_ADRESS, PORT);

                    if (_client.Connected)
                    {
                        UnityEngine.Debug.Log("Unity successfully connected with SUMO");
                        _stream = _client.GetStream();
                        break;
                    }
                    else
                    {
                        sleepDuration = i * 25;
                        UnityEngine.Debug.Log("Connect to SUMO, retry in: " + sleepDuration);
                        Thread.Sleep(sleepDuration);
                    }
                }
            }
            catch (SocketException e)
            {
                throw e;
            }
        }
    }
}
