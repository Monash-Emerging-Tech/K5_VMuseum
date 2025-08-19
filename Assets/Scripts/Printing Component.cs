using UnityEngine;

public class PrinterComponent : MonoBehaviour
{
    public float stepSize = 0.5f;       // How much to move each step
    public float stepTime = 1f;         // Time between each step (seconds)
    public float lowerBound = 0f;       // Minimum Y position
    public float upperBound = 5f;       // Maximum Y position

    private float nextStepTime;
    private int direction = 1;          // 1 = up, -1 = down

    void Start()
    {
        // Start at the lower bound
        Vector3 pos = transform.position;
        pos.y = lowerBound;
        transform.position = pos;

        nextStepTime = Time.time + stepTime;
    }

    void Update()
    {
        if (Time.time >= nextStepTime)
        {
            // Move the component by stepSize in the current direction
            Vector3 pos = transform.position;
            pos.y += stepSize * direction;

            // Clamp within bounds
            if (pos.y >= upperBound)
            {
                pos.y = upperBound;
                direction = -1; // reverse to down
            }
            else if (pos.y <= lowerBound)
            {
                pos.y = lowerBound;
                direction = 1;  // reverse to up
            }

            transform.position = pos;

            // Schedule next step
            nextStepTime = Time.time + stepTime;
        }
    }
}
