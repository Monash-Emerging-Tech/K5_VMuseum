//using UnityEngine;

//public class SevenJointXDriveControllerV3 : MonoBehaviour
//{
//    [System.Serializable]
//    public class JointMapping
//    {
//        [Tooltip("Name of the joint for reference.")]
//        public string jointName;

//        [Tooltip("ArticulationBody component representing the joint.")]
//        public ArticulationBody joint;

//        [Tooltip("Offset (in degrees) to add to the OPC UA joint value.")]
//        public float offset = 0f;

//        [Tooltip("Scale factor to apply to the OPC UA joint value (for unit conversion if needed).")]
//        public float scale = 1f;
//    }

//    [Header("Mapping from OPC UA Joint Values to URDF Joints (7 joints)")]
//    [Tooltip("Ensure these mappings are in the same order as the OPC UA joint values.")]
//    public JointMapping[] jointMappings = new JointMapping[7];

//    // Hardcoded input values for the 7 joints.
//    // Replace these with your OPC UA values when available.
//    private float[] inputJointValues = new float[7] { 45f, 30f, 60f, 90f, 15f, 120f, 75f };

//    private OPCUAClientV2 opcClientV2;  // Reference to OPCUAClientV2
//    private OPCUAClientV3 opcClientV3;  // Reference to OPCUAClientV3

//    private int i;

//    void Start()
//    {
//        // Find the OPCUAClient instance in the scene
//        opcClientV2 = FindFirstObjectByType<OPCUAClientV2>();
//        i = 2;
//        if (opcClientV2 == null)
//        {
//            i = 3;
//            opcClientV3 = FindFirstObjectByType<OPCUAClientV3>();
//            if (opcClientV3 == null)
//            {
//                Debug.LogError("OPCUAClient script is missing in the scene! Please add it.");
//            }
//        }
//    }

//    void FixedUpdate()
//    {
//        // Fetch live joint values from OPCUAClient
//        if (i==2) 
//        {
//            float[] inputJointValues = opcClientV2.jointValues;
//        }else if (i==3){
//            float[] inputJointValues = opcClientV3.jointValues;
//        }

        

//        if (jointMappings.Length != 7)
//        {
//            Debug.LogError("Please ensure exactly 7 joint mappings are provided.");
//            return;
//        }

//        // Loop over each joint and update its xDrive target.
//        for (int i = 0; i < 7; i++)
//        {
//            JointMapping mapping = jointMappings[i];

//            if (mapping.joint == null)
//            {
//                Debug.LogWarning($"Joint mapping '{mapping.jointName}' does not have an assigned ArticulationBody.");
//                continue;
//            }

//            // Calculate the adjusted target value.
//            // The input value is multiplied by the scale and then an offset is added.
//            float targetValue = inputJointValues[i] * mapping.scale + mapping.offset;

//            // Get the current xDrive configuration for the joint.
//            ArticulationDrive drive = mapping.joint.xDrive;

//            // Set the target value for the drive.
//            drive.target = targetValue;

//            // Re-assign the drive back to the joint.
//            mapping.joint.xDrive = drive;
//        }
//    }
//}
