using UnityEngine;
using System;

public class SevenJointXDriveControllerV2 : MonoBehaviour
{
    [System.Serializable]
    public class JointMapping
    {
        [Tooltip("Name of the joint for reference.")]
        public string jointName;

        [Tooltip("ArticulationBody component representing the joint.")]
        public ArticulationBody joint;

        [Tooltip("Offset (in degrees) to add to the OPC UA joint value.")]
        public float offset = 0f;

        [Tooltip("Scale factor to apply to the OPC UA joint value (for unit conversion if needed).")]
        public float scale = 1f;

        [Tooltip("Maximum allowed change in joint angle per second (degrees).")]
        public float maxVelocity = 180f;

        [Tooltip("Current target value for smooth interpolation.")]
        public float currentTarget = 0f;
    }

    [Header("Mapping from OPC UA Joint Values to URDF Joints (7 joints)")]
    [Tooltip("Ensure these mappings are in the same order as the OPC UA joint values.")]
    public JointMapping[] jointMappings = new JointMapping[7];

    [Header("Select Robot (1=Robot1, 2=Robot2, 3=Robot3, 4=Robot4)")]
    public int robotID = 3;

    [Header("Interpolation Settings")]
    [Tooltip("Maximum allowed change in joint angle per second (degrees).")]
    public float defaultMaxVelocity = 180f;

    [Tooltip("Maximum age of OPC UA data before considering it stale (seconds).")]
    public float maxDataAge = 0.1f;

    private OPCUAClientV2 opcClientV2;
    private bool isConnected = false;
    private float lastUpdateTime = 0f;
    private const float UPDATE_INTERVAL = 0.02f; // 50Hz update rate

    void Start()
    {
        opcClientV2 = FindFirstObjectByType<OPCUAClientV2>();
        if (opcClientV2 == null)
        {
            Debug.LogError("No instance of OPCUAClientV2 found in the scene. Please add it before using this controller.");
            enabled = false;
            return;
        }

        // Subscribe to connection status changes
        opcClientV2.OnConnectionStatusChanged += HandleConnectionStatusChanged;

        // Validate robot ID
        if (robotID < 1 || robotID > 4)
        {
            Debug.LogWarning("Robot ID is out of range (must be 1..4). Defaulting to Robot3 (ID=3).");
            robotID = 3;
        }

        // Initialize joint mappings
        for (int i = 0; i < jointMappings.Length; i++)
        {
            if (jointMappings[i].joint == null)
            {
                Debug.LogError($"Joint mapping {i} is missing an ArticulationBody reference!");
                enabled = false;
                return;
            }
            jointMappings[i].maxVelocity = defaultMaxVelocity;
        }
    }

    void OnDestroy()
    {
        if (opcClientV2 != null)
        {
            opcClientV2.OnConnectionStatusChanged -= HandleConnectionStatusChanged;
        }
    }

    private void HandleConnectionStatusChanged(bool connected)
    {
        isConnected = connected;
        if (!connected)
        {
            Debug.LogWarning($"Lost connection to OPC UA server for Robot {robotID}");
        }
        else
        {
            Debug.Log($"Reconnected to OPC UA server for Robot {robotID}");
        }
    }

    void FixedUpdate()
    {
        if (!isConnected || !opcClientV2.IsDataFresh(robotID, maxDataAge))
        {
            return;
        }

        float currentTime = Time.time;
        if (currentTime - lastUpdateTime < UPDATE_INTERVAL)
        {
            return;
        }
        lastUpdateTime = currentTime;

        float[] inputJointValues = opcClientV2.robots[robotID].JointValues;
        float deltaTime = Time.fixedDeltaTime;

        for (int i = 0; i < 7; i++)
        {
            JointMapping mapping = jointMappings[i];
            
            // Calculate target value with scale and offset
            float targetValue = inputJointValues[i] * mapping.scale + mapping.offset;

            // Smoothly interpolate to the target value
            float maxDelta = mapping.maxVelocity * deltaTime;
            mapping.currentTarget = Mathf.MoveTowards(mapping.currentTarget, targetValue, maxDelta);

            // Update the joint's xDrive
            ArticulationDrive drive = mapping.joint.xDrive;
            drive.target = mapping.currentTarget;
            mapping.joint.xDrive = drive;
        }
    }
}
