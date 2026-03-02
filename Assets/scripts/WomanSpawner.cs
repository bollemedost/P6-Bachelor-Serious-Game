using UnityEngine;

public class WomanSpawner : MonoBehaviour
{
    public GameObject[] womanPrefabs;   // multiple prefabs
    public Transform[] spawnPoints;     // multiple spawn points
    public Transform[] waypoints;       // path for women to follow
    public float spawnInterval = 3f;    // time between spawns

    void Start()
    {
        InvokeRepeating(nameof(SpawnWoman), 0f, spawnInterval);
    }

    void SpawnWoman()
    {
        if (womanPrefabs.Length == 0 || spawnPoints.Length == 0) return;

        // Pick random prefab
        GameObject randomPrefab = womanPrefabs[Random.Range(0, womanPrefabs.Length)];

        // Pick random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Instantiate woman
        GameObject woman = Instantiate(
            randomPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        // Assign waypoints
        WaypointWalker walker = woman.GetComponent<WaypointWalker>();
        walker.waypoints = waypoints;
    }
}