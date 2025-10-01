using System.Collections.Generic;
using UnityEngine;

public class NewGCodeReader : MonoBehaviour
{
    [Header("Control Options")]
    public bool playAutomatically = true;
    public bool loopPath = false;
    public float pauseBetweenMoves = 0.5f;

    [Header("Scaling Distances")]
    public Vector3 originOffset = Vector3.zero;
    public float maxXDistance = 5f;
    public float maxYDistance = 5f;
    public float maxZDistance = 5f;

    [Header("Axis Speeds (units per second)")]
    public float speedXMultiplier = 1f;
    public float speedYMultiplier = 1f;
    public float speedZMultiplier = 1f;

    [Header("Axis Movement Enabled")]
    public bool moveX = true;
    public bool moveY = true;
    public bool moveZ = true;

    [Header("Test GCode")]
    [System.NonSerialized]
    public string[] originalGCode = new string[]
    {
        "G1 X0 Y0 Z0 F1500",
        "G1 X100 Y0 Z1 F1500",
        "G1 X100 Y100 Z2 F1500",
        "G1 X0 Y100 Z3 F1500",
        "G1 X0 Y0 Z4 F1500",
        "G1 X75 Y50 Z5 F1500",
        "G1 X0 Y0 Z6 F1500",
        "G1 X100 Y0 Z7 F1500",
        "G1 X100 Y100 Z8 F1500",
        "G1 X0 Y100 Z9 F1500",
        "G1 X0 Y0 Z10 F1500",
        "G1 X75 Y50 Z11 F1500",
        "G1 X0 Y0 Z12 F1500",
        "G1 X100 Y0 Z45 F1500",
        "G1 X100 Y100 Z0 F1500",
        "G1 X0 Y100 Z20 F1500",
        "G1 X0 Y0 Z50 F1500",
        "G1 X75 Y50 Z0 F1500",
        "G1 X0 Y0 Z0 F1500",
        "G1 X100 Y0 Z0 F1500",
        "G1 X100 Y100 Z0 F1500",
        "G1 X0 Y100 Z0 F1500",
        "G1 X0 Y0 Z0 F1500",
        "G1 X75 Y50 Z0 F1500",
        "G1 X0 Y0 Z0 F1500",
        "G1 X0 Y0 Z0 F1500",
        "G1 X10 Y0 Z0 F1500",
        "G1 X20 Y0 Z0 F1500",
        "G1 X30 Y0 Z0 F1500",
        "G1 X40 Y0 Z0 F1500",
        "G1 X50 Y0 Z0 F1500",
        "G1 X50 Y10 Z0 F1500",
        "G1 X40 Y10 Z0 F1500",
        "G1 X30 Y10 Z0 F1500",
        "G1 X20 Y10 Z0 F1500",
        "G1 X10 Y10 Z0 F1500",
        "G1 X0 Y10 Z0 F1500",
        "G1 X0 Y20 Z0 F1500",
        "G1 X10 Y20 Z0 F1500",
        "G1 X20 Y20 Z0 F1500",
        "G1 X30 Y20 Z0 F1500",
        "G1 X40 Y20 Z0 F1500",
        "G1 X50 Y20 Z0 F1500",
        "G1 X50 Y30 Z0 F1500",
        "G1 X40 Y30 Z0 F1500",
        "G1 X30 Y30 Z0 F1500",
        "G1 X20 Y30 Z0 F1500",
        "G1 X10 Y30 Z0 F1500",
        "G1 X0 Y30 Z0 F1500",
        "G1 X0 Y40 Z0 F1500",
        "G1 X10 Y40 Z0 F1500",
        "G1 X20 Y40 Z0 F1500",
        "G1 X30 Y40 Z0 F1500",
        "G1 X40 Y40 Z0 F1500",
        "G1 X50 Y40 Z0 F1500",
        "G1 X50 Y50 Z0 F1500",
        "G1 X40 Y50 Z0 F1500",
        "G1 X30 Y50 Z0 F1500",
        "G1 X20 Y50 Z0 F1500",
        "G1 X10 Y50 Z0 F1500",
        "G1 X0 Y50 Z0 F1500",
        "G1 X0 Y60 Z0 F1500",
        "G1 X10 Y60 Z0 F1500",
        "G1 X20 Y60 Z0 F1500",
        "G1 X30 Y60 Z0 F1500",
        "G1 X40 Y60 Z0 F1500",
        "G1 X50 Y60 Z0 F1500",
        "G1 X50 Y70 Z0 F1500",
        "G1 X40 Y70 Z0 F1500",
        "G1 X30 Y70 Z0 F1500",
        "G1 X20 Y70 Z0 F1500",
        "G1 X10 Y70 Z0 F1500",
        "G1 X0 Y70 Z0 F1500",
        "G1 X0 Y80 Z0 F1500",
        "G1 X50 Y80 Z0 F1500",
        "G1 X50 Y90 Z0 F1500",
        "G1 X0 Y90 Z0 F1500"
    };

    private Vector3 origin;
    private List<Vector3> positionsToMove = new List<Vector3>();
    private int currentIndex = 0;
    private Vector3 startPos;
    private Vector3 targetPos;
    private float waitTimer = 0f;

    private float gcodeXMax = 100f;
    private float gcodeYMax = 100f;
    private float gcodeZMax = 100f;

    // **Shared moveTime for all objects**
    public static float moveTime = 0.1f;
    private float elapsed = 0f;

    void Start()
    {
        origin = transform.position + originOffset;
        ParseGCode();

        if (playAutomatically && positionsToMove.Count > 0)
            StartNextMove();
    }

    void Update()
    {
        if (currentIndex > positionsToMove.Count) return;

        // Pause timer
        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
                StartNextMove();
            return;
        }

        // Move towards target using shared moveTime
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / moveTime);

        Vector3 nextPos = Vector3.Lerp(startPos, targetPos, t);

        // Apply axis checkboxes
        if (!moveX) nextPos.x = transform.position.x;
        if (!moveY) nextPos.y = transform.position.y;
        if (!moveZ) nextPos.z = transform.position.z;

        transform.position = nextPos;

        if (t >= 1f)
        {
            if (pauseBetweenMoves > 0f)
                waitTimer = pauseBetweenMoves;
            else
                StartNextMove();
        }
    }

    void StartNextMove()
    {
        if (currentIndex >= positionsToMove.Count)
        {
            if (loopPath && positionsToMove.Count > 0)
            {
                currentIndex = 0;
            }
            else
                return;
        }

        startPos = transform.position;
        targetPos = positionsToMove[currentIndex];

        // **Calculate moveTime based on all axes**
        Vector3 delta = targetPos - startPos;
        float dx = Mathf.Abs(delta.x) / speedXMultiplier;
        float dy = Mathf.Abs(delta.y) / speedYMultiplier;
        float dz = Mathf.Abs(delta.z) / speedZMultiplier;

        moveTime = Mathf.Max(dx, dy, dz, 0.1f); // shared across all objects

        elapsed = 0f;
        currentIndex++;
    }

    void ParseGCode()
    {
        positionsToMove.Clear();

        foreach (string line in originalGCode)
        {
            string[] parts = line.Split(' ');

            float x = origin.x;
            float y = origin.y; // Unity Y = GCode Z
            float z = origin.z; // Unity Z = GCode Y

            foreach (string part in parts)
            {
                if (part.StartsWith("X"))
                    x = origin.x + (float.Parse(part.Substring(1)) / gcodeXMax) * maxXDistance;
                if (part.StartsWith("Y"))
                    z = origin.z + (float.Parse(part.Substring(1)) / gcodeYMax) * maxYDistance;
                if (part.StartsWith("Z"))
                    y = origin.y + (float.Parse(part.Substring(1)) / gcodeZMax) * maxZDistance;
            }

            positionsToMove.Add(new Vector3(x, y, z));
        }
    }
}
