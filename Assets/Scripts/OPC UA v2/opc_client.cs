using System.Threading.Tasks;
using UnityEngine;
using Opc.Ua;
using Opc.Ua.Client;
using System;
using System.Threading;
using System.Collections.Concurrent;

public class OPCUAClientV2 : MonoBehaviour
{
    private Session session;
    private string serverUrl = "opc.tcp://172.24.200.1:4840/server/";
    private bool isConnected = false;
    private readonly object lockObject = new object();
    private CancellationTokenSource cancellationTokenSource;
    private ConcurrentDictionary<int, RobotData> robotsData = new ConcurrentDictionary<int, RobotData>();

    // Helper class to store NodeIds and Joint Values for one robot
    [Serializable]
    public class RobotData
    {
        public NodeId[] NodeIds;       // NodeId for each joint
        public float[] JointValues;    // The actual read values
        public DateTime LastUpdateTime; // Track when the data was last updated

        public RobotData()
        {
            NodeIds = new NodeId[7];
            JointValues = new float[7];
            LastUpdateTime = DateTime.MinValue;
        }
    }

    // We will store 4 robots (indexing from 1..4 for clarity)
    public RobotData[] robots = new RobotData[5];

    // The actual instance so other scripts can call this client
    public static OPCUAClientV2 Instance { get; private set; }

    // Events for connection status changes
    public event Action<bool> OnConnectionStatusChanged;

    async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        cancellationTokenSource = new CancellationTokenSource();
        await InitializeOPCUAClient();
        StartPolling();
    }

    private void OnDestroy()
    {
        cancellationTokenSource?.Cancel();
        Disconnect();
    }

    private async Task InitializeOPCUAClient()
    {
        try
        {
            Debug.Log("Connecting to OPC UA server...");
            var endpointDescription = CoreClientUtils.SelectEndpoint(serverUrl, false);

            var config = new ApplicationConfiguration
            {
                ApplicationName = "Unity OPC UA Client Test",
                ApplicationType = ApplicationType.Client,
                SecurityConfiguration = new SecurityConfiguration
                {
                    AutoAcceptUntrustedCertificates = true
                },
                TransportConfigurations = new TransportConfigurationCollection(),
                TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 }
            };

            EndpointConfiguration endpointConfig = EndpointConfiguration.Create(config);
            ConfiguredEndpoint endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfig);
            session = await Session.Create(config, endpoint, false, "Unity OPC UA Client Session", 60000, null, null);
            
            // Create data structures for Robot1..Robot4
            for (int i = 1; i <= 4; i++)
            {
                robots[i] = new RobotData();
                for (int j = 0; j < 7; j++)
                {
                    string nodeName = $"R{i}d_Joi{j + 1}";
                    ushort nsIndex = GetNamespaceIndexFromRobotID(i);
                    robots[i].NodeIds[j] = new NodeId(nodeName, nsIndex);
                }
            }

            isConnected = true;
            OnConnectionStatusChanged?.Invoke(true);
            Debug.Log($"Connected to OPC UA server at {serverUrl}");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error initializing OPC UA client: " + ex.Message);
            isConnected = false;
            OnConnectionStatusChanged?.Invoke(false);
        }
    }

    private void StartPolling()
    {
        Task.Run(async () =>
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    if (!isConnected)
                    {
                        await Task.Delay(5000, cancellationTokenSource.Token); // Wait 5 seconds before retrying
                        await InitializeOPCUAClient();
                        continue;
                    }

                    await ReadFromOPCUA();
                    await Task.Delay(50, cancellationTokenSource.Token); // Poll every 50ms
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error in polling loop: " + ex.Message);
                    isConnected = false;
                    OnConnectionStatusChanged?.Invoke(false);
                }
            }
        }, cancellationTokenSource.Token);
    }

    private void Disconnect()
    {
        try
        {
            session?.Close();
            isConnected = false;
            OnConnectionStatusChanged?.Invoke(false);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error disconnecting: " + ex.Message);
        }
    }

    private async Task ReadFromOPCUA()
    {
        if (!isConnected || session == null) return;

        try
        {
            for (int i = 1; i <= 4; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    DataValue val = await session.ReadValueAsync(robots[i].NodeIds[j]);
                    if (val?.Value != null)
                    {
                        robots[i].JointValues[j] = (float)(double)val.Value;
                    }
                }
                robots[i].LastUpdateTime = DateTime.Now;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error reading from OPC UA: {ex.Message}");
            isConnected = false;
            OnConnectionStatusChanged?.Invoke(false);
        }
    }

    // Helper function to map Robot ID to the right namespace index
    private ushort GetNamespaceIndexFromRobotID(int robotID)
    {
        switch (robotID)
        {
            case 1: return 21;
            case 2: return 22;
            case 3: return 23;
            case 4: return 24;
            default:
                throw new ArgumentOutOfRangeException("Invalid robot ID");
        }
    }

    // Public method to check if data is fresh
    public bool IsDataFresh(int robotID, float maxAgeSeconds = 0.1f)
    {
        if (robotID < 1 || robotID > 4) return false;
        var age = (DateTime.Now - robots[robotID].LastUpdateTime).TotalSeconds;
        return age <= maxAgeSeconds;
    }
}
