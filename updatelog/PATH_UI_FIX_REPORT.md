# 학습 경로 UI 설정 기능 수정 보고서

## 목적
VirtualBox/Ubuntu 내부에서 DonkeyCar 학습을 실행하는 경우 Windows 경로(`C:\\...`)와 Linux 경로(`/media/sf_...`)가 달라 학습 명령 수정이 어려운 문제를 해결했습니다.

## 변경 사항
- AI 학습 실행 영역에 `경로 설정` 버튼 추가
- `학습 data/tub 경로`와 `모델 저장 경로`를 별도 UI에서 직접 설정 가능
- `{MODEL_PATH}` 플레이스홀더 추가
- `VirtualBox 경로 변환` 버튼 추가
  - 예: `C:\\Users\\...\\virtualbox_share\\data`
  - 변환: `/media/sf_virtualbox_share/data`
- `명령 템플릿에 반영` 버튼 추가
  - `donkey train --tub "{DATA_FOLDER}" --model "{MODEL_PATH}"`
- Linux/VirtualBox 경로를 사용할 경우 Windows에서 모델 폴더 생성을 시도하지 않도록 처리

## 사용법
1. 데이터를 먼저 불러옵니다.
2. AI 학습 실행 영역에서 `경로 설정`을 누릅니다.
3. VirtualBox에서 보이는 경로를 직접 입력하거나 `VirtualBox 경로 변환`을 누릅니다.
4. `적용`을 누릅니다.
5. 학습 명령에서 `{DATA_FOLDER}`, `{MODEL_PATH}`가 설정한 경로로 치환됩니다.

## 예시
Windows 공유폴더가 다음과 같다면:

```text
C:\\Users\\jh031\\Desktop\\virtualbox_share\\data
```

VirtualBox Ubuntu에서는 보통 다음처럼 지정합니다.

```text
/media/sf_virtualbox_share/data
/media/sf_virtualbox_share/models/mypilot.h5
```
