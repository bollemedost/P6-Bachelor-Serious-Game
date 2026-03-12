using UnityEngine;

public class WomanSpawner : MonoBehaviour
{
    public GameObject[] womanPrefabs;
    public Transform[] spawnPoints;
    public Transform[] waypoints;
    public float spawnInterval = 3f;

    [Header("Pre-fill the path at scene start")]
    public bool prefillPathOnStart = true;
    public float prefillDurationSeconds = 40f;

    void Start()
    {
        if (prefillPathOnStart)
        {
            SpawnPrefilledWomen();
        }

        InvokeRepeating(nameof(SpawnWoman), 0f, spawnInterval);
    }

    void SpawnPrefilledWomen()
    {
        if (womanPrefabs.Length == 0 || spawnPoints.Length == 0 || waypoints.Length == 0) return;

        // Spawn women spaced exactly like the normal spawn interval,
        // so the road already looks populated when the player enters.
        for (float t = spawnInterval; t <= prefillDurationSeconds; t += spawnInterval)
        {
            SpawnWomanWithHeadStart(t);
        }
    }

    void SpawnWoman()
    {
        SpawnWomanWithHeadStart(0f);
    }

    void SpawnWomanWithHeadStart(float headStartSeconds)
    {
        if (womanPrefabs.Length == 0 || spawnPoints.Length == 0 || waypoints.Length == 0) return;

        GameObject randomPrefab = womanPrefabs[Random.Range(0, womanPrefabs.Length)];
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject woman = Instantiate(
            randomPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        WaypointWalker walker = woman.GetComponent<WaypointWalker>();
        if (walker != null)
        {
            walker.waypoints = waypoints;

            if (headStartSeconds > 0f)
            {
                walker.ApplyHeadStart(headStartSeconds);
            }
        }
        else
        {
            Debug.LogWarning("Spawned woman does not have a WaypointWalker script.");
        }
    }
}