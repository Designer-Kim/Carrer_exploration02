using UnityEngine;

/// <summary>
/// 플레이어 이동을 담당하는 스크립트.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // ✅ 이동 속도
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        Move();
    }

    /// <summary>
    /// 플레이어 이동 처리
    /// </summary>
    private void Move()
    {
        float moveX = Input.GetAxis("Horizontal");  // A, D 키 또는 ←, →
        float moveZ = Input.GetAxis("Vertical");    // W, S 키 또는 ↑, ↓

        Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;
        rb.velocity = moveDirection * moveSpeed;
    }
}