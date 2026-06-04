using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace TeamApp
{
    public partial class Form1 : Form
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false
        };

        private readonly List<FrameRecord> allFrames = new List<FrameRecord>();
        private readonly List<FrameRecord> visibleFrames = new List<FrameRecord>();
        private readonly System.Windows.Forms.Timer autoPlayTimer = new System.Windows.Forms.Timer();

        private string rootFolder = string.Empty;
        private string dataFolder = string.Empty;
        private string imagesFolder = string.Empty;
        private string trainingDataPathOverride = string.Empty;
        private string trainingModelPathOverride = string.Empty;
        private string trainingSshPassword = string.Empty;
        private bool trainingUseSshPasswordInput;
        private bool trainingCommandGenerated;
        private const string TrainingCommandPlaceholder = "먼저 [학습 명령 생성] 버튼으로 실행 환경과 경로를 선택하세요.";
        private const string SshPasswordPlaceholder = "{SSH_PASSWORD}";
        private readonly HashSet<int> checkedFrameOrders = new HashSet<int>();
        private int currentVisibleIndex = -1;

        private string loadedImagePath = string.Empty;
        private Rectangle? selectedImageRect;
        private Point selectionStartImagePoint;
        private bool isSelectingImage;
        private bool imageDirty;

        public Form1()
        {
            InitializeComponent();

            // Visual Studio 디자이너에서 실제 컨트롤이 보이고 편집 가능하도록
            // 컨트롤은 Form1.Designer.cs에서 생성합니다. 런타임 전용 초기화만 여기서 수행합니다.
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            InitializeRuntime();
        }

        private void InitializeRuntime()
        {
            if (cmbMaskMode.Items.Count == 0)
            {
                cmbMaskMode.Items.AddRange(new object[] { "검정", "흰색", "회색", "평균색" });
            }
            cmbMaskMode.SelectedIndex = Math.Min(2, Math.Max(0, cmbMaskMode.Items.Count - 1));
            pnlTimeline.MouseClick += pnlTimeline_MouseClick;
            chkAnomalyOnly.CheckedChanged += chkAnomalyOnly_CheckedChanged;
            chkDeletedOnly.CheckedChanged += chkStatusFilter_CheckedChanged;
            chkEditedOnly.CheckedChanged += chkStatusFilter_CheckedChanged;
            btnClearCheckedFrames.Text = "전체 해제";
            btnDelete.Text = "이미지 삭제";
            btnUndo.Text = "삭제 이미지 복구";
            chkDeletedOnly.Text = "삭제 이미지만";
            chkEditedOnly.Text = "교체/편집만";
            btnCheckDonkey.Visible = false;
            grpTrain.Text = "AI 학습";
            lblCommand.Visible = false;
            chkManualCommandEdit.Visible = false;
            txtTrainCommand.Visible = false;
            btnTrain.Visible = false;
            btnCheckDonkey.Visible = false;
            btnTrainingPaths.Text = "AI 학습";
            lblHint.Text = "AI 학습 버튼을 눌러 학습 환경, 경로, 실행 명령, 학습 결과를 별도 창에서 관리하세요.";
            lstFrames.CheckOnClick = false;
            lstFrames.ResolveVisualState = index => index >= 0 && index < visibleFrames.Count
                ? new FrameListVisualState(visibleFrames[index].Deleted, visibleFrames[index].Edited, visibleFrames[index].IsAnomaly)
                : FrameListVisualState.Normal;
            UpdateAutoPlaySpeedLabel();
            ResetTrainingCommandState();
            Resize += (_, _) => ApplyResponsiveLayout();

            autoPlayTimer.Interval = GetAutoPlayInterval();
            autoPlayTimer.Tick += (_, _) => MoveSelection(1, true);

            DrawPlaceholder(picFrame, "폴더를 열면 이미지가 표시됩니다.\n이미지 편집은 '이미지 / 프레임 탐색 · 편집' 영역에서 사용하세요.");
            DrawPlaceholder(picGraph, "데이터 그래프");
            ApplyResponsiveLayout();
            DrawTimeline();
            UpdateSelectionLabel();
        }

        private void ApplyResponsiveLayout()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime || grpList == null)
            {
                return;
            }

            SuspendLayout();
            try
            {
                var margin = 12;
                var gap = 10;
                var top = 12;
                var topHeight = 30;
                var contentTop = 52;
                var contentHeight = Math.Max(560, ClientSize.Height - contentTop - margin);
                var clientWidth = Math.Max(980, ClientSize.Width);
                var rightWidth = ClampInt((int)(clientWidth * 0.31), 340, 430);
                var leftWidth = ClampInt((int)(clientWidth * 0.23), 260, 360);
                var centerWidth = clientWidth - margin * 2 - gap * 2 - leftWidth - rightWidth;
                if (centerWidth < 440)
                {
                    var deficit = 440 - centerWidth;
                    rightWidth = Math.Max(320, rightWidth - deficit / 2);
                    leftWidth = Math.Max(240, leftWidth - deficit / 2);
                    centerWidth = clientWidth - margin * 2 - gap * 2 - leftWidth - rightWidth;
                }

                btnOpenFolder.SetBounds(margin, top, 96, topHeight);
                btnOpenDataFolder.SetBounds(clientWidth - margin - 210, top, 210, topHeight);
                txtSelectedFolder.SetBounds(btnOpenFolder.Right + gap, top + 3, Math.Max(220, btnOpenDataFolder.Left - btnOpenFolder.Right - gap * 2), 23);

                grpList.SetBounds(margin, contentTop, leftWidth, contentHeight);
                grpPreview.SetBounds(grpList.Right + gap, contentTop, Math.Max(360, centerWidth), contentHeight);
                var rightX = grpPreview.Right + gap;
                grpFilter.SetBounds(rightX, contentTop, rightWidth, 285);
                grpAnomaly.SetBounds(rightX, grpFilter.Bottom + gap, rightWidth, 190);
                grpTrain.SetBounds(rightX, grpAnomaly.Bottom + gap, rightWidth, 120);
                grpLog.SetBounds(rightX, grpTrain.Bottom + gap, rightWidth, Math.Max(100, contentTop + contentHeight - (grpTrain.Bottom + gap)));

                LayoutFrameList();
                LayoutPreviewPanel();
                LayoutFilterPanel();
                LayoutAnomalyPanel();
                LayoutTrainPanel();
                LayoutLogPanel();
            }
            finally
            {
                ResumeLayout(false);
            }
        }

        private void LayoutFrameList()
        {
            var w = grpList.ClientSize.Width;
            var h = grpList.ClientSize.Height;
            var selectionButtonWidth = Math.Max(96, (w - 28) / 2);
            btnCheckAllFrames.SetBounds(10, 24, selectionButtonWidth, 28);
            btnClearCheckedFrames.SetBounds(btnCheckAllFrames.Right + 8, 24, selectionButtonWidth, 28);
            lstFrames.SetBounds(10, 58, Math.Max(120, w - 20), Math.Max(140, h - 150));
            lblStats.SetBounds(10, lstFrames.Bottom + 8, Math.Max(120, w - 20), Math.Max(56, h - lstFrames.Bottom - 14));
            UpdateFrameListHorizontalExtent();
        }

        private void LayoutPreviewPanel()
        {
            var w = grpPreview.ClientSize.Width;
            var h = grpPreview.ClientSize.Height;
            var innerW = Math.Max(320, w - 24);
            var graphHeight = 70;
            var bottomInfoHeight = 176;
            var editHeight = 78;
            var deleteHeight = 64;
            var pictureHeight = Math.Max(190, h - 24 - editHeight - deleteHeight - 22 - 45 - 36 - bottomInfoHeight - graphHeight);

            picFrame.SetBounds(12, 22, innerW, pictureHeight);
            grpImageEdit.SetBounds(12, picFrame.Bottom + 8, innerW, editHeight);
            lblEditHint.SetBounds(10, 20, Math.Max(120, grpImageEdit.ClientSize.Width - 20), 18);
            cmbMaskMode.SetBounds(10, 44, 80, 23);
            var btnY = 42;
            var buttonW = Math.Max(80, (grpImageEdit.ClientSize.Width - 110) / 4);
            btnMaskRegion.SetBounds(98, btnY, buttonW, 27);
            btnReplaceRegion.SetBounds(btnMaskRegion.Right + 6, btnY, buttonW + 10, 27);
            btnClearSelection.SetBounds(btnReplaceRegion.Right + 6, btnY, buttonW, 27);
            btnRestoreImage.SetBounds(btnClearSelection.Right + 6, btnY, Math.Max(90, grpImageEdit.ClientSize.Width - btnClearSelection.Right - 16), 27);

            grpDeleteOps.SetBounds(12, grpImageEdit.Bottom + 8, innerW, deleteHeight);
            var deleteButtonW = Math.Max(120, (grpDeleteOps.ClientSize.Width - 30) / 2);
            btnDelete.SetBounds(10, 24, deleteButtonW, 28);
            btnUndo.SetBounds(btnDelete.Right + 10, 24, deleteButtonW, 28);

            pnlTimeline.SetBounds(12, grpDeleteOps.Bottom + 8, innerW, 18);
            trbFrame.SetBounds(12, pnlTimeline.Bottom + 2, innerW, 45);

            var btnTop = trbFrame.Bottom + 2;
            var gap = 8;
            var buttonWidth = Math.Max(54, (innerW - gap * 3) / 4);
            btnPrev.SetBounds(12, btnTop, buttonWidth, 30);
            btnPlay.SetBounds(btnPrev.Right + gap, btnTop, buttonWidth, 30);
            btnNext.SetBounds(btnPlay.Right + gap, btnTop, buttonWidth, 30);
            btnSave.SetBounds(btnNext.Right + gap, btnTop, buttonWidth, 30);

            var speedTop = btnTop + 36;
            lblPlaySpeed.SetBounds(btnPlay.Left, speedTop + 4, Math.Max(110, buttonWidth), 20);
            trbPlaySpeed.SetBounds(lblPlaySpeed.Right + 6, speedTop, Math.Max(120, btnSave.Right - lblPlaySpeed.Right - 6), 35);

            var infoTop = speedTop + 38;
            lblCurrentIndex.SetBounds(12, infoTop, innerW, 20);
            lblCurrentImage.SetBounds(12, infoTop + 23, innerW, 20);
            lblCurrentMode.SetBounds(12, infoTop + 46, innerW, 20);
            lblAngle.SetBounds(12, infoTop + 72, 70, 20);
            txtAngle.SetBounds(86, infoTop + 69, Math.Max(100, innerW / 4), 23);
            lblThrottle.SetBounds(txtAngle.Right + 24, infoTop + 72, 80, 20);
            txtThrottle.SetBounds(lblThrottle.Right + 8, infoTop + 69, Math.Max(100, innerW / 4), 23);
            lblGraph.SetBounds(12, infoTop + 100, innerW, 20);
            picGraph.SetBounds(12, infoTop + 123, innerW, Math.Max(50, grpPreview.ClientSize.Height - (infoTop + 135)));

            DrawGraph();
            DrawTimeline();
        }

        private void LayoutFilterPanel()
        {
            var w = grpFilter.ClientSize.Width;
            var valueLeft = Math.Max(140, w - 230);
            var maxLeft = Math.Max(valueLeft + 110, w - 108);

            chkThrottlePositive.SetBounds(14, 28, 170, 24);
            chkExcludeAngleZero.SetBounds(14, 56, 120, 24);
            chkAngleRange.SetBounds(14, 90, 110, 24);
            chkThrottleRange.SetBounds(14, 124, 120, 24);
            chkAnomalyOnly.SetBounds(14, 156, 170, 24);
            chkDeletedOnly.SetBounds(14, 186, 170, 24);
            chkEditedOnly.SetBounds(14, 216, 170, 24);

            lblRangeMin.SetBounds(valueLeft, 72, 50, 18);
            lblRangeMax.SetBounds(maxLeft, 72, 50, 18);
            numAngleMin.SetBounds(valueLeft, 90, 95, 23);
            numAngleMax.SetBounds(maxLeft, 90, 95, 23);
            numThrottleMin.SetBounds(valueLeft, 124, 95, 23);
            numThrottleMax.SetBounds(maxLeft, 124, 95, 23);
            btnApplyFilter.SetBounds(14, grpFilter.ClientSize.Height - 40, 100, 30);
            btnClearFilter.SetBounds(122, grpFilter.ClientSize.Height - 40, 110, 30);
        }

        private void LayoutAnomalyPanel()
        {
            var w = grpAnomaly.ClientSize.Width;
            lblAnomalyWindow.SetBounds(14, 28, 120, 20);
            numAnomalyWindow.SetBounds(140, 26, 80, 23);
            lblAnomalySigma.SetBounds(230, 28, 60, 20);
            numAnomalySigma.SetBounds(Math.Min(292, w - 76), 26, 65, 23);
            btnAnalyzeAnomaly.SetBounds(14, 62, 120, 30);
            btnClearAnomaly.SetBounds(144, 62, 110, 30);
            btnNextAnomaly.SetBounds(264, 62, Math.Max(88, w - 278), 30);
            lblAnomalyStatus.SetBounds(14, 104, Math.Max(180, w - 28), 22);
            lblAnomalyHint.SetBounds(14, 130, Math.Max(180, w - 28), Math.Max(40, grpAnomaly.ClientSize.Height - 136));
        }

        private void LayoutTrainPanel()
        {
            var w = grpTrain.ClientSize.Width;
            lblCommand.Visible = false;
            chkManualCommandEdit.Visible = false;
            txtTrainCommand.Visible = false;
            btnTrain.Visible = false;
            btnCheckDonkey.Visible = false;

            var buttonWidth = Math.Max(160, Math.Min(260, w - 28));
            btnTrainingPaths.Text = "AI 학습";
            btnTrainingPaths.SetBounds(14, 28, buttonWidth, 38);
            lblHint.SetBounds(14, 74, Math.Max(180, w - 28), Math.Max(34, grpTrain.ClientSize.Height - 82));
            lblHint.Text = "학습 환경/경로/명령 실행/성공률은 AI 학습 창에서 관리합니다.";
        }

        private void LayoutLogPanel()
        {
            txtLog.SetBounds(10, 22, Math.Max(180, grpLog.ClientSize.Width - 20), Math.Max(60, grpLog.ClientSize.Height - 32));
        }

        private static int ClampInt(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        private void btnOpenFolder_Click(object? sender, EventArgs e)
        {
            using var dlg = new FolderBrowserDialog
            {
                Description = "DonkeyCar mycar 폴더, data 폴더, 또는 tub 폴더를 선택하세요",
                UseDescriptionForTitle = true
            };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                LoadDataset(dlg.SelectedPath);
            }
        }

        private void btnOpenDataFolder_Click(object? sender, EventArgs e)
        {
            var folder = Directory.Exists(dataFolder) ? dataFolder : rootFolder;
            if (!Directory.Exists(folder))
            {
                MessageBox.Show("먼저 데이터를 불러오세요.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = folder,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"폴더를 여는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDataset(string selectedPath)
        {
            if (!TryResolveDonkeyFolder(selectedPath, out var resolvedRoot, out var resolvedData, out var resolvedImages, out var message))
            {
                MessageBox.Show(message, "데이터 폴더 확인", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            rootFolder = resolvedRoot;
            dataFolder = resolvedData;
            imagesFolder = resolvedImages;
            txtSelectedFolder.Text = selectedPath;
            RefreshTrainingPathDefaults(false);
            allFrames.Clear();
            visibleFrames.Clear();
            checkedFrameOrders.Clear();
            currentVisibleIndex = -1;
            loadedImagePath = string.Empty;
            selectedImageRect = null;
            imageDirty = false;

            try
            {
                var catalogs = GetCatalogFiles(dataFolder);
                var globalOrder = 0;

                foreach (var catalogPath in catalogs)
                {
                    var records = ReadCatalogRecords(catalogPath, globalOrder, out globalOrder);
                    allFrames.AddRange(records);
                }

                if (allFrames.Count == 0)
                {
                    // 빈 데이터셋을 새로 불러올 때 이전 프레임 목록/트랙바/그래프가 남지 않도록
                    // 필터 적용 경로를 한 번 태워 모든 UI 상태를 일관되게 초기화한다.
                    ApplyFilters(null);
                    MessageBox.Show("catalog 파일에서 유효한 프레임 데이터를 찾지 못했습니다.", "데이터 없음", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                allFrames.Sort((a, b) => a.GlobalOrder.CompareTo(b.GlobalOrder));
                LoadUiMarks();
                DetectAnomalies(false);
                ApplyFilters(allFrames[0].GlobalOrder);
                AppendLog($"데이터 로드 완료: catalog {catalogs.Length}개, frame {allFrames.Count}개");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터를 불러오는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static bool TryResolveDonkeyFolder(string selectedPath, out string resolvedRoot, out string resolvedData, out string resolvedImages, out string message)
        {
            resolvedRoot = selectedPath;
            resolvedData = string.Empty;
            resolvedImages = string.Empty;
            message = string.Empty;

            if (!Directory.Exists(selectedPath))
            {
                message = "선택한 폴더가 존재하지 않습니다.";
                return false;
            }

            var candidates = new List<(string Root, string Data, string Images)>();

            var dataUnderRoot = Path.Combine(selectedPath, "data");
            candidates.Add((selectedPath, dataUnderRoot, Path.Combine(dataUnderRoot, "images")));
            candidates.Add((Directory.GetParent(selectedPath)?.FullName ?? selectedPath, selectedPath, Path.Combine(selectedPath, "images")));

            if (string.Equals(Path.GetFileName(selectedPath), "images", StringComparison.OrdinalIgnoreCase))
            {
                var parentData = Directory.GetParent(selectedPath)?.FullName ?? selectedPath;
                var parentRoot = Directory.GetParent(parentData)?.FullName ?? parentData;
                candidates.Add((parentRoot, parentData, selectedPath));
            }

            candidates.Add((Directory.GetParent(selectedPath)?.FullName ?? selectedPath, selectedPath, selectedPath));

            foreach (var candidate in candidates)
            {
                if (Directory.Exists(candidate.Data) && Directory.Exists(candidate.Images) && GetCatalogFiles(candidate.Data).Length > 0)
                {
                    resolvedRoot = candidate.Root;
                    resolvedData = candidate.Data;
                    resolvedImages = candidate.Images;
                    return true;
                }
            }

            message = "catalog_*.catalog/catalog.json 파일과 images 폴더를 함께 찾지 못했습니다.\n" +
                      "mycar 루트, mycar/data, 또는 catalog와 images가 있는 tub 폴더를 선택해 주세요.";
            return false;
        }

        private static string[] GetCatalogFiles(string folder)
        {
            if (!Directory.Exists(folder))
            {
                return Array.Empty<string>();
            }

            var files = new List<string>();
            foreach (var pattern in new[] { "catalog_*.catalog", "catalog_*.json", "catalog.catalog", "catalog.json" })
            {
                files.AddRange(Directory.GetFiles(folder, pattern));
            }

            return files
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(CatalogSortKey)
                .ThenBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private static int CatalogSortKey(string path)
        {
            var name = Path.GetFileName(path);
            var match = Regex.Match(name, @"catalog_(\d+)", RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var number))
            {
                return number;
            }

            return int.MaxValue;
        }

        private static List<FrameRecord> ReadCatalogRecords(string catalogPath, int startOrder, out int nextOrder)
        {
            var records = new List<FrameRecord>();
            var order = startOrder;
            var text = File.ReadAllText(catalogPath, Encoding.UTF8);

            if (string.IsNullOrWhiteSpace(text))
            {
                nextOrder = order;
                return records;
            }

            if (text.TrimStart().StartsWith("[", StringComparison.Ordinal))
            {
                try
                {
                    using var document = JsonDocument.Parse(text);
                    var itemNumber = 0;
                    foreach (var item in document.RootElement.EnumerateArray())
                    {
                        itemNumber++;
                        if (TryParseCatalogLine(item.GetRawText(), catalogPath, itemNumber, order, out var record))
                        {
                            records.Add(record);
                            order++;
                        }
                    }
                }
                catch
                {
                    // 배열 형식 catalog 파싱 실패 시 줄 단위 파싱으로 다시 시도한다.
                }
            }

            if (records.Count == 0)
            {
                var lineNumber = 0;
                foreach (var rawLine in text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
                {
                    lineNumber++;
                    var line = rawLine.Trim();
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    if (TryParseCatalogLine(line, catalogPath, lineNumber, order, out var record))
                    {
                        records.Add(record);
                        order++;
                    }
                }
            }

            nextOrder = order;
            return records;
        }

        private static bool TryParseCatalogLine(string line, string catalogPath, int lineNumber, int globalOrder, out FrameRecord record)
        {
            record = new FrameRecord();

            try
            {
                using var document = JsonDocument.Parse(line);
                var root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                {
                    return false;
                }

                var imageFile = GetString(root, "cam/image_array", "cam/image_array_", "image", "image_file", "image_path");
                if (string.IsNullOrWhiteSpace(imageFile))
                {
                    return false;
                }

                record = new FrameRecord
                {
                    GlobalOrder = globalOrder,
                    OriginalLineNumber = lineNumber,
                    CatalogPath = catalogPath,
                    RawJson = line,
                    Index = GetNullableInt(root, "_index") ?? globalOrder,
                    SessionId = GetString(root, "_session_id"),
                    TimestampMs = GetNullableLong(root, "_timestamp_ms"),
                    ImageFile = imageFile,
                    Angle = GetNullableDouble(root, "user/angle", "pilot/angle"),
                    Throttle = GetNullableDouble(root, "user/throttle", "pilot/throttle"),
                    Mode = GetString(root, "user/mode", "pilot/mode")
                };
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string GetString(JsonElement root, params string[] names)
        {
            foreach (var name in names)
            {
                if (!root.TryGetProperty(name, out var value))
                {
                    continue;
                }

                return value.ValueKind switch
                {
                    JsonValueKind.String => value.GetString() ?? string.Empty,
                    JsonValueKind.Number => value.GetRawText(),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    _ => string.Empty
                };
            }

            return string.Empty;
        }

        private static int? GetNullableInt(JsonElement root, string name)
        {
            if (!root.TryGetProperty(name, out var value))
            {
                return null;
            }

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
            {
                return number;
            }

            if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
            {
                return number;
            }

            return null;
        }

        private static long? GetNullableLong(JsonElement root, string name)
        {
            if (!root.TryGetProperty(name, out var value))
            {
                return null;
            }

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var number))
            {
                return number;
            }

            if (value.ValueKind == JsonValueKind.String && long.TryParse(value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
            {
                return number;
            }

            return null;
        }

        private static double? GetNullableDouble(JsonElement root, params string[] names)
        {
            foreach (var name in names)
            {
                if (!root.TryGetProperty(name, out var value))
                {
                    continue;
                }

                if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var number))
                {
                    return number;
                }

                if (value.ValueKind == JsonValueKind.String &&
                    (double.TryParse(value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out number) ||
                     double.TryParse(value.GetString(), NumberStyles.Float, CultureInfo.CurrentCulture, out number)))
                {
                    return number;
                }
            }

            return null;
        }

        private void btnApplyFilter_Click(object? sender, EventArgs e)
        {
            ApplyFilters(CurrentRecord()?.GlobalOrder);
        }

        private void ApplyFilters(int? preferredGlobalOrder = null)
        {
            var previousGlobalOrder = preferredGlobalOrder ?? CurrentRecord()?.GlobalOrder;

            PreserveCheckedOrders();
            visibleFrames.Clear();
            visibleFrames.AddRange(allFrames.Where(PassesFilter).OrderBy(record => record.GlobalOrder));

            lstFrames.BeginUpdate();
            lstFrames.Items.Clear();
            foreach (var record in visibleFrames)
            {
                lstFrames.Items.Add(ToListText(record), checkedFrameOrders.Contains(record.GlobalOrder));
            }
            lstFrames.EndUpdate();
            UpdateFrameListHorizontalExtent();
            lstFrames.Invalidate();
            lstFrames.Refresh();

            trbFrame.Minimum = 0;
            trbFrame.Maximum = Math.Max(0, visibleFrames.Count - 1);
            trbFrame.Value = 0;

            UpdateStats();
            DrawGraph();
            DrawTimeline();

            if (visibleFrames.Count == 0)
            {
                ClearCurrentFrame();
                return;
            }

            var selectedIndex = 0;
            if (previousGlobalOrder.HasValue)
            {
                var found = visibleFrames.FindIndex(record => record.GlobalOrder == previousGlobalOrder.Value);
                if (found >= 0)
                {
                    selectedIndex = found;
                }
            }

            lstFrames.SelectedIndex = selectedIndex;
        }

        private bool PassesFilter(FrameRecord record)
        {
            if (chkDeletedOnly.Checked || chkEditedOnly.Checked)
            {
                var matchesStatusFilter =
                    (chkDeletedOnly.Checked && record.Deleted) ||
                    (chkEditedOnly.Checked && record.Edited);
                if (!matchesStatusFilter)
                {
                    return false;
                }
            }

            if (chkAnomalyOnly.Checked && !record.IsAnomaly)
            {
                return false;
            }

            if (chkThrottlePositive.Checked && (!record.Throttle.HasValue || record.Throttle.Value <= 0))
            {
                return false;
            }

            if (chkExcludeAngleZero.Checked && (!record.Angle.HasValue || Math.Abs(record.Angle.Value) < 0.000001))
            {
                return false;
            }

            if (chkAngleRange.Checked)
            {
                if (!record.Angle.HasValue)
                {
                    return false;
                }

                var min = (double)numAngleMin.Value;
                var max = (double)numAngleMax.Value;
                if (record.Angle.Value < Math.Min(min, max) || record.Angle.Value > Math.Max(min, max))
                {
                    return false;
                }
            }

            if (chkThrottleRange.Checked)
            {
                if (!record.Throttle.HasValue)
                {
                    return false;
                }

                var min = (double)numThrottleMin.Value;
                var max = (double)numThrottleMax.Value;
                if (record.Throttle.Value < Math.Min(min, max) || record.Throttle.Value > Math.Max(min, max))
                {
                    return false;
                }
            }

            return true;
        }

        private void btnClearFilter_Click(object? sender, EventArgs e)
        {
            chkThrottlePositive.Checked = false;
            chkExcludeAngleZero.Checked = false;
            chkAngleRange.Checked = false;
            chkThrottleRange.Checked = false;
            chkAnomalyOnly.Checked = false;
            chkDeletedOnly.Checked = false;
            chkEditedOnly.Checked = false;
            numAngleMin.Value = -1m;
            numAngleMax.Value = 1m;
            numThrottleMin.Value = 0m;
            numThrottleMax.Value = 1m;
            ApplyFilters(CurrentRecord()?.GlobalOrder);
        }

        private void chkAnomalyOnly_CheckedChanged(object? sender, EventArgs e)
        {
            ApplyFilters(CurrentRecord()?.GlobalOrder);
        }

        private void chkStatusFilter_CheckedChanged(object? sender, EventArgs e)
        {
            ApplyFilters(CurrentRecord()?.GlobalOrder);
        }

        private void PreserveCheckedOrders()
        {
            for (var i = 0; i < visibleFrames.Count && i < lstFrames.Items.Count; i++)
            {
                if (lstFrames.GetItemChecked(i))
                {
                    checkedFrameOrders.Add(visibleFrames[i].GlobalOrder);
                }
                else
                {
                    checkedFrameOrders.Remove(visibleFrames[i].GlobalOrder);
                }
            }
        }

        private List<FrameRecord> GetCheckedRecords()
        {
            PreserveCheckedOrders();
            return visibleFrames
                .Where(record => checkedFrameOrders.Contains(record.GlobalOrder))
                .OrderBy(record => record.GlobalOrder)
                .ToList();
        }

        private List<FrameRecord> GetTargetRecordsForBatch(bool includeCurrentFallback = true)
        {
            var checkedRecords = GetCheckedRecords();
            if (checkedRecords.Count > 0)
            {
                return checkedRecords;
            }

            var current = includeCurrentFallback ? CurrentRecord() : null;
            return current == null ? new List<FrameRecord>() : new List<FrameRecord> { current };
        }

        private void btnCheckAllFrames_Click(object? sender, EventArgs e)
        {
            for (var i = 0; i < lstFrames.Items.Count; i++)
            {
                lstFrames.SetItemChecked(i, true);
                if (i < visibleFrames.Count)
                {
                    checkedFrameOrders.Add(visibleFrames[i].GlobalOrder);
                }
            }
        }

        private void btnClearCheckedFrames_Click(object? sender, EventArgs e)
        {
            for (var i = 0; i < lstFrames.Items.Count; i++)
            {
                lstFrames.SetItemChecked(i, false);
            }
            checkedFrameOrders.Clear();
        }

        private void UpdateFrameListHorizontalExtent()
        {
            if (lstFrames.Items.Count == 0)
            {
                lstFrames.HorizontalExtent = 0;
                return;
            }

            var maxWidth = 0;
            foreach (var item in lstFrames.Items)
            {
                var text = item?.ToString() ?? string.Empty;
                maxWidth = Math.Max(maxWidth, TextRenderer.MeasureText(text, lstFrames.Font).Width);
            }

            lstFrames.HorizontalExtent = Math.Max(lstFrames.ClientSize.Width, maxWidth + 24);
        }

        private void lstFrames_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= lstFrames.Items.Count)
            {
                return;
            }

            var record = e.Index < visibleFrames.Count ? visibleFrames[e.Index] : null;
            var selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            var backColor = selected ? SystemColors.Highlight : e.BackColor;
            var foreColor = selected ? SystemColors.HighlightText : e.ForeColor;

            if (record != null)
            {
                if (record.Deleted)
                {
                    backColor = selected ? Color.IndianRed : Color.LightCoral;
                    foreColor = selected ? Color.White : Color.DarkRed;
                }
                else if (record.Edited)
                {
                    backColor = selected ? Color.SeaGreen : Color.LightGreen;
                    foreColor = selected ? Color.White : Color.DarkGreen;
                }
                else if (!selected && record.IsAnomaly)
                {
                    foreColor = Color.Red;
                }
            }

            using (var brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            var isChecked = lstFrames.GetItemChecked(e.Index);
            var checkState = isChecked ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal;
            CheckBoxRenderer.DrawCheckBox(e.Graphics, new Point(e.Bounds.Left + 3, e.Bounds.Top + 2), checkState);

            var text = lstFrames.Items[e.Index]?.ToString() ?? string.Empty;
            var textRect = new Rectangle(e.Bounds.Left + 24, e.Bounds.Top, Math.Max(10, e.Bounds.Width - 26), e.Bounds.Height);
            TextRenderer.DrawText(
                e.Graphics,
                text,
                e.Font ?? Font,
                textRect,
                foreColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
            e.DrawFocusRectangle();
        }

        private void lstFrames_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (lstFrames.SelectedIndex >= 0)
            {
                ShowFrame(lstFrames.SelectedIndex);
            }
        }

        private void trbFrame_Scroll(object? sender, EventArgs e)
        {
            if (trbFrame.Value >= 0 && trbFrame.Value < visibleFrames.Count)
            {
                lstFrames.SelectedIndex = trbFrame.Value;
            }
        }

        private void btnPrev_Click(object? sender, EventArgs e)
        {
            MoveSelection(-1, false);
        }

        private void btnNext_Click(object? sender, EventArgs e)
        {
            MoveSelection(1, false);
        }

        private void MoveSelection(int delta, bool wrap)
        {
            if (visibleFrames.Count == 0)
            {
                return;
            }

            var index = lstFrames.SelectedIndex;
            if (index < 0)
            {
                index = 0;
            }
            else
            {
                index += delta;
            }

            if (wrap)
            {
                if (index < 0)
                {
                    index = visibleFrames.Count - 1;
                }
                else if (index >= visibleFrames.Count)
                {
                    index = 0;
                }
            }
            else
            {
                index = Math.Max(0, Math.Min(visibleFrames.Count - 1, index));
            }

            lstFrames.SelectedIndex = index;
        }

        private int GetAutoPlayInterval()
        {
            var value = trbPlaySpeed == null ? 4 : Math.Max(1, trbPlaySpeed.Value);
            // 오른쪽으로 갈수록 빠르게 재생: 1=1000ms, 20=50ms
            return Math.Max(50, 1050 - value * 50);
        }

        private void UpdateAutoPlaySpeedLabel()
        {
            if (lblPlaySpeed == null)
            {
                return;
            }

            var interval = GetAutoPlayInterval();
            lblPlaySpeed.Text = $"재생속도 {interval}ms";
            autoPlayTimer.Interval = interval;
        }

        private void trbPlaySpeed_Scroll(object? sender, EventArgs e)
        {
            UpdateAutoPlaySpeedLabel();
        }

        private void btnPlay_Click(object? sender, EventArgs e)
        {
            if (visibleFrames.Count == 0)
            {
                MessageBox.Show("자동 재생할 데이터가 없습니다.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            autoPlayTimer.Enabled = !autoPlayTimer.Enabled;
            btnPlay.Text = autoPlayTimer.Enabled ? "재생 중지" : "자동 재생";
        }

        private void ShowFrame(int visibleIndex)
        {
            if (visibleIndex < 0 || visibleIndex >= visibleFrames.Count)
            {
                ClearCurrentFrame();
                return;
            }

            currentVisibleIndex = visibleIndex;
            var record = visibleFrames[visibleIndex];

            var anomalyText = record.IsAnomaly ? $"  [이상 score={record.AnomalyScore:0.##}]" : string.Empty;
            lblCurrentIndex.Text = $"현재 인덱스: {record.Index}  ({visibleIndex + 1}/{visibleFrames.Count}){anomalyText}";
            lblCurrentImage.Text = $"이미지: {record.ImageFile}";
            lblCurrentMode.Text = $"mode/catalog: {NullIfEmpty(record.Mode)} / {Path.GetFileName(record.CatalogPath)}";
            txtAngle.Text = FormatForInput(record.Angle);
            txtThrottle.Text = FormatForInput(record.Throttle);

            if (trbFrame.Maximum >= visibleIndex)
            {
                trbFrame.Value = visibleIndex;
            }

            LoadImage(record);
            DrawTimeline();
        }

        private void LoadImage(FrameRecord record)
        {
            var imagePath = ResolveImagePath(record);
            loadedImagePath = imagePath;
            selectedImageRect = null;
            isSelectingImage = false;
            imageDirty = false;
            UpdateSelectionLabel();

            if (!File.Exists(imagePath))
            {
                loadedImagePath = string.Empty;
                DrawPlaceholder(picFrame, "이미지 파일을 찾을 수 없습니다.\n" + imagePath);
                return;
            }

            try
            {
                using var stream = File.OpenRead(imagePath);
                using var source = Image.FromStream(stream);
                var copy = new Bitmap(source);
                ReplaceFrameImage(copy);
            }
            catch (Exception ex)
            {
                loadedImagePath = string.Empty;
                DrawPlaceholder(picFrame, "이미지를 여는 중 오류가 발생했습니다.\n" + ex.Message);
            }
        }

        private void ReplaceFrameImage(Bitmap image)
        {
            var oldImage = picFrame.Image;
            picFrame.Image = image;
            oldImage?.Dispose();
            picFrame.Invalidate();
        }

        private string ResolveImagePath(FrameRecord record)
        {
            var originalPath = ResolveOriginalImagePath(record);
            if (File.Exists(originalPath))
            {
                return originalPath;
            }

            if (record.Deleted)
            {
                var backupPath = ResolveDeletedBackupPath(record);
                if (!string.IsNullOrWhiteSpace(backupPath) && File.Exists(backupPath))
                {
                    return backupPath;
                }
            }

            return originalPath;
        }

        private string ResolveOriginalImagePath(FrameRecord record)
        {
            if (string.IsNullOrWhiteSpace(record.ImageFile))
            {
                return string.Empty;
            }

            var imageFile = record.ImageFile.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            if (Path.IsPathRooted(imageFile))
            {
                return imageFile;
            }

            var dataRelative = Path.Combine(dataFolder, imageFile);
            if (File.Exists(dataRelative))
            {
                return dataRelative;
            }

            var imagesPrefix = "images" + Path.DirectorySeparatorChar;
            if (imageFile.StartsWith(imagesPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return Path.Combine(dataFolder, imageFile);
            }

            return Path.Combine(imagesFolder, imageFile);
        }

        private void ClearCurrentFrame()
        {
            currentVisibleIndex = -1;
            loadedImagePath = string.Empty;
            selectedImageRect = null;
            imageDirty = false;
            lblCurrentIndex.Text = "현재 인덱스: -";
            lblCurrentImage.Text = "이미지: -";
            lblCurrentMode.Text = "mode/catalog: -";
            txtAngle.Text = string.Empty;
            txtThrottle.Text = string.Empty;
            DrawPlaceholder(picFrame, "표시할 프레임이 없습니다.");
            UpdateSelectionLabel();
            DrawTimeline();
        }

        private FrameRecord? CurrentRecord()
        {
            if (currentVisibleIndex < 0 || currentVisibleIndex >= visibleFrames.Count)
            {
                return null;
            }

            return visibleFrames[currentVisibleIndex];
        }

        private void btnSave_Click(object? sender, EventArgs e)
        {
            var record = CurrentRecord();
            if (record == null)
            {
                MessageBox.Show("저장할 프레임을 선택하세요.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!TryParseNullableDouble(txtAngle.Text, out var angle))
            {
                MessageBox.Show("angle 값은 숫자여야 합니다.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAngle.Focus();
                return;
            }

            if (!TryParseNullableDouble(txtThrottle.Text, out var throttle))
            {
                MessageBox.Show("throttle 값은 숫자여야 합니다.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtThrottle.Focus();
                return;
            }

            var oldAngle = record.Angle;
            var oldThrottle = record.Throttle;

            try
            {
                record.Angle = angle;
                record.Throttle = throttle;
                RewriteCatalogs();
                RecalculateAnomaliesIfNeeded();
                ApplyFilters(record.GlobalOrder);
                AppendLog($"값 저장: index={record.Index}, angle={FormatForInput(angle)}, throttle={FormatForInput(throttle)}");
            }
            catch (Exception ex)
            {
                record.Angle = oldAngle;
                record.Throttle = oldThrottle;
                MessageBox.Show($"저장 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object? sender, EventArgs e)
        {
            var targets = GetTargetRecordsForBatch()
                .Where(record => !record.Deleted)
                .ToList();
            if (targets.Count == 0)
            {
                MessageBox.Show("삭제할 프레임을 체크하거나 선택하세요.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"선택한 {targets.Count}개 프레임의 이미지 파일을 _deleted_backup 폴더로 이동할까요?\n프레임 목록에서는 빨간색으로 남겨두며, 이미지 복구 버튼으로 되돌릴 수 있습니다.",
                "이미지 삭제 확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            var success = 0;
            foreach (var record in targets)
            {
                try
                {
                    if (MoveImageToDeletedBackup(record))
                    {
                        success++;
                    }
                }
                catch (Exception ex)
                {
                    AppendLog($"이미지 삭제 실패: {record.ImageFile} / {ex.Message}");
                }
            }

            SaveUiMarks();
            RecalculateAnomaliesIfNeeded();
            ApplyFilters(targets[0].GlobalOrder);
            lstFrames.Invalidate();
            AppendLog($"이미지 삭제: {success}/{targets.Count}개를 _deleted_backup으로 이동");
        }

        private void btnUndo_Click(object? sender, EventArgs e)
        {
            var targets = GetTargetRecordsForBatch()
                .Where(record => record.Deleted)
                .ToList();

            if (targets.Count == 0)
            {
                MessageBox.Show("복구할 삭제 프레임을 체크하거나 선택하세요.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var success = 0;
            foreach (var record in targets)
            {
                try
                {
                    if (RestoreImageFromDeletedBackup(record))
                    {
                        success++;
                    }
                }
                catch (Exception ex)
                {
                    AppendLog($"이미지 복구 실패: {record.ImageFile} / {ex.Message}");
                }
            }

            SaveUiMarks();
            RecalculateAnomaliesIfNeeded();
            ApplyFilters(targets[0].GlobalOrder);
            lstFrames.Invalidate();
            AppendLog($"이미지 복구: {success}/{targets.Count}개");
        }

        private bool MoveImageToDeletedBackup(FrameRecord record)
        {
            var sourcePath = ResolveOriginalImagePath(record);
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                throw new InvalidOperationException("이미지 경로가 비어 있습니다.");
            }

            var backupPath = GetDeletedBackupPath(sourcePath);
            if (!File.Exists(sourcePath))
            {
                if (File.Exists(backupPath))
                {
                    record.Deleted = true;
                    record.DeletedBackupPath = backupPath;
                    return false;
                }

                throw new FileNotFoundException("원본 이미지와 삭제 백업을 모두 찾을 수 없습니다.", sourcePath);
            }

            var backupDirectory = Path.GetDirectoryName(backupPath);
            if (!string.IsNullOrWhiteSpace(backupDirectory))
            {
                Directory.CreateDirectory(backupDirectory);
            }

            File.Move(sourcePath, backupPath, true);
            record.Deleted = true;
            record.DeletedBackupPath = backupPath;
            return true;
        }

        private bool RestoreImageFromDeletedBackup(FrameRecord record)
        {
            var targetPath = ResolveOriginalImagePath(record);
            if (string.IsNullOrWhiteSpace(targetPath))
            {
                throw new InvalidOperationException("복구 대상 이미지 경로가 비어 있습니다.");
            }

            var backupPath = ResolveDeletedBackupPath(record);
            if (string.IsNullOrWhiteSpace(backupPath) || !File.Exists(backupPath))
            {
                if (File.Exists(targetPath))
                {
                    record.Deleted = false;
                    record.DeletedBackupPath = string.Empty;
                    return false;
                }

                throw new FileNotFoundException("삭제 백업 이미지를 찾을 수 없습니다.", backupPath);
            }

            var targetDirectory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            File.Move(backupPath, targetPath, true);
            record.Deleted = false;
            record.DeletedBackupPath = string.Empty;
            return true;
        }

        private string GetUiMarksPath()
        {
            return string.IsNullOrWhiteSpace(dataFolder) ? string.Empty : Path.Combine(dataFolder, "_ui_marks.json");
        }

        private static string GetFrameMarkKey(FrameRecord record)
        {
            return string.IsNullOrWhiteSpace(record.ImageFile) ? record.GlobalOrder.ToString(CultureInfo.InvariantCulture) : record.ImageFile;
        }

        private void LoadUiMarks()
        {
            var path = GetUiMarksPath();
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return;
            }

            try
            {
                var root = JsonNode.Parse(File.ReadAllText(path, Encoding.UTF8)) as JsonObject;
                if (root == null)
                {
                    return;
                }

                var deleted = ReadStringSet(root["deleted"] as JsonArray);
                var edited = ReadStringSet(root["edited"] as JsonArray);
                var deletedBackups = root["deletedBackups"] as JsonObject;
                foreach (var record in allFrames)
                {
                    var key = GetFrameMarkKey(record);
                    record.Deleted = deleted.Contains(key);
                    record.Edited = edited.Contains(key);
                    if (!record.Deleted)
                    {
                        var imagePath = ResolveImagePath(record);
                        if (!File.Exists(imagePath) && File.Exists(GetDeletedBackupPath(imagePath)))
                        {
                            record.Deleted = true;
                        }
                    }
                    record.DeletedBackupPath = deletedBackups != null && deletedBackups.TryGetPropertyValue(key, out var backupNode)
                        ? backupNode?.GetValue<string>() ?? string.Empty
                        : string.Empty;

                    if (record.Deleted && string.IsNullOrWhiteSpace(record.DeletedBackupPath))
                    {
                        record.DeletedBackupPath = ResolveDeletedBackupPath(record);
                    }
                }
            }
            catch (Exception ex)
            {
                AppendLog("UI 표시 상태 파일을 읽지 못했습니다: " + ex.Message);
            }
        }

        private static HashSet<string> ReadStringSet(JsonArray? array)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (array == null)
            {
                return result;
            }

            foreach (var item in array)
            {
                var value = item?.GetValue<string>();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    result.Add(value);
                }
            }

            return result;
        }

        private void SaveUiMarks()
        {
            var path = GetUiMarksPath();
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var deleted = new JsonArray();
            var deletedBackups = new JsonObject();
            foreach (var record in allFrames.Where(record => record.Deleted).OrderBy(record => record.GlobalOrder))
            {
                var key = GetFrameMarkKey(record);
                deleted.Add(key);
                var backupPath = ResolveDeletedBackupPath(record);
                if (!string.IsNullOrWhiteSpace(backupPath))
                {
                    deletedBackups[key] = MakeUiMarkPathValue(backupPath);
                }
            }

            var edited = new JsonArray();
            foreach (var record in allFrames.Where(record => record.Edited).OrderBy(record => record.GlobalOrder))
            {
                edited.Add(GetFrameMarkKey(record));
            }

            var root = new JsonObject
            {
                ["deleted"] = deleted,
                ["deletedBackups"] = deletedBackups,
                ["edited"] = edited,
                ["updatedAt"] = DateTime.Now.ToString("O", CultureInfo.InvariantCulture)
            };

            var tempPath = path + ".tmp";
            try
            {
                File.WriteAllText(tempPath, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }), Encoding.UTF8);
                ReplaceFileAtomically(tempPath, path);
            }
            catch (Exception ex)
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
                AppendLog("UI 표시 상태 저장 실패: " + ex.Message);
            }
        }

        private void RewriteCatalogs()
        {
            if (string.IsNullOrWhiteSpace(dataFolder) || !Directory.Exists(dataFolder))
            {
                throw new InvalidOperationException("데이터 폴더가 설정되지 않았습니다.");
            }

            foreach (var catalogPath in GetCatalogFiles(dataFolder))
            {
                var rows = allFrames
                    .Where(record => string.Equals(record.CatalogPath, catalogPath, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(record => record.OriginalLineNumber)
                    .ThenBy(record => record.GlobalOrder)
                    .ToList();

                var tempPath = catalogPath + ".tmp";
                try
                {
                    using (var writer = new StreamWriter(tempPath, false, Encoding.UTF8))
                    {
                        foreach (var record in rows)
                        {
                            var json = BuildCatalogJson(record);
                            writer.WriteLine(json);
                            record.RawJson = json;
                        }
                    }

                    ReplaceFileAtomically(tempPath, catalogPath);
                }
                catch
                {
                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }

                    throw;
                }
            }
        }

        private static string BuildCatalogJson(FrameRecord record)
        {
            JsonObject json;
            try
            {
                json = JsonNode.Parse(record.RawJson) as JsonObject ?? new JsonObject();
            }
            catch
            {
                json = new JsonObject();
            }

            json["_index"] = record.Index;
            if (!string.IsNullOrWhiteSpace(record.SessionId))
            {
                json["_session_id"] = record.SessionId;
            }

            if (record.TimestampMs.HasValue)
            {
                json["_timestamp_ms"] = record.TimestampMs.Value;
            }

            json["cam/image_array"] = record.ImageFile;

            if (record.Angle.HasValue)
            {
                json["user/angle"] = record.Angle.Value;
            }
            else
            {
                json.Remove("user/angle");
            }

            if (record.Throttle.HasValue)
            {
                json["user/throttle"] = record.Throttle.Value;
            }
            else
            {
                json.Remove("user/throttle");
            }

            if (!string.IsNullOrWhiteSpace(record.Mode))
            {
                json["user/mode"] = record.Mode;
            }

            return json.ToJsonString(JsonOptions);
        }

        private void UpdateStats()
        {
            var active = allFrames.Where(record => !record.Deleted).ToList();
            if (active.Count == 0)
            {
                lblStats.Text = "전체 0개 / 표시 0개";
                return;
            }

            var angles = active.Where(record => record.Angle.HasValue).Select(record => record.Angle.Value).ToList();
            var throttles = active.Where(record => record.Throttle.HasValue).Select(record => record.Throttle.Value).ToList();
            var avgAngle = angles.Count > 0 ? angles.Average().ToString("0.###", CultureInfo.InvariantCulture) : "-";
            var avgThrottle = throttles.Count > 0 ? throttles.Average().ToString("0.###", CultureInfo.InvariantCulture) : "-";
            var anomalyCount = active.Count(record => record.IsAnomaly);

            var deletedCount = allFrames.Count(record => record.Deleted);
            var editedCount = allFrames.Count(record => record.Edited);
            lblStats.Text = $"사용 {active.Count}개 / 표시 {visibleFrames.Count}개\n" +
                            $"삭제 {deletedCount}개 / 편집 {editedCount}개\n" +
                            $"평균 angle {avgAngle} / throttle {avgThrottle}\n" +
                            $"이상 주행 {anomalyCount}개";
        }

        private void btnAnalyzeAnomaly_Click(object? sender, EventArgs e)
        {
            if (allFrames.Count == 0)
            {
                MessageBox.Show("먼저 DonkeyCar 데이터를 불러오세요.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DetectAnomalies();
            ApplyFilters(CurrentRecord()?.GlobalOrder);
        }

        private void btnClearAnomaly_Click(object? sender, EventArgs e)
        {
            chkAnomalyOnly.Checked = false;
            ClearAnomalyFlags();
            ApplyFilters(CurrentRecord()?.GlobalOrder);
        }

        private void btnNextAnomaly_Click(object? sender, EventArgs e)
        {
            if (visibleFrames.Count == 0)
            {
                return;
            }

            var start = Math.Max(0, lstFrames.SelectedIndex + 1);
            var next = -1;
            for (var i = start; i < visibleFrames.Count; i++)
            {
                if (visibleFrames[i].IsAnomaly)
                {
                    next = i;
                    break;
                }
            }

            if (next < 0)
            {
                for (var i = 0; i < start && i < visibleFrames.Count; i++)
                {
                    if (visibleFrames[i].IsAnomaly)
                    {
                        next = i;
                        break;
                    }
                }
            }

            if (next >= 0)
            {
                lstFrames.SelectedIndex = next;
            }
            else
            {
                MessageBox.Show("현재 표시 목록에는 감지된 이상 주행이 없습니다.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ClearAnomalyFlags()
        {
            foreach (var record in allFrames)
            {
                record.IsAnomaly = false;
                record.MovingAverage = null;
                record.Volatility = null;
                record.AnomalyScore = 0;
            }

            lblAnomalyStatus.Text = "탐지 전: 이동평균/변동성 밴드로 조향 스파이크를 찾습니다.";
        }

        private void RecalculateAnomaliesIfNeeded()
        {
            if (allFrames.Any(record => record.IsAnomaly || record.MovingAverage.HasValue || record.Volatility.HasValue))
            {
                DetectAnomalies(false);
            }
        }

        private void DetectAnomalies(bool writeLog = true)
        {
            foreach (var record in allFrames)
            {
                record.IsAnomaly = false;
                record.MovingAverage = null;
                record.Volatility = null;
                record.AnomalyScore = 0;
            }

            var active = allFrames.Where(record => !record.Deleted).OrderBy(record => record.GlobalOrder).ToList();
            var window = Math.Max(3, (int)numAnomalyWindow.Value);
            var sigma = Math.Max(1.0, (double)numAnomalySigma.Value);
            var minDelta = 0.15; // 최소 조향 튐 기준(Designer에 별도 입력란 없이 고정)
            double? previousAngle = null;

            for (var i = 0; i < active.Count; i++)
            {
                var record = active[i];
                if (!record.Angle.HasValue)
                {
                    continue;
                }

                var angle = record.Angle.Value;
                var previousDelta = previousAngle.HasValue ? Math.Abs(angle - previousAngle.Value) : (double?)null;
                previousAngle = angle;

                var start = Math.Max(0, i - window);
                var end = Math.Min(active.Count - 1, i + window);
                var count = 0;
                var sum = 0.0;
                var sumSquares = 0.0;
                for (var j = start; j <= end; j++)
                {
                    if (j == i || !active[j].Angle.HasValue)
                    {
                        continue;
                    }

                    var value = active[j].Angle!.Value;
                    count++;
                    sum += value;
                    sumSquares += value * value;
                }

                if (count < 3)
                {
                    continue;
                }

                var mean = sum / count;
                var variance = Math.Max(0, (sumSquares / count) - (mean * mean));
                var std = Math.Sqrt(variance);
                var deviation = Math.Abs(angle - mean);
                var band = Math.Max(sigma * std, minDelta);

                record.MovingAverage = mean;
                record.Volatility = std;

                var bandOut = deviation >= band && deviation >= minDelta;
                var sharpJump = previousDelta.HasValue && previousDelta.Value >= Math.Max(minDelta * 2.0, band);
                if (bandOut || sharpJump)
                {
                    record.IsAnomaly = true;
                    record.AnomalyScore = std > 0.000001 ? deviation / std : deviation;
                }
            }

            var anomalyCount = active.Count(record => record.IsAnomaly);
            lblAnomalyStatus.Text = $"감지 결과: 이상 주행 {anomalyCount}개 / 전체 {active.Count}개";
            if (writeLog)
            {
                AppendLog($"이상 주행 탐지 완료: {anomalyCount}개, window={window}, sigma={sigma:0.##}, minJump={minDelta:0.##}");
            }
        }

        private void DrawGraph()
        {
            if (picGraph.Width <= 0 || picGraph.Height <= 0)
            {
                return;
            }

            var source = visibleFrames.Count > 0 ? visibleFrames : allFrames.Where(record => !record.Deleted).OrderBy(record => record.GlobalOrder).ToList();
            if (source.Count < 2)
            {
                DrawPlaceholder(picGraph, "그래프를 표시하려면 2개 이상의 프레임이 필요합니다.");
                return;
            }

            var sigma = Math.Max(1.0, (double)numAnomalySigma.Value);
            var values = new List<double>();
            foreach (var record in source)
            {
                if (record.Angle.HasValue) values.Add(record.Angle.Value);
                if (record.Throttle.HasValue) values.Add(record.Throttle.Value);
                if (record.MovingAverage.HasValue)
                {
                    values.Add(record.MovingAverage.Value);
                    if (record.Volatility.HasValue)
                    {
                        values.Add(record.MovingAverage.Value + sigma * record.Volatility.Value);
                        values.Add(record.MovingAverage.Value - sigma * record.Volatility.Value);
                    }
                }
            }

            if (values.Count == 0)
            {
                DrawPlaceholder(picGraph, "angle/throttle 값이 없습니다.");
                return;
            }

            var min = values.Min();
            var max = values.Max();
            if (Math.Abs(max - min) < 0.000001)
            {
                min -= 1;
                max += 1;
            }

            var bitmap = new Bitmap(picGraph.Width, picGraph.Height);
            using (var graphics = Graphics.FromImage(bitmap))
            using (var axisPen = new Pen(Color.LightGray))
            using (var anglePen = new Pen(Color.RoyalBlue, 2f))
            using (var throttlePen = new Pen(Color.DarkOrange, 2f))
            using (var averagePen = new Pen(Color.SeaGreen, 2f))
            using (var bandPen = new Pen(Color.LightGreen, 1f))
            using (var anomalyPen = new Pen(Color.Red, 1.5f))
            using (var anomalyBrush = new SolidBrush(Color.Red))
            using (var textBrush = new SolidBrush(Color.DimGray))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                bandPen.DashStyle = DashStyle.Dash;
                graphics.Clear(Color.White);
                graphics.DrawLine(axisPen, 42, 10, 42, bitmap.Height - 20);
                graphics.DrawLine(axisPen, 42, bitmap.Height - 20, bitmap.Width - 10, bitmap.Height - 20);
                graphics.DrawString("angle", Font, Brushes.RoyalBlue, 48, 8);
                graphics.DrawString("throttle", Font, Brushes.DarkOrange, 100, 8);
                graphics.DrawString("MA/Band", Font, Brushes.SeaGreen, 168, 8);
                graphics.DrawString("Anomaly", Font, Brushes.Red, 238, 8);
                graphics.DrawString(max.ToString("0.##", CultureInfo.InvariantCulture), Font, textBrush, 2, 8);
                graphics.DrawString(min.ToString("0.##", CultureInfo.InvariantCulture), Font, textBrush, 2, bitmap.Height - 34);

                var averages = source.Select(record => record.MovingAverage).ToList();
                var upperBand = source.Select(record => record.MovingAverage.HasValue && record.Volatility.HasValue ? record.MovingAverage.Value + sigma * record.Volatility.Value : (double?)null).ToList();
                var lowerBand = source.Select(record => record.MovingAverage.HasValue && record.Volatility.HasValue ? record.MovingAverage.Value - sigma * record.Volatility.Value : (double?)null).ToList();

                DrawSeries(graphics, upperBand, min, max, bandPen, bitmap.Width, bitmap.Height);
                DrawSeries(graphics, lowerBand, min, max, bandPen, bitmap.Width, bitmap.Height);
                DrawSeries(graphics, averages, min, max, averagePen, bitmap.Width, bitmap.Height);
                DrawSeries(graphics, source.Select(record => record.Angle).ToList(), min, max, anglePen, bitmap.Width, bitmap.Height);
                DrawSeries(graphics, source.Select(record => record.Throttle).ToList(), min, max, throttlePen, bitmap.Width, bitmap.Height);

                for (var i = 0; i < source.Count; i++)
                {
                    if (!source[i].IsAnomaly)
                    {
                        continue;
                    }

                    var x = 42 + (int)Math.Round((bitmap.Width - 52) * (source.Count == 1 ? 0 : (double)i / (source.Count - 1)));
                    graphics.DrawLine(anomalyPen, x, 22, x, bitmap.Height - 20);
                    if (source[i].Angle.HasValue)
                    {
                        var y = ValueToY(source[i].Angle.Value, min, max, bitmap.Height);
                        graphics.FillEllipse(anomalyBrush, x - 4, y - 4, 8, 8);
                    }
                }
            }

            var old = picGraph.Image;
            picGraph.Image = bitmap;
            old?.Dispose();
        }

        private static void DrawSeries(Graphics graphics, IList<double?> values, double min, double max, Pen pen, int width, int height)
        {
            Point? previous = null;
            for (var i = 0; i < values.Count; i++)
            {
                if (!values[i].HasValue)
                {
                    previous = null;
                    continue;
                }

                var x = 42 + (int)Math.Round((width - 52) * (values.Count == 1 ? 0 : (double)i / (values.Count - 1)));
                var y = ValueToY(values[i]!.Value, min, max, height);
                var current = new Point(x, y);

                if (previous.HasValue)
                {
                    graphics.DrawLine(pen, previous.Value, current);
                }

                previous = current;
            }
        }

        private static int ValueToY(double value, double min, double max, int height)
        {
            var normalized = (value - min) / (max - min);
            return 10 + (int)Math.Round((height - 30) * (1 - normalized));
        }

        private void DrawTimeline()
        {
            pnlTimeline.Invalidate();
        }

        private void pnlTimeline_Paint(object? sender, PaintEventArgs e)
        {
            var width = Math.Max(1, pnlTimeline.Width);
            var height = Math.Max(1, pnlTimeline.Height);
            using var basePen = new Pen(Color.Silver, 4f);
            using var anomalyPen = new Pen(Color.Red, 3f);
            using var currentPen = new Pen(Color.RoyalBlue, 2f);
            using var textBrush = new SolidBrush(Color.DimGray);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(Color.White);
            var y = height / 2;
            e.Graphics.DrawLine(basePen, 8, y, width - 8, y);

            if (visibleFrames.Count == 0)
            {
                e.Graphics.DrawString("이상 구간 타임라인", Font, textBrush, 8, 1);
                return;
            }

            for (var i = 0; i < visibleFrames.Count; i++)
            {
                if (!visibleFrames[i].IsAnomaly)
                {
                    continue;
                }

                var x = 8 + (int)Math.Round((width - 16) * (visibleFrames.Count == 1 ? 0 : (double)i / (visibleFrames.Count - 1)));
                e.Graphics.DrawLine(anomalyPen, x, 2, x, height - 3);
            }

            var selected = lstFrames.SelectedIndex >= 0 ? lstFrames.SelectedIndex : currentVisibleIndex;
            if (selected >= 0 && selected < visibleFrames.Count)
            {
                var x = 8 + (int)Math.Round((width - 16) * (visibleFrames.Count == 1 ? 0 : (double)selected / (visibleFrames.Count - 1)));
                e.Graphics.DrawLine(currentPen, x, 1, x, height - 2);
            }
        }

        private void pnlTimeline_MouseClick(object? sender, MouseEventArgs e)
        {
            if (visibleFrames.Count == 0)
            {
                return;
            }

            var ratio = Math.Max(0, Math.Min(1, (double)(e.X - 8) / Math.Max(1, pnlTimeline.Width - 16)));
            var index = (int)Math.Round(ratio * (visibleFrames.Count - 1));
            lstFrames.SelectedIndex = Math.Max(0, Math.Min(visibleFrames.Count - 1, index));
        }

        private static void DrawPlaceholder(PictureBox pictureBox, string text)
        {
            var width = Math.Max(1, pictureBox.Width);
            var height = Math.Max(1, pictureBox.Height);
            var bitmap = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(bitmap))
            using (var brush = new SolidBrush(Color.White))
            using (var textBrush = new SolidBrush(Color.DimGray))
            using (var font = new Font("맑은 고딕", 10F, FontStyle.Regular, GraphicsUnit.Point, 129))
            {
                graphics.Clear(pictureBox.BackColor);
                graphics.FillRectangle(brush, new Rectangle(0, 0, width, height));
                var rect = new RectangleF(10, 10, width - 20, height - 20);
                graphics.DrawString(text, font, textBrush, rect);
            }

            var old = pictureBox.Image;
            pictureBox.Image = bitmap;
            old?.Dispose();
        }

        private void picFrame_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                selectedImageRect = null;
                UpdateSelectionLabel();
                picFrame.Invalidate();
                return;
            }

            if (e.Button != MouseButtons.Left || string.IsNullOrWhiteSpace(loadedImagePath) || picFrame.Image == null)
            {
                return;
            }

            var point = ControlPointToImagePoint(e.Location);
            if (!point.HasValue)
            {
                return;
            }

            selectionStartImagePoint = point.Value;
            selectedImageRect = new Rectangle(point.Value.X, point.Value.Y, 1, 1);
            isSelectingImage = true;
            picFrame.Capture = true;
            UpdateSelectionLabel();
            picFrame.Invalidate();
        }

        private void picFrame_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!isSelectingImage || picFrame.Image == null)
            {
                return;
            }

            var point = ControlPointToImagePoint(e.Location, true);
            if (!point.HasValue)
            {
                return;
            }

            selectedImageRect = BuildImageRectangle(selectionStartImagePoint, point.Value);
            UpdateSelectionLabel();
            picFrame.Invalidate();
        }

        private void picFrame_MouseUp(object? sender, MouseEventArgs e)
        {
            if (!isSelectingImage)
            {
                return;
            }

            isSelectingImage = false;
            picFrame.Capture = false;
            var point = ControlPointToImagePoint(e.Location, true);
            if (point.HasValue)
            {
                selectedImageRect = BuildImageRectangle(selectionStartImagePoint, point.Value);
            }

            if (selectedImageRect.HasValue && (selectedImageRect.Value.Width < 2 || selectedImageRect.Value.Height < 2))
            {
                selectedImageRect = null;
            }

            UpdateSelectionLabel();
            picFrame.Invalidate();

            // 이미지 가리기 작업은 드래그 선택 후 마우스를 놓는 즉시 적용합니다.
            // 기존 버튼 방식도 유지하지만, 수업 시연에서는 선택-적용 흐름이 한 번에 보이도록 처리합니다.
            if (selectedImageRect.HasValue && selectedImageRect.Value.Width > 1 && selectedImageRect.Value.Height > 1)
            {
                ApplyMaskRegionFromSelection(true);
            }
        }

        private void picFrame_Paint(object? sender, PaintEventArgs e)
        {
            if (!selectedImageRect.HasValue || picFrame.Image == null)
            {
                return;
            }

            var controlRect = ImageRectToControlRect(selectedImageRect.Value);
            if (controlRect.Width <= 0 || controlRect.Height <= 0)
            {
                return;
            }

            using var pen = new Pen(Color.Red, 2f) { DashStyle = DashStyle.Dash };
            using var brush = new SolidBrush(Color.FromArgb(40, Color.Red));
            e.Graphics.FillRectangle(brush, controlRect);
            e.Graphics.DrawRectangle(pen, controlRect);
        }

        private Rectangle BuildImageRectangle(Point a, Point b)
        {
            if (picFrame.Image == null)
            {
                return Rectangle.Empty;
            }

            var x = Math.Min(a.X, b.X);
            var y = Math.Min(a.Y, b.Y);
            var right = Math.Max(a.X, b.X);
            var bottom = Math.Max(a.Y, b.Y);
            var rect = Rectangle.FromLTRB(x, y, right + 1, bottom + 1);
            var bounds = new Rectangle(0, 0, picFrame.Image.Width, picFrame.Image.Height);
            return Rectangle.Intersect(rect, bounds);
        }

        private Point? ControlPointToImagePoint(Point point, bool clamp = false)
        {
            if (picFrame.Image == null)
            {
                return null;
            }

            var viewport = GetImageViewport();
            if (viewport.Width <= 0 || viewport.Height <= 0)
            {
                return null;
            }

            if (!viewport.Contains(point) && !clamp)
            {
                return null;
            }

            var xInView = Math.Max(0, Math.Min(viewport.Width - 1, point.X - viewport.X));
            var yInView = Math.Max(0, Math.Min(viewport.Height - 1, point.Y - viewport.Y));
            var imageX = (int)Math.Round(xInView * (picFrame.Image.Width - 1) / Math.Max(1.0, viewport.Width - 1));
            var imageY = (int)Math.Round(yInView * (picFrame.Image.Height - 1) / Math.Max(1.0, viewport.Height - 1));
            return new Point(
                Math.Max(0, Math.Min(picFrame.Image.Width - 1, imageX)),
                Math.Max(0, Math.Min(picFrame.Image.Height - 1, imageY)));
        }

        private Rectangle GetImageViewport()
        {
            if (picFrame.Image == null)
            {
                return Rectangle.Empty;
            }

            var client = picFrame.ClientRectangle;
            if (client.Width <= 0 || client.Height <= 0)
            {
                return Rectangle.Empty;
            }

            if (picFrame.SizeMode != PictureBoxSizeMode.Zoom)
            {
                return client;
            }

            var imageRatio = (double)picFrame.Image.Width / Math.Max(1, picFrame.Image.Height);
            var controlRatio = (double)client.Width / Math.Max(1, client.Height);

            if (controlRatio > imageRatio)
            {
                var width = (int)Math.Round(client.Height * imageRatio);
                var x = client.X + (client.Width - width) / 2;
                return new Rectangle(x, client.Y, width, client.Height);
            }
            else
            {
                var height = (int)Math.Round(client.Width / imageRatio);
                var y = client.Y + (client.Height - height) / 2;
                return new Rectangle(client.X, y, client.Width, height);
            }
        }

        private Rectangle ImageRectToControlRect(Rectangle imageRect)
        {
            if (picFrame.Image == null)
            {
                return Rectangle.Empty;
            }

            var viewport = GetImageViewport();
            if (viewport.Width <= 0 || viewport.Height <= 0)
            {
                return Rectangle.Empty;
            }

            var x = viewport.X + (int)Math.Round(imageRect.X * viewport.Width / (double)picFrame.Image.Width);
            var y = viewport.Y + (int)Math.Round(imageRect.Y * viewport.Height / (double)picFrame.Image.Height);
            var width = Math.Max(1, (int)Math.Round(imageRect.Width * viewport.Width / (double)picFrame.Image.Width));
            var height = Math.Max(1, (int)Math.Round(imageRect.Height * viewport.Height / (double)picFrame.Image.Height));
            return new Rectangle(x, y, width, height);
        }

        private void UpdateSelectionLabel()
        {
            if (!selectedImageRect.HasValue)
            {
                lblEditHint.Text = imageDirty ? "선택 영역: 없음 / 저장 필요" : "선택 영역: 없음";
                return;
            }

            var rect = selectedImageRect.Value;
            lblEditHint.Text = $"선택 영역: x={rect.X}, y={rect.Y}, w={rect.Width}, h={rect.Height}" + (imageDirty ? " / 저장 필요" : string.Empty);
        }

        private bool TryGetSelectedBitmapAndRect(out Bitmap bitmap, out Rectangle rect)
        {
            bitmap = null!;
            rect = Rectangle.Empty;

            if (string.IsNullOrWhiteSpace(loadedImagePath) || !File.Exists(loadedImagePath))
            {
                MessageBox.Show("편집할 이미지가 없습니다. 먼저 프레임을 선택하세요.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            if (picFrame.Image is not Bitmap currentBitmap)
            {
                MessageBox.Show("현재 이미지를 편집할 수 없습니다.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            if (!selectedImageRect.HasValue || selectedImageRect.Value.Width <= 1 || selectedImageRect.Value.Height <= 1)
            {
                MessageBox.Show("이미지에서 편집할 영역을 드래그로 선택하세요.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            var bounds = new Rectangle(0, 0, currentBitmap.Width, currentBitmap.Height);
            var normalized = Rectangle.Intersect(selectedImageRect.Value, bounds);
            if (normalized.Width <= 1 || normalized.Height <= 1)
            {
                MessageBox.Show("선택 영역이 너무 작습니다.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            bitmap = currentBitmap;
            rect = normalized;
            return true;
        }

        private void btnMaskRegion_Click(object? sender, EventArgs e)
        {
            ApplyMaskRegionFromSelection(false);
        }

        private bool ApplyMaskRegionFromSelection(bool autoApplied)
        {
            if (!TryGetSelectedBitmapAndRect(out var bitmap, out var rect))
            {
                return false;
            }

            var targets = GetTargetRecordsForBatch();
            if (targets.Count == 0)
            {
                return false;
            }

            Color? fixedColor = cmbMaskMode.Text switch
            {
                "검정" => Color.Black,
                "흰색" => Color.White,
                "회색" => Color.Gray,
                _ => null
            };

            var success = 0;
            foreach (var record in targets)
            {
                try
                {
                    ApplyMaskToImageFile(ResolveImagePath(record), rect, fixedColor);
                    record.Edited = true;
                    success++;
                }
                catch (Exception ex)
                {
                    AppendLog($"이미지 가리기 실패: {record.ImageFile} / {ex.Message}");
                }
            }

            SaveUiMarks();
            var current = CurrentRecord();
            if (current != null)
            {
                LoadImage(current);
            }
            ApplyFilters(current?.GlobalOrder ?? targets[0].GlobalOrder);
            AppendLog($"이미지 영역 가리기 {(autoApplied ? "자동 " : string.Empty)}적용: {success}/{targets.Count}개, 영역={rect}");
            return success > 0;
        }

        private void btnReplaceRegion_Click(object? sender, EventArgs e)
        {
            if (!TryGetSelectedBitmapAndRect(out _, out var rect))
            {
                return;
            }

            var targets = GetTargetRecordsForBatch();
            if (targets.Count == 0)
            {
                return;
            }

            using var dlg = new OpenFileDialog
            {
                Title = "선택 영역에 넣을 이미지 선택",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*"
            };

            if (dlg.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var success = 0;
            foreach (var record in targets)
            {
                try
                {
                    ApplyReplacementToImageFile(ResolveImagePath(record), rect, dlg.FileName);
                    record.Edited = true;
                    success++;
                }
                catch (Exception ex)
                {
                    AppendLog($"이미지 교체 실패: {record.ImageFile} / {ex.Message}");
                }
            }

            SaveUiMarks();
            var current = CurrentRecord();
            if (current != null)
            {
                LoadImage(current);
            }
            ApplyFilters(current?.GlobalOrder ?? targets[0].GlobalOrder);
            AppendLog($"이미지 영역 교체 적용: {success}/{targets.Count}개 <- {Path.GetFileName(dlg.FileName)}");
        }

        private void ApplyMaskToImageFile(string imagePath, Rectangle requestedRect, Color? fixedColor)
        {
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                throw new FileNotFoundException("이미지 파일을 찾을 수 없습니다.", imagePath);
            }

            using var bitmap = LoadBitmapCopy(imagePath);
            var rect = Rectangle.Intersect(requestedRect, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
            if (rect.Width <= 1 || rect.Height <= 1)
            {
                throw new InvalidOperationException("선택 영역이 이미지 범위를 벗어났습니다.");
            }

            var color = fixedColor ?? CalculateAverageColor(bitmap, rect);
            BackupImageIfNeeded(imagePath);
            using (var graphics = Graphics.FromImage(bitmap))
            using (var brush = new SolidBrush(color))
            {
                graphics.FillRectangle(brush, rect);
            }
            SaveBitmapAtomically(bitmap, imagePath);
        }

        private void ApplyReplacementToImageFile(string imagePath, Rectangle requestedRect, string replacementPath)
        {
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                throw new FileNotFoundException("이미지 파일을 찾을 수 없습니다.", imagePath);
            }

            if (string.IsNullOrWhiteSpace(replacementPath) || !File.Exists(replacementPath))
            {
                throw new FileNotFoundException("교체 이미지를 찾을 수 없습니다.", replacementPath);
            }

            using var bitmap = LoadBitmapCopy(imagePath);
            var rect = Rectangle.Intersect(requestedRect, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
            if (rect.Width <= 1 || rect.Height <= 1)
            {
                throw new InvalidOperationException("선택 영역이 이미지 범위를 벗어났습니다.");
            }

            BackupImageIfNeeded(imagePath);
            using var replacement = Image.FromFile(replacementPath);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(replacement, rect);
            }
            SaveBitmapAtomically(bitmap, imagePath);
        }

        private static Bitmap LoadBitmapCopy(string imagePath)
        {
            using var stream = File.OpenRead(imagePath);
            using var source = Image.FromStream(stream);
            return new Bitmap(source);
        }

        private void BackupImageIfNeeded(string imagePath)
        {
            var backupPath = GetEditedBackupPath(imagePath);
            var backupDirectory = Path.GetDirectoryName(backupPath);
            if (!string.IsNullOrWhiteSpace(backupDirectory))
            {
                Directory.CreateDirectory(backupDirectory);
            }

            if (!File.Exists(backupPath))
            {
                File.Copy(imagePath, backupPath, false);
            }
        }

        private void SaveBitmapAtomically(Bitmap bitmap, string imagePath)
        {
            var tempPath = imagePath + ".editing.tmp";
            try
            {
                bitmap.Save(tempPath, GetImageFormat(imagePath));
                ReplaceFileAtomically(tempPath, imagePath);
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        private void btnSaveImageEdit_Click(object? sender, EventArgs e)
        {
            SaveCurrentImageEdit(true);
        }

        private bool SaveCurrentImageEdit(bool showMessage)
        {
            if (string.IsNullOrWhiteSpace(loadedImagePath) || !File.Exists(loadedImagePath) || picFrame.Image is not Bitmap bitmap)
            {
                if (showMessage)
                {
                    MessageBox.Show("저장할 이미지가 없습니다.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return false;
            }

            if (!imageDirty)
            {
                if (showMessage)
                {
                    MessageBox.Show("저장할 이미지 편집 내용이 없습니다.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return false;
            }

            try
            {
                var backupPath = GetEditedBackupPath(loadedImagePath);
                var backupDirectory = Path.GetDirectoryName(backupPath);
                if (!string.IsNullOrWhiteSpace(backupDirectory))
                {
                    Directory.CreateDirectory(backupDirectory);
                }

                if (!File.Exists(backupPath))
                {
                    File.Copy(loadedImagePath, backupPath, false);
                }

                var tempPath = loadedImagePath + ".editing.tmp";
                try
                {
                    bitmap.Save(tempPath, GetImageFormat(loadedImagePath));
                    ReplaceFileAtomically(tempPath, loadedImagePath);
                }
                finally
                {
                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }
                }

                imageDirty = false;
                var currentRecord = CurrentRecord();
                if (currentRecord != null)
                {
                    currentRecord.Edited = true;
                    SaveUiMarks();
                }
                UpdateSelectionLabel();
                AppendLog($"이미지 편집 저장: {loadedImagePath}");
                if (showMessage)
                {
                    MessageBox.Show("이미지 편집 내용을 저장했습니다. 원본은 _edited_backup 폴더에 보관됩니다.", "저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"이미지 저장 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void btnCancelImageEdit_Click(object? sender, EventArgs e)
        {
            var record = CurrentRecord();
            if (record == null)
            {
                return;
            }

            LoadImage(record);
            AppendLog("이미지 편집 취소: 현재 프레임 다시 로드");
        }

        private void btnRestoreImage_Click(object? sender, EventArgs e)
        {
            var targets = GetTargetRecordsForBatch();
            if (targets.Count == 0)
            {
                MessageBox.Show("복구할 이미지를 체크하거나 선택하세요.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var success = 0;
            foreach (var record in targets)
            {
                var imagePath = ResolveImagePath(record);
                var backupPath = GetEditedBackupPath(imagePath);
                if (!File.Exists(backupPath))
                {
                    AppendLog("원본 백업 없음: " + record.ImageFile);
                    continue;
                }

                try
                {
                    File.Copy(backupPath, imagePath, true);
                    record.Edited = false;
                    success++;
                }
                catch (Exception ex)
                {
                    AppendLog($"이미지 원본 복구 실패: {record.ImageFile} / {ex.Message}");
                }
            }

            SaveUiMarks();
            var current = CurrentRecord();
            if (current != null)
            {
                LoadImage(current);
            }
            ApplyFilters(current?.GlobalOrder ?? targets[0].GlobalOrder);
            AppendLog($"이미지 원본 복구: {success}/{targets.Count}개");
        }

        private void btnClearSelection_Click(object? sender, EventArgs e)
        {
            selectedImageRect = null;
            UpdateSelectionLabel();
            picFrame.Invalidate();
        }

        private static Color CalculateAverageColor(Bitmap bitmap, Rectangle rect)
        {
            long r = 0, g = 0, b = 0, count = 0;
            var stepX = Math.Max(1, rect.Width / 40);
            var stepY = Math.Max(1, rect.Height / 40);

            for (var y = rect.Top; y < rect.Bottom; y += stepY)
            {
                for (var x = rect.Left; x < rect.Right; x += stepX)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    r += pixel.R;
                    g += pixel.G;
                    b += pixel.B;
                    count++;
                }
            }

            if (count == 0)
            {
                return Color.Gray;
            }

            return Color.FromArgb((int)(r / count), (int)(g / count), (int)(b / count));
        }

        private string GetEditedBackupPath(string imagePath)
        {
            var backupRoot = string.IsNullOrWhiteSpace(dataFolder)
                ? Path.Combine(Path.GetDirectoryName(imagePath) ?? string.Empty, "_edited_backup")
                : Path.Combine(dataFolder, "_edited_backup");

            string relative;
            try
            {
                relative = !string.IsNullOrWhiteSpace(dataFolder)
                    ? Path.GetRelativePath(dataFolder, imagePath)
                    : Path.GetFileName(imagePath);
            }
            catch
            {
                relative = Path.GetFileName(imagePath);
            }

            if (relative.StartsWith("..", StringComparison.Ordinal))
            {
                relative = Path.GetFileName(imagePath);
            }

            return Path.Combine(backupRoot, relative);
        }

        private string GetDeletedBackupPath(string imagePath)
        {
            var backupRoot = string.IsNullOrWhiteSpace(dataFolder)
                ? Path.Combine(Path.GetDirectoryName(imagePath) ?? string.Empty, "_deleted_backup")
                : Path.Combine(dataFolder, "_deleted_backup");

            string relative;
            try
            {
                relative = !string.IsNullOrWhiteSpace(dataFolder)
                    ? Path.GetRelativePath(dataFolder, imagePath)
                    : Path.GetFileName(imagePath);
            }
            catch
            {
                relative = Path.GetFileName(imagePath);
            }

            if (relative.StartsWith("..", StringComparison.Ordinal))
            {
                relative = Path.GetFileName(imagePath);
            }

            return Path.Combine(backupRoot, relative);
        }


        private string ResolveDeletedBackupPath(FrameRecord record)
        {
            if (!string.IsNullOrWhiteSpace(record.DeletedBackupPath))
            {
                var saved = record.DeletedBackupPath.Trim();
                if (Path.IsPathRooted(saved))
                {
                    return saved;
                }

                return Path.Combine(dataFolder, saved);
            }

            var originalPath = ResolveOriginalImagePath(record);
            if (string.IsNullOrWhiteSpace(originalPath))
            {
                return string.Empty;
            }

            return GetDeletedBackupPath(originalPath);
        }

        private string MakeUiMarkPathValue(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(dataFolder))
            {
                return path;
            }

            try
            {
                var relative = Path.GetRelativePath(dataFolder, path);
                if (!relative.StartsWith("..", StringComparison.Ordinal) && !Path.IsPathRooted(relative))
                {
                    return relative;
                }
            }
            catch
            {
                // 상대 경로 변환 실패 시 절대 경로를 저장한다.
            }

            return path;
        }

        private static void ReplaceFileAtomically(string sourcePath, string destinationPath)
        {
            var directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.Move(sourcePath, destinationPath, true);
        }

        private static ImageFormat GetImageFormat(string path)
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();
            return extension switch
            {
                ".png" => ImageFormat.Png,
                ".bmp" => ImageFormat.Bmp,
                ".gif" => ImageFormat.Gif,
                ".jpg" or ".jpeg" => ImageFormat.Jpeg,
                _ => ImageFormat.Jpeg
            };
        }

        private void RefreshTrainingPathDefaults(bool overwriteExisting)
        {
            if (string.IsNullOrWhiteSpace(dataFolder))
            {
                return;
            }

            if (overwriteExisting || string.IsNullOrWhiteSpace(trainingDataPathOverride))
            {
                trainingDataPathOverride = dataFolder;
            }

            if (overwriteExisting || string.IsNullOrWhiteSpace(trainingModelPathOverride))
            {
                var modelRoot = string.IsNullOrWhiteSpace(rootFolder) ? dataFolder : rootFolder;
                trainingModelPathOverride = Path.Combine(modelRoot, "models", "mypilot.h5");
            }
        }

        private string GetTrainingDataPath()
        {
            return string.IsNullOrWhiteSpace(trainingDataPathOverride) ? dataFolder : trainingDataPathOverride.Trim();
        }

        private string GetTrainingModelPath()
        {
            if (!string.IsNullOrWhiteSpace(trainingModelPathOverride))
            {
                return trainingModelPathOverride.Trim();
            }

            var modelRoot = string.IsNullOrWhiteSpace(rootFolder) ? dataFolder : rootFolder;
            return Path.Combine(modelRoot, "models", "mypilot.h5");
        }

        private void EnsureLocalModelDirectory(string modelPath)
        {
            if (string.IsNullOrWhiteSpace(modelPath) || IsLikelyRemoteOrUnixPath(modelPath))
            {
                return;
            }

            var directory = Path.GetDirectoryName(modelPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static bool IsLikelyRemoteOrUnixPath(string path)
        {
            var value = path.Trim();
            if (value.StartsWith("/", StringComparison.Ordinal) ||
                value.StartsWith("~", StringComparison.Ordinal) ||
                value.Contains(":/", StringComparison.Ordinal) ||
                value.Contains("@", StringComparison.Ordinal))
            {
                return true;
            }

            return false;
        }


        internal string TrainingRootFolder => rootFolder;
        internal string TrainingDataFolder => dataFolder;
        internal string TrainingImagesFolder => imagesFolder;

        internal string GetTrainingDataPathForDialog()
        {
            return GetTrainingDataPath();
        }

        internal string GetTrainingModelPathForDialog()
        {
            return GetTrainingModelPath();
        }

        internal void RefreshTrainingPathDefaultsForDialog(bool overwriteExisting)
        {
            RefreshTrainingPathDefaults(overwriteExisting);
        }

        internal void SetTrainingOverridesFromDialog(string dataPath, string modelPath)
        {
            trainingDataPathOverride = dataPath ?? string.Empty;
            trainingModelPathOverride = modelPath ?? string.Empty;
        }

        internal void StoreGeneratedTrainingCommand(string command, bool interactiveSshPassword)
        {
            trainingCommandGenerated = true;
            trainingUseSshPasswordInput = interactiveSshPassword;
            trainingSshPassword = string.Empty;
            txtTrainCommand.Text = command;
            UpdateTrainingCommandEditState();
        }

        internal void AppendTrainingLog(string text)
        {
            AppendLog(text);
        }

        internal string ConvertVirtualBoxPathForDialog(string path)
        {
            return ConvertWindowsSharedFolderPathToVirtualBoxPath(path);
        }

        internal string CreateTrainingDatasetForDialog(int datasetModeIndex, bool excludeAnomalyFromSelection, string modeName)
        {
            if ((datasetModeIndex == 1 || (datasetModeIndex == 2 && excludeAnomalyFromSelection)) &&
                !allFrames.Any(record => record.IsAnomaly || record.MovingAverage.HasValue || record.Volatility.HasValue))
            {
                DetectAnomalies(false);
            }

            List<FrameRecord> records = datasetModeIndex switch
            {
                1 => allFrames.Where(record => !record.Deleted && !record.IsAnomaly).ToList(),
                2 => visibleFrames.Where(record => !record.Deleted).ToList(),
                _ => allFrames.Where(record => !record.Deleted).ToList()
            };

            if (datasetModeIndex == 2 && excludeAnomalyFromSelection)
            {
                records = records.Where(record => !record.IsAnomaly).ToList();
            }

            records = records.OrderBy(record => record.GlobalOrder).ToList();
            if (records.Count == 0)
            {
                throw new InvalidOperationException("학습 데이터셋으로 내보낼 프레임이 없습니다.");
            }

            return CreateTrainingDataset(records, modeName);
        }

        internal bool TryMapTrainingPathToLocalFile(string trainingPath, out string localPath)
        {
            localPath = string.Empty;
            if (string.IsNullOrWhiteSpace(trainingPath))
            {
                return false;
            }

            var value = trainingPath.Trim().Trim('"');
            if (!IsLikelyRemoteOrUnixPath(value))
            {
                localPath = value;
                return true;
            }

            if (TryConvertWslPathToWindows(value, out var windowsPath))
            {
                localPath = windowsPath;
                return true;
            }

            var vboxRoot = ConvertWindowsSharedFolderPathToVirtualBoxPath(rootFolder).Replace('\\', '/').TrimEnd('/');
            if (!string.IsNullOrWhiteSpace(vboxRoot) && value.Replace('\\', '/').StartsWith(vboxRoot + "/", StringComparison.OrdinalIgnoreCase))
            {
                var relative = value.Replace('\\', '/').Substring(vboxRoot.Length).TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                localPath = Path.Combine(rootFolder, relative);
                return true;
            }

            var vboxData = ConvertWindowsSharedFolderPathToVirtualBoxPath(dataFolder).Replace('\\', '/').TrimEnd('/');
            if (!string.IsNullOrWhiteSpace(vboxData) && value.Replace('\\', '/').StartsWith(vboxData + "/", StringComparison.OrdinalIgnoreCase))
            {
                var relative = value.Replace('\\', '/').Substring(vboxData.Length).TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                localPath = Path.Combine(dataFolder, relative);
                return true;
            }

            return false;
        }

        private static bool TryConvertWslPathToWindows(string path, out string windowsPath)
        {
            windowsPath = string.Empty;
            var value = path.Replace('\\', '/');
            if (!value.StartsWith("/mnt/", StringComparison.OrdinalIgnoreCase) || value.Length < 7)
            {
                return false;
            }

            var drive = value[5];
            if (!char.IsLetter(drive) || value[6] != '/')
            {
                return false;
            }

            var rest = value.Substring(7).Replace('/', Path.DirectorySeparatorChar);
            windowsPath = char.ToUpperInvariant(drive) + ":" + Path.DirectorySeparatorChar + rest;
            return true;
        }

        private void btnTrainingPaths_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(dataFolder) || !Directory.Exists(dataFolder))
            {
                MessageBox.Show("먼저 DonkeyCar 데이터를 불러오세요.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            RefreshTrainingPathDefaults(false);
            using var dialog = new Form2(this);
            dialog.ShowDialog(this);
        }

        internal string BuildTrainingCommand(string environment, string dataPath, string modelPath, string extraArgs, string activateCommand, string sshUser, string sshHost, string sshPort, string remoteWorkDir, string sshPassword)
        {
            var extra = string.IsNullOrWhiteSpace(extraArgs) ? string.Empty : " " + extraArgs.Trim();
            if (environment == "wsl")
            {
                var bashData = ConvertPathForEnvironment(dataPath, "wsl");
                var bashModel = ConvertPathForEnvironment(modelPath, "wsl");
                var body = BuildBashTrainBody(activateCommand, ConvertPathForEnvironment(rootFolder, "wsl"), bashData, bashModel, extraArgs);
                return "wsl bash -lc \"" + body.Replace("\"", "\\\"") + "\"";
            }

            if (environment == "virtualbox")
            {
                var vboxData = ConvertPathForEnvironment(dataPath, "virtualbox");
                var vboxModel = ConvertPathForEnvironment(modelPath, "virtualbox");
                var work = string.IsNullOrWhiteSpace(remoteWorkDir) ? string.Empty : ConvertPathForEnvironment(remoteWorkDir, "virtualbox");
                var body = BuildBashTrainBody(activateCommand, work, vboxData, vboxModel, extraArgs);
                var userHost = string.IsNullOrWhiteSpace(sshUser) ? sshHost : sshUser + "@" + sshHost;
                var portPart = string.IsNullOrWhiteSpace(sshPort) ? string.Empty : "-p " + sshPort.Trim() + " ";
                if (!string.IsNullOrWhiteSpace(sshPassword))
                {
                    var sshPasswordOptions = "-T -o PreferredAuthentications=password -o PubkeyAuthentication=no -o NumberOfPasswordPrompts=1 -o ConnectionAttempts=1 -o StrictHostKeyChecking=accept-new -o ConnectTimeout=10 -o ServerAliveInterval=30 -o ServerAliveCountMax=2 ";
                    return "ssh " + sshPasswordOptions + portPart + userHost + " \"" + body.Replace("\"", "\\\"") + "\"";
                }

                var sshOptions = "-T -o BatchMode=yes -o NumberOfPasswordPrompts=0 -o ConnectionAttempts=1 -o StrictHostKeyChecking=accept-new -o ConnectTimeout=10 -o ServerAliveInterval=30 -o ServerAliveCountMax=2 ";
                return "ssh " + sshOptions + portPart + userHost + " \"" + body.Replace("\"", "\\\"") + "\"";
            }

            return "python train.py --tub \"" + dataPath + "\" --model \"" + modelPath + "\"" + extra;
        }

        private static string BuildBashTrainBody(string activateCommand, string workDir, string dataPath, string modelPath, string extraArgs)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(activateCommand))
            {
                parts.Add(activateCommand.Trim());
            }
            if (!string.IsNullOrWhiteSpace(workDir))
            {
                parts.Add("cd '" + EscapeBashSingleQuoted(workDir) + "'");
            }

            var command = "python train.py --tub '" + EscapeBashSingleQuoted(dataPath) + "' --model '" + EscapeBashSingleQuoted(modelPath) + "'";
            if (!string.IsNullOrWhiteSpace(extraArgs))
            {
                command += " " + extraArgs.Trim();
            }
            parts.Add(command);
            return string.Join(" && ", parts);
        }

        internal string ConvertPathForEnvironment(string path, string environment)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return path;
            }

            if (environment == "wsl")
            {
                return TryConvertWindowsPathToWsl(path, out var wslPath) ? wslPath : path.Replace('\\', '/');
            }

            if (environment == "virtualbox")
            {
                return ConvertWindowsSharedFolderPathToVirtualBoxPath(path);
            }

            return path;
        }

        private string CreateTrainingDataset(IReadOnlyList<FrameRecord> records, string modeName)
        {
            var safeMode = Regex.Replace(modeName, @"[^A-Za-z0-9가-힣_\-]+", "_").Trim('_');
            if (string.IsNullOrWhiteSpace(safeMode))
            {
                safeMode = "training";
            }

            var exportRoot = Path.Combine(dataFolder, "_training_sets", safeMode + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture));
            var exportImages = Path.Combine(exportRoot, "images");
            Directory.CreateDirectory(exportImages);

            var catalogPath = Path.Combine(exportRoot, "catalog_0.catalog");
            var catalogLines = new List<string>();
            var catalogLineLengths = new List<int>();
            var count = 0;
            foreach (var record in records)
            {
                var sourceImage = ResolveImagePath(record);
                if (!File.Exists(sourceImage))
                {
                    AppendLog("학습 데이터 이미지 누락: " + record.ImageFile);
                    continue;
                }

                if (!IsReadableImageFile(sourceImage))
                {
                    AppendLog("학습 데이터 이미지 손상 제외: " + record.ImageFile);
                    continue;
                }

                var extension = Path.GetExtension(sourceImage);
                if (string.IsNullOrWhiteSpace(extension))
                {
                    extension = ".jpg";
                }

                var exportedFileName = count.ToString("000000", CultureInfo.InvariantCulture) + "_" + Path.GetFileNameWithoutExtension(sourceImage) + extension;
                File.Copy(sourceImage, Path.Combine(exportImages, exportedFileName), true);

                // DonkeyCar v5 TubRecord는 base_path/images/<cam/image_array> 형태로 이미지를 찾는다.
                // 따라서 catalog에는 images/ 접두사를 넣지 않고 파일명만 기록한다.
                var line = BuildCatalogJsonForExport(record, exportedFileName, count);
                catalogLines.Add(line);
                catalogLineLengths.Add(line.Length + 1); // LF 기준. 아래 WriteTextLf()도 LF로 저장한다.
                count++;
            }

            if (count == 0)
            {
                throw new InvalidOperationException("학습 데이터셋에 복사할 수 있는 이미지가 없습니다.");
            }

            WriteTextLf(catalogPath, catalogLines);
            WriteCatalogManifestForExport(exportRoot, catalogLineLengths);
            WriteTubManifestForExport(exportRoot, count);

            AppendLog($"학습 데이터셋 생성: {count}개 -> {exportRoot}");
            return exportRoot;
        }

        private static bool IsReadableImageFile(string path)
        {
            try
            {
                using var image = Image.FromFile(path);
                return image.Width > 0 && image.Height > 0;
            }
            catch
            {
                return false;
            }
        }

        private void WriteTubManifestForExport(string exportRoot, int recordCount)
        {
            var sourceManifest = Path.Combine(dataFolder, "manifest.json");
            var manifestPath = Path.Combine(exportRoot, "manifest.json");
            var lines = ReadManifestHeaderLines(sourceManifest);

            var paths = new JsonArray();
            paths.Add("catalog_0.catalog");
            var deletedIndexes = new JsonArray();
            var catalogMetadata = new JsonObject
            {
                ["paths"] = paths,
                ["current_index"] = recordCount,
                ["max_len"] = Math.Max(1000, recordCount),
                ["deleted_indexes"] = deletedIndexes
            };

            lines.Add(catalogMetadata.ToJsonString(JsonOptions));
            WriteTextLf(manifestPath, lines);
        }

        private static List<string> ReadManifestHeaderLines(string sourceManifest)
        {
            var lines = new List<string>();
            if (File.Exists(sourceManifest))
            {
                foreach (var line in File.ReadLines(sourceManifest, Encoding.UTF8).Take(4))
                {
                    lines.Add(line.TrimEnd('\r', '\n'));
                }
            }

            while (lines.Count < 4)
            {
                lines.Add(lines.Count switch
                {
                    0 => "[\"cam/image_array\",\"user/angle\",\"user/throttle\",\"user/mode\"]",
                    1 => "[\"image_array\",\"float\",\"float\",\"str\"]",
                    2 => "{}",
                    _ => "{\"created_at\":" + DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture) + "}"
                });
            }

            return lines;
        }

        private static void WriteCatalogManifestForExport(string exportRoot, IReadOnlyList<int> catalogLineLengths)
        {
            var lineLengths = new JsonArray();
            foreach (var length in catalogLineLengths)
            {
                lineLengths.Add(length);
            }

            var manifest = new JsonObject
            {
                ["path"] = "catalog_0.catalog_manifest",
                ["created_at"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["start_index"] = 0,
                ["line_lengths"] = lineLengths
            };

            WriteTextLf(Path.Combine(exportRoot, "catalog_0.catalog_manifest"), new[] { manifest.ToJsonString(JsonOptions) });
        }

        private static void WriteTextLf(string path, IEnumerable<string> lines)
        {
            File.WriteAllText(path, string.Join("\n", lines) + "\n", new UTF8Encoding(false));
        }

        private static string BuildCatalogJsonForExport(FrameRecord record, string relativeImage, int newIndex)
        {
            JsonObject json;
            try
            {
                json = JsonNode.Parse(record.RawJson) as JsonObject ?? new JsonObject();
            }
            catch
            {
                json = new JsonObject();
            }

            json["_index"] = newIndex;
            json["cam/image_array"] = relativeImage;
            if (record.Angle.HasValue)
            {
                json["user/angle"] = record.Angle.Value;
            }
            if (record.Throttle.HasValue)
            {
                json["user/throttle"] = record.Throttle.Value;
            }
            if (!string.IsNullOrWhiteSpace(record.Mode))
            {
                json["user/mode"] = record.Mode;
            }
            return json.ToJsonString(JsonOptions);
        }

        internal static string ConvertWindowsSharedFolderPathToVirtualBoxPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return path;
            }

            var value = path.Trim();
            if (value.StartsWith("/", StringComparison.Ordinal))
            {
                return value.Replace('\\', '/');
            }

            var parts = value.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                if (part.Contains("virtualbox_share", StringComparison.OrdinalIgnoreCase) ||
                    part.Contains("vbox", StringComparison.OrdinalIgnoreCase) ||
                    part.Contains("share", StringComparison.OrdinalIgnoreCase))
                {
                    var remainder = string.Join('/', parts.Skip(i + 1));
                    var basePath = "/media/sf_" + part;
                    return string.IsNullOrWhiteSpace(remainder) ? basePath : basePath + "/" + remainder;
                }
            }

            return value.Replace('\\', '/');
        }

        private void ResetTrainingCommandState()
        {
            trainingUseSshPasswordInput = false;
            trainingSshPassword = string.Empty;
            trainingCommandGenerated = false;
            if (!chkManualCommandEdit.Checked)
            {
                txtTrainCommand.Text = TrainingCommandPlaceholder;
            }
            UpdateTrainingCommandEditState();
        }

        private void chkManualCommandEdit_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateTrainingCommandEditState();
        }

        private void UpdateTrainingCommandEditState()
        {
            txtTrainCommand.ReadOnly = !chkManualCommandEdit.Checked;
            txtTrainCommand.BackColor = chkManualCommandEdit.Checked ? Color.White : SystemColors.Control;
            if (chkManualCommandEdit.Checked)
            {
                if (!trainingCommandGenerated && IsTrainingCommandPlaceholder(txtTrainCommand.Text.Trim()))
                {
                    txtTrainCommand.Text = "donkey train --tub \"{DATA_FOLDER}\" --model \"{MODEL_PATH}\"";
                }
                lblHint.Text = "수동 편집 모드입니다. 직접 입력한 명령을 실행합니다.";
            }
            else
            {
                if (!trainingCommandGenerated)
                {
                    txtTrainCommand.Text = TrainingCommandPlaceholder;
                }
                lblHint.Text = "먼저 학습 명령 생성으로 환경/경로를 설정하세요. 수동 편집 체크 시에만 명령 수정 가능.";
            }
        }

        private static bool IsTrainingCommandPlaceholder(string command)
        {
            return string.IsNullOrWhiteSpace(command) ||
                   string.Equals(command, TrainingCommandPlaceholder, StringComparison.Ordinal) ||
                   command.StartsWith("먼저 [학습 명령 생성]", StringComparison.Ordinal);
        }

        private static bool IsSshCommand(string command)
        {
            var trimmed = command.TrimStart();
            return trimmed.Equals("ssh", StringComparison.OrdinalIgnoreCase) ||
                   trimmed.StartsWith("ssh ", StringComparison.OrdinalIgnoreCase) ||
                   trimmed.StartsWith("ssh.exe ", StringComparison.OrdinalIgnoreCase) ||
                   trimmed.StartsWith("\"ssh.exe\" ", StringComparison.OrdinalIgnoreCase);
        }

        private async void btnTrain_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(dataFolder) || !Directory.Exists(dataFolder))
            {
                MessageBox.Show("먼저 DonkeyCar 데이터를 불러오세요.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var command = txtTrainCommand.Text.Trim();
            if (!trainingCommandGenerated && !chkManualCommandEdit.Checked)
            {
                MessageBox.Show("먼저 [학습 명령 생성]을 눌러 학습 범위와 실행 환경을 선택하세요. 직접 입력하려면 [수동 편집]을 체크하세요.", "학습 명령 필요", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnTrainingPaths.PerformClick();
                return;
            }

            if (IsTrainingCommandPlaceholder(command))
            {
                MessageBox.Show("실행할 학습 명령이 아직 생성되지 않았습니다. [학습 명령 생성]을 누르거나 [수동 편집]을 체크한 뒤 명령을 직접 입력하세요.", "학습 명령 필요", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var trainingDataPath = GetTrainingDataPath();
            var trainingModelPath = GetTrainingModelPath();

            command = command
                .Replace("{ROOT_FOLDER}", rootFolder)
                .Replace("{DATA_FOLDER}", trainingDataPath)
                .Replace("{IMAGES_FOLDER}", imagesFolder)
                .Replace("{MODEL_PATH}", trainingModelPath)
                .Replace("{ROOT}", rootFolder)
                .Replace("{DATA}", trainingDataPath)
                .Replace("{IMAGES}", imagesFolder)
                .Replace("{MODEL}", trainingModelPath);

            btnTrain.Enabled = false;

            try
            {
                EnsureLocalModelDirectory(trainingModelPath);
                var preparedCommand = PrepareTrainingCommand(command);
                var shouldUseInteractiveSshPassword = StartsWithSshCommand(preparedCommand) &&
                    (trainingUseSshPasswordInput || ContainsSshPasswordBlockingOptions(preparedCommand));
                if (shouldUseInteractiveSshPassword)
                {
                    preparedCommand = EnableOpenSshPasswordPrompt(preparedCommand);
                    AppendLog("> " + MaskSensitiveCommand(preparedCommand));
                    AppendLog("SSH 비밀번호 입력형 실행: 열린 콘솔 창에서 VM 비밀번호를 입력하세요. 입력값은 WinForms에 저장되지 않습니다.");
                    var interactiveExitCode = await RunInteractiveConsoleCommandAsync(preparedCommand);
                    AppendLog($"프로세스 종료 코드: {interactiveExitCode}");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(trainingSshPassword))
                {
                    preparedCommand = EnableOpenSshPasswordPrompt(preparedCommand);
                }
                AppendLog("> " + MaskSensitiveCommand(preparedCommand));
                var exitCode = await RunCommandAsync(preparedCommand, string.IsNullOrWhiteSpace(trainingSshPassword) ? null : trainingSshPassword);
                AppendLog($"프로세스 종료 코드: {exitCode}");
            }
            catch (Exception ex)
            {
                AppendLog("실행 오류: " + ex.Message);
                MessageBox.Show($"학습 명령 실행 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTrain.Enabled = true;
            }
        }

        internal static string EnableOpenSshPasswordPrompt(string command)
        {
            var trimmed = command.TrimStart();
            if (!trimmed.StartsWith("ssh ", StringComparison.OrdinalIgnoreCase))
            {
                return command;
            }

            // SSH_ASKPASS cannot work when BatchMode disables every password prompt.
            // Older generated commands may still contain those options, so strip them just before execution.
            var normalized = Regex.Replace(command, @"(?i)\s+-o\s+BatchMode\s*=\s*yes", string.Empty);
            normalized = Regex.Replace(normalized, @"(?i)\s+-o\s+NumberOfPasswordPrompts\s*=\s*0", string.Empty);

            if (!Regex.IsMatch(normalized, @"(?i)\s+-o\s+NumberOfPasswordPrompts\s*="))
            {
                normalized = Regex.Replace(normalized, @"(?i)^\s*ssh\s+", "ssh -o NumberOfPasswordPrompts=1 ", RegexOptions.CultureInvariant);
            }
            if (!Regex.IsMatch(normalized, @"(?i)\s+-o\s+PreferredAuthentications\s*="))
            {
                normalized = Regex.Replace(normalized, @"(?i)^\s*ssh\s+", "ssh -o PreferredAuthentications=password ", RegexOptions.CultureInvariant);
            }
            if (!Regex.IsMatch(normalized, @"(?i)\s+-o\s+PubkeyAuthentication\s*="))
            {
                normalized = Regex.Replace(normalized, @"(?i)^\s*ssh\s+", "ssh -o PubkeyAuthentication=no ", RegexOptions.CultureInvariant);
            }
            if (!Regex.IsMatch(normalized, @"(?i)\s+-o\s+KbdInteractiveAuthentication\s*="))
            {
                normalized = Regex.Replace(normalized, @"(?i)^\s*ssh\s+", "ssh -o KbdInteractiveAuthentication=no ", RegexOptions.CultureInvariant);
            }

            return normalized;
        }

        private static bool StartsWithSshCommand(string command)
        {
            var trimmed = command.TrimStart();
            return trimmed.StartsWith("ssh ", StringComparison.OrdinalIgnoreCase) ||
                   trimmed.StartsWith("ssh.exe ", StringComparison.OrdinalIgnoreCase) ||
                   trimmed.Equals("ssh", StringComparison.OrdinalIgnoreCase) ||
                   trimmed.Equals("ssh.exe", StringComparison.OrdinalIgnoreCase);
        }

        private static bool ContainsSshPasswordBlockingOptions(string command)
        {
            return Regex.IsMatch(command, @"(?i)\s+-o\s+BatchMode\s*=\s*yes") ||
                   Regex.IsMatch(command, @"(?i)\s+-o\s+NumberOfPasswordPrompts\s*=\s*0");
        }

        internal Task<int> RunInteractiveConsoleCommandAsync(string command)
        {
            var workingDirectory = Directory.Exists(rootFolder) ? rootFolder : dataFolder;
            var shell = Environment.GetEnvironmentVariable("ComSpec");
            if (string.IsNullOrWhiteSpace(shell))
            {
                shell = "cmd.exe";
            }

            var commandWithExit = command + " & set TEAMAPP_EXIT=!ERRORLEVEL! & echo. & echo [TeamApp] Process exit code: !TEAMAPP_EXIT! & echo [TeamApp] Press any key to return to the UI... & pause > nul & exit /b !TEAMAPP_EXIT!";
            var startInfo = new ProcessStartInfo
            {
                FileName = shell,
                Arguments = "/D /V:ON /S /C " + commandWithExit,
                WorkingDirectory = workingDirectory,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Normal
            };

            return RunExternalProcessAsync(startInfo);
        }

        private static Task<int> RunExternalProcessAsync(ProcessStartInfo startInfo)
        {
            var completion = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            try
            {
                var process = new Process
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true
                };

                process.Exited += (_, _) =>
                {
                    try
                    {
                        completion.TrySetResult(process.ExitCode);
                    }
                    catch (Exception ex)
                    {
                        completion.TrySetException(ex);
                    }
                    finally
                    {
                        process.Dispose();
                    }
                };

                if (!process.Start())
                {
                    process.Dispose();
                    completion.TrySetException(new InvalidOperationException("프로세스를 시작하지 못했습니다."));
                }
            }
            catch (Exception ex)
            {
                completion.TrySetException(ex);
            }

            return completion.Task;
        }

        private string ResolveCommandSecrets(string command)
        {
            if (command.Contains(SshPasswordPlaceholder, StringComparison.Ordinal))
            {
                if (string.IsNullOrWhiteSpace(trainingSshPassword))
                {
                    throw new InvalidOperationException("SSH 비밀번호 자동입력이 켜져 있지만 비밀번호가 비어 있습니다. 학습 명령 생성을 다시 열어 비밀번호를 입력하거나 SSH 키 로그인을 사용하세요.");
                }

                return command.Replace(SshPasswordPlaceholder, trainingSshPassword);
            }

            return command;
        }

        internal static string MaskSensitiveCommand(string command)
        {
            var masked = command.Replace(SshPasswordPlaceholder, "********");
            return Regex.Replace(masked, "(?i)(-pw\\s+)(\"[^\"]*\"|\\S+)", "$1********");
        }

        private string PrepareTrainingCommand(string command)
        {
            if (!StartsWithDonkeyCommand(command))
            {
                return command;
            }

            if (TryResolveDonkeyExecutable(out var donkeyExecutable))
            {
                var remainder = GetCommandRemainderAfterDonkey(command);
                var resolved = QuoteCommandToken(donkeyExecutable) + remainder;
                if (!string.Equals(donkeyExecutable, "donkey", StringComparison.OrdinalIgnoreCase))
                {
                    AppendLog("donkey 실행파일 자동 연결: " + donkeyExecutable);
                }

                return resolved;
            }

            if (TryBuildWslDonkeyCommand(command, out var wslCommand))
            {
                AppendLog("Windows PATH에서 donkey를 찾지 못해 WSL donkey로 자동 전환합니다.");
                return wslCommand;
            }

            throw new InvalidOperationException(BuildDonkeyMissingGuide());
        }

        private void btnCheckDonkey_Click(object? sender, EventArgs e)
        {
            AppendLog("DonkeyCar 학습 환경 진단 시작");

            if (TryFindExecutableOnPath("donkey", out var pathDonkey))
            {
                AppendLog("PATH donkey: " + pathDonkey);
            }
            else
            {
                AppendLog("PATH donkey: 찾지 못함");
            }

            if (TryResolveDonkeyExecutable(out var resolvedDonkey))
            {
                AppendLog("사용 가능한 donkey 실행파일: " + resolvedDonkey);
            }
            else
            {
                AppendLog("사용 가능한 donkey 실행파일: 찾지 못함");
            }

            CheckPythonDonkeyCar("py");
            CheckPythonDonkeyCar("python");

            if (TryFindExecutableOnPath("wsl.exe", out var wslExe))
            {
                AppendLog("WSL: " + wslExe);
                if (TryRunProbe(wslExe, "bash -lc \"command -v donkey\"", 5000, out var wslOutput, out var wslExit) && wslExit == 0)
                {
                    AppendLog("WSL donkey: " + OneLine(wslOutput));
                }
                else
                {
                    AppendLog("WSL donkey: 찾지 못함");
                }
            }
            else
            {
                AppendLog("WSL: wsl.exe 찾지 못함");
            }

            AppendLog("진단 종료. donkey를 찾지 못하면 DonkeyCar가 설치된 Python/venv의 Scripts 폴더를 PATH에 추가하거나 학습 명령에 실행파일 전체 경로를 넣으세요.");
        }

        private void CheckPythonDonkeyCar(string pythonCommand)
        {
            const string code = "import donkeycar, sysconfig; print(sysconfig.get_path('scripts') or '')";
            var args = "-c \"" + code.Replace("\"", "\\\"") + "\"";
            if (TryRunProbe(pythonCommand, args, 5000, out var output, out var exitCode) && exitCode == 0)
            {
                var scriptsPath = OneLine(output);
                AppendLog($"{pythonCommand}: donkeycar import 가능, Scripts={scriptsPath}");
            }
            else
            {
                AppendLog($"{pythonCommand}: donkeycar import 불가 또는 Python 실행 불가");
            }
        }

        private static string BuildDonkeyMissingGuide()
        {
            return "donkey 명령을 찾지 못했습니다.\n\n" +
                   "해결 방법:\n" +
                   "1) DonkeyCar가 설치된 Python/venv/conda 환경의 Scripts 폴더를 PATH에 추가하세요.\n" +
                   "2) 또는 학습 명령의 donkey 대신 전체 경로를 넣으세요. 예: \"C:\\...\\Scripts\\donkey.exe\" train ...\n" +
                   "3) 가상환경을 써야 하면 명령을 call \"...\\activate.bat\" && donkey train ... 형태로 바꿔 실행하세요.\n" +
                   "4) WSL에 DonkeyCar가 설치되어 있으면 wsl.exe와 WSL 내부 donkey를 자동으로 사용합니다.";
        }

        private static bool StartsWithDonkeyCommand(string command)
        {
            var trimmed = command.TrimStart();
            return trimmed.Equals("donkey", StringComparison.OrdinalIgnoreCase) ||
                   trimmed.StartsWith("donkey ", StringComparison.OrdinalIgnoreCase) ||
                   trimmed.StartsWith("donkey\t", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetCommandRemainderAfterDonkey(string command)
        {
            var start = command.Length - command.TrimStart().Length;
            var index = start + "donkey".Length;
            return index < command.Length ? command.Substring(index) : string.Empty;
        }

        private bool TryResolveDonkeyExecutable(out string donkeyExecutable)
        {
            donkeyExecutable = string.Empty;

            var envDonkey = Environment.GetEnvironmentVariable("DONKEY_EXE");
            if (!string.IsNullOrWhiteSpace(envDonkey) && File.Exists(envDonkey))
            {
                donkeyExecutable = envDonkey;
                return true;
            }

            if (TryFindExecutableOnPath("donkey", out donkeyExecutable))
            {
                return true;
            }

            foreach (var candidate in EnumerateDonkeyExecutableCandidates())
            {
                if (File.Exists(candidate))
                {
                    donkeyExecutable = candidate;
                    return true;
                }
            }

            if (TryFindDonkeyFromPython("py", out donkeyExecutable))
            {
                return true;
            }

            if (TryFindDonkeyFromPython("python", out donkeyExecutable))
            {
                return true;
            }

            donkeyExecutable = string.Empty;
            return false;
        }

        private static bool TryFindDonkeyFromPython(string pythonCommand, out string donkeyExecutable)
        {
            donkeyExecutable = string.Empty;
            const string code = "import donkeycar, sysconfig; print(sysconfig.get_path('scripts') or '')";
            var args = "-c \"" + code.Replace("\"", "\\\"") + "\"";
            if (!TryRunProbe(pythonCommand, args, 5000, out var output, out var exitCode) || exitCode != 0)
            {
                return false;
            }

            var scriptsPath = OneLine(output);
            if (string.IsNullOrWhiteSpace(scriptsPath) || !Directory.Exists(scriptsPath))
            {
                return false;
            }

            foreach (var name in DonkeyExecutableNames())
            {
                var candidate = Path.Combine(scriptsPath, name);
                if (File.Exists(candidate))
                {
                    donkeyExecutable = candidate;
                    return true;
                }
            }

            return false;
        }

        private IEnumerable<string> EnumerateDonkeyExecutableCandidates()
        {
            var directories = new List<string>();
            void AddDirectory(string? directory)
            {
                if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory) && !directories.Contains(directory, StringComparer.OrdinalIgnoreCase))
                {
                    directories.Add(directory);
                }
            }

            void AddScriptsUnder(string? baseDirectory)
            {
                if (string.IsNullOrWhiteSpace(baseDirectory))
                {
                    return;
                }

                AddDirectory(Path.Combine(baseDirectory, "Scripts"));
                AddDirectory(Path.Combine(baseDirectory, ".venv", "Scripts"));
                AddDirectory(Path.Combine(baseDirectory, "venv", "Scripts"));
                AddDirectory(Path.Combine(baseDirectory, "env", "Scripts"));
            }

            AddScriptsUnder(rootFolder);
            AddScriptsUnder(dataFolder);
            AddDirectory(Path.Combine(Environment.CurrentDirectory, ".venv", "Scripts"));
            AddDirectory(Path.Combine(Environment.CurrentDirectory, "venv", "Scripts"));

            var condaPrefix = Environment.GetEnvironmentVariable("CONDA_PREFIX");
            AddDirectory(Path.Combine(condaPrefix ?? string.Empty, "Scripts"));

            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            foreach (var condaRootName in new[] { "anaconda3", "miniconda3", "mambaforge", "miniforge3" })
            {
                var condaRoot = Path.Combine(userProfile, condaRootName);
                AddDirectory(Path.Combine(condaRoot, "Scripts"));
                var envRoot = Path.Combine(condaRoot, "envs");
                if (Directory.Exists(envRoot))
                {
                    foreach (var env in Directory.GetDirectories(envRoot))
                    {
                        AddDirectory(Path.Combine(env, "Scripts"));
                    }
                }
            }

            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var pythonRoot = Path.Combine(localAppData, "Programs", "Python");
            if (Directory.Exists(pythonRoot))
            {
                foreach (var python in Directory.GetDirectories(pythonRoot, "Python*"))
                {
                    AddDirectory(Path.Combine(python, "Scripts"));
                }
            }

            foreach (var directory in directories)
            {
                foreach (var name in DonkeyExecutableNames())
                {
                    yield return Path.Combine(directory, name);
                }
            }
        }

        private static string[] DonkeyExecutableNames()
        {
            return new[] { "donkey.exe", "donkey.cmd", "donkey.bat", "donkey" };
        }

        private static bool TryFindExecutableOnPath(string commandName, out string executablePath)
        {
            executablePath = string.Empty;
            var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            var extensions = new[] { string.Empty, ".exe", ".cmd", ".bat" };
            var names = Path.HasExtension(commandName)
                ? new[] { commandName }
                : extensions.Select(extension => commandName + extension).ToArray();

            foreach (var rawDirectory in path.Split(Path.PathSeparator))
            {
                var directory = rawDirectory.Trim('"', ' ');
                if (string.IsNullOrWhiteSpace(directory))
                {
                    continue;
                }

                foreach (var name in names)
                {
                    var candidate = Path.Combine(directory, name);
                    if (File.Exists(candidate))
                    {
                        executablePath = candidate;
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool TryBuildWslDonkeyCommand(string command, out string wslCommand)
        {
            wslCommand = string.Empty;
            if (!TryFindExecutableOnPath("wsl.exe", out var wslExe))
            {
                return false;
            }

            if (!TryRunProbe(wslExe, "bash -lc \"command -v donkey\"", 5000, out _, out var exitCode) || exitCode != 0)
            {
                return false;
            }

            var bashCommand = ConvertQuotedWindowsPathsToBash(command);
            wslCommand = QuoteCommandToken(wslExe) + " bash -lc \"" + bashCommand.Replace("\"", "\\\"") + "\"";
            return true;
        }

        private static string ConvertQuotedWindowsPathsToBash(string command)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < command.Length; i++)
            {
                if (command[i] != '"')
                {
                    builder.Append(command[i]);
                    continue;
                }

                var end = command.IndexOf('"', i + 1);
                if (end < 0)
                {
                    builder.Append(command.Substring(i));
                    break;
                }

                var content = command.Substring(i + 1, end - i - 1);
                if (TryConvertWindowsPathToWsl(content, out var wslPath))
                {
                    builder.Append('\'').Append(EscapeBashSingleQuoted(wslPath)).Append('\'');
                }
                else
                {
                    builder.Append('\'').Append(EscapeBashSingleQuoted(content)).Append('\'');
                }

                i = end;
            }

            return builder.ToString();
        }

        private static bool TryConvertWindowsPathToWsl(string path, out string wslPath)
        {
            wslPath = string.Empty;
            if (path.Length < 3 || path[1] != ':' || (path[2] != '\\' && path[2] != '/'))
            {
                return false;
            }

            var drive = char.ToLowerInvariant(path[0]);
            var rest = path.Substring(3).Replace('\\', '/');
            wslPath = "/mnt/" + drive + "/" + rest;
            return true;
        }

        private static string EscapeBashSingleQuoted(string value)
        {
            return value.Replace("'", "'\\''");
        }

        private static string QuoteCommandToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "\"\"";
            }

            return value.Any(char.IsWhiteSpace) ? "\"" + value.Replace("\"", "\\\"") + "\"" : value;
        }

        private static bool RequiresShell(string command)
        {
            var trimmed = command.TrimStart();
            return ContainsShellOperatorOutsideQuotes(command) ||
                   trimmed.StartsWith("call ", StringComparison.OrdinalIgnoreCase) ||
                   trimmed.StartsWith("set ", StringComparison.OrdinalIgnoreCase);
        }

        private static bool ContainsShellOperatorOutsideQuotes(string command)
        {
            var inDoubleQuote = false;
            var inSingleQuote = false;
            for (var i = 0; i < command.Length; i++)
            {
                var ch = command[i];
                if (ch == '"' && !inSingleQuote)
                {
                    inDoubleQuote = !inDoubleQuote;
                    continue;
                }

                if (ch == '\'' && !inDoubleQuote)
                {
                    inSingleQuote = !inSingleQuote;
                    continue;
                }

                if (inDoubleQuote || inSingleQuote)
                {
                    continue;
                }

                if (ch == '>' || ch == '<')
                {
                    return true;
                }

                if ((ch == '&' || ch == '|' ) && i + 1 < command.Length && command[i + 1] == ch)
                {
                    return true;
                }

                if (ch == '|')
                {
                    return true;
                }
            }

            return false;
        }

        private Task<int> RunCommandAsync(string command, string? sshPassword = null)
        {
            var workingDirectory = Directory.Exists(rootFolder) ? rootFolder : dataFolder;
            if (!RequiresShell(command) && TryCreateDirectProcessStartInfo(command, workingDirectory, out var directStartInfo))
            {
                ApplySshAskPassIfNeeded(directStartInfo, sshPassword);
                var isPasswordSsh = !string.IsNullOrWhiteSpace(sshPassword) && IsSshCommand(command);
                if (isPasswordSsh)
                {
                    directStartInfo.RedirectStandardInput = true;
                }
                return RunProcessAsync(directStartInfo, isPasswordSsh ? sshPassword : null, isPasswordSsh);
            }

            var shell = Environment.GetEnvironmentVariable("ComSpec");
            if (string.IsNullOrWhiteSpace(shell))
            {
                shell = "cmd.exe";
            }

            var shellStartInfo = new ProcessStartInfo
            {
                FileName = shell,
                Arguments = "/D /S /C " + command,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            ApplySshAskPassIfNeeded(shellStartInfo, sshPassword);
            var shellPasswordSsh = !string.IsNullOrWhiteSpace(sshPassword) && IsSshCommand(command);
            if (shellPasswordSsh)
            {
                shellStartInfo.RedirectStandardInput = true;
            }

            return RunProcessAsync(shellStartInfo, shellPasswordSsh ? sshPassword : null, shellPasswordSsh);
        }

        private void ApplySshAskPassIfNeeded(ProcessStartInfo startInfo, string? sshPassword)
        {
            if (string.IsNullOrWhiteSpace(sshPassword))
            {
                return;
            }

            var askPassDir = Path.Combine(Path.GetTempPath(), "TeamAppSshAskPass");
            Directory.CreateDirectory(askPassDir);
            var scriptFile = Path.Combine(askPassDir, "askpass.ps1");
            var cmdFile = Path.Combine(askPassDir, "askpass.cmd");

            File.WriteAllText(scriptFile,
                "$p = [Environment]::GetEnvironmentVariable('TEAMAPP_SSH_PASSWORD')\r\n" +
                "[Console]::Out.Write($p)",
                new UTF8Encoding(false));
            File.WriteAllText(cmdFile,
                "@echo off\r\n" +
                "powershell.exe -NoProfile -ExecutionPolicy Bypass -File \"" + scriptFile + "\"\r\n",
                new UTF8Encoding(false));

            startInfo.Environment["TEAMAPP_SSH_PASSWORD"] = sshPassword;
            startInfo.Environment["SSH_ASKPASS"] = cmdFile;
            startInfo.Environment["SSH_ASKPASS_REQUIRE"] = "force";
            startInfo.Environment["DISPLAY"] = "teamapp:0";
            startInfo.Environment["MSYS2_ARG_CONV_EXCL"] = "*";
        }

        private static bool TryCreateDirectProcessStartInfo(string command, string workingDirectory, out ProcessStartInfo startInfo)
        {
            startInfo = null!;
            var tokens = SplitCommandLine(command);
            if (tokens.Count == 0)
            {
                return false;
            }

            var fileName = tokens[0];
            var extension = Path.GetExtension(fileName);
            if (string.Equals(extension, ".cmd", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(extension, ".bat", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            for (var i = 1; i < tokens.Count; i++)
            {
                startInfo.ArgumentList.Add(tokens[i]);
            }

            return true;
        }

        private static List<string> SplitCommandLine(string command)
        {
            var tokens = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < command.Length; i++)
            {
                var c = command[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (char.IsWhiteSpace(c) && !inQuotes)
                {
                    if (current.Length > 0)
                    {
                        tokens.Add(current.ToString());
                        current.Clear();
                    }
                    continue;
                }

                current.Append(c);
            }

            if (current.Length > 0)
            {
                tokens.Add(current.ToString());
            }

            return tokens;
        }

        private async Task<int> RunProcessAsync(ProcessStartInfo startInfo, string? stdinPassword = null, bool enableSshAuthTimeout = false)
        {
            using var process = new Process
            {
                StartInfo = startInfo
            };

            try
            {
                if (!process.Start())
                {
                    throw new InvalidOperationException("프로세스를 시작하지 못했습니다.");
                }
            }
            catch
            {
                process.Dispose();
                throw;
            }

            var lastOutputUtc = DateTime.UtcNow;
            var outputTask = Task.Run(() => ReadProcessStream(process.StandardOutput, false, () => lastOutputUtc = DateTime.UtcNow));
            var errorTask = Task.Run(() => ReadProcessStream(process.StandardError, true, () => lastOutputUtc = DateTime.UtcNow));

            Task? passwordFallbackTask = null;
            if (!string.IsNullOrWhiteSpace(stdinPassword) && startInfo.RedirectStandardInput)
            {
                passwordFallbackTask = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(900).ConfigureAwait(false);
                        if (!process.HasExited)
                        {
                            process.StandardInput.WriteLine(stdinPassword);
                            process.StandardInput.Flush();
                        }
                    }
                    catch
                    {
                        // OpenSSH가 stdin 비밀번호 입력을 지원하지 않는 경우가 있어도 AskPass가 우선 처리하므로 무시한다.
                    }
                });
            }

            var exitTask = Task.Run(() => process.WaitForExit());
            if (enableSshAuthTimeout)
            {
                while (!exitTask.IsCompleted)
                {
                    var completed = await Task.WhenAny(exitTask, Task.Delay(5000)).ConfigureAwait(false);
                    if (completed == exitTask)
                    {
                        break;
                    }

                    var idleSeconds = (DateTime.UtcNow - lastOutputUtc).TotalSeconds;
                    if (idleSeconds >= 60)
                    {
                        try
                        {
                            process.Kill(true);
                        }
                        catch
                        {
                            // 이미 종료된 경우 무시한다.
                        }

                        throw new InvalidOperationException("SSH 비밀번호 입력 대기 또는 인증 지연으로 보이는 상태가 60초 이상 지속되어 실행을 중단했습니다. 학습 명령 생성 창에서 SSH 비밀번호와 자동입력을 확인하거나 PowerShell에서 ssh 접속이 되는지 먼저 확인하세요.");
                    }
                }
            }
            else
            {
                await exitTask.ConfigureAwait(false);
            }

            if (passwordFallbackTask != null)
            {
                await Task.WhenAny(passwordFallbackTask, Task.Delay(1000)).ConfigureAwait(false);
            }
            await Task.WhenAll(outputTask, errorTask).ConfigureAwait(false);
            return process.ExitCode;
        }

        private void ReadProcessStream(StreamReader reader, bool isError, Action? markActivity = null)
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                markActivity?.Invoke();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                AppendLog(isError ? "ERR: " + line : line);
            }
        }

        private static bool TryRunProbe(string fileName, string arguments, int timeoutMilliseconds, out string output, out int exitCode)
        {
            output = string.Empty;
            exitCode = -1;

            try
            {
                if (!Path.IsPathRooted(fileName) && !Path.HasExtension(fileName) && TryFindExecutableOnPath(fileName, out var resolvedFileName))
                {
                    fileName = resolvedFileName;
                }

                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                if (!process.Start())
                {
                    return false;
                }

                if (!process.WaitForExit(timeoutMilliseconds))
                {
                    try
                    {
                        process.Kill(true);
                    }
                    catch
                    {
                        // probe 정리 실패는 무시한다.
                    }

                    return false;
                }

                output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
                exitCode = process.ExitCode;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string OneLine(string text)
        {
            return (text ?? string.Empty)
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .FirstOrDefault(line => !string.IsNullOrWhiteSpace(line)) ?? string.Empty;
        }

        private void AppendLog(string text)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.BeginInvoke(new Action<string>(AppendLog), text);
                return;
            }

            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {text}{Environment.NewLine}");
        }

        private static bool TryParseNullableDouble(string text, out double? value)
        {
            value = null;
            var trimmed = text.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return true;
            }

            if (double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ||
                double.TryParse(trimmed, NumberStyles.Float, CultureInfo.CurrentCulture, out parsed))
            {
                value = parsed;
                return true;
            }

            return false;
        }

        private static string ToListText(FrameRecord record)
        {
            var flags = string.Empty;
            if (record.Deleted) flags += "삭제 ";
            if (record.Edited) flags += "편집 ";
            if (record.IsAnomaly) flags += "이상 ";
            if (string.IsNullOrWhiteSpace(flags)) flags = "정상 ";
            var fileName = Path.GetFileName(record.ImageFile);
            return $"{flags.Trim(),-6} | {record.Index,5} | a={FormatCompact(record.Angle),7} | t={FormatCompact(record.Throttle),7} | {fileName}";
        }

        private static string FormatCompact(double? value)
        {
            return value.HasValue ? value.Value.ToString("0.###", CultureInfo.InvariantCulture) : "-";
        }

        private static string FormatForInput(double? value)
        {
            return value.HasValue ? value.Value.ToString("0.######", CultureInfo.InvariantCulture) : string.Empty;
        }

        private static string NullIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : value;
        }

        private sealed class FrameRecord
        {
            public int GlobalOrder { get; set; }
            public int OriginalLineNumber { get; set; }
            public string CatalogPath { get; set; } = string.Empty;
            public string RawJson { get; set; } = string.Empty;
            public int Index { get; set; }
            public string SessionId { get; set; } = string.Empty;
            public long? TimestampMs { get; set; }
            public string ImageFile { get; set; } = string.Empty;
            public double? Angle { get; set; }
            public double? Throttle { get; set; }
            public string Mode { get; set; } = string.Empty;
            public bool Deleted { get; set; }
            public string DeletedBackupPath { get; set; } = string.Empty;
            public bool Edited { get; set; }
            public bool IsAnomaly { get; set; }
            public double? MovingAverage { get; set; }
            public double? Volatility { get; set; }
            public double AnomalyScore { get; set; }
        }
    }

    internal readonly struct FrameListVisualState
    {
        public static readonly FrameListVisualState Normal = new FrameListVisualState(false, false, false);

        public FrameListVisualState(bool deleted, bool edited, bool anomaly)
        {
            Deleted = deleted;
            Edited = edited;
            Anomaly = anomaly;
        }

        public bool Deleted { get; }
        public bool Edited { get; }
        public bool Anomaly { get; }
    }

    internal sealed class ColoredCheckedListBox : CheckedListBox
    {
        public ColoredCheckedListBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            CheckOnClick = false;
        }

        public Func<int, FrameListVisualState>? ResolveVisualState { get; set; }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            var index = IndexFromPoint(e.Location);
            if (index >= 0 && index < Items.Count && IsPointInsideCheckBox(index, e.Location))
            {
                SelectedIndex = index;
                SetItemChecked(index, !GetItemChecked(index));
                Invalidate(GetItemRectangle(index));
                return;
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            var index = IndexFromPoint(e.Location);
            if (index >= 0 && index < Items.Count && !IsPointInsideCheckBox(index, e.Location))
            {
                SelectedIndex = index;
                return;
            }

            base.OnMouseDoubleClick(e);
        }

        private bool IsPointInsideCheckBox(int index, Point point)
        {
            if (index < 0 || index >= Items.Count)
            {
                return false;
            }

            var itemBounds = GetItemRectangle(index);
            using var graphics = CreateGraphics();
            var glyphSize = CheckBoxRenderer.GetGlyphSize(graphics, CheckBoxState.UncheckedNormal);
            var glyphBounds = new Rectangle(
                itemBounds.Left + 3,
                itemBounds.Top + Math.Max(0, (itemBounds.Height - glyphSize.Height) / 2),
                glyphSize.Width + 6,
                glyphSize.Height);
            return glyphBounds.Contains(point);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= Items.Count)
            {
                return;
            }

            var state = ResolveVisualState?.Invoke(e.Index) ?? FrameListVisualState.Normal;
            var selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            var backColor = selected ? SystemColors.Highlight : BackColor;
            var foreColor = selected ? SystemColors.HighlightText : ForeColor;

            if (state.Deleted)
            {
                backColor = selected ? Color.Firebrick : Color.LightCoral;
                foreColor = selected ? Color.White : Color.DarkRed;
            }
            else if (state.Edited)
            {
                backColor = selected ? Color.SeaGreen : Color.LightGreen;
                foreColor = selected ? Color.White : Color.DarkGreen;
            }
            else if (state.Anomaly && !selected)
            {
                foreColor = Color.Red;
            }

            using (var background = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(background, e.Bounds);
            }

            var isChecked = GetItemChecked(e.Index);
            var checkBoxState = isChecked ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal;
            var glyphSize = CheckBoxRenderer.GetGlyphSize(e.Graphics, checkBoxState);
            var glyphLocation = new Point(e.Bounds.Left + 3, e.Bounds.Top + Math.Max(0, (e.Bounds.Height - glyphSize.Height) / 2));
            CheckBoxRenderer.DrawCheckBox(e.Graphics, glyphLocation, checkBoxState);

            var text = Items[e.Index]?.ToString() ?? string.Empty;
            var textRect = new Rectangle(e.Bounds.Left + glyphSize.Width + 10, e.Bounds.Top, Math.Max(10, e.Bounds.Width - glyphSize.Width - 14), e.Bounds.Height);
            TextRenderer.DrawText(
                e.Graphics,
                text,
                e.Font ?? Font,
                textRect,
                foreColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);

            e.DrawFocusRectangle();
        }
    }


}
