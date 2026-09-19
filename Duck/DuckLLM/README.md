# DuckLLM

ONNX Runtime GenAI를 사용하는 최소 C++ CLI 템플릿입니다. 현재는 FTXUI 없이 모델을 로드하고 생성 결과를 토큰 단위로 출력합니다.

## 준비

ONNX Runtime GenAI의 C++ 헤더와 라이브러리를 같은 SDK 루트에 준비합니다.

```text
<onnxruntime-genai-root>/
├─ include/ort_genai.h
└─ lib/
   ├─ onnxruntime.lib 또는 libonnxruntime.so
   └─ onnxruntime-genai.lib 또는 libonnxruntime-genai.so
```

공식 저장소에서 소스 빌드하는 경우 설치 결과 또는 빌드 결과의 `include`와 `lib`를 이 구조로 지정하면 됩니다. GenAI 라이브러리와 ONNX Runtime 라이브러리는 같은 버전에 맞춰야 합니다.

## 빌드

PowerShell에서 `ONNX_RUNTIME_GENAI_ROOT`를 SDK 경로로 바꿔 실행합니다.

```powershell
cmake -S . -B build -DONNX_RUNTIME_GENAI_ROOT="C:/path/to/onnxruntime-genai"
cmake --build build --config Release
```

FTXUI 준비 옵션은 다음 단계에서 사용할 수 있습니다.

```powershell
cmake -S . -B build -DONNX_RUNTIME_GENAI_ROOT="C:/path/to/onnxruntime-genai" -DDUCKLLM_ENABLE_FTXUI=ON
```

## 실행

모델 디렉터리는 `genai_config.json`과 해당 ONNX 모델 파일을 포함해야 합니다.

```powershell
.\build\Release\DuckLLM.exe --model "C:/path/to/model" --prompt "안녕하세요. 자기소개를 해주세요." --max-length 256
```

모델별 chat template이 필요한 경우, 다음 단계에서 모델 설정에 맞는 프롬프트 포맷 또는 `ApplyChatTemplate` 처리를 추가합니다.
