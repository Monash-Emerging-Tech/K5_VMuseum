using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GCodePlayer_X : MonoBehaviour
{
    private Transform toolHead;
    public Vector3 originOffset = Vector3.zero;

    [Tooltip("Maximum distance along X the part can move from its starting (leftmost) position")]
    public float maxXDistance = 5f; // in Unity units

    private Vector3 origin;

    // Full G-code with X/Y/Z, F etc.
    private string[] originalGCode = new string[]
    {
        "G1 X50 Y50 Z0.3 F1500",  // Start at center
        "G1 X0 Y0 Z0.3 F1500",    // Bottom-left corner
        "G1 X100 Y0 Z0.3 F1500",  // Bottom-right corner
        "G1 X100 Y100 Z0.3 F1500",// Top-right corner
        "G1 X0 Y100 Z0.3 F1500",  // Top-left corner
        "G1 X50 Y50 Z0.3 F1500"   // Back to center
    };

    // Repeat 3 times
    private string[] gcodeLines;

    private int currentLine = 0;
    private Vector3 targetPos;
    private float moveSpeed = 1f; // Unity units per second

    // Original G-code X max value (used for scaling)
    private float gcodeXMax = 100f;

    private float moveStartTime;
    private float moveDuration;

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

    void Start()
    {
        toolHead = transform;
        origin = toolHead.position + originOffset;
        ParseLine(gcodeLines[0]);
    }

    void Update()
    {
        // Move only in X
        toolHead.position = Vector3.MoveTowards(toolHead.position, targetPos, moveSpeed * Time.deltaTime);

        // If move duration elapsed, go to next line
        if (Time.time - moveStartTime >= moveDuration && currentLine < gcodeLines.Length - 1)
        {
            currentLine++;
            ParseLine(gcodeLines[currentLine]);
        }
    }

    void ParseLine(string line)
    {
        string[] parts = line.Split(' ');

        // Current position as starting point
        float x = toolHead.position.x;
        float y = toolHead.position.z; // Keep Z fixed
        float z = toolHead.position.y; // Vertical stays the same

        float feedRate = 1500f; // Default F if none provided
        float oldX = x;

        foreach (string part in parts)
        {
            if (part.StartsWith("X"))
            {
                float originalX = float.Parse(part.Substring(1));
                x = origin.x + (originalX / gcodeXMax) * maxXDistance;
            }

            if (part.StartsWith("F"))
            {
                feedRate = float.Parse(part.Substring(1));
            }
        }

        // Convert feedrate to Unity units/sec and slow down by factor 0.5
        moveSpeed = (feedRate / 1000f) * 0.5f;

        // Calculate distance to move in X
        float distance = Mathf.Abs(x - oldX);

        // Calculate duration to move that distance
        moveDuration = distance / moveSpeed;

        // If distance is zero, still pause to simulate move
        if (distance == 0f)
        {
            moveDuration = 0.1f; // minimal pause
        }

        moveStartTime = Time.time;
        targetPos = new Vector3(x, z, y);
    }
}
