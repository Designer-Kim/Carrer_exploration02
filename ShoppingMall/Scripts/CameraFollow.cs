using UnityEngine;

/// <summary>
/// AI 로봇을 부드럽게 따라오는 카메라 기능 (FixedUpdate 사용).
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform aiRobot; // ✅ 따라갈 AI 로봇
    [SerializeField] private Vector3 offset = new Vector3(0, 10, -5); // ✅ 카메라 위치 오프셋
    [SerializeField] private float followSpeed = 10f; // ✅ 따라오는 속도

    private void FixedUpdate() // ✅ Rigidbody 이동과 동기화
    {
        if (aiRobot == null) return;

        Vector3 targetPosition = aiRobot.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.fixedDeltaTime);
        transform.LookAt(aiRobot); // ✅ AI 로봇을 바라보도록 설정
    }
}