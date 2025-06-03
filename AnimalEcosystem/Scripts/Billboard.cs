using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCam == null) return;

        // 카메라를 향하도록 회전
        transform.rotation = Quaternion.LookRotation(transform.position - mainCam.transform.position);
    }
}