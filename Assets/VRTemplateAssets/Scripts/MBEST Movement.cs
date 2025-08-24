using UnityEngine;

public class WormMovement2 : MonoBehaviour
{
    [Tooltip("Direction of movement in local space")]
    public Vector3 direction = Vector3.forward;

    [Tooltip("Distance moved per step")]
    public float stepLength = 1f;

    [Tooltip("Speed of movement in units per second")]
    public float speed = 0.5f;

    [Tooltip("Time to pause between steps (seconds)")]
    public float pauseTime = 0.2f;

    [Tooltip("Time to wait before starting movement after input (seconds)")]
    public float inputStartDelay = 1f;

    [Tooltip("Key to trigger worm movement")]
    public KeyCode startKey = KeyCode.Space;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float stepProgress = 0f;
    private bool isMoving = false;
    private float pauseTimer = 0f;

    // Input-based delay handling
    private bool waitingForInput = true;
    private bool waitingInitialDelay = false;
    private float initialDelayTimer = 0f;

    void Start()
    {
        startPosition = transform.position;
        targetPosition = startPosition + direction.normalized * stepLength;
    }

    void Update()
    {
        // --- Waiting for player input ---
        if (waitingForInput)
        {
            if (Input.GetKeyDown(startKey))
            {
                waitingForInput = false;
                waitingInitialDelay = true;
                initialDelayTimer = 0f;
            }
            return; // don’t move until input is pressed
        }

        // --- Waiting the one-time initial delay ---
        if (waitingInitialDelay)
        {
            initialDelayTimer += Time.deltaTime;
            if (initialDelayTimer >= inputStartDelay)
            {
                waitingInitialDelay = false;
                isMoving = true; // start movement
                stepProgress = 0f;
            }
            return; // skip until delay finishes
        }

        // --- Normal movement cycle ---
        if (isMoving)
        {
            // Move towards target position
            stepProgress += speed * Time.deltaTime / stepLength;
            transform.position = Vector3.Lerp(startPosition, targetPosition, stepProgress);

            if (stepProgress >= 1f)
            {
                // Reached target, start pause
                isMoving = false;
                pauseTimer = 0f;
            }
        }
        else
        {
            // Pause between steps
            pauseTimer += Time.deltaTime;
            if (pauseTimer >= pauseTime)
            {
                // Start next step
                startPosition = transform.position;
                targetPosition = startPosition + direction.normalized * stepLength;
                stepProgress = 0f;
                isMoving = true;
            }
        }
    }
}
