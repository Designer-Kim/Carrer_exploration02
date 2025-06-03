using UnityEngine;

public class GrassSpawner : MonoBehaviour
{
    public GameObject grassPrefab;
    public int grassCount = 60;
    public float edgeMargin = 10f;  // Terrain 끝에서 얼마나 안쪽으로 스폰할지
    public LayerMask terrainLayer;
    public float maxSlopeAngle = 15f;

    private Terrain terrain;

    void Start()
    {
        terrain = Terrain.activeTerrain;
        // 여기서 직접 LayerMask 지정 (Environment라는 이름의 레이어 사용)
        terrainLayer = LayerMask.GetMask("Environment");
        SpawnGrass();
    }

    // void Update()
    // {
    //     MaintainGrassCount();
    // }

    // void MaintainGrassCount()
    // {
    //     int currentCount = GameObject.FindGameObjectsWithTag("Grass").Length;
    //
    //     if (currentCount < grassCount)
    //     {
    //         int toSpawn = grassCount - currentCount;
    //
    //         for (int i = 0; i < toSpawn; i++)
    //         {
    //             Vector3 spawnPos = GetValidPosition();
    //             Instantiate(grassPrefab, spawnPos, Quaternion.identity);
    //         }
    //     }
    // }
    
    void SpawnGrass()
    {
        for (int i = 0; i < grassCount; i++)
        {
            Vector3 spawnPos = GetValidPosition();
            Instantiate(grassPrefab, spawnPos, Quaternion.identity);
        }
    }

    // public void RespawnGrass(Vector3 oldPosition)
    // {
    //     Vector3 newPos = GetValidPosition();
    //     if (newPos != Vector3.negativeInfinity)
    //     {
    //         Instantiate(grassPrefab, newPos, Quaternion.identity);
    //     }
    // }

    private Vector3 GetValidPosition()
    {
        int maxAttempts = 30;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomPos = GetEdgePosition();

            if (Physics.Raycast(randomPos, Vector3.down, out RaycastHit hit, Mathf.Infinity, terrainLayer))
            {
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

                if (slopeAngle <= maxSlopeAngle)
                {
                    return new Vector3(hit.point.x, hit.point.y + 0.1f, hit.point.z);
                }
            }
        }

        // 실패 시 중앙 근처 기본 위치 리턴
        return new Vector3(36f, 6f, 40f);
    }

    private Vector3 GetEdgePosition()
    {
        float terrainWidth = terrain.terrainData.size.x;
        float terrainLength = terrain.terrainData.size.z;

        float x = Random.Range(edgeMargin, terrainWidth - edgeMargin);
        float z = Random.Range(edgeMargin, terrainLength - edgeMargin);
        float y = 100f; // 높이에서 Raycast용

        return new Vector3(x, y, z) + terrain.transform.position;
    }
}