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
        "G1 X0.1 Y0.1 Z0",
        "G1 X25 Y0.1 Z0",
        "G1 X25 Y25 Z0",
        "G1 X0.1 Y25 Z0",
        "G1 X0.1 Y10 Z0",
        "G1 X0.1 Y5 Z0",
        "G1 X0.1 Y0.1 Z0",
        "G1 X0.1 Y0.1 Z1",
        "G1 X25 Y0.1 Z1",
        "G1 X25 Y25 Z1",
        "G1 X0.1 Y25 Z1",
        "G1 X0.1 Y10 Z1",
        "G1 X0.1 Y5 Z1",
        "G1 X0.1 Y0.1 Z1",

        "G1 X0.1 Y0.1 Z1",
        "G1 X25 Y0.1 Z2",
        "G1 X25 Y25 Z2",
        "G1 X0.1 Y25 Z2",
        "G1 X0.1 Y10 Z2",
        "G1 X0.1 Y5 Z2",
        "G1 X0.1 Y0.1 Z2",
        "G1 X0.1 Y0.1 Z3",
        "G1 X25 Y0.1 Z3",
        "G1 X25 Y25 Z3",
        "G1 X0.1 Y25 Z3",
        "G1 X0.1 Y10 Z3",
        "G1 X0.1 Y5 Z3",
        "G1 X0.1 Y0.1 Z3",



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

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
                StartNextMove();
            return;
        }

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / moveTime);

        Vector3 nextPos = Vector3.Lerp(startPos, targetPos, t);

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

        Vector3 delta = targetPos - startPos;
        float dx = Mathf.Abs(delta.x) / speedXMultiplier;
        float dy = Mathf.Abs(delta.y) / speedYMultiplier;
        float dz = Mathf.Abs(delta.z) / speedZMultiplier;

        // Shared moveTime
        moveTime = Mathf.Max(dx, dy, dz, 0.1f);
        elapsed = 0f;
        currentIndex++;
    }

    void ParseGCode()
    {
        positionsToMove.Clear();
        Vector3 lastPos = transform.position;

        foreach (string line in originalGCode)
        {
            string[] parts = line.Split(' ');

            float x = lastPos.x;
            float y = lastPos.y; // Unity Y = GCode Z
            float z = lastPos.z; // Unity Z = GCode Y

            foreach (string part in parts)
            {
                if (part.StartsWith("X"))
                    x = origin.x + (float.Parse(part.Substring(1)) / gcodeXMax) * maxXDistance;
                if (part.StartsWith("Y"))
                    z = origin.z + (float.Parse(part.Substring(1)) / gcodeYMax) * maxYDistance;
                if (part.StartsWith("Z"))
                    y = origin.y + (float.Parse(part.Substring(1)) / gcodeZMax) * maxZDistance;
            }

            Vector3 nextPos = new Vector3(x, y, z);
            positionsToMove.Add(nextPos);
            lastPos = nextPos; // remember for next line to avoid teleport
        }
    }
}
