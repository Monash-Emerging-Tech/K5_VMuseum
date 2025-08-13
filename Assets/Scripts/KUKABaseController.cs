using UnityEngine;

public class SimpleCameraBasedController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed in units per second.")]
    public float movementSpeed = 5f;

    [Header("Rotation Settings")]
    [Tooltip("Rotation speed in degrees per second.")]
    public float rotationSpeed = 90f;

    [Header("Camera Reference")]
    [Tooltip("Reference to the camera whose orientation determines movement direction. " +
             "This camera should be oriented parallel to the ground (global XZ plane).")]
    public Transform cameraTransform;

    private ArticulationBody articulationBody;

    // Cached input values (updated every frame)
    private float verticalInput;
    private float horizontalInput;
    private float yawInput;
    private float pitchInput;

    void Start()
    {
        articulationBody = GetComponent<ArticulationBody>();
    }

    // Read input every frame
    void Update()
    {
        // Use built-in axes for WASD movement (make sure these are set up in the Input Manager)
        verticalInput = Input.GetAxisRaw("Vertical");   // W/S keys
        horizontalInput = Input.GetAxisRaw("Horizontal"); // A/D keys

        // Custom keys for yaw (J, L) and pitch (I, K)
        yawInput = (Input.GetKey(KeyCode.J) ? -1f : 0f) + (Input.GetKey(KeyCode.L) ? 1f : 0f);
        pitchInput = (Input.GetKey(KeyCode.I) ? -1f : 0f) + (Input.GetKey(KeyCode.K) ? 1f : 0f);
    }

    // Apply movement and rotation in sync with physics
    void FixedUpdate()
    {
        // Compute camera's forward and right directions, flattened to the XZ plane
        Vector3 camForward = cameraTransform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = cameraTransform.right;
        camRight.y = 0f;
        camRight.Normalize();

        // Calculate movement direction from input and ensure normalization to avoid faster diagonal movement
        Vector3 moveDir = (verticalInput * camForward + horizontalInput * camRight).normalized;
        Vector3 newPosition = articulationBody.transform.position + moveDir * movementSpeed * Time.fixedDeltaTime;

        // Calculate base yaw rotation (around global Y axis)
        float yawAmount = yawInput * rotationSpeed * Time.fixedDeltaTime;
        Quaternion newRotation = Quaternion.Euler(0f, yawAmount, 0f) * articulationBody.transform.rotation;

        // Apply movement and base (yaw) rotation to the ArticulationBody
        articulationBody.TeleportRoot(newPosition, newRotation);

        // Apply camera pitch rotation (around the camera's right axis) if needed
        float pitchAmount = pitchInput * rotationSpeed * Time.fixedDeltaTime;
        if (!Mathf.Approximately(pitchAmount, 0f))
        {
            cameraTransform.Rotate(cameraTransform.right, pitchAmount, Space.World);
        }
    }
}
