using UnityEngine;
using System.Collections;

public class PeriodicMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Direction of movement (local space)")]
    public Vector3 direction = Vector3.forward;

    [Tooltip("Movement speed (units per second)")]
    public float speed = 2f;

    [Header("Timing Settings")]
    [Tooltip("Initial pause before first movement (seconds)")]
    public float initialDelay = 2f;

    [Tooltip("Duration of movement phase (seconds)")]
    public float moveDuration = 3f;

    [Tooltip("Duration of pause phase (seconds)")]
    public float pauseDuration = 2f;

    private bool isMoving = false;

    void Start()
    {
        // Start the movement cycle after initial delay
        StartCoroutine(MovementRoutine());
    }

    IEnumerator MovementRoutine()
    {
        // Initial one-off delay
        if (initialDelay > 0f)
            yield return new WaitForSeconds(initialDelay);

        // Infinite loop of move → pause
        while (true)
        {
            // Move phase
            isMoving = true;
            float moveTimer = 0f;
            while (moveTimer < moveDuration)
            {
                transform.Translate(direction.normalized * speed * Time.deltaTime, Space.Self);
                moveTimer += Time.deltaTime;
                yield return null;
            }
            isMoving = false;

            // Pause phase
            if (pauseDuration > 0f)
                yield return new WaitForSeconds(pauseDuration);
        }
    }
}