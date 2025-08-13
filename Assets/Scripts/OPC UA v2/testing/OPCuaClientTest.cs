using System;
using System.Threading.Tasks;
using UnityEngine;
using Opc.Ua;
using Opc.Ua.Client;

public class OPCUASubscriberRobot1 : MonoBehaviour
{
    private Session session;
    private string serverUrl = "opc.tcp://172.24.200.1:4840/server/";

    // Array of NodeIds for the 7 joints of Robot 1
    private NodeId[] jointNodeIds = new NodeId[7];

    // Array to hold the latest joint values (for external use)
    public float[] JointValues = new float[7];

    // OPC UA subscription and its monitored items
    private Subscription subscription;
    private MonitoredItem[] monitoredItems;

    async void Awake()
    {
        await InitializeOPCUAClient();
        CreateSubscription();
    }

    private async Task InitializeOPCUAClient()
    {
        try
        {
            Debug.Log("Connecting to OPC UA server for Robot 1 subscriber...");

            // Select the endpoint and build the client configuration
            var endpointDescription = CoreClientUtils.SelectEndpoint(serverUrl, false);
            var config = new ApplicationConfiguration
            {
                ApplicationName = "Unity OPC UA Subscriber for Robot1",
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
            session = await Session.Create(config, endpoint, false, "Unity OPC UA Subscriber Session", 60000, null, null);
            Debug.Log($"Connected to OPC UA server at {serverUrl}");

            // For Robot 1, use namespace index 21 and initialize nodeIds for each joint (R1d_Joi1 to R1d_Joi7)
            for (int i = 0; i < 7; i++)
            {
                string nodeName = $"R1d_Joi{i + 1}";
                jointNodeIds[i] = new NodeId(nodeName, 21);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error initializing OPC UA client: " + ex.Message);
        }
    }

    private void CreateSubscription()
    {
        try
        {
            // Create a new subscription with a publishing interval of 100 ms
            subscription = new Subscription(session.DefaultSubscription)
            {
                PublishingInterval = 100
            };

            // Initialize and configure monitored items for each joint node
            monitoredItems = new MonitoredItem[7];
            for (int i = 0; i < 7; i++)
            {
                monitoredItems[i] = new MonitoredItem(subscription.DefaultItem)
                {
                    DisplayName = jointNodeIds[i].ToString(),
                    StartNodeId = jointNodeIds[i],
                    AttributeId = Attributes.Value,
                    SamplingInterval = 100,
                    QueueSize = 0,
                    DiscardOldest = true
                };
                monitoredItems[i].Notification += OnMonitoredItemNotification;
            }

            // Add the monitored items to the subscription and add the subscription to the session
            subscription.AddItems(monitoredItems);
            session.AddSubscription(subscription);
            subscription.Create();
            Debug.Log("Subscription created for Robot 1 monitored items.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error creating subscription: " + ex.Message);
        }
    }

    // This event handler is called whenever a monitored item's value changes
    private void OnMonitoredItemNotification(MonitoredItem item, MonitoredItemNotificationEventArgs e)
    {
        foreach (var value in item.DequeueValues())
        {
            if (value?.Value != null)
            {
                try
                {
                    // Assume the value is sent as a double and convert it to float
                    float floatVal = (float)(double)value.Value;
                    Debug.Log($"{item.DisplayName} changed to: {floatVal}");

                    // Update the JointValues array (match the item by DisplayName)
                    for (int i = 0; i < 7; i++)
                    {
                        if (jointNodeIds[i].ToString() == item.DisplayName)
                        {
                            JointValues[i] = floatVal;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error processing monitored item notification: " + ex.Message);
                }
            }
        }
    }

    async void OnApplicationQuit()
    {
        if (session != null)
        {
            await session.CloseAsync();
        }
    }
}
