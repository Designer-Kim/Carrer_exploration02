using UnityEngine;

public class TigerSpawner : MonoBehaviour
{
    public GameObject tigerPrefab;
    public int tigerCount = 2;
    public Vector3 centerPosition = new Vector3(36.5f, 6f, 40f);
    public float spawnRange = 15f;
    public LayerMask terrainLayer;

    void Start()
    {
        SpawnTigers();
    }

    void SpawnTigers()
    {
        for (int i = 0; i < tigerCount; i++)
        {
            Vector3 randomPos = new Vector3(
                centerPosition.x + Random.Range(-spawnRange, spawnRange),
                100f,
                centerPosition.z + Random.Range(-spawnRange, spawnRange)
            );

            if (Physics.Raycast(randomPos, Vector3.down, out RaycastHit hit, Mathf.Infinity, terrainLayer))
            {
                Vector3 finalPosition = hit.point;
                Instantiate(tigerPrefab, finalPosition, Quaternion.identity);
            }
        }
    }
}