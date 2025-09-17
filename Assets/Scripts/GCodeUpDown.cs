using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GCodePlayer_Z : MonoBehaviour
{
    private Transform toolHead;
    public float scale = 0.1f;
    public Vector3 originOffset = Vector3.zero;

    [Tooltip("Maximum distance along Z the part can move upward from its starting (lowest) position")]
    public float maxZDistance = 5f; // in Unity units

    private Vector3 origin;

    // Full G-code array (unchanged)
    private string[] gcodeLines = new string[]
    {
        "G1 X0 Y0 Z0.3 F1500",
        "G1 X100 Y0 Z0.3 F1500",
        "G1 X100 Y50 Z0.3 F1500",
        "G1 X0 Y50 Z0.3 F1500",
        "G1 X0 Y0 Z0.3 F1500",

        "G1 X0 Y0 Z0.3 F1500",
        "G1 X100 Y50 Z0.3 F1500",
        "G1 X100 Y0 Z0.3 F1500",
        "G1 X0 Y50 Z0.3 F1500",

        "G1 X25 Y15 Z0.3 F1500",
        "G1 X75 Y15 Z0.3 F1500",
        "G1 X75 Y35 Z0.3 F1500",
        "G1 X25 Y35 Z0.3 F1500",
        "G1 X25 Y15 Z0.3 F1500",

        "G1 X25 Y15 Z0.3 F1500",
        "G1 X75 Y15 Z0.3 F1500",
        "G1 X25 Y20 Z0.3 F1500",
        "G1 X75 Y20 Z0.3 F1500",
        "G1 X25 Y25 Z0.3 F1500",
        "G1 X75 Y25 Z0.3 F1500",
        "G1 X25 Y30 Z0.3 F1500",
        "G1 X75 Y30 Z0.3 F1500",
        "G1 X25 Y35 Z0.3 F1500",
        "G1 X75 Y35 Z0.3 F1500",

        "G1 X50 Y25 Z0.3 F1500"
    };

    private int currentLine = 0;
    private Vector3 targetPos;
    private float moveSpeed = 1f;

    // Original G-code Z max value (used for scaling)
    private float gcodeZMax = 0.3f;

    void Start()
    {
        toolHead = transform;
        origin = toolHead.position + originOffset;
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

        // Keep X/Y fixed
        float x = toolHead.position.x;
        float y = toolHead.position.z; // Unity Z-axis as G-code Y
        float z = toolHead.position.y;

        foreach (string part in parts)
        {
            if (part.StartsWith("Z"))
            {
                float originalZ = float.Parse(part.Substring(1));
                // Scale Z from G-code range (0 → gcodeZMax) to (0 → maxZDistance) upward
                z = origin.y + (originalZ / gcodeZMax) * maxZDistance;
            }

            if (part.StartsWith("F")) moveSpeed = float.Parse(part.Substring(1)) / 1000f;
        }

        targetPos = new Vector3(x, z, y);
    }
}
