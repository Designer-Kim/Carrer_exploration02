using System.Collections;
using UnityEngine;

public class ChickenSpawner : MonoBehaviour
{
    public GameObject chickenPrefab;
    public int chickenCount = 5;
    public Vector3 centerPosition = new Vector3(36.5f, 6f, 40f); // 중심 좌표
    public float spawnRange = 15f; // 스폰 범위 조절
    public LayerMask terrainLayer; // Terrain 레이어 추가
    public static ChickenSpawner Instance; // 싱글톤 선언
    
    void Awake()
    {
        Instance = this;
    }

    public void RespawnAfterDelay(float delay)
    {
        // StartCoroutine(RespawnRoutine(delay));
    }

    private IEnumerator RespawnRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 현재 개체 수 확인 후 최대치보다 작을 때만 리스폰
        int currentCount = GameObject.FindGameObjectsWithTag("Chicken").Length;
        if (currentCount < chickenCount)
        {
            SpawnChickens(1);
        }
    }
    
    void Start()
    {
        // InvokeRepeating(nameof(CheckAndSpawnChickens), 1f, 5f); // 5초마다 닭 체크
        SpawnChickens(chickenCount);
    }
    
    void CheckAndSpawnChickens()
    {
        int currentChickenCount = GameObject.FindGameObjectsWithTag("Chicken").Length;
        int toSpawn = chickenCount - currentChickenCount;

        for (int i = 0; i < toSpawn; i++)
        {
            Vector3 randomPos = new Vector3(
                centerPosition.x + Random.Range(-spawnRange, spawnRange),
                100f,
                centerPosition.z + Random.Range(-spawnRange, spawnRange)
            );

            if (Physics.Raycast(randomPos, Vector3.down, out RaycastHit hit, Mathf.Infinity, terrainLayer))
            {
                Vector3 finalPosition = hit.point;
                Instantiate(chickenPrefab, finalPosition, Quaternion.identity);
            }
        }
    }

    public void SpawnChickens(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 randomPos = new Vector3(
                centerPosition.x + Random.Range(-spawnRange, spawnRange),
                100f,
                centerPosition.z + Random.Range(-spawnRange, spawnRange)
            );

            if (Physics.Raycast(randomPos, Vector3.down, out RaycastHit hit, Mathf.Infinity, terrainLayer))
            {
                Vector3 finalPosition = hit.point;
                Instantiate(chickenPrefab, finalPosition, Quaternion.identity);
            }
        }
    }
}