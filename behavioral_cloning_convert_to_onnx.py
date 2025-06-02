import torch
import torch.onnx

# Actor 클래스 정의 (이전에 정의했던 것과 동일)
class Actor(torch.nn.Module):
    def __init__(self):
        super(Actor, self).__init__()
        self.fc1 = torch.nn.Linear(12 * 4, 128)  # 상태 크기에 맞춰서
        self.fc2 = torch.nn.Linear(128, 128)
        self.mu = torch.nn.Linear(128, 1)  # 행동 크기에 맞춰서
        
    def forward(self, state):
        x = torch.relu(self.fc1(state))
        x = torch.relu(self.fc2(x))
        return torch.tanh(self.mu(x))

# 모델 로드 (이미 학습된 모델)
device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
model = Actor().to(device)

# 경로 수정 (역슬래시가 포함된 경로에서 \를 \\로 수정)
checkpoint_path = r"C:\Users\power\OneDrive\바탕 화면\진로탐색(1)\ML-Agents_Project\Kart_bc\saved_models\BC\20250118180908\ckpt"

# 모델 가중치 불러오기
checkpoint = torch.load(checkpoint_path, map_location=device)
model.load_state_dict(checkpoint["actor"])

# 더미 입력 생성 (모델이 예상하는 입력 크기에 맞게)
dummy_input = torch.randn(1, 12 * 4).to(device)  # 예시: 상태 크기가 (1, 12*4)

# 모델을 ONNX 형식으로 저장
onnx_path = 'behavioral_model.onnx'  # 저장될 ONNX 파일 경로

# 모델을 ONNX로 변환 (opset_version 명시)
torch.onnx.export(model, dummy_input, onnx_path, verbose=True, input_names=['input'], output_names=['output'], opset_version=11)

print(f"Model successfully converted to {onnx_path}")
