using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class AIRobotAgent : Agent
{
    public Transform[] shops; // 방문해야 할 상점들
    public Transform[] obstacles; // 장애물 리스트
    private int currentTargetIndex = 0; // 현재 목표 Shop 인덱스
    private Rigidbody rb;

    public float moveSpeed = 3f; // 이동 속도
    public float stoppingDistance = 1.5f; // 목표 도착 거리
    private float totalReward = 0f; // ⭐ 총 보상 저장 변수

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        // AI 로봇 위치 초기화
        transform.position = new Vector3(-25, 1, -30);
        transform.rotation = Quaternion.identity;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        currentTargetIndex = 0; // 첫 번째 상점부터 시작
        totalReward = 0f; // ⭐ 총 보상 리셋
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (currentTargetIndex >= shops.Length)
            return;

        // 현재 AI 로봇 위치
        sensor.AddObservation(transform.position.x);
        sensor.AddObservation(transform.position.z);

        // 현재 목표 상점 위치
        sensor.AddObservation(shops[currentTargetIndex].position.x);
        sensor.AddObservation(shops[currentTargetIndex].position.z);

        // 장애물 감지 (가까운 장애물 3개까지 거리 정보 추가)
        for (int i = 0; i < 3; i++)
        {
            if (i < obstacles.Length)
            {
                float distanceToObstacle = Vector3.Distance(transform.position, obstacles[i].position);
                sensor.AddObservation(distanceToObstacle);
            }
            else
            {
                sensor.AddObservation(0);
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0]; // 좌우 이동
        float moveZ = actions.ContinuousActions[1]; // 전후 이동

        Debug.Log($"🎯 MoveX: {moveX}, MoveZ: {moveZ}");

        // 이동 처리
        Vector3 move = new Vector3(moveX, 0, moveZ) * moveSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + move);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        Debug.Log("🕹 Heuristic 실행됨!");
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }

    private void FixedUpdate()
    {
        if (rb == null || currentTargetIndex >= shops.Length)
            return;

        // 현재 목표 지점 설정
        Vector3 targetPosition = shops[currentTargetIndex].position;

        // 장애물 감지 (Raycast로 앞으로 1.5m 거리 체크)
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 1.5f))
        {
            if (hit.collider.CompareTag("Obstacle")) // 장애물 감지 시 회피 행동 수행
            {
                Debug.Log("⚠ 장애물 감지됨! 우회 경로 설정");

                // 회피 방향 설정 (랜덤하게 왼쪽 또는 오른쪽으로 회피)
                float evadeDirection = Random.Range(0, 2) == 0 ? -1f : 1f;
                Vector3 evadeMove = new Vector3(evadeDirection, 0, 0) * moveSpeed * 0.5f;

                rb.velocity = evadeMove; // 회피 행동 적용
                AddReward(-0.2f); // 장애물 회피 보상(패널티)
                return;
            }
        }

        // 목표 방향 계산 및 이동
        Vector3 moveDirection = (targetPosition - transform.position).normalized;
        if (moveDirection.magnitude > 0.01f)
        {
            rb.velocity = moveDirection * moveSpeed;
            Debug.Log($"🚀 이동 중! 목표: {currentTargetIndex}번 상점 ({targetPosition})");
        }
        else
        {
            rb.velocity = Vector3.zero;
            Debug.Log("⚠ 이동 방향이 0이어서 멈춤!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ShopTarget"))
        {
            Debug.Log($"✅ 목표 {currentTargetIndex + 1}번 상점 도착! 현재 위치: {transform.position}");

            float reward = 1.0f; // 목표 도착 보상
            AddReward(reward);
            totalReward += reward; // ⭐ 총 보상 업데이트

            Debug.Log($"🏆 보상 획득! 현재 총 보상: {totalReward}");

            currentTargetIndex++; // 다음 상점으로 변경
            Debug.Log($"🔄 다음 목표: {currentTargetIndex}번 상점으로 이동");

            if (currentTargetIndex >= shops.Length)
            {
                Debug.Log($"🎯 모든 상점을 방문 완료! 최종 보상: {totalReward}");
                EndEpisode();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            float penalty = -0.5f; // 장애물 충돌 패널티
            AddReward(penalty);
            totalReward += penalty; // ⭐ 총 보상 업데이트

            Debug.Log($"❌ 장애물과 충돌! 패널티 적용. 현재 총 보상: {totalReward}");

            // 충돌한 방향을 피해서 이동하도록 설정
            Vector3 newDirection = transform.right * (Random.Range(0, 2) == 0 ? -1f : 1f);
            rb.velocity = newDirection * moveSpeed * 0.5f; // 장애물 회피 이동

            // 🔥 `EndEpisode()`를 호출하지 않음 → 에피소드가 끝나지 않고 계속 이동하도록 설정!
        }
    }
}
