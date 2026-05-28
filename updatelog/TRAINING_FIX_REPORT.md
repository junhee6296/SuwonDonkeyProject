# Training & Gemini Review Fix Report

## 해결한 문제

### 1. `donkey` 명령 인식 실패
기존 학습 실행은 `cmd /C donkey train ...`만 호출했습니다. Windows PATH에 DonkeyCar CLI가 없으면 다음 오류가 발생했습니다.

```text
'donkey'은(는) 내부 또는 외부 명령, 실행할 수 있는 프로그램, 또는 배치 파일이 아닙니다.
```

수정 사항:
- `donkey` 명령 실행 전 실제 `donkey.exe` 위치를 자동 탐색합니다.
- 탐색 대상:
  - PATH
  - `DONKEY_EXE` 환경 변수
  - 프로젝트 루트/데이터 폴더의 `.venv`, `venv`, `env` Scripts 폴더
  - Anaconda/Miniconda/Mambaforge env Scripts 폴더
  - Windows Python 설치 폴더의 Scripts 폴더
  - `py` 또는 `python`에서 import 가능한 donkeycar 패키지의 Scripts 폴더
- 로컬 Windows에서 찾지 못하고 WSL 내부에 `donkey`가 있으면 WSL 명령으로 자동 전환합니다.
- `models` 폴더가 없으면 학습 실행 전 자동 생성합니다.
- 학습 실행 그룹에 `환경 진단` 버튼을 추가했습니다.

### 2. Process 실행 안정성
기존 방식은 프로세스가 빠르게 종료될 때 이벤트/비동기 출력 읽기 순서에 따라 누락이나 핸들 정리 문제가 생길 수 있었습니다.

수정 사항:
- 명령을 가능한 경우 shell 없이 `ProcessStartInfo.ArgumentList`로 직접 실행합니다.
- stdout/stderr를 별도 Task로 읽고 `WaitForExit` 이후 정상 정리합니다.
- `cmd` shell은 `call`, `&&`, pipe/redirection 같은 shell 문법이 있는 경우에만 사용합니다.

### 3. Gemini 지적: 빈 데이터셋 로드 시 UI 초기화 불완전
`allFrames.Count == 0`일 때 `ClearCurrentFrame()`만 호출하면 이전 프레임 목록, 트랙바, 그래프 상태가 남을 수 있었습니다.

수정 사항:
- 빈 데이터셋인 경우 `ApplyFilters(null)`를 호출하도록 변경했습니다.
- 이 경로에서 프레임 목록, 트랙바, 그래프, 현재 프레임 표시가 함께 초기화됩니다.

## 사용 방법

1. 데이터를 로드합니다.
2. `환경 진단` 버튼을 눌러 DonkeyCar CLI가 잡히는지 확인합니다.
3. 기본 명령은 그대로 사용할 수 있습니다.

```text
donkey train --tub "{DATA_FOLDER}" --model "{ROOT_FOLDER}\models\mypilot.h5"
```

DonkeyCar가 별도 가상환경에만 설치되어 있으면 학습 명령을 다음처럼 직접 바꿔도 됩니다.

```text
call "C:\path\to\venv\Scripts\activate.bat" && donkey train --tub "{DATA_FOLDER}" --model "{ROOT_FOLDER}\models\mypilot.h5"
```
