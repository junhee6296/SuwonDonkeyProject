# Password / Playback Speed / Frame Color Hotfix

## 반영 사항

1. VirtualBox SSH 비밀번호 자동 입력 옵션 추가
   - `학습 명령 생성` 창의 WSL/VirtualBox 실행 옵션에 `SSH 비밀번호` 입력칸과 `자동입력` 체크박스를 추가했습니다.
   - 비밀번호 입력칸은 `UseSystemPasswordChar`를 사용해 화면에 표시되지 않습니다.
   - 명령 미리보기와 실행 로그에는 실제 비밀번호 대신 `{SSH_PASSWORD}` 또는 `********`가 표시됩니다.
   - 비밀번호 자동 입력은 PuTTY의 `plink.exe`를 사용하도록 구성했습니다.
   - 자동 입력을 사용할 경우 생성 명령은 `plink -ssh -batch -pw "{SSH_PASSWORD}" ...` 형태가 됩니다.

2. SSH 무한 대기 방지
   - 비밀번호 자동입력을 사용하지 않는 VirtualBox SSH 명령은 기존처럼 `BatchMode=yes`, `NumberOfPasswordPrompts=0`, `ConnectTimeout=10` 옵션을 유지해 WinForms 내부에서 비밀번호 입력 대기로 멈추는 상황을 방지합니다.
   - 비밀번호 자동입력을 사용할 경우 `plink -batch -pw`를 사용해 입력 대기 없이 진행합니다.

3. 자동 재생 속도 조절 UI 추가
   - 자동 재생 버튼 주변에 `재생속도` 라벨과 TrackBar를 추가했습니다.
   - TrackBar를 오른쪽으로 이동할수록 빠르게 재생됩니다.
   - 범위는 약 1000ms ~ 50ms입니다.

4. 프레임 목록 색상 표시 보강
   - `CheckedListBox`를 `ColoredCheckedListBox` 커스텀 컨트롤로 교체했습니다.
   - 삭제된 프레임은 빨간 배경, 교체/편집된 프레임은 초록 배경으로 직접 그립니다.
   - 기존 DrawItem 이벤트가 환경에 따라 제대로 표시되지 않는 문제를 줄이기 위해 컨트롤 내부 `OnDrawItem` 오버라이드 방식으로 변경했습니다.

## 사용 주의

- Windows에서 비밀번호 자동 입력을 사용하려면 `plink.exe`가 필요합니다.
  - PuTTY 설치 후 plink.exe가 PATH에 잡혀 있어야 합니다.
  - 또는 `plink.exe`를 프로젝트 실행 파일과 같은 폴더에 두면 됩니다.
- 보안상 가장 권장되는 방식은 SSH 키 로그인입니다. 이 핫픽스의 비밀번호 자동입력은 수업/시연 환경에서 빠르게 실행하기 위한 옵션입니다.
