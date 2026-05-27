# Designer timeout fix

Visual Studio WinForms 디자이너는 Form 생성자를 실행합니다. 기존 수정본은 생성자에서 `BuildUi()`로 모든 컨트롤을 동적 생성했기 때문에 Designer Host가 오래 걸리거나 멈추면서 `명명된 파이프에 연결하는 동안 제한 시간이 초과되었습니다` 오류가 발생할 수 있었습니다.

수정 내용:

```csharp
if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
{
    BuildUi();
}
```

따라서 디자이너에서는 빈 Form만 안전하게 열리고, 실제 실행 시에는 DonkeyCar UI가 정상 생성됩니다.

삭제 파일 복구 관련: 실행에 필요한 `Program.cs`, `Form1.cs`, `Form1.Designer.cs`, `Form1.resx`, `TeamApp.csproj`, `TeamApp.sln`은 모두 포함되어 있습니다. 이전에 제거한 것은 `.vs`, `bin`, `obj` 같은 Visual Studio 임시/빌드 산출물입니다.
