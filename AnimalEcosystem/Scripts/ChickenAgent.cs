using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections;

public class ChickenAgent : Agent
{
    public float moveSpeed = 0.5f;  // 속도 줄이기
    public float turnSpeed = 120f;  // 회전 속도 조정
    private Rigidbody rb;
    private Transform targetGrass;
    private float timeSinceLastFood = 0f;
    private Terrain terrain;
    private bool isEating = false;
    private Vector3 lastPosition;
    private Vector3 spawnPosition;
    private float maxDistanceFromSpawn = 20f;
    private float maxSlopeAngle = 15f;
    private bool isSearching = false;

    private SphereCollider triggerCollider;  // 트리거 영역 추가
    public float detectionRadius = 8.0f; // 탐색 반경 설정
    private GrassSpawner grassSpawner;   // 풀 스폰 기능 추가

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.mass = 2f;
        rb.drag = 2f;
        rb.angularDrag = 2f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        terrain = Terrain.activeTerrain;
        spawnPosition = transform.position;
        FindNewTarget();

        // 트리거 콜라이더 추가 (닭 주변 감지용)
        triggerCollider = gameObject.AddComponent<SphereCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = detectionRadius;

        // GrassSpawner 찾기
        grassSpawner = FindObjectOfType<GrassSpawner>();
    }

    public override void OnEpisodeBegin()
    {
        transform.position = GetValidSpawnPosition();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        timeSinceLastFood = 0f;
        isEating = false;
        isSearching = false;
        lastPosition = transform.position;
        FindNewTarget();
    }

    private Vector3 GetValidSpawnPosition()
    {
        if (terrain == null)
        {
            return new Vector3(0f, 5f, 0f);
        }

        for (int i = 0; i < 15; i++)
        {
            float randomX = Random.Range(terrain.transform.position.x + 5f, terrain.transform.position.x + terrain.terrainData.size.x - 5f);
            float randomZ = Random.Range(terrain.transform.position.z + 5f, terrain.transform.position.z + terrain.terrainData.size.z - 5f);
            float terrainHeight = terrain.SampleHeight(new Vector3(randomX, 0, randomZ));

            Vector3 spawnPos = new Vector3(randomX, terrainHeight + 0.5f, randomZ);

            if (!IsSteepSlope(spawnPos))
            {
                return spawnPos;
            }
        }

        return new Vector3(30f, terrain.SampleHeight(new Vector3(30f, 0, 40f)) + 0.5f, 40f);
    }

    private bool IsSteepSlope(Vector3 position)
    {
        if (Physics.Raycast(position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 2.0f))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            return slopeAngle > maxSlopeAngle;
        }
        return false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 닭이 바라보는 방향
        sensor.AddObservation(transform.forward.x);
        sensor.AddObservation(transform.forward.z);

        // 현재 속도
        sensor.AddObservation(rb.velocity.x);
        sensor.AddObservation(rb.velocity.z);

        // 배고픔 정도 (0~1 정규화)
        sensor.AddObservation(timeSinceLastFood / 20f);

        // 타겟이 있을 경우 방향 + 거리
        if (targetGrass != null)
        {
            Vector3 dir = (targetGrass.position - transform.position).normalized;
            float dist = Vector3.Distance(transform.position, targetGrass.position) / detectionRadius;
            sensor.AddObservation(dir.x);
            sensor.AddObservation(dir.z);
            sensor.AddObservation(dist);
        }
        else
        {
            sensor.AddObservation(0f); // dir.x
            sensor.AddObservation(0f); // dir.z
            sensor.AddObservation(1f); // 가장 멀리 있음
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (isEating) return;

        // 이동 속도와 회전 속도 조절 (속도가 너무 크지 않도록)
        float move = Mathf.Clamp(actions.ContinuousActions[0], -0.3f, 0.3f);
        float turn = Mathf.Clamp(actions.ContinuousActions[1], -0.5f, 0.5f);

        transform.Rotate(Vector3.up, turn * turnSpeed * Time.deltaTime);
        rb.velocity = transform.forward * move * moveSpeed;

        // 목표(풀)로 이동 중일 때 보상 추가
        if (targetGrass != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetGrass.position);
            float reward = Mathf.Clamp(0.5f - (distanceToTarget / 10f), 0.05f, 1.0f);  // 거리가 가까울수록 보상 증가
            AddReward(reward * 0.05f);
        }

        float distanceMoved = Vector3.Distance(lastPosition, transform.position);
        AddReward(distanceMoved * 0.01f);
        lastPosition = transform.position;
    
        if (Vector3.Distance(transform.position, spawnPosition) > maxDistanceFromSpawn)
        {
            Vector3 toCenter = (spawnPosition - transform.position).normalized;
            float angleToCenter = Vector3.SignedAngle(transform.forward, toCenter, Vector3.up);

            // 목표 방향을 바라보도록 조금씩 회전
            transform.Rotate(Vector3.up, Mathf.Clamp(angleToCenter, -20f, 20f) * Time.deltaTime);

            AddReward(-0.01f); // 부드러운 경고
        }

        timeSinceLastFood += Time.deltaTime;
        if (Mathf.Approximately(move, 0f) && Mathf.Approximately(turn, 0f))
        {
            AddReward(-0.002f * Time.deltaTime);  // 가만히 있을 때만 벌점
        }

        if (timeSinceLastFood > 120f) // 30f
        {
            Debug.Log("닭이 굶주려서 사망...");
            AddReward(-3.0f);
            // ChickenSpawner에 리스폰 요청
            // if (ChickenSpawner.Instance != null)
            // {
            //     ChickenSpawner.Instance.RespawnAfterDelay(3f); // 3초 후 리스폰
            // }
            EndEpisode();
            Destroy(gameObject);
        }

        if (targetGrass != null && Vector3.Distance(transform.position, targetGrass.position) < 1.5f)
        {
            StartCoroutine(EatGrassRoutine());
        }
        
        if (transform.position.y > spawnPosition.y + 0.5f)
        {
            Vector3 pos = transform.position;
            pos.y = spawnPosition.y + 0.5f;
            transform.position = pos;

            Vector3 vel = rb.velocity;
            vel.y = 0f;
            rb.velocity = vel;
        }
    }
    
    private float targetRefreshTimer = 0f;
    private float targetRefreshInterval = 3f;

    private void FixedUpdate()
    {
        targetRefreshTimer += Time.fixedDeltaTime;
        if (targetRefreshTimer >= targetRefreshInterval)
        {
            FindNewTarget();
            targetRefreshTimer = 0f;
        }Vector3 euler = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, euler.y, 0f);  // Y축 회전만 유지
    }

    private IEnumerator EatGrassRoutine()
    {
        if (isEating) yield break;

        isEating = true;
        moveSpeed = 0f;
        Debug.Log("풀을 먹는 중...");
        yield return new WaitForSeconds(2f);

        if (targetGrass != null)
        {
            Vector3 oldPosition = targetGrass.position;
            Destroy(targetGrass.gameObject);
            AddReward(30.0f);

            timeSinceLastFood = 0f;

            // 풀을 다시 스폰하여 에피소드가 끊기지 않도록 함
            // if (grassSpawner != null)
            // {
            //     grassSpawner.RespawnGrass(oldPosition);
            // }

            FindNewTarget();
        }

        moveSpeed = 0.5f;
        isEating = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Grass"))
        {
            float distance = Vector3.Distance(transform.position, other.transform.position);
            if (targetGrass == null || distance < Vector3.Distance(transform.position, targetGrass.position))
            {
                targetGrass = other.transform;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Grass") && other.transform == targetGrass)
        {
            targetGrass = null;
            FindNewTarget();
        }
    }

    private void FindNewTarget()
    {
        Collider[] grasses = Physics.OverlapSphere(transform.position, detectionRadius, LayerMask.GetMask("Grass"));
    
        if (grasses.Length > 0)
        {
            Transform closestGrass = null;
            float minDistance = float.MaxValue;

            foreach (Collider grassCollider in grasses)
            {
                Transform grass = grassCollider.transform;
                float distance = Vector3.Distance(transform.position, grass.position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestGrass = grass;
                }
            }

            if (closestGrass != null)
            {
                targetGrass = closestGrass;
            }
        }
        else
        {
            // 일정 확률(10%)로 랜덤 방향 변경
            if (Random.value < 0.1f)  
            {
                transform.Rotate(Vector3.up, Random.Range(-60f, 60f)); // 랜덤 방향 변경
            }
            StartCoroutine(SearchForGrass());
        }
    }

    private IEnumerator SearchForGrass()
    {
        isSearching = true;

        for (int i = 0; i < 8; i++)
        {
            // 랜덤 방향으로 회전만 시켜서 '탐색 행동'처럼 보이게 함
            transform.Rotate(Vector3.up, Random.Range(-90f, 90f));
        
            yield return new WaitForSeconds(0.8f); // 기다렸다가

            // 현재 주변에 풀 있는지 확인
            GameObject[] grasses = GameObject.FindGameObjectsWithTag("Grass");
            if (grasses.Length > 0)
            {
                FindNewTarget();
                yield break;
            }
        }

        AddReward(-2.0f); // 탐색 실패시 패널티
        isSearching = false;
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
    
        // 키보드 입력을 기반으로 움직임 제어
        continuousActions[0] = Input.GetAxis("Vertical");  // 앞뒤 이동
        continuousActions[1] = Input.GetAxis("Horizontal"); // 좌우 회전
    }
}