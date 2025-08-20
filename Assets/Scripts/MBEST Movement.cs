using UnityEngine;

public class WormMovement : MonoBehaviour
{
    [Tooltip("Direction of movement in local space")]
    public Vector3 direction = Vector3.forward;

    [Tooltip("Distance moved per step")]
    public float stepLength = 1f;

    [Tooltip("Speed of movement in units per second")]
    public float speed = 0.5f;

    [Tooltip("Time to pause between steps (seconds)")]
    public float pauseTime = 0.2f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float stepProgress = 0f;
    private bool isMoving = true;
    private float pauseTimer = 0f;

    void Start()
    {
        startPosition = transform.position;
        targetPosition = startPosition + direction.normalized * stepLength;
    }

    void Update()
    {
        if (isMoving)
        {
            // Move towards target position
            stepProgress += speed * Time.deltaTime / stepLength; // normalized 0..1
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
