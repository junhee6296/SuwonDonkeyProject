# 디자이너 무한 로딩 수정

원인: TeamApp.csproj의 TargetFramework가 `net10.0-windows`로 되어 있어, Visual Studio에 .NET 10 SDK가 설치되어 있지 않은 환경에서 WinForms Designer가 프로젝트를 로드하지 못했습니다. 솔루션 탐색기에도 “설치되지 않은 .NET 버전” 경고가 표시됩니다.

수정: TargetFramework를 안정적인 LTS 버전인 `net8.0-windows`로 변경하고, Visual Studio 2022에서 바로 열 수 있도록 표준 `TeamApp.sln`을 추가했습니다.

확인 방법:
1. Visual Studio 2022에서 TeamApp.sln 열기
2. NuGet/SDK 복원이 끝날 때까지 대기
3. Form1.cs 우클릭 → 디자이너 보기

필요 SDK: .NET 8 SDK
