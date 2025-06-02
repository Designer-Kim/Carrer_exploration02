import torch
import torch.onnx

# 모델 정의 (state_size를 6으로 수정)
class Actor(torch.nn.Module):
    def __init__(self):
        super(Actor, self).__init__()
        self.d1 = torch.nn.Linear(6, 128)  # state_size를 6으로 수정
        self.d2 = torch.nn.Linear(128, 128)
        self.pi = torch.nn.Linear(128, 3)  # 행동 크기에 맞춰서 (예: 3개 행동)
        self.v = torch.nn.Linear(128, 1)
        
    def forward(self, state):
        x = torch.relu(self.d1(state))
        x = torch.relu(self.d2(x))
        return torch.softmax(self.pi(x), dim=-1), self.v(x)

# `A` 모델의 ckpt 경로
checkpoint_path_A = r"C:\Users\power\OneDrive\바탕 화면\진로탐색(1)\ML-Agents_Project\Pong_ppo\saved_models\APPO\20221004220009\A\ckpt"
model_A = Actor().to('cpu')  # CPU에서 모델 로드

# checkpoint 파일 로드
checkpoint_A = torch.load(checkpoint_path_A, map_location='cpu')
print(checkpoint_A.keys())  # checkpoint 파일에서 저장된 키들을 확인

# 올바른 키 이름을 사용하여 모델 파라미터 로드 (예: 'network')
model_A.load_state_dict(checkpoint_A['network'])

# 모델을 추론 모드로 설정
model_A.eval()

# 더미 입력 생성 (모델이 예상하는 입력 크기)
dummy_input = torch.randn(1, 6)  # state_size=6에 맞는 입력 크기

# 모델을 ONNX 형식으로 저장 (A 모델)
onnx_path_A = 'pong_model_A.onnx'
torch.onnx.export(model_A, dummy_input, onnx_path_A, verbose=True, input_names=['input'], output_names=['output'])

# 동일하게 `B` 모델도 변환
checkpoint_path_B = r"C:\Users\power\OneDrive\바탕 화면\진로탐색(1)\ML-Agents_Project\Pong_ppo\saved_models\APPO\20221004220009\B\ckpt"
model_B = Actor().to('cpu')  # CPU에서 모델 로드
checkpoint_B = torch.load(checkpoint_path_B, map_location='cpu')

# 올바른 키 이름을 사용하여 모델 파라미터 로드 (예: 'network')
model_B.load_state_dict(checkpoint_B['network'])

# 모델을 추론 모드로 설정
model_B.eval()

# 모델을 ONNX 형식으로 저장 (B 모델)
onnx_path_B = 'pong_model_B.onnx'
torch.onnx.export(model_B, dummy_input, onnx_path_B, verbose=True, input_names=['input'], output_names=['output'])

print("ONNX Models for A and B are successfully created.")
