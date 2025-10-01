using System.Collections.Generic;
using UnityEngine;

public class NewGCodeReader : MonoBehaviour
{
    [Header("Control Options")]
    public bool playAutomatically = true;
    public bool loopPath = false;
    public float pauseBetweenMoves = 0f;

    [Header("Scaling Distances")]
    public Vector3 originOffset = Vector3.zero;
    public float maxXDistance = 5f;
    public float maxYDistance = 5f;
    public float maxZDistance = 5f;

    [Header("Axis Speeds (units per second)")]
    public float speedXMultiplier = 1f;
    public float speedYMultiplier = 1f;
    public float speedZMultiplier = 1f;

    [Header("Test GCode")]
    [System.NonSerialized] // Prevent Unity from overriding this in the Inspector
    public string[] originalGCode = new string[]
       {
         "G1 X0 Y0 Z0 F1500",
         "G1 X100 Y0 Z0 F1500",
         "G1 X100 Y100 Z0 F1500",
         "G1 X0 Y100 Z0 F1500",
         "G1 X0 Y0 Z0 F1500",
         "G1 X75 Y50 Z0 F1500",
         "G1 X74.8 Y54.36 Z0 F1500",
         "G1 X73.5 Y58.30 Z0 F1500",
         "G1 X71.2 Y61.80 Z0 F1500",
         "G1 X68.0 Y64.68 Z0 F1500",
         "G1 X64.0 Y66.83 Z0 F1500",
         "G1 X59.5 Y68.16 Z0 F1500",
         "G1 X54.7 Y68.57 Z0 F1500",
         "G1 X50.0 Y68.00 Z0 F1500",
         "G1 X45.3 Y68.57 Z0 F1500",
         "G1 X40.5 Y68.16 Z0 F1500",
         "G1 X35.9 Y66.83 Z0 F1500",
         "G1 X31.9 Y64.68 Z0 F1500",
         "G1 X28.7 Y61.80 Z0 F1500",
         "G1 X26.4 Y58.30 Z0 F1500",
         "G1 X25.2 Y54.36 Z0 F1500",
         "G1 X25.0 Y50.0 Z0 F1500",
         "G1 X25.2 Y45.64 Z0 F1500",
         "G1 X26.4 Y41.70 Z0 F1500",
         "G1 X28.7 Y38.20 Z0 F1500",
         "G1 X31.9 Y35.32 Z0 F1500",
         "G1 X35.9 Y33.17 Z0 F1500",
         "G1 X40.5 Y31.84 Z0 F1500",
         "G1 X45.3 Y31.43 Z0 F1500",
         "G1 X50.0 Y32.00 Z0 F1500",
         "G1 X54.7 Y31.43 Z0 F1500",
         "G1 X59.5 Y31.84 Z0 F1500",
         "G1 X64.0 Y33.17 Z0 F1500",
         "G1 X68.0 Y35.32 Z0 F1500",
         "G1 X71.2 Y38.20 Z0 F1500",
         "G1 X73.5 Y41.70 Z0 F1500",
         "G1 X74.8 Y45.64 Z0 F1500",
         "G1 X75 Y50 Z0 F1500"
    };

    private Vector3 origin;
    private List<Vector3> positionsToMove = new List<Vector3>();
    private int currentIndex = 0;
    private float moveTime = 0f;
    private float elapsed = 0f;
    private Vector3 startPos;
    private Vector3 targetPos;
    private float waitTimer = 0f;

    private float gcodeXMax = 100f;
    private float gcodeYMax = 100f;
    private float gcodeZMax = 100f;

    void Start()
    {
        origin = transform.position + originOffset;
        ParseGCode();

        if (playAutomatically && positionsToMove.Count > 0)
            StartNextMove();
    }

    void Update()
    {
        if (currentIndex >= positionsToMove.Count)
        {
            if (loopPath && positionsToMove.Count > 0)
            {
                currentIndex = 0;
                StartNextMove();
            }
            return;
        }

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / moveTime);
        transform.position = Vector3.Lerp(startPos, targetPos, t);

        if (t >= 1f)
            StartNextMove();
    }

    void StartNextMove()
    {
        if (currentIndex >= positionsToMove.Count) return;

        startPos = transform.position;
        targetPos = positionsToMove[currentIndex];

        Vector3 delta = targetPos - startPos;
        float tx = Mathf.Abs(delta.x) / speedXMultiplier;
        float ty = Mathf.Abs(delta.y) / speedYMultiplier;
        float tz = Mathf.Abs(delta.z) / speedZMultiplier;

        moveTime = Mathf.Max(tx, ty, tz);
        if (moveTime <= 0f) moveTime = 0.1f;

        elapsed = 0f;
        currentIndex++;

        if (pauseBetweenMoves > 0f)
            waitTimer = pauseBetweenMoves;
    }

    void ParseGCode()
    {
        positionsToMove.Clear();

        foreach (string line in originalGCode)
        {
            string[] parts = line.Split(' ');

            float x = origin.x;
            float y = origin.y;
            float z = origin.z;

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
