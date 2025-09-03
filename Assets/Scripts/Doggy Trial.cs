using UnityEngine;

public class DogLegController : MonoBehaviour
{
    public ArticulationBody legJoint;
    public float targetAngle = 30f;
    public float speed = 2f;

    void Start()
    {
        if (legJoint == null)
            legJoint = GetComponent<ArticulationBody>();

        // Configure drive so the leg can actually move
        var drive = legJoint.xDrive;
        drive.stiffness = 500f;  // How strongly the leg tries to reach the target
        drive.damping = 20f;     // Smooths movement
        drive.forceLimit = 1000f; // Maximum torque applied
        legJoint.xDrive = drive;
    }

    void Update()
    {
        // Animate the leg back and forth
        var drive = legJoint.xDrive;
        drive.target = targetAngle * Mathf.Sin(Time.time * speed);
        legJoint.xDrive = drive;
    }
}
