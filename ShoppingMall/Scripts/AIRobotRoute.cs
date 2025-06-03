using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// AI 로봇이 백화점 내에서 상점 앞 경로를 따라 이동하는 기능을 담당.
/// </summary>
public class AIRobotRoute : MonoBehaviour
{
    [SerializeField] private List<Transform> waypoints = new List<Transform>(); // ✅ 이동할 Waypoints 리스트
    [SerializeField] private float speed = 2f; // ✅ 이동 속도
    private int currentTargetIndex = 0;  // 현재 목표 Waypoint 인덱스
    private Transform currentTarget;  // 현재 목표 지점

    private void Start()
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogError("[AIRobotRoute] 🚨 Waypoint 리스트가 비어 있습니다! 이동할 경로를 추가하세요.");
            enabled = false; // 스크립트 비활성화
            return;
        }

        // ✅ 시작 위치를 첫 번째 Waypoint로 설정하고 초기 목표 설정
        transform.position = waypoints[0].position;
        currentTarget = waypoints[0];
    }

    private void Update()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        MoveToTarget();
        CheckArrival();
    }

    /// <summary>
    /// 현재 목표를 향해 이동
    /// </summary>
    private void MoveToTarget()
    {
        if (currentTarget == null) return;
        transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, speed * Time.deltaTime);
    }

    /// <summary>
    /// 목표 지점 도착 여부 확인 후 다음 목표 설정
    /// </summary>
    private void CheckArrival()
    {
        if (Vector3.Distance(transform.position, currentTarget.position) < 0.1f)
        {
            SetNextTarget();
        }
    }

    /// <summary>
    /// 다음 목표 Waypoint를 설정
    /// </summary>
    private void SetNextTarget()
    {
        currentTargetIndex++;

        if (currentTargetIndex >= waypoints.Count)
        {
            currentTargetIndex = 0; // 마지막 Waypoint 도착 후 다시 처음으로
        }

        currentTarget = waypoints[currentTargetIndex];
    }

    /// <summary>
    /// 외부에서 Waypoint 리스트를 설정할 수 있도록 메서드 추가 (확장성 고려)
    /// </summary>
    public void SetWaypoints(List<Transform> newWaypoints)
    {
        if (newWaypoints == null || newWaypoints.Count == 0)
        {
            Debug.LogError("[AIRobotRoute] 🚨 새로운 Waypoint 리스트가 비어 있습니다!");
            return;
        }

        waypoints = newWaypoints;
        currentTargetIndex = 0;
        currentTarget = waypoints[0];
        transform.position = waypoints[0].position;
    }
}
