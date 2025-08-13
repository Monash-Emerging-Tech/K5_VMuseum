using UnityEngine;

public class FreeCameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float movementSpeed = 5f;      // Movement speed in units per second.
    
    [Header("Rotation Settings")]
    public float rotationSpeed = 90f;     // Rotation speed in degrees per second.
    
    // Private variables to track pitch and yaw.
    private float yaw;
    private float pitch;

    void Start()
    {
        // Initialize yaw and pitch based on the current rotation.
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    /// <summary>
    /// Handles free movement in 3D space:
    /// - WASD moves relative to the current view.
    /// - Space moves up, LeftAlt moves down.
    /// </summary>
    private void HandleMovement()
    {
        Vector3 moveDirection = Vector3.zero;

        // Move forward/backward using W/S.
        if (Input.GetKey(KeyCode.W))
        {
            moveDirection += transform.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDirection -= transform.forward;
        }

        // Strafe left/right using A/D.
        if (Input.GetKey(KeyCode.A))
        {
            moveDirection -= transform.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveDirection += transform.right;
        }

        // Move up/down using Space (up) and LeftAlt (down).
        if (Input.GetKey(KeyCode.Q))
        {
            moveDirection += transform.up;
        }
        if (Input.GetKey(KeyCode.E))
        {
            moveDirection -= transform.up;
        }

        // Normalize to avoid faster diagonal movement and update position.
        if (moveDirection != Vector3.zero)
        {
            moveDirection.Normalize();
            transform.position += moveDirection * movementSpeed * Time.deltaTime;
        }
    }

    /// <summary>
    /// Handles camera rotation with I/K for pitch and J/L for yaw,
    /// ensuring that roll stays at 0.
    /// </summary>
    private void HandleRotation()
    {
        // Update pitch (look up/down) with I (up) and K (down).
        if (Input.GetKey(KeyCode.I))
        {
            pitch += rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.K))
        {
            pitch -= rotationSpeed * Time.deltaTime;
        }

        // Update yaw (look left/right) with J (left) and L (right).
        if (Input.GetKey(KeyCode.J))
        {
            yaw -= rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.L))
        {
            yaw += rotationSpeed * Time.deltaTime;
        }

        // Optionally clamp the pitch to prevent flipping (e.g., between -85 and 85 degrees)
        pitch = Mathf.Clamp(pitch, -85f, 85f);

        // Create a rotation from the pitch and yaw, with roll set to 0.
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
