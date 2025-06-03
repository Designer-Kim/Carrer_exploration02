using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

/// <summary>
/// AI 로봇이 NavMesh를 이용하여 Waypoint를 따라 이동.
/// </summary>
public class AIRobotNavMesh : MonoBehaviour
{
    private NavMeshAgent agent;
    [SerializeField] private List<Transform> waypoints = new List<Transform>();  // 이동 경로 (Waypoint 목록)
    private int currentTargetIndex = 0;  // 현재 목표 인덱스

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogError("[AIRobotNavMesh] 🚨 Waypoint 리스트가 비어 있습니다!");
            return;
        }

        agent.SetDestination(waypoints[currentTargetIndex].position);  // 첫 번째 목적지 설정
    }

    private void Update()
    {
        if (waypoints.Count == 0) return;

        // 현재 목표 지점에 도착했는지 확인
        if (!agent.pathPending && agent.remainingDistance < agent.stoppingDistance)
        {
            SetNextTarget();  // 다음 Waypoint로 이동
        }
    }

    private void SetNextTarget()
    {
        currentTargetIndex = (currentTargetIndex + 1) % waypoints.Count;  // 다음 Waypoint로 순환 이동
        agent.SetDestination(waypoints[currentTargetIndex].position);
    }
}