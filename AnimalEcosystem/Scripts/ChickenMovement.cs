using UnityEngine;

public class ChickenMovement : MonoBehaviour
{
    public float moveSpeed = 1f;  // 이동 속도
    public float detectionRange = 15f;  // 풀 탐지 범위
    public float stopDistance = 1.5f; // 풀에 도착하면 멈추는 거리

    private GameObject targetGrass; // 현재 목표 풀

    void Start()
    {
        FindNewGrass(); // 처음 시작할 때 풀 찾기
    }

    void Update()
    {
        if (targetGrass == null)
        {
            FindNewGrass(); // 목표가 없으면 새 풀 찾기
        }
        else
        {
            MoveToTarget(); // 목표를 향해 이동
        }
    }

    void FindNewGrass()
    {
        GameObject[] grasses = GameObject.FindGameObjectsWithTag("Grass");
        if (grasses.Length > 0)
        {
            targetGrass = grasses[Random.Range(0, grasses.Length)];
        }
    }

    void MoveToTarget()
    {
        if (targetGrass == null) return;

        Vector3 direction = (targetGrass.transform.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime; // 직접 이동

        if (Vector3.Distance(transform.position, targetGrass.transform.position) < stopDistance)
        {
            EatGrass();
        }
    }

    void EatGrass()
    {
        Destroy(targetGrass); // 풀 제거
        FindNewGrass(); // 새로운 풀 찾기
    }
}