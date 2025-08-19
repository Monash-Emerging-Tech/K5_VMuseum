using UnityEngine;

public class PrinterBed : MonoBehaviour
{
    public float stepSize = 0.5f;      // Step size going forward
    public float stepPeriod = 1f;      // Time between steps going forward
    public float returnSpeed = 2f;     // Speed returning back to initial position
    public float endOffset = 3f;       // Distance from initial position

    private float startX;              // Initial X position
    private float targetOffset;        // Target offset when stepping forward
    private float stepTimer = 0f;      // Timer for stepping
    private bool returning = false;    // Are we returning smoothly?

    void Start()
    {
        startX = transform.position.x;
        targetOffset = 0f;
    }

    void Update()
    {
        if (!returning)
        {
            // Stepped forward movement
            stepTimer += Time.deltaTime;

            if (stepTimer >= stepPeriod)
            {
                stepTimer = 0f;
                targetOffset += stepSize;

                if (targetOffset >= endOffset)
                {
                    targetOffset = endOffset;
                    returning = true; // switch to smooth return
                }
            }

            // Move instantly to current targetOffset (stepped)
            transform.position = new Vector3(startX + targetOffset, transform.position.y, transform.position.z);
        }
        else
        {
            // Smooth return to initial position
            float currentX = transform.position.x;
            float newX = Mathf.MoveTowards(currentX, startX, returnSpeed * Time.deltaTime);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);

            // When reached initial, reset to step again
            if (Mathf.Approximately(newX, startX))
            {
                targetOffset = 0f;
                stepTimer = 0f;
                returning = false;
            }
        }
    }
}