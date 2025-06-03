using UnityEngine;

public class DetectionVisualizer : MonoBehaviour
{
    [Tooltip("시각화할 감지 반경 (단위: 미터)")]
    public float radius = 5f;

    [Tooltip("감지 범위 시각화용 프리팹 (반투명 Sphere)")]
    public GameObject visualPrefab;

    private GameObject visualInstance;

    void Start()
    {
        if (visualPrefab != null)
        {
            visualInstance = Instantiate(visualPrefab, transform.position, Quaternion.identity);
            visualInstance.transform.SetParent(transform); // 에이전트 따라가도록 설정
            visualInstance.transform.localPosition = Vector3.zero;
            visualInstance.transform.localScale = Vector3.one * radius * 2f;
        }
        else
        {
            Debug.LogWarning($"[DetectionVisualizer] '{gameObject.name}'에 visualPrefab이 설정되지 않았습니다.");
        }
    }
}