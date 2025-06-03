using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)]
    private float timeScale = 0.25f;

    private float defaultFixedDeltaTime;

    void Awake()
    {
        defaultFixedDeltaTime = Time.fixedDeltaTime;
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = defaultFixedDeltaTime * timeScale;
    }
}