# OpenSSH 비밀번호/프레임 색상 핫픽스

## 수정 내용

1. Form1.cs 3314 인근 문자열 이스케이프 오류 수정
   - `Regex.Replace` 패턴의 잘못된 verbatim string escape를 일반 문자열로 변경했습니다.
   - CS1525, CS1056, CS1003, CS1009, CS1026 계열 컴파일 오류가 나지 않도록 수정했습니다.

2. PuTTY/plink 의존성 제거
   - 비밀번호 자동입력 모드를 `plink -pw` 방식에서 Windows OpenSSH + 임시 `SSH_ASKPASS` 방식으로 변경했습니다.
   - 명령 미리보기와 로그에는 비밀번호가 표시되지 않습니다.
   - 실행 시 임시 askpass 스크립트를 만들어 ssh의 password prompt에만 비밀번호를 전달합니다.

3. VirtualBox SSH 실행 흐름 유지
   - 비밀번호 자동입력을 켠 경우에도 생성 명령은 `ssh ...` 형태입니다.
   - 비밀번호 자동입력을 끈 경우에는 기존처럼 `BatchMode=yes`로 빠르게 실패하도록 유지했습니다.

4. 프레임 목록 색상 표시 보강
   - `CheckedListBox`의 외부 DrawItem 이벤트 연결을 제거하고, 커스텀 `ColoredCheckedListBox` 내부 렌더링만 사용하도록 정리했습니다.
   - 삭제 항목은 빨간 계열, 교체/편집 항목은 초록 계열로 표시됩니다.
   - 선택된 상태에서도 삭제/편집 색이 유지되도록 했습니다.

## 사용 방법

- 학습 명령 생성 창에서 VirtualBox 환경을 선택합니다.
- SSH 비밀번호와 자동입력을 체크합니다.
- 생성된 명령은 `ssh` 명령으로 표시되며 비밀번호는 표시되지 않습니다.
- 학습 실행 시 비밀번호는 임시 askpass를 통해 자동 전달됩니다.
