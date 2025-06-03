using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

/// <summary>
/// 고대 수호자 에이전트 (Ancient Guardian Agent)
/// 목표 지점까지 이동하는 강화학습 AI
/// </summary>
public class AncientGuardianAgent : Agent
{
    private Rigidbody rb;
    private Vector3 startPos;
    private Vector3 moveDir;

    private GameObject goal;  // ✅ 개별 목표 객체 (자동으로 할당)
    private Transform platform;  // ✅ 자신이 속한 플랫폼

    [SerializeField] private float moveSpeed = 2f;  // 이동 속도
    [SerializeField] private float goalRadius = 1f;  // 목표 반경
    [SerializeField] private float maxDistance = 12f;  // 최대 허용 거리

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        
        // ✅ 자신이 속한 플랫폼(바닥)을 자동으로 찾기
        platform = transform.parent;
        if (platform == null)
        {
            Debug.LogError("[AncientGuardianAgent] 부모 오브젝트가 없습니다! 플랫폼을 설정하세요.");
        }

        // ✅ 씬에서 자신과 같은 플랫폼에 있는 Goal 찾기
        goal = FindGoalInPlatform();

        // ✅ 시작 위치 설정
        startPos = transform.position;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position.x);
        sensor.AddObservation(transform.position.z);
        sensor.AddObservation(rb.velocity.x);
        sensor.AddObservation(rb.velocity.z);
        sensor.AddObservation(goal.transform.position.x);
        sensor.AddObservation(goal.transform.position.z);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions[0]);
        EvaluateReward();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = Random.Range(0, 5);
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

    public override void OnEpisodeBegin()
    {
        ResetAgent();
    }

    private void MoveAgent(int action)
    {
        switch (action)
        {
            case 0: moveDir = Vector3.zero; break;
            case 1: moveDir = Vector3.forward; break;
            case 2: moveDir = Vector3.back; break;
            case 3: moveDir = Vector3.left; break;
            case 4: moveDir = Vector3.right; break;
        }
    }

    private void ApplyMovement()
    {
        rb.AddForce(moveDir * moveSpeed, ForceMode.VelocityChange);
    }

    private void EvaluateReward()
    {
        float distance = Vector3.Distance(transform.position, goal.transform.position);

        if (distance < goalRadius)
        {
            SetReward(1f);
            EndEpisode();
        }
        else if (distance > maxDistance)
        {
            SetReward(-1f);
            EndEpisode();
        }
        else
        {
            AddReward(0.1f * (maxDistance - distance));
        }
    }

    private void ResetAgent()
    {
        // ✅ 플랫폼(바닥) 범위 내에서 무작위 초기화
        transform.position = new Vector3(
            platform.position.x + Random.Range(-3f, 3f),
            1f,
            platform.position.z + Random.Range(-3f, 3f)
        );

        rb.velocity = Vector3.zero;

        // ✅ 해당 플랫폼 내의 목표 위치 변경
        goal.transform.position = new Vector3(
            platform.position.x + Random.Range(-3f, 3f),
            1f,
            platform.position.z + Random.Range(-3f, 3f)
        );
    }

    /// <summary>
    /// 자신이 속한 플랫폼 내에서 Goal 찾기
    /// </summary>
    private GameObject FindGoalInPlatform()
    {
        foreach (Transform child in platform)
        {
            if (child.CompareTag("Goal"))
            {
                return child.gameObject;
            }
        }
        Debug.LogError("[AncientGuardianAgent] 같은 플랫폼 내에서 Goal을 찾을 수 없습니다.");
        return null;
    }
}
