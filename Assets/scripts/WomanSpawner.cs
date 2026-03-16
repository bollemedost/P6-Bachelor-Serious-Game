using UnityEngine;

public class WomanSpawner : MonoBehaviour
{
    [Header("Normal women")]
    public GameObject[] womanPrefabs;

    [Header("Women less frequently spawned")]
    public GameObject[] lessFrequentWomanPrefabs;
    public float lessFrequentSpawnInterval = 10f;

    [Header("Spawn setup")]
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

        // Normal women spawn
        if (womanPrefabs != null && womanPrefabs.Length > 0)
        {
            InvokeRepeating(nameof(SpawnWoman), 0f, spawnInterval);
        }

        // Less frequent women spawn
        if (lessFrequentWomanPrefabs != null && lessFrequentWomanPrefabs.Length > 0)
        {
            InvokeRepeating(nameof(SpawnLessFrequentWoman), lessFrequentSpawnInterval, lessFrequentSpawnInterval);
        }
    }

    void SpawnPrefilledWomen()
    {
        if ((womanPrefabs == null || womanPrefabs.Length == 0) &&
            (lessFrequentWomanPrefabs == null || lessFrequentWomanPrefabs.Length == 0))
            return;

        if (spawnPoints.Length == 0 || waypoints.Length == 0) return;

        // Prefill only with the normal women,
        // so the rare women stay rare.
        if (womanPrefabs != null && womanPrefabs.Length > 0)
        {
            for (float t = spawnInterval; t <= prefillDurationSeconds; t += spawnInterval)
            {
                SpawnWomanWithHeadStartFromArray(womanPrefabs, t);
            }
        }
    }

    void SpawnWoman()
    {
        SpawnWomanWithHeadStartFromArray(womanPrefabs, 0f);
    }

    void SpawnLessFrequentWoman()
    {
        SpawnWomanWithHeadStartFromArray(lessFrequentWomanPrefabs, 0f);
    }

    void SpawnWomanWithHeadStartFromArray(GameObject[] prefabArray, float headStartSeconds)
    {
        if (prefabArray == null || prefabArray.Length == 0 || spawnPoints.Length == 0 || waypoints.Length == 0)
            return;

        GameObject randomPrefab = prefabArray[Random.Range(0, prefabArray.Length)];
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