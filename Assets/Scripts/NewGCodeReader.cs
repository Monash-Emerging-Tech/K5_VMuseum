using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GCodeExecutor : MonoBehaviour
{
    [Header("References")]
    public Transform nozzle; // Assign your nozzle object in Inspector

    [Header("Settings")]
    public float maxX = 1f; // Max travel distance in Unity units
    public float maxY = 1f;
    public float maxZ = 1f;

    public float speedX = 1f; // Units per second in X
    public float speedY = 1f; // Units per second in Y
    public float speedZ = 1f; // Units per second in Z

    public float lineDelay = 0.5f; // Delay between each G-code line (seconds)

    private Vector3 startPos;

    private string[] gcode = new string[]
    {
        "G1 X0 Y0 Z0 F1500",
        "G1 X100 Y0 Z0 F1500",
        "G1 X100 Y100 Z0 F1500",
        "G1 X0 Y100 Z0 F1500",
        "G1 X0 Y0 Z0 F1500"
    };

    void Start()
    {
        startPos = nozzle.position;
        StartCoroutine(ExecuteGCode());
    }

    IEnumerator ExecuteGCode()
    {
        foreach (string line in gcode)
        {
            Vector3 target = ParseAndScale(line);
            yield return MoveTo(target);
            yield return new WaitForSeconds(lineDelay);
        }
    }

    Vector3 ParseAndScale(string line)
    {
        // Default values
        float x = 0, y = 0, z = 0;

        // Split by spaces and check for X, Y, Z commands
        string[] parts = line.Split(' ');
        foreach (string p in parts)
        {
            if (p.StartsWith("X"))
                x = float.Parse(p.Substring(1)) / 100f * maxX;
            else if (p.StartsWith("Y"))
                y = float.Parse(p.Substring(1)) / 100f * maxY;
            else if (p.StartsWith("Z"))
                z = float.Parse(p.Substring(1)) / 100f * maxZ;
        }

        return startPos + new Vector3(x, y, z);
    }

    IEnumerator MoveTo(Vector3 target)
    {
        Vector3 start = nozzle.position;
        Vector3 delta = target - start;

        // Time required in each axis
        float tx = Mathf.Abs(delta.x) / speedX;
        float ty = Mathf.Abs(delta.y) / speedY;
        float tz = Mathf.Abs(delta.z) / speedZ;

        float totalTime = Mathf.Max(tx, ty, tz); // Ensure correct sync speed

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / totalTime;
            nozzle.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        nozzle.position = target; // Snap to target
    }
}
