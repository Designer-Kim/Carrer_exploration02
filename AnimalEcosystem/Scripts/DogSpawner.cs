using UnityEngine;
using System.Collections;

public class DogSpawner : MonoBehaviour
{
    public static DogSpawner Instance;

    public GameObject dogPrefab;
    public int dogCount = 3; // 최대 개체 수
    public Vector3 centerPosition = new Vector3(36.5f, 6f, 40f);
    public float spawnRange = 15f;
    public LayerMask terrainLayer;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SpawnDogs(dogCount);
    }

    public void SpawnDogs(int count)
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
                Instantiate(dogPrefab, hit.point, Quaternion.identity);
            }
        }
    }

    public void RespawnAfterDelay(float delay)
    {
        // StartCoroutine(RespawnRoutine(delay));
    }

    private IEnumerator RespawnRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        int currentCount = GameObject.FindGameObjectsWithTag("Dog").Length;

        if (currentCount < dogCount)
        {
            SpawnDogs(1); // 한 마리만 생성 (최대 수 이하일 때만)
        }
    }
}