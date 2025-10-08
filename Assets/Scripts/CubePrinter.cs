using UnityEngine;

public class NozzleSpawner : MonoBehaviour
{
    public GameObject nozzle;         // assign nozzle object
    public GameObject cubePrefab;     // assign cube prefab
    public Vector3 offset = new Vector3(0, -0.05f, 0); // relative to nozzle
    public float spawnInterval = 0.5f;
    public float scaleFactor = 1f;
    public Transform moveWithTarget;  // assign the component to move with

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnCube();
            timer = 0f;
        }
    }

    void SpawnCube()
    {
        Vector3 spawnPos = nozzle.transform.position
                         + nozzle.transform.right * offset.x
                         + nozzle.transform.up * offset.y
                         + nozzle.transform.forward * offset.z;

        GameObject newCube = Instantiate(cubePrefab, spawnPos, Quaternion.identity);

        // Apply scaling
        newCube.transform.localScale = cubePrefab.transform.localScale * scaleFactor;

        // If a move-with target is assigned, parent the cube to it
        if (moveWithTarget != null)
        {
            newCube.transform.SetParent(moveWithTarget, true);
        }
    }
}
