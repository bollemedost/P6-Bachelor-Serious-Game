using UnityEngine;

public class TerrainDataReassigner : MonoBehaviour
{
    public Terrain targetTerrain;
    public TerrainData newTerrainData;

    [ContextMenu("Apply New Terrain Data")]
    public void ApplyNewTerrainData()
    {
        if (targetTerrain == null)
        {
            Debug.LogError("No target Terrain assigned.");
            return;
        }

        if (newTerrainData == null)
        {
            Debug.LogError("No new TerrainData assigned.");
            return;
        }

        TerrainCollider terrainCollider = targetTerrain.GetComponent<TerrainCollider>();

        targetTerrain.terrainData = newTerrainData;

        if (terrainCollider != null)
        {
            terrainCollider.terrainData = newTerrainData;
        }

        Debug.Log("Terrain and TerrainCollider now both use: " + newTerrainData.name);
    }
}