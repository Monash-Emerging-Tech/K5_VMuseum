using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

public class OPCUAClientV3 : MonoBehaviour
{
    private const string serverUrl = "opc.tcp://192.168.0.3:4840"; // OPC UA server address
    private Session session;
    
    [Header("Polled Joint Values (Accessible to Other Scripts)")]
    public float[] jointValues = new float[7]; // Stores live joint values

    async void Start()
    {
        await ConnectToServer();

        if (session != null && session.Connected)
        {
            Debug.Log("Connected to OPC UA server!");
            InvokeRepeating(nameof(ReadRobot3Joints), 1.0f, 0.1f); // Poll every 100ms
        }
        else
        {
            Debug.LogError("Failed to connect to OPC UA server.");
        }
    }

    private async Task ConnectToServer()
    {
        try
        {
            ApplicationConfiguration config = new ApplicationConfiguration()
            {
                ApplicationName = "UnityOPCUAClient",
                ApplicationType = ApplicationType.Client,
                SecurityConfiguration = new SecurityConfiguration
                {
                    AutoAcceptUntrustedCertificates = true
                },
                ClientConfiguration = new ClientConfiguration
                {
                    DefaultSessionTimeout = 60000
                },
                TransportConfigurations = new TransportConfigurationCollection(),
                TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                ServerConfiguration = new ServerConfiguration(),
                CertificateValidator = new CertificateValidator()
            };

            config.CertificateValidator.CertificateValidation += (sender, e) => { e.Accept = true; };

            var endpointConfiguration = EndpointConfiguration.Create(config);
            var selectedEndpoint = CoreClientUtils.SelectEndpoint(serverUrl, false);
            var endpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);

            var session = await Session.Create(
                config, endpoint, false, "Unity OPCUA Client",
                60000, null, null
            );

            this.session = session;
        }
        catch (Exception ex)
        {
            Debug.LogError("Error connecting to OPC UA server: " + ex.Message);
        }
    }

    private void ReadRobot3Joints()
    {
        if (session == null || !session.Connected)
        {
            Debug.LogError("Session is not connected!");
            return;
        }

        string[] nodeIds = {
            "ns=3;s=Rd_Joi1",
            "ns=3;s=Rd_Joi2",
            "ns=3;s=Rd_Joi3",
            "ns=3;s=Rd_Joi4",
            "ns=3;s=Rd_Joi5",
            "ns=3;s=Rd_Joi6",
            "ns=3;s=Rd_Joi7"
        };

        ReadValueIdCollection nodesToRead = new ReadValueIdCollection();

        foreach (string nodeId in nodeIds)
        {
            nodesToRead.Add(new ReadValueId()
            {
                NodeId = new NodeId(nodeId),
                AttributeId = Attributes.Value
            });
        }

        DataValueCollection results;
        DiagnosticInfoCollection diagnosticInfos;
        session.Read(null, 0, TimestampsToReturn.Both, nodesToRead, out results, out diagnosticInfos);

        for (int i = 0; i < nodeIds.Length; i++)
        {
            if (results[i].Value != null)
            {
                jointValues[i] = Convert.ToSingle(results[i].Value);
                Debug.Log($"Joint {i + 1} Value: {jointValues[i]}");
            }
        }
    }

    private void OnApplicationQuit()
    {
        session?.Close();
        Debug.Log("Disconnected from OPC UA server.");
    }
}
