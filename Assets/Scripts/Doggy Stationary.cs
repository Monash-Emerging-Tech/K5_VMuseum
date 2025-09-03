using UnityEngine;

public class LockArticulationBody : MonoBehaviour
{
    private ArticulationBody ab;

    void Start()
    {
        ab = GetComponent<ArticulationBody>();

        if (ab != null)
        {
            // Lock X axis
            var xDrive = ab.xDrive;
            xDrive.stiffness = 100000f;  // very high stiffness
            xDrive.damping = 1000f;      // high damping
            xDrive.forceLimit = 100000f; // ensure it can hold
            xDrive.target = 0f;          // lock at 0 degrees
            ab.xDrive = xDrive;

            // Lock Y axis
            var yDrive = ab.yDrive;
            yDrive.stiffness = 100000f;
            yDrive.damping = 1000f;
            yDrive.forceLimit = 100000f;
            yDrive.target = 0f;
            ab.yDrive = yDrive;

            // Lock Z axis
            var zDrive = ab.zDrive;
            zDrive.stiffness = 100000f;
            zDrive.damping = 1000f;
            zDrive.forceLimit = 100000f;
            zDrive.target = 0f;
            ab.zDrive = zDrive;
        }
        else
        {
            Debug.LogWarning("No ArticulationBody found on this GameObject.");
        }
    }
}
