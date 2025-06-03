using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections;

public class DogAgent : Agent
{
    public float moveSpeed = 0.3f;
    public float turnSpeed = 80f;
    public float detectionRadius = 10f;
    private Rigidbody rb;
    private Transform targetChicken;
    private Vector3 lastPosition;
    private SphereCollider triggerCollider;

    private bool isEating = false;
    private float timeSinceLastFood = 0f;
    private Terrain terrain;
    private bool isSearching = false;

    private float targetRefreshTimer = 0f;
    private float targetRefreshInterval = 3f;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        terrain = Terrain.activeTerrain;
        lastPosition = transform.position;

        triggerCollider = gameObject.AddComponent<SphereCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = detectionRadius;

        FindNewTarget();
    }

    public override void OnEpisodeBegin()
    {
        transform.position = GetValidSpawnPosition();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        lastPosition = transform.position;
        timeSinceLastFood = 0f;
        isEating = false;
        isSearching = false;
        FindNewTarget();
    }

    private Vector3 GetValidSpawnPosition()
    {
        for (int i = 0; i < 15; i++)
        {
            float x = Random.Range(terrain.transform.position.x + 5f, terrain.transform.position.x + terrain.terrainData.size.x - 5f);
            float z = Random.Range(terrain.transform.position.z + 5f, terrain.transform.position.z + terrain.terrainData.size.z - 5f);
            float y = terrain.SampleHeight(new Vector3(x, 0, z));

            Vector3 pos = new Vector3(x, y + 1.5f, z);
            if (!IsSteepSlope(pos))
            {
                return pos;
            }
        }

        // 실패 시 기본 위치
        return new Vector3(30f, terrain.SampleHeight(new Vector3(30f, 0f, 30f)) + 1.5f, 30f);
    }

    private bool IsSteepSlope(Vector3 position)
    {
        if (Physics.Raycast(position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 2.0f))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            return slopeAngle > 15f;
        }
        return false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.forward.x);
        sensor.AddObservation(transform.forward.z);

        sensor.AddObservation(rb.velocity.x);
        sensor.AddObservation(rb.velocity.z);

        sensor.AddObservation(timeSinceLastFood / 30f);

        if (targetChicken != null)
        {
            Vector3 dir = (targetChicken.position - transform.position).normalized;
            float dist = Vector3.Distance(transform.position, targetChicken.position) / detectionRadius;

            sensor.AddObservation(dir.x);
            sensor.AddObservation(dir.z);
            sensor.AddObservation(dist);
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(1f);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (isEating) return;

        float move = Mathf.Clamp(actions.ContinuousActions[0], -0.3f, 0.3f);
        float turn = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f); // 더 넓은 회전 허용

        // 회전: 부드럽게 목표 방향으로
        if (targetChicken != null)
        {
            Vector3 toTarget = (targetChicken.position - transform.position).normalized;
            Quaternion targetRot = Quaternion.LookRotation(toTarget, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 3f); // 부드럽게 회전
        }
        else
        {
            transform.Rotate(Vector3.up, turn * turnSpeed * Time.deltaTime); // 일반 회전
        }

        // 이동
        Vector3 moveDir = transform.forward * move * moveSpeed;
        rb.MovePosition(rb.position + moveDir * Time.fixedDeltaTime);

        // 보상: 이동 거리
        float moveDist = Vector3.Distance(transform.position, lastPosition);
        AddReward(moveDist * 0.02f); // 기존보다 강화
        lastPosition = transform.position;

        // 타겟이 가까워지면 보상 증가
        if (targetChicken != null)
        {
            float dist = Vector3.Distance(transform.position, targetChicken.position);
            float reward = Mathf.Clamp(1.5f - (dist / detectionRadius), 0f, 1.5f);
            AddReward(reward * 0.1f); // 보상 강화

            if (dist < 1.2f) // 닿았으면 먹기
            {
                StartCoroutine(EatChickenRoutine());
            }
        }

        // 가만히 있으면 페널티
        if (Mathf.Approximately(move, 0f) && Mathf.Approximately(turn, 0f))
        {
            AddReward(-0.003f * Time.deltaTime);
        }

        // 사망 조건
        timeSinceLastFood += Time.deltaTime;
        if (timeSinceLastFood > 160f) // 40f
        {
            AddReward(-3.0f);
            Debug.Log("개가 굶주려 사망");
            // DogSpawner.Instance?.RespawnAfterDelay(3f);
            EndEpisode();
            Destroy(gameObject);
        }

        if (transform.position.y < 0f || transform.position.y > 25f)
        {
            AddReward(-3.0f);
            EndEpisode();
        }
    }
    
    private void SnapToTerrain()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 1f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 5f))
        {
            if (hit.collider.CompareTag("Terrain"))
            {
                Vector3 pos = transform.position;
                pos.y = hit.point.y + 0.3f; // 살짝 위로 띄워줌
                transform.position = pos;
            }
        }
    }

    private void FixedUpdate()
    {
        targetRefreshTimer += Time.fixedDeltaTime;
        if (targetRefreshTimer >= targetRefreshInterval)
        {
            FindNewTarget();
            targetRefreshTimer = 0f;
        }
    }

    private IEnumerator EatChickenRoutine()
    {
        if (isEating || targetChicken == null) yield break;

        isEating = true;
        moveSpeed = 0f;
        Debug.Log("닭을 먹는 중...");
        yield return new WaitForSeconds(3f);

        if (targetChicken != null)
        {
            Destroy(targetChicken.gameObject);
            AddReward(10.0f); // 닭 보상 증가
            timeSinceLastFood = 0f;
            targetChicken = null;
            FindNewTarget();
        }

        moveSpeed = 0.3f;
        isEating = false;
    }

    private void FindNewTarget()
    {
        Collider[] chickens = Physics.OverlapSphere(transform.position, detectionRadius, LayerMask.GetMask("Chicken"));

        Transform closest = null;
        float minDist = float.MaxValue;

        foreach (var c in chickens)
        {
            Vector3 toChicken = (c.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, toChicken);

            if (angle < 100f) // 100도 시야
            {
                float dist = Vector3.Distance(transform.position, c.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = c.transform;
                }
            }
        }

        targetChicken = closest;
    }

    private IEnumerator SearchForChicken()
    {
        isSearching = true;

        for (int i = 0; i < 6; i++)
        {
            transform.Rotate(Vector3.up, Random.Range(-90f, 90f));
            yield return new WaitForSeconds(0.7f);

            Collider[] chickens = Physics.OverlapSphere(transform.position, detectionRadius, LayerMask.GetMask("Chicken"));
            if (chickens.Length > 0)
            {
                FindNewTarget();
                isSearching = false;
                yield break;
            }
        }

        Debug.Log("닭 탐색 실패. 패널티!");
        AddReward(-1.0f);
        isSearching = false;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var ca = actionsOut.ContinuousActions;
        ca[0] = Input.GetAxis("Vertical");
        ca[1] = Input.GetAxis("Horizontal");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Chicken"))
        {
            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (targetChicken == null || dist < Vector3.Distance(transform.position, targetChicken.position))
            {
                targetChicken = other.transform;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Chicken") && other.transform == targetChicken)
        {
            targetChicken = null;
            FindNewTarget();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
    
    private void LateUpdate()
    {
        if (transform.position.y > terrain.SampleHeight(transform.position) + 0.5f)
        {
            Vector3 pos = transform.position;
            pos.y = terrain.SampleHeight(transform.position) + 0.3f;
            transform.position = pos;

            // Y축 속도 제거
            Vector3 vel = rb.velocity;
            vel.y = 0;
            rb.velocity = vel;
        }
    }

}
