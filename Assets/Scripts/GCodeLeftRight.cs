using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GCodePlayer_X : MonoBehaviour
{
    private Transform toolHead;
    public float scale = 0.1f;
    public Vector3 originOffset = Vector3.zero;

    [Tooltip("Maximum distance along X the part can move from its starting (leftmost) position")]
    public float maxXDistance = 5f; // in Unity units

    private Vector3 origin;

    // Single-layer filled square G-code
    private string[] originalGCode = new string[]
        {
        "G1 X0 Y0 Z0.3 F1500",
        "G1 X100 Y0 Z0.3 F1500",
        "G1 X100 Y5 Z0.3 F1500",
        "G1 X0 Y5 Z0.3 F1500",
        "G1 X0 Y10 Z0.3 F1500",
        "G1 X100 Y10 Z0.3 F1500",
        "G1 X100 Y15 Z0.3 F1500",
        "G1 X0 Y15 Z0.3 F1500",
        "G1 X0 Y20 Z0.3 F1500",
        "G1 X100 Y20 Z0.3 F1500",
        "G1 X100 Y25 Z0.3 F1500",
        "G1 X0 Y25 Z0.3 F1500",
        "G1 X0 Y30 Z0.3 F1500",
        "G1 X100 Y30 Z0.3 F1500",
        "G1 X100 Y35 Z0.3 F1500",
        "G1 X0 Y35 Z0.3 F1500",
        "G1 X0 Y40 Z0.3 F1500",
        "G1 X100 Y40 Z0.3 F1500",
        "G1 X100 Y45 Z0.3 F1500",
        "G1 X0 Y45 Z0.3 F1500",
        "G1 X0 Y50 Z0.3 F1500",
        "G1 X100 Y50 Z0.3 F1500",
        "G1 X100 Y55 Z0.3 F1500",
        "G1 X0 Y55 Z0.3 F1500",
        "G1 X0 Y60 Z0.3 F1500",
        "G1 X100 Y60 Z0.3 F1500",
        "G1 X100 Y65 Z0.3 F1500",
        "G1 X0 Y65 Z0.3 F1500",
        "G1 X0 Y70 Z0.3 F1500",
        "G1 X100 Y70 Z0.3 F1500",
        "G1 X100 Y75 Z0.3 F1500",
        "G1 X0 Y75 Z0.3 F1500",
        "G1 X0 Y80 Z0.3 F1500",
        "G1 X100 Y80 Z0.3 F1500",
        "G1 X100 Y85 Z0.3 F1500",
        "G1 X0 Y85 Z0.3 F1500",
        "G1 X0 Y90 Z0.3 F1500",
        "G1 X100 Y90 Z0.3 F1500",
        "G1 X100 Y95 Z0.3 F1500",
        "G1 X0 Y95 Z0.3 F1500",
        "G1 X0 Y100 Z0.3 F1500",
        "G1 X100 Y100 Z0.3 F1500",

        // Finish at center
        "G1 X50 Y50 Z0.3 F1500"
        };

    // Repeat 3 times
    private string[] gcodeLines;

    void Awake()
    {
        int repeatCount = 3;
        gcodeLines = new string[originalGCode.Length * repeatCount];

        for (int i = 0; i < repeatCount; i++)
        {
            for (int j = 0; j < originalGCode.Length; j++)
            {
                gcodeLines[i * originalGCode.Length + j] = originalGCode[j];
            }
        }
    }

    private int currentLine = 0;
    private Vector3 targetPos;
    private float moveSpeed = 1f;

    // Original G-code X max value (used for scaling)
    private float gcodeXMax = 100f;

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

        // Keep Y/Z fixed
        float x = toolHead.position.x;
        float y = toolHead.position.z;
        float z = toolHead.position.y;

        foreach (string part in parts)
        {
            if (part.StartsWith("X"))
            {
                float originalX = float.Parse(part.Substring(1));
                // Scale X from G-code range (0 → gcodeXMax) to (0 → maxXDistance)
                x = origin.x + (originalX / gcodeXMax) * maxXDistance;
            }

            if (part.StartsWith("F")) moveSpeed = float.Parse(part.Substring(1)) / 1000f;
        }

        targetPos = new Vector3(x, z, y);
    }
}
