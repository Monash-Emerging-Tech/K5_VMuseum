using UnityEngine;

public class ZAxisMover : MonoBehaviour
{
    public float speed = 2f;       // Movement speed
    public float lowerOffset = -3f; // Minimum offset from initial Z
    public float upperOffset = 3f;  // Maximum offset from initial Z

    private float startZ;           // Initial Z position
    private int direction = 1;      // 1 = forward, -1 = backward

    void Start()
    {
        startZ = transform.position.z; // Record initial Z
    }

    void Update()
    {
        // Move along Z relative to initial position
        float move = speed * Time.deltaTime * direction;
        float currentZ = transform.position.z;
        float newZ = currentZ + move;

        // Calculate relative position to initial
        float relativeZ = newZ - startZ;

        // Check bounds relative to start
        if (relativeZ >= upperOffset)
        {
            relativeZ = upperOffset;
            direction = -1;
        }
        else if (relativeZ <= lowerOffset)
        {
            relativeZ = lowerOffset;
            direction = 1;
        }

        // Apply movement relative to initial position
        transform.position = new Vector3(transform.position.x,
                                         transform.position.y,
                                         startZ + relativeZ);
    }
}