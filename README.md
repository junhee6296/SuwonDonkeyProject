# DonkeyCar UI 데이터 관리 도구

WinForms 기반 DonkeyCar 학습 데이터 관리 프로그램입니다. DonkeyCar의 `catalog` 데이터와 `images` 폴더를 불러와 프레임을 확인하고, 조향값(`angle`)·스로틀값(`throttle`)을 수정하거나 불량 데이터를 삭제/복구할 수 있습니다. 또한 이상 주행 자동 탐지, 이미지 일부 가리기/교체, 학습 데이터 생성, VirtualBox/WSL/Windows 환경별 AI 학습 실행까지 하나의 UI에서 처리하도록 구성했습니다.

---

## 1. 화면 구성 순서

이 프로젝트의 사용 흐름은 다음 3단계 화면 순서로 이해하면 됩니다.

![화면 구성](img/1.png)

### 1단계. 메인 화면: 데이터 조회 및 프레임 탐색

첫 번째로 사용하는 화면은 메인 Form1 화면입니다. `폴더 열기` 버튼으로 DonkeyCar 데이터 폴더를 불러오면 왼쪽 프레임 목록에 catalog의 프레임들이 표시되고, 중앙에는 선택한 프레임 이미지가 표시됩니다. 하단에는 선택 프레임의 `angle`, `throttle`, 이미지명, catalog 파일명이 표시됩니다.

주요 기능은 다음과 같습니다.

- DonkeyCar `mycar`, `data`, `tub` 폴더 자동 인식
- `catalog_*.catalog`, `catalog.json`, `manifest` 기반 데이터 로드
- 이미지 프레임 미리보기
- 프레임 이전/다음 이동
- 트랙바 기반 프레임 탐색
- 자동 재생 및 재생 속도 조절
- `angle`, `throttle` 값 수정 후 저장
- 그래프를 통한 `angle`, `throttle`, 이동평균, 이상 후보 시각화

### 2단계. 메인 화면: 데이터 정제, 삭제/복구, 이미지 편집

두 번째 단계는 불량 데이터 정제입니다. 프레임 목록에서 체크박스를 이용해 여러 프레임을 선택할 수 있고, 선택된 프레임에 대해 삭제, 복구, 이미지 가리기, 이미지 교체를 수행할 수 있습니다.

상태 색상은 다음과 같이 표시됩니다.

| 상태 | 표시 방식 | 의미 |
|---|---|---|
| 정상 | 기본 배경 | 원본 이미지와 catalog가 정상인 프레임 |
| 삭제 | 빨간 배경 | 이미지가 `_deleted_backup`으로 이동된 프레임 |
| 편집 | 초록 배경 | 이미지 일부 가리기 또는 교체가 적용된 프레임 |
| 이상 후보 | 그래프/타임라인 빨간 표시 | 조향값 변동성이 큰 자동 탐지 프레임 |

삭제 방식은 단순 표시가 아니라 실제 이미지 파일을 `data/_deleted_backup` 폴더로 이동하는 방식입니다. 따라서 학습 데이터 생성 시 삭제된 프레임은 제외할 수 있고, 필요하면 `삭제 이미지 복구` 버튼으로 원래 위치에 복구할 수 있습니다.

이미지 편집 기능은 중앙 이미지 위에서 마우스로 드래그해 영역을 선택한 뒤 사용할 수 있습니다.

- `영역 가리기`: 선택 영역을 흰색, 검정, 회색, 평균색 등으로 마스킹
- `이미지로 교체`: 선택 영역을 다른 이미지로 대체
- `선택 해제`: 현재 선택 영역 제거
- `원본 복구`: `_edited_backup`에 저장된 원본 이미지로 복구

### 3단계. AI 학습 화면: 학습 환경 설정 및 실행 결과 확인

세 번째 단계는 AI 학습입니다. 메인 화면에서는 복잡한 학습 설정을 노출하지 않고 `AI 학습` 버튼만 제공합니다. 이 버튼을 누르면 별도의 Form2 창이 열리며, 여기에서 학습 범위, 실행 환경, 경로, 명령어 생성, 학습 실행, 학습 결과 확인을 수행합니다.

AI 학습 창에서는 다음을 설정합니다.

- 학습 데이터 범위
  - 이상치 포함 전체 데이터 학습
  - 이상치 제외 학습
  - 데이터 필터링 선택군만 학습
- 실행 환경
  - Windows 환경 빌드
  - WSL 환경 빌드
  - VirtualBox 환경 빌드
- 학습 data/tub 경로
- 모델 저장 경로
- conda/venv 환경 활성화 명령
- SSH user, host, port
- VirtualBox 원격 `mycar` 경로
- 비밀번호 입력형 콘솔 사용 여부
- 추가/보정 인자, 예: `--type linear`

학습이 끝나면 오른쪽 결과 패널에 성공률이 크게 표시됩니다. 성공률은 종료 코드, 모델 파일 생성 여부, 로그 내용을 바탕으로 계산되며, 낮은 점수는 빨간색, 높은 점수는 초록색으로 표시됩니다.

---

## 2. 프로젝트 파일 구조

```text
TeamApp/
├─ Program.cs
├─ Form1.cs
├─ Form1.Designer.cs
├─ Form1.resx
├─ Form2.cs
├─ TeamApp.csproj
├─ TeamApp.sln
└─ README.md
```

---

## 3. 코드별 역할

### Program.cs

프로그램의 시작점입니다.

주요 역할:

- WinForms 애플리케이션 초기화
- `Form1` 메인 창 실행

핵심 코드 흐름:

```csharp
ApplicationConfiguration.Initialize();
Application.Run(new Form1());
```

---

### Form1.Designer.cs

Form1의 기본 컨트롤 선언과 초기 배치를 담당합니다. 실제 세부 동작은 `Form1.cs`에서 처리합니다.

주요 역할:

- 메인 폼의 기본 컨트롤 생성
- 버튼, 리스트, 패널, PictureBox, TextBox 등의 초기화
- 디자이너에서 폼을 열 수 있도록 기본 UI 구조 제공

---

### Form1.cs

프로그램의 핵심 메인 기능이 들어 있는 파일입니다. 데이터 로드, 프레임 탐색, 필터링, 이미지 편집, 삭제/복구, 이상 탐지, 학습 버튼 연결을 담당합니다.

#### 3.1 런타임 초기화와 반응형 레이아웃

관련 메서드:

```csharp
InitializeRuntime()
ApplyResponsiveLayout()
LayoutFrameList()
LayoutPreviewPanel()
LayoutFilterPanel()
LayoutAnomalyPanel()
LayoutTrainPanel()
LayoutLogPanel()
```

역할:

- 폼이 실행될 때 이벤트 핸들러 연결
- 창 크기에 따라 좌측 프레임 목록, 중앙 이미지 패널, 우측 필터/학습 패널 자동 재배치
- 화면 확대/축소 시 UI가 비율에 맞게 확장 또는 축소되도록 처리

#### 3.2 DonkeyCar 데이터 로드

관련 메서드:

```csharp
LoadDataset()
TryResolveDonkeyFolder()
GetCatalogFiles()
ReadCatalogRecords()
TryParseCatalogLine()
```

역할:

- 사용자가 선택한 폴더가 `mycar`, `data`, `tub` 중 무엇인지 자동 판단
- `catalog_*.catalog`, `catalog.json`, `manifest` 파일 탐색
- 각 프레임의 이미지 경로, `angle`, `throttle`, catalog 위치를 `FrameRecord` 객체로 변환
- 누락되거나 잘못된 데이터를 최대한 건너뛰며 프레임 목록 구성

#### 3.3 필터링

관련 메서드:

```csharp
ApplyFilters()
PassesFilter()
btnClearFilter_Click()
chkAnomalyOnly_CheckedChanged()
chkStatusFilter_CheckedChanged()
```

구현 기능:

- `throttle > 0`만 표시
- `angle = 0` 제외
- angle 범위 필터
- throttle 범위 필터
- 이상 후보만 표시
- 삭제 이미지만 표시
- 교체/편집 이미지만 표시

#### 3.4 프레임 목록과 체크박스 제어

관련 메서드:

```csharp
GetCheckedRecords()
GetTargetRecordsForBatch()
btnCheckAllFrames_Click()
btnClearCheckedFrames_Click()
lstFrames_DrawItem()
ColoredCheckedListBox.OnMouseDown()
ColoredCheckedListBox.OnDrawItem()
```

구현 기능:

- 프레임 목록 왼쪽 체크박스를 통해 다중 선택
- `전체 선택`, `전체 해제`
- 체크박스 영역을 눌렀을 때만 체크 상태 변경
- 글자 영역 클릭 시 프레임 선택만 수행
- 삭제/편집/이상 후보 상태에 따른 색상 표시

#### 3.5 프레임 탐색과 자동 재생

관련 메서드:

```csharp
ShowFrame()
LoadImage()
ReplaceFrameImage()
MoveSelection()
btnPrev_Click()
btnNext_Click()
btnPlay_Click()
GetAutoPlayInterval()
trbPlaySpeed_Scroll()
```

구현 기능:

- 선택한 프레임 이미지 표시
- 이전/다음 이동
- 자동 재생
- 재생 속도 조절
- 트랙바를 통한 위치 이동

#### 3.6 catalog 저장 및 값 수정

관련 메서드:

```csharp
btnSave_Click()
RewriteCatalogs()
BuildCatalogJson()
ReplaceFileAtomically()
```

구현 기능:

- 사용자가 수정한 `angle`, `throttle` 값을 catalog에 반영
- catalog 직접 덮어쓰기 대신 임시 파일 저장 후 교체 방식 사용
- 저장 중 오류가 발생해도 원본 손상을 줄이도록 처리

#### 3.7 이미지 삭제 및 복구

관련 메서드:

```csharp
btnDelete_Click()
btnUndo_Click()
MoveImageToDeletedBackup()
RestoreImageFromDeletedBackup()
GetDeletedBackupPath()
ResolveDeletedBackupPath()
```

구현 기능:

- 선택 프레임 또는 체크된 여러 프레임의 이미지를 `data/_deleted_backup`으로 이동
- 삭제된 프레임은 목록에서 제거하지 않고 빨간색으로 표시
- 삭제 이미지 복구 시 백업 폴더에서 원래 이미지 위치로 복원
- 삭제 상태는 `_ui_marks.json`에 저장

#### 3.8 UI 상태 저장

관련 메서드:

```csharp
GetUiMarksPath()
LoadUiMarks()
SaveUiMarks()
MakeUiMarkPathValue()
```

구현 기능:

- 삭제된 이미지 목록 저장
- 편집된 이미지 목록 저장
- 프로그램을 다시 열어도 삭제/편집 상태 유지

#### 3.9 비밀 기능 1: 이상 주행 자동 탐지

관련 메서드:

```csharp
DetectAnomalies()
RecalculateAnomaliesIfNeeded()
ClearAnomalyFlags()
btnAnalyzeAnomaly_Click()
btnNextAnomaly_Click()
DrawGraph()
DrawTimeline()
```

구현 내용:

- 조향값(`angle`)의 이동평균과 변동성 밴드를 계산
- 평소 흐름에서 크게 벗어난 조향 스파이크를 이상 주행 후보로 판단
- 이상 후보를 프레임 목록, 그래프, 타임라인에 표시
- `다음 이상` 버튼으로 이상 후보 프레임을 빠르게 탐색

차별점:

- 사람이 수천 장의 프레임을 모두 확인하지 않아도 불량 데이터 후보를 먼저 찾을 수 있음
- 데이터 클리닝 시간을 줄이고 학습 품질을 높임

#### 3.10 비밀 기능 2: 이미지 일부 가리기 / 바꾸기

관련 메서드:

```csharp
picFrame_MouseDown()
picFrame_MouseMove()
picFrame_MouseUp()
picFrame_Paint()
TryGetSelectedBitmapAndRect()
btnMaskRegion_Click()
btnReplaceRegion_Click()
ApplyMaskToImageFile()
ApplyReplacementToImageFile()
BackupImageIfNeeded()
SaveBitmapAtomically()
btnRestoreImage_Click()
```

구현 내용:

![이미지 편집](img/2.png)

- 이미지 위에서 마우스로 드래그하여 영역 선택
- 선택 영역을 흰색, 검정, 회색, 평균색으로 가리기
- 선택 영역을 다른 이미지로 교체
- 편집 전 원본을 `_edited_backup`에 백업
- 편집된 프레임은 초록색으로 표시

활용 예시:

- 시뮬레이터 화면의 불필요한 UI 일부 가리기
- 특정 방해 요소를 가려 학습 데이터 품질 개선
- 이미지 전처리 효과를 직접 확인

#### 3.11 비밀 기능 3: 학습 데이터셋 생성

관련 메서드:

```csharp
CreateTrainingDatasetForDialog()
CreateTrainingDataset()
BuildCatalogJsonForExport()
```

구현 내용:

- 현재 필터 조건 또는 이상치 제외 조건을 반영해 별도 학습 데이터셋 생성
- 삭제된 프레임은 학습 데이터에서 제외
- 선택군만 학습하는 경우 `_training_sets` 폴더에 catalog와 이미지를 복사해 독립적인 학습셋 구성

#### 3.12 AI 학습 연동

관련 메서드:

```csharp
btnTrainingPaths_Click()
BuildTrainingCommand()
BuildBashTrainBody()
ConvertPathForEnvironment()
ConvertWindowsSharedFolderPathToVirtualBoxPath()
StoreGeneratedTrainingCommand()
btnTrain_Click()
RunInteractiveConsoleCommandAsync()
RunCommandAsync()
MaskSensitiveCommand()
PrepareTrainingCommand()
```

구현 내용:

- Windows, WSL, VirtualBox 환경에 맞는 경로 변환
- VirtualBox에서는 SSH로 Ubuntu 내부의 `mycar` 폴더에 접속해 `python train.py` 실행
- 비밀번호 입력형 콘솔 모드 지원
- 로그에 비밀번호가 표시되지 않도록 마스킹
- 학습 실행 결과를 Form2로 전달

---

### Form2.cs

AI 학습 전용 창입니다. 기존에 메인 화면에 있던 복잡한 학습 설정을 분리해 Form2에 모았습니다.

#### 4.1 학습 환경 설정 UI

관련 컨트롤:

```csharp
cmbDatasetMode
cmbEnvironment
txtData
txtModel
txtExtraArgs
txtActivate
txtSshUser
txtSshHost
txtSshPort
txtRemoteWork
chkPasswordInput
chkManualEdit
```

역할:

- 학습 범위 선택
- 실행 환경 선택
- data/tub 경로 설정
- 모델 저장 경로 설정
- 추가 학습 인자 입력
- conda/venv 환경 활성화 명령 입력
- SSH 접속 정보 입력
- 명령어 수동 편집 여부 제어

#### 4.2 경로 변환 및 명령 생성

관련 메서드:

```csharp
UseLoadedPaths()
ConvertCurrentPathsForEnvironment()
ExportTrainingData()
GenerateCommand()
```

구현 기능:

- 현재 메인 화면에서 불러온 경로를 학습 창으로 가져오기
- Windows 경로를 WSL 또는 VirtualBox 경로로 변환
- 학습 데이터 생성
- 환경별 학습 명령 자동 생성

VirtualBox 명령 예시:

```bash
ssh -T -o PreferredAuthentications=password -o PubkeyAuthentication=no -o NumberOfPasswordPrompts=1 -p 2222 xytron@127.0.0.1 "source ~/miniconda3/bin/activate donkey_conda_tf && cd '/media/sf_virtualbox_share/mycar' && python train.py --tub '/media/sf_virtualbox_share/mycar/data' --model '/media/sf_virtualbox_share/mycar/models/mypilot.h5'"
```

#### 4.3 학습 실행과 결과 평가

관련 메서드:

```csharp
RunTrainingAsync()
ShowTrainingResult()
CalculateScore()
SetScore()
ScoreToColor()
AppendLog()
```

구현 기능:

- 생성된 명령 실행
- 학습 로그 표시
- 종료 코드 확인
- 모델 파일 생성 여부 확인
- 학습 성공률 계산
- 성공률에 따라 빨간색에서 초록색으로 색상 표시
- 사람이 이해하기 쉬운 피드백 제공

성공 판정 기준 예시:

- 종료 코드가 `0`인가?
- `mypilot.h5` 파일이 생성 또는 갱신되었는가?
- 로그에 SSH, TensorFlow, 이미지 손상 오류가 없는가?

---

## 4. 대표 실행 흐름

### 4.1 데이터 불러오기

1. `폴더 열기` 클릭
2. `mycar`, `data`, 또는 tub 폴더 선택
3. 프레임 목록과 이미지 미리보기 확인
4. 필요하면 `angle`, `throttle` 수정 후 저장

### 4.2 불량 데이터 정제

1. `이상 탐지 실행` 클릭
2. 타임라인과 그래프에서 빨간 이상 후보 확인
3. 이상 후보 또는 직접 선택한 프레임 체크
4. 불량 이미지는 `이미지 삭제`
5. 잘못 삭제한 경우 `삭제 이미지 복구`
6. 삭제/편집 상태는 프레임 목록 색상으로 확인

### 4.3 이미지 일부 편집

1. 중앙 이미지에서 마우스로 영역 드래그
2. `영역 가리기` 또는 `이미지로 교체` 실행
3. 편집된 항목이 초록색으로 표시되는지 확인
4. 필요하면 `원본 복구`

### 4.4 AI 학습

1. 메인 화면 오른쪽의 `AI 학습` 클릭
2. Form2에서 학습 범위 선택
3. 실행 환경 선택
4. data/tub 경로와 model 저장 경로 확인
5. VirtualBox 환경이면 원격 `mycar` 경로와 SSH 정보 확인
6. `명령 생성` 클릭
7. `학습 실행` 클릭
8. 콘솔에서 SSH 비밀번호 입력
9. 학습 완료 후 성공률과 피드백 확인

![AI 학습 결과](img/3.png)

---

## 5. 실행 환경 예시

### Windows 환경

Windows에 Python, DonkeyCar, TensorFlow가 설치되어 있을 때 사용합니다.

```bash
python train.py --tub "C:\Users\...\mycar\data" --model "C:\Users\...\mycar\models\mypilot.h5"
```

### WSL 환경

Windows 파일을 WSL 경로로 변환해 사용합니다.

```bash
cd /mnt/c/Users/jh031/Desktop/virtualbox_share/mycar
python train.py --tub data --model models/mypilot.h5
```

### VirtualBox 환경

Windows WinForms에서 VirtualBox Ubuntu로 SSH 접속하여 학습합니다.

```bash
ssh -T -p 2222 xytron@127.0.0.1 "source ~/miniconda3/bin/activate donkey_conda_tf && cd '/media/sf_virtualbox_share/mycar' && python train.py --tub data --model models/mypilot.h5"
```

---

## 6. 주의사항

### 6.1 VirtualBox 경로

Windows에서 보이는 경로와 Ubuntu에서 보이는 경로는 다릅니다.

| 위치 | 예시 |
|---|---|
| Windows | `C:\Users\jh031\Desktop\virtualbox_share\mycar\data` |
| VirtualBox Ubuntu | `/media/sf_virtualbox_share/mycar/data` |

학습 명령은 DonkeyCar가 실제 실행되는 환경 기준 경로를 사용해야 합니다.

### 6.2 config.py 위치

VirtualBox 학습 실행 시 `cd` 위치는 반드시 `config.py`가 있는 `mycar` 폴더여야 합니다.

```bash
cd /media/sf_virtualbox_share/mycar
```

`/media/sf_virtualbox_share`에서 실행하면 `No config file at location: ./config.py` 오류가 발생할 수 있습니다.

### 6.3 깨진 이미지 파일

학습 중 다음 오류가 발생하면 이미지 파일이 손상된 것입니다.

```text
PIL.UnidentifiedImageError: cannot identify image file
```

해결 방법:

- 해당 프레임을 UI에서 삭제 처리
- 삭제된 프레임을 제외하고 학습 데이터 생성
- 또는 손상 이미지 파일을 직접 복구

### 6.4 matplotlib 경고

학습 종료 후 다음 메시지가 떠도 모델 학습 자체는 성공한 것입니다.

```text
problems with loss graph: No module named 'matplotlib'
```

loss 그래프 생성을 원하면 Ubuntu 학습 환경에서 설치합니다.

```bash
conda activate donkey_conda_tf
conda install -c conda-forge matplotlib -y
```

---

## 7. 구현된 비밀 기능 요약

| 비밀 기능 | 구현 위치 | 설명 |
|---|---|---|
| 이상 주행 자동 탐지 | Form1 | 이동평균과 변동성 밴드로 조향 스파이크 탐지 |
| 이미지 일부 가리기/교체 | Form1 | 드래그 영역을 마스킹하거나 다른 이미지로 교체 |
| 상태 기반 색상 표시 | Form1 | 삭제는 빨강, 편집은 초록으로 프레임 목록 표시 |
| 학습 데이터셋 생성 | Form1 + Form2 | 삭제/필터/이상치 조건을 반영한 학습셋 생성 |
| AI 학습 성공률 평가 | Form2 | 모델 생성 여부와 종료 코드를 바탕으로 성공률 및 피드백 표시 |

---

## 8. 프로젝트 목표와 의의

이 프로그램은 단순히 DonkeyCar 학습 명령을 실행하는 도구가 아니라, 학습 데이터의 품질을 높이기 위한 데이터 관리 UI입니다. 수천 장의 주행 이미지를 사람이 모두 확인하지 않아도 이상 주행 후보를 자동으로 찾고, 불량 이미지를 삭제하거나 복구하며, 필요한 경우 이미지 일부를 가려 학습 전처리를 수행할 수 있습니다.

또한 C# WinForms UI와 Python 기반 DonkeyCar 학습 환경을 연결하여, 사용자가 명령어를 직접 외우지 않아도 Windows, WSL, VirtualBox 환경에 맞는 학습 명령을 생성하고 실행할 수 있도록 구성했습니다.

최종적으로 이 프로젝트는 다음 목표를 달성합니다.

- DonkeyCar 학습 데이터 탐색 편의성 향상
- 불량 데이터 정제 시간 단축
- 이미지 전처리 기능 제공
- 이상 주행 자동 탐지 기능 제공
- C# UI와 Python 학습 환경 연동
- 학습 결과를 성공률과 피드백으로 시각화
