using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GCodePlayer_XYZ : MonoBehaviour
{
    private Transform toolHead;
    public Vector3 originOffset = Vector3.zero;

    [Tooltip("Maximum distance along X the part can move from its starting position")]
    public float maxXDistance = 5f; // Unity units

    [Tooltip("Maximum distance along Y the part can move from its starting position")]
    public float maxYDistance = 5f;

    [Tooltip("Maximum distance along Z the part can move vertically")]
    public float maxZDistance = 5f;

    [Tooltip("Speed multiplier for X axis")]
    public float speedXMultiplier = 1f;

    [Tooltip("Speed multiplier for Y axis")]
    public float speedYMultiplier = 1f;

    [Tooltip("Speed multiplier for Z axis")]
    public float speedZMultiplier = 1f;

    private Vector3 origin;

    // Example G-code: moves around a square
    public string[] originalGCode = new string[]
    {
        "G1 X0 Y0 Z0 F1500",    // Start bottom-left
        "G1 X100 Y0 Z0 F1500",  // Bottom-right
        "G1 X100 Y100 Z0 F1500",// Top-right
        "G1 X0 Y100 Z0 F1500",  // Top-left
        "G1 X0 Y0 Z0 F1500"     // Back to start
    };

    private List<Vector3> positionsToMove = new List<Vector3>();
    private List<float> speeds = new List<float>();
    private int currentIndex = 0;

    private float gcodeXMax = 100f;
    private float gcodeYMax = 100f;
    private float gcodeZMax = 100f;

    void Awake()
    {
        toolHead = transform;
        origin = toolHead.position + originOffset;
        ParseGCode();
    }

    void Update()
    {
        if (currentIndex >= positionsToMove.Count) return;

        Vector3 target = positionsToMove[currentIndex];
        float speed = speeds[currentIndex];

        // Move toolhead toward current target
        toolHead.position = Vector3.MoveTowards(toolHead.position, target, speed * Time.deltaTime);

        // Move to next step when reached
        if (Vector3.Distance(toolHead.position, target) < 0.001f)
        {
            currentIndex++;
        }
    }

    void ParseGCode()
    {
        positionsToMove.Clear();
        speeds.Clear();

        Vector3 lastPos = toolHead.position;

        foreach (string line in originalGCode)
        {
            string[] parts = line.Split(' ');

            float x = lastPos.x;
            float y = lastPos.z; // Unity Z-axis = G-code Y
            float z = lastPos.y; // Unity Y-axis = G-code Z
            float feedRate = 1500f;

            foreach (string part in parts)
            {
                if (part.StartsWith("X"))
                    x = origin.x + (float.Parse(part.Substring(1)) / gcodeXMax) * maxXDistance;
                if (part.StartsWith("Y"))
                    y = origin.z + (float.Parse(part.Substring(1)) / gcodeYMax) * maxYDistance;
                if (part.StartsWith("Z"))
                    z = origin.y + (float.Parse(part.Substring(1)) / gcodeZMax) * maxZDistance;
                if (part.StartsWith("F"))
                    feedRate = float.Parse(part.Substring(1));
            }

            // Sequentially move each axis if it changed
            if (Mathf.Abs(x - lastPos.x) > 0.001f)
            {
                positionsToMove.Add(new Vector3(x, lastPos.y, lastPos.z));
                speeds.Add((feedRate / 1000f) * speedXMultiplier);
                lastPos.x = x;
            }
            if (Mathf.Abs(y - lastPos.z) > 0.001f)
            {
                positionsToMove.Add(new Vector3(lastPos.x, lastPos.y, y));
                speeds.Add((feedRate / 1000f) * speedYMultiplier);
                lastPos.z = y;
            }
            if (Mathf.Abs(z - lastPos.y) > 0.001f)
            {
                positionsToMove.Add(new Vector3(lastPos.x, z, lastPos.z));
                speeds.Add((feedRate / 1000f) * speedZMultiplier);
                lastPos.y = z;
            }
        }
    }
}
