using UnityEngine;

public class WomanSpawner : MonoBehaviour
{
    public GameObject[] womanPrefabs;   // Multiple prefabs
    public Transform spawnPoint;
    public Transform[] waypoints;
    public float spawnInterval = 3f;

    void Start()
    {
        InvokeRepeating(nameof(SpawnWoman), 0f, spawnInterval);
    }

    void SpawnWoman()
    {
        if (womanPrefabs.Length == 0) return;

        GameObject randomPrefab = womanPrefabs[Random.Range(0, womanPrefabs.Length)];

        GameObject woman = Instantiate(
            randomPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        WaypointWalker walker = woman.GetComponent<WaypointWalker>();
        walker.waypoints = waypoints;
    }
}