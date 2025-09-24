using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GCodePlayer_Y : MonoBehaviour
{
    private Transform toolHead;
    public Vector3 originOffset = Vector3.zero;

    [Tooltip("Maximum distance along Y the part can move backward from its starting (forwardmost) position")]
    public float maxYDistance = 5f; // in Unity units

    private Vector3 origin;

    // Full G-code with X/Y/Z, F etc.
    private string[] originalGCode = new string[]
    {
        "G1 X50 Y50 Z0.3 F1500",
        "G1 X0 Y0 Z0.3 F1500",
        "G1 X100 Y0 Z0.3 F1500",
        "G1 X100 Y100 Z0.3 F1500",
        "G1 X0 Y100 Z0.3 F1500",
        "G1 X50 Y50 Z0.3 F1500"
    };

    private string[] gcodeLines;
    private int currentLine = 0;
    private Vector3 targetPos;
    private float moveSpeed = 1f;

    // Original G-code Y max value (used for scaling)
    private float gcodeYMax = 100f;

    void Start()
    {
        toolHead = transform;
        origin = toolHead.position + originOffset;

        gcodeLines = originalGCode;
        ParseLine(gcodeLines[0]);
    }

    void Update()
    {
        toolHead.position = Vector3.MoveTowards(toolHead.position, targetPos, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(toolHead.position, targetPos) < 0.001f && currentLine < gcodeLines.Length - 1)
        {
            currentLine++;
            ParseLine(gcodeLines[currentLine]);
        }
    }

    void ParseLine(string line)
    {
        string[] parts = line.Split(' ');

        float x = toolHead.position.x; // Ignore X
        float y = toolHead.position.z; // Unity Z-axis as G-code Y
        float z = toolHead.position.y;

        foreach (string part in parts)
        {
            if (part.StartsWith("Y"))
            {
                float originalY = float.Parse(part.Substring(1));
                // Only move along Y
                y = origin.z - (originalY / gcodeYMax) * maxYDistance;
            }

            if (part.StartsWith("F"))
            {
                moveSpeed = float.Parse(part.Substring(1)) / 1000f;
            }
        }

        targetPos = new Vector3(x, z, y);
    }
}
