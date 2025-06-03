using UnityEngine;
using Unity.Barracuda;

public class ModelInference : MonoBehaviour
{
    public NNModel modelAsset; // Unity에 임포트한 모델
    private IWorker worker;    // 추론을 위한 워커
    private Tensor inputTensor;  // 모델 입력
    private Tensor outputTensor;  // 모델 출력

    void Start()
    {
        // 모델 로딩
        var model = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharpBurst, model);  // CSharp 또는 GPU/CPU 선택

        // 예시로 입력 텐서 생성 (입력 크기에 맞게 조정)
        inputTensor = new Tensor(1, 48); // 입력 크기는 모델에 맞게 설정
    }

    void Update()
    {
        // 예시: 모델 추론을 위한 입력값 업데이트
        // 예를 들어, 게임 상태에 따라 입력값을 업데이트
        inputTensor[0] = Mathf.Sin(Time.time);  // 상태에 맞게 입력 값 설정

        // 모델 추론
        worker.Execute(inputTensor);

        // 추론된 출력값을 얻음
        outputTensor = worker.PeekOutput();

        // 출력값을 게임 환경에 적용 (예: 캐릭터 이동)
        float action = outputTensor[0]; // 예시: 출력값이 단일 액션일 경우
        Debug.Log("Action: " + action);

        // 텐서 클리어
        inputTensor.Dispose();
        outputTensor.Dispose();
    }

    void OnDestroy()
    {
        // 종료 시 워커를 닫음
        worker.Dispose();
    }
}