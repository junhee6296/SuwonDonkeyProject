# Gemini 코드 리뷰 반영 내역

## 1. 프레임 목록 글씨 잘림 완화
- 프레임 목록 항목에서 긴 이미지 파일명을 제거하고 index/angle/throttle 중심의 짧은 표시 형식으로 변경했습니다.
- 전체 이미지 파일명은 기존처럼 중앙 정보 영역의 `이미지:` 라벨에서 확인할 수 있습니다.
- ListBox에 가로 스크롤과 `HorizontalExtent` 자동 계산을 추가했습니다.
- 긴 라벨에는 `AutoEllipsis` 또는 더 큰 높이를 적용해 Windows 배율 환경에서 텍스트가 잘리지 않도록 조정했습니다.

## 2. 이상 주행 탐지 O(N^2) 병목 제거
- `active.Take(i).LastOrDefault(...)` 방식으로 이전 angle을 찾던 코드를 제거했습니다.
- 루프 안에서 `previousAngle`을 유지하여 이전 프레임 변화량을 O(1)로 계산하게 바꿨습니다.
- 이웃 통계 계산도 `List<double>` 생성 없이 `sum/sumSquares/count`로 계산해 불필요한 할당을 줄였습니다.

## 3. 이미지 편집 GDI+ 안정성 개선
- `picFrame.Image`로 사용 중인 Bitmap을 직접 수정하지 않도록 변경했습니다.
- 현재 Bitmap을 복제한 뒤 복제본에 마스킹/교체 작업을 적용하고, 완료 후 PictureBox 이미지로 교체합니다.
- UI 스레드가 표시 중인 이미지와 GDI+ 편집 대상이 겹쳐 발생할 수 있는 `InvalidOperationException` 가능성을 낮췄습니다.

## 4. catalog 저장 안정성 개선
- catalog 파일을 직접 덮어쓰지 않고 `.tmp` 파일에 먼저 기록한 뒤 교체하도록 수정했습니다.
- 쓰기 도중 예외가 발생하면 임시 파일을 삭제하고 원본 catalog는 그대로 남도록 했습니다.

## 5. 이미지 저장 안정성 개선
- 이미지 저장도 임시 파일에 저장한 뒤 원본 파일과 교체하도록 변경했습니다.
- 저장 실패 시 임시 파일을 정리합니다.

## 6. Python/DonkeyCar 학습 프로세스 실행 안정성 개선
- `process.Start()`와 `BeginOutputReadLine()` 주변을 `try-catch`로 보호했습니다.
- 프로세스 시작 실패 또는 예외 발생 시 `Process.Dispose()`가 반드시 호출되도록 수정했습니다.
- `TaskCompletionSource`는 `RunContinuationsAsynchronously` 옵션을 사용하도록 변경했습니다.

## 주의
- 이 패키지는 `.NET 8 Windows Forms` 대상입니다.
- Visual Studio 2022에서 `TeamApp.sln`으로 여세요.
