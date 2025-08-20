using UnityEngine;

public class RotateObject2 : MonoBehaviour
{
    [Tooltip("Axis to rotate around (local space)")]
    public Vector3 rotationAxis = Vector3.up;

    [Tooltip("Rotation speed in degrees per second")]
    public float rotationSpeed = 100f;

    [Tooltip("Geometric center for rotation relative to object")]
    public Vector3 geometricCenter = Vector3.zero;

    void Update()
    {
        // Normalize axis
        Vector3 axis = rotationAxis.normalized;

        // Calculate world position of geometric center
        Vector3 worldCenter = transform.TransformPoint(geometricCenter);

        // Rotate around the given axis through the geometric center
        transform.RotateAround(worldCenter, transform.TransformDirection(axis), rotationSpeed * Time.deltaTime);
    }
}
