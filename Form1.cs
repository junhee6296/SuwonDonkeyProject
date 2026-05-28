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
            Resize += (_, _) => ApplyResponsiveLayout();

            autoPlayTimer.Interval = 250;
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
                btnOpenDataFolder.SetBounds(clientWidth - margin - 190, top, 190, topHeight);
                txtSelectedFolder.SetBounds(btnOpenFolder.Right + gap, top + 3, Math.Max(220, btnOpenDataFolder.Left - btnOpenFolder.Right - gap * 2), 23);

                grpList.SetBounds(margin, contentTop, leftWidth, contentHeight);
                grpPreview.SetBounds(grpList.Right + gap, contentTop, Math.Max(360, centerWidth), contentHeight);
                var rightX = grpPreview.Right + gap;
                grpFilter.SetBounds(rightX, contentTop, rightWidth, 245);
                grpAnomaly.SetBounds(rightX, grpFilter.Bottom + gap, rightWidth, 190);
                grpTrain.SetBounds(rightX, grpAnomaly.Bottom + gap, rightWidth, 210);
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
            btnCheckAllFrames.SetBounds(10, 24, Math.Max(96, (w - 30) / 2), 28);
            btnClearCheckedFrames.SetBounds(btnCheckAllFrames.Right + 8, 24, Math.Max(96, w - btnCheckAllFrames.Right - 18), 28);
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
            var pictureHeight = Math.Max(220, h - 24 - editHeight - 20 - 45 - 36 - bottomInfoHeight - graphHeight);

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

            pnlTimeline.SetBounds(12, grpImageEdit.Bottom + 8, innerW, 18);
            trbFrame.SetBounds(12, pnlTimeline.Bottom + 2, innerW, 45);

            var btnTop = trbFrame.Bottom + 2;
            var gap = 6;
            var buttonWidth = Math.Max(42, (innerW - gap * 5) / 6);
            btnPrev.SetBounds(12, btnTop, buttonWidth, 30);
            btnPlay.SetBounds(btnPrev.Right + gap, btnTop, buttonWidth, 30);
            btnNext.SetBounds(btnPlay.Right + gap, btnTop, buttonWidth, 30);
            btnUndo.SetBounds(btnNext.Right + gap, btnTop, buttonWidth, 30);
            btnDelete.SetBounds(btnUndo.Right + gap, btnTop, buttonWidth, 30);
            btnSave.SetBounds(btnDelete.Right + gap, btnTop, buttonWidth, 30);

            var infoTop = btnTop + 38;
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
            chkThrottlePositive.SetBounds(14, 28, 170, 24);
            chkExcludeAngleZero.SetBounds(14, 56, 120, 24);
            chkAngleRange.SetBounds(14, 90, 110, 24);
            chkThrottleRange.SetBounds(14, 124, 120, 24);
            chkAnomalyOnly.SetBounds(14, 156, 160, 24);
            chkDeletedOnly.SetBounds(180, 156, 130, 24);
            chkEditedOnly.SetBounds(180, 184, 130, 24);
            lblRangeMin.SetBounds(Math.Max(130, w - 250), 72, 50, 18);
            lblRangeMax.SetBounds(Math.Max(240, w - 130), 72, 50, 18);
            numAngleMin.SetBounds(lblRangeMin.Left, 90, 95, 23);
            numAngleMax.SetBounds(lblRangeMax.Left, 90, 95, 23);
            numThrottleMin.SetBounds(lblRangeMin.Left, 124, 95, 23);
            numThrottleMax.SetBounds(lblRangeMax.Left, 124, 95, 23);
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
            lblCommand.SetBounds(14, 25, 100, 20);
            txtTrainCommand.SetBounds(14, 48, Math.Max(180, w - 28), Math.Max(58, grpTrain.ClientSize.Height - 132));
            var btnTop = txtTrainCommand.Bottom + 8;
            btnTrain.SetBounds(14, btnTop, 90, 30);
            btnCheckDonkey.SetBounds(112, btnTop, 90, 30);
            btnTrainingPaths.SetBounds(210, btnTop, Math.Max(130, w - 224), 30);
            lblHint.SetBounds(14, btnTop + 36, Math.Max(180, w - 28), 34);
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
            if (chkDeletedOnly.Checked && !record.Deleted)
            {
                return false;
            }

            if (chkEditedOnly.Checked && !record.Edited)
            {
                return false;
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
            if (e.Index < 0)
            {
                return;
            }

            var record = e.Index < visibleFrames.Count ? visibleFrames[e.Index] : null;
            var selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            var backColor = selected ? SystemColors.Highlight : e.BackColor;
            var foreColor = selected ? SystemColors.HighlightText : e.ForeColor;

            if (!selected && record != null)
            {
                if (record.Deleted)
                {
                    backColor = Color.MistyRose;
                    foreColor = Color.DarkRed;
                }
                else if (record.Edited)
                {
                    backColor = Color.Honeydew;
                    foreColor = Color.DarkGreen;
                }
                else if (record.IsAnomaly)
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
            var targets = GetTargetRecordsForBatch();
            if (targets.Count == 0)
            {
                MessageBox.Show("삭제 표시할 프레임을 체크하거나 선택하세요.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"선택한 {targets.Count}개 프레임을 삭제 표시할까요?\n실제 이미지와 catalog 파일은 지우지 않고 목록에서 빨간색으로 표시합니다.",
                "삭제 표시 확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            foreach (var record in targets)
            {
                record.Deleted = true;
            }

            SaveUiMarks();
            RecalculateAnomaliesIfNeeded();
            ApplyFilters(targets[0].GlobalOrder);
            AppendLog($"삭제 표시: {targets.Count}개 프레임");
        }

        private void btnUndo_Click(object? sender, EventArgs e)
        {
            var targets = GetTargetRecordsForBatch()
                .Where(record => record.Deleted)
                .ToList();

            if (targets.Count == 0)
            {
                MessageBox.Show("복구할 삭제 표시 프레임을 체크하거나 선택하세요.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            foreach (var record in targets)
            {
                record.Deleted = false;
            }

            SaveUiMarks();
            RecalculateAnomaliesIfNeeded();
            ApplyFilters(targets[0].GlobalOrder);
            AppendLog($"삭제 표시 복구: {targets.Count}개 프레임");
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
                foreach (var record in allFrames)
                {
                    var key = GetFrameMarkKey(record);
                    record.Deleted = deleted.Contains(key);
                    record.Edited = edited.Contains(key);
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
            foreach (var record in allFrames.Where(record => record.Deleted).OrderBy(record => record.GlobalOrder))
            {
                deleted.Add(GetFrameMarkKey(record));
            }

            var edited = new JsonArray();
            foreach (var record in allFrames.Where(record => record.Edited).OrderBy(record => record.GlobalOrder))
            {
                edited.Add(GetFrameMarkKey(record));
            }

            var root = new JsonObject
            {
                ["deleted"] = deleted,
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

            lblStats.Text = $"전체 {active.Count}개 / 표시 {visibleFrames.Count}개\n" +
                            $"평균 angle {avgAngle}\n" +
                            $"평균 throttle {avgThrottle}\n" +
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
            if (!TryGetSelectedBitmapAndRect(out var bitmap, out var rect))
            {
                return;
            }

            var targets = GetTargetRecordsForBatch();
            if (targets.Count == 0)
            {
                return;
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
            AppendLog($"이미지 영역 가리기 적용: {success}/{targets.Count}개, 영역={rect}");
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

        private void btnTrainingPaths_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(dataFolder) || !Directory.Exists(dataFolder))
            {
                MessageBox.Show("먼저 DonkeyCar 데이터를 불러오세요.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            RefreshTrainingPathDefaults(false);

            using var dlg = new Form
            {
                Text = "AI 학습 명령 생성",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MinimizeBox = false,
                MaximizeBox = false,
                ClientSize = new Size(880, 610)
            };

            var lblInfo = new Label
            {
                Location = new Point(14, 12),
                Size = new Size(850, 38),
                Text = "학습 데이터 범위와 실행 환경을 선택하면 경로를 환경에 맞게 변환하고 donkey train 명령을 자동 생성합니다. 삭제 표시된 프레임은 학습 데이터셋에서 제외됩니다."
            };

            var lblDatasetMode = new Label { Location = new Point(14, 62), Size = new Size(130, 22), Text = "학습 데이터 범위" };
            var cmbDatasetMode = new ComboBox
            {
                Location = new Point(150, 60),
                Size = new Size(360, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbDatasetMode.Items.AddRange(new object[]
            {
                "이상치 포함 전체 데이터 학습",
                "이상치 제외 학습",
                "데이터 필터링 선택군만 학습"
            });
            cmbDatasetMode.SelectedIndex = 0;

            var chkFilterExcludeAnomaly = new CheckBox
            {
                Location = new Point(530, 60),
                Size = new Size(310, 24),
                Text = "선택군에서도 이상치 제외",
                Checked = false
            };

            var lblEnv = new Label { Location = new Point(14, 98), Size = new Size(130, 22), Text = "실행 환경" };
            var cmbEnvironment = new ComboBox
            {
                Location = new Point(150, 96),
                Size = new Size(220, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbEnvironment.Items.AddRange(new object[] { "Windows 환경 빌드", "WSL 환경 빌드", "VirtualBox 환경 빌드" });
            cmbEnvironment.SelectedIndex = 2;

            var lblData = new Label { Location = new Point(14, 138), Size = new Size(130, 22), Text = "학습 data/tub" };
            var txtData = new TextBox { Location = new Point(150, 136), Size = new Size(570, 23), Text = GetTrainingDataPath() };
            var btnBrowseData = new Button { Location = new Point(730, 134), Size = new Size(110, 28), Text = "찾아보기" };

            var lblModel = new Label { Location = new Point(14, 174), Size = new Size(130, 22), Text = "모델 저장 경로" };
            var txtModel = new TextBox { Location = new Point(150, 172), Size = new Size(570, 23), Text = GetTrainingModelPath() };
            var btnBrowseModel = new Button { Location = new Point(730, 170), Size = new Size(110, 28), Text = "저장 위치" };

            var lblExtra = new Label { Location = new Point(14, 210), Size = new Size(130, 22), Text = "추가/보정 인자" };
            var txtExtraArgs = new TextBox { Location = new Point(150, 208), Size = new Size(570, 23), Text = string.Empty };
            var lblExtraHint = new Label { Location = new Point(730, 210), Size = new Size(125, 22), Text = "예: --type linear" };

            var grpRemote = new GroupBox
            {
                Location = new Point(14, 246),
                Size = new Size(840, 112),
                Text = "WSL / VirtualBox 실행 옵션"
            };
            var lblActivate = new Label { Location = new Point(12, 26), Size = new Size(120, 22), Text = "환경 활성화" };
            var txtActivate = new TextBox { Location = new Point(130, 24), Size = new Size(330, 23), Text = "source ~/miniconda3/bin/activate e2e_env" };
            var lblSshUser = new Label { Location = new Point(12, 62), Size = new Size(70, 22), Text = "SSH user" };
            var txtSshUser = new TextBox { Location = new Point(82, 60), Size = new Size(110, 23), Text = "xytron" };
            var lblSshHost = new Label { Location = new Point(205, 62), Size = new Size(70, 22), Text = "host" };
            var txtSshHost = new TextBox { Location = new Point(250, 60), Size = new Size(120, 23), Text = "127.0.0.1" };
            var lblSshPort = new Label { Location = new Point(383, 62), Size = new Size(40, 22), Text = "port" };
            var txtSshPort = new TextBox { Location = new Point(425, 60), Size = new Size(70, 23), Text = "2222" };
            var lblRemoteWork = new Label { Location = new Point(510, 26), Size = new Size(95, 22), Text = "원격 mycar" };
            var txtRemoteWork = new TextBox { Location = new Point(605, 24), Size = new Size(220, 23), Text = ConvertWindowsSharedFolderPathToVirtualBoxPath(rootFolder) };
            var lblRemoteHint = new Label { Location = new Point(510, 62), Size = new Size(320, 36), Text = "VirtualBox는 포트포워딩 SSH 기준입니다. mycar/data가 공유폴더라면 /media/sf_... 경로를 사용하세요." };
            grpRemote.Controls.AddRange(new Control[] { lblActivate, txtActivate, lblSshUser, txtSshUser, lblSshHost, txtSshHost, lblSshPort, txtSshPort, lblRemoteWork, txtRemoteWork, lblRemoteHint });

            var btnUseLoaded = new Button { Location = new Point(150, 372), Size = new Size(135, 30), Text = "현재 로드 경로" };
            var btnConvertWsl = new Button { Location = new Point(294, 372), Size = new Size(135, 30), Text = "WSL 경로 변환" };
            var btnConvertVBox = new Button { Location = new Point(438, 372), Size = new Size(160, 30), Text = "VirtualBox 경로 변환" };
            var btnExportSet = new Button { Location = new Point(608, 372), Size = new Size(152, 30), Text = "학습 데이터 생성" };
            var btnBuild = new Button { Location = new Point(770, 372), Size = new Size(84, 30), Text = "명령 생성" };

            var lblPreviewTitle = new Label { Location = new Point(14, 418), Size = new Size(130, 22), Text = "명령 미리보기" };
            var txtPreview = new TextBox
            {
                Location = new Point(150, 416),
                Size = new Size(704, 88),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true
            };

            var lblGuide = new Label
            {
                Location = new Point(150, 512),
                Size = new Size(704, 42),
                Text = "Windows: C:\\... 경로 / WSL: /mnt/c/... 경로 / VirtualBox: ssh + /media/sf_... 경로로 생성됩니다. 생성된 _training_sets 폴더는 공유폴더 안에 만들어집니다."
            };

            var btnOk = new Button { Location = new Point(638, 568), Size = new Size(100, 32), Text = "적용", DialogResult = DialogResult.OK };
            var btnCancel = new Button { Location = new Point(748, 568), Size = new Size(100, 32), Text = "취소", DialogResult = DialogResult.Cancel };

            List<FrameRecord> SelectTrainingRecords()
            {
                if ((cmbDatasetMode.SelectedIndex == 1 || (cmbDatasetMode.SelectedIndex == 2 && chkFilterExcludeAnomaly.Checked)) &&
                    !allFrames.Any(record => record.IsAnomaly || record.MovingAverage.HasValue || record.Volatility.HasValue))
                {
                    DetectAnomalies(false);
                }

                var records = cmbDatasetMode.SelectedIndex switch
                {
                    1 => allFrames.Where(record => !record.Deleted && !record.IsAnomaly).ToList(),
                    2 => visibleFrames.Where(record => !record.Deleted).ToList(),
                    _ => allFrames.Where(record => !record.Deleted).ToList()
                };

                if (cmbDatasetMode.SelectedIndex == 2 && chkFilterExcludeAnomaly.Checked)
                {
                    records = records.Where(record => !record.IsAnomaly).ToList();
                }

                return records.OrderBy(record => record.GlobalOrder).ToList();
            }

            string EnvironmentName()
            {
                return cmbEnvironment.SelectedIndex switch
                {
                    1 => "wsl",
                    2 => "virtualbox",
                    _ => "windows"
                };
            }

            void RefreshPreview()
            {
                txtPreview.Text = BuildTrainingCommand(
                    EnvironmentName(),
                    txtData.Text.Trim(),
                    txtModel.Text.Trim(),
                    txtExtraArgs.Text.Trim(),
                    txtActivate.Text.Trim(),
                    txtSshUser.Text.Trim(),
                    txtSshHost.Text.Trim(),
                    txtSshPort.Text.Trim(),
                    txtRemoteWork.Text.Trim());
            }

            void ExportAndSetPath(bool showMessage)
            {
                var records = SelectTrainingRecords();
                if (records.Count == 0)
                {
                    MessageBox.Show("학습 데이터셋으로 내보낼 프레임이 없습니다.", "데이터 없음", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var exportPath = CreateTrainingDataset(records, cmbDatasetMode.Text);
                var env = EnvironmentName();
                txtData.Text = ConvertPathForEnvironment(exportPath, env);
                var modelPath = Path.Combine(Path.GetDirectoryName(exportPath) ?? dataFolder, "models", "mypilot.h5");
                txtModel.Text = ConvertPathForEnvironment(modelPath, env);
                if (env == "virtualbox")
                {
                    txtRemoteWork.Text = ConvertWindowsSharedFolderPathToVirtualBoxPath(rootFolder);
                }
                RefreshPreview();
                if (showMessage)
                {
                    MessageBox.Show($"학습 데이터 {records.Count}개를 생성했습니다.\n{exportPath}", "학습 데이터 생성", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            btnBrowseData.Click += (_, _) =>
            {
                using var folderDialog = new FolderBrowserDialog
                {
                    Description = "학습에 사용할 data/tub 폴더를 선택하세요",
                    UseDescriptionForTitle = true,
                    SelectedPath = Directory.Exists(txtData.Text) ? txtData.Text : (Directory.Exists(dataFolder) ? dataFolder : string.Empty)
                };

                if (folderDialog.ShowDialog(dlg) == DialogResult.OK)
                {
                    txtData.Text = folderDialog.SelectedPath;
                    RefreshPreview();
                }
            };

            btnBrowseModel.Click += (_, _) =>
            {
                using var saveDialog = new SaveFileDialog
                {
                    Title = "학습 모델 저장 경로",
                    Filter = "Keras/TensorFlow model (*.h5;*.keras)|*.h5;*.keras|All files (*.*)|*.*",
                    FileName = Path.GetFileName(txtModel.Text)
                };

                var currentModelDirectory = Path.GetDirectoryName(txtModel.Text);
                if (!string.IsNullOrWhiteSpace(currentModelDirectory) && Directory.Exists(currentModelDirectory))
                {
                    saveDialog.InitialDirectory = currentModelDirectory;
                }

                if (saveDialog.ShowDialog(dlg) == DialogResult.OK)
                {
                    txtModel.Text = saveDialog.FileName;
                    RefreshPreview();
                }
            };

            btnUseLoaded.Click += (_, _) =>
            {
                txtData.Text = dataFolder;
                var modelRoot = string.IsNullOrWhiteSpace(rootFolder) ? dataFolder : rootFolder;
                txtModel.Text = Path.Combine(modelRoot, "models", "mypilot.h5");
                RefreshPreview();
            };

            btnConvertWsl.Click += (_, _) =>
            {
                txtData.Text = ConvertPathForEnvironment(txtData.Text, "wsl");
                txtModel.Text = ConvertPathForEnvironment(txtModel.Text, "wsl");
                RefreshPreview();
            };

            btnConvertVBox.Click += (_, _) =>
            {
                txtData.Text = ConvertPathForEnvironment(txtData.Text, "virtualbox");
                txtModel.Text = ConvertPathForEnvironment(txtModel.Text, "virtualbox");
                txtRemoteWork.Text = ConvertPathForEnvironment(rootFolder, "virtualbox");
                RefreshPreview();
            };

            btnExportSet.Click += (_, _) => ExportAndSetPath(true);
            btnBuild.Click += (_, _) =>
            {
                if (cmbDatasetMode.SelectedIndex != 0 || allFrames.Any(record => record.Deleted))
                {
                    ExportAndSetPath(false);
                }
                RefreshPreview();
            };

            cmbEnvironment.SelectedIndexChanged += (_, _) =>
            {
                var env = EnvironmentName();
                txtData.Text = ConvertPathForEnvironment(GetTrainingDataPath(), env);
                txtModel.Text = ConvertPathForEnvironment(GetTrainingModelPath(), env);
                RefreshPreview();
            };
            cmbDatasetMode.SelectedIndexChanged += (_, _) => RefreshPreview();
            chkFilterExcludeAnomaly.CheckedChanged += (_, _) => RefreshPreview();
            txtData.TextChanged += (_, _) => RefreshPreview();
            txtModel.TextChanged += (_, _) => RefreshPreview();
            txtExtraArgs.TextChanged += (_, _) => RefreshPreview();
            txtActivate.TextChanged += (_, _) => RefreshPreview();
            txtSshUser.TextChanged += (_, _) => RefreshPreview();
            txtSshHost.TextChanged += (_, _) => RefreshPreview();
            txtSshPort.TextChanged += (_, _) => RefreshPreview();
            txtRemoteWork.TextChanged += (_, _) => RefreshPreview();

            dlg.Controls.AddRange(new Control[]
            {
                lblInfo, lblDatasetMode, cmbDatasetMode, chkFilterExcludeAnomaly,
                lblEnv, cmbEnvironment, lblData, txtData, btnBrowseData,
                lblModel, txtModel, btnBrowseModel, lblExtra, txtExtraArgs, lblExtraHint,
                grpRemote, btnUseLoaded, btnConvertWsl, btnConvertVBox, btnExportSet, btnBuild,
                lblPreviewTitle, txtPreview, lblGuide, btnOk, btnCancel
            });
            dlg.AcceptButton = btnOk;
            dlg.CancelButton = btnCancel;
            RefreshPreview();

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                trainingDataPathOverride = txtData.Text.Trim();
                trainingModelPathOverride = txtModel.Text.Trim();
                txtTrainCommand.Text = txtPreview.Text.Trim();
                AppendLog("학습 명령 자동 생성 완료");
                AppendLog("학습 data 경로 설정: " + trainingDataPathOverride);
                AppendLog("모델 저장 경로 설정: " + trainingModelPathOverride);
            }
        }

        private string BuildTrainingCommand(string environment, string dataPath, string modelPath, string extraArgs, string activateCommand, string sshUser, string sshHost, string sshPort, string remoteWorkDir)
        {
            var extra = string.IsNullOrWhiteSpace(extraArgs) ? string.Empty : " " + extraArgs.Trim();
            if (environment == "wsl")
            {
                var bashData = ConvertPathForEnvironment(dataPath, "wsl");
                var bashModel = ConvertPathForEnvironment(modelPath, "wsl");
                var body = BuildBashTrainBody(activateCommand, string.Empty, bashData, bashModel, extraArgs);
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
                return "ssh " + portPart + userHost + " \"" + body.Replace("\"", "\\\"") + "\"";
            }

            return "donkey train --tub \"" + dataPath + "\" --model \"" + modelPath + "\"" + extra;
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

            var command = "donkey train --tub '" + EscapeBashSingleQuoted(dataPath) + "' --model '" + EscapeBashSingleQuoted(modelPath) + "'";
            if (!string.IsNullOrWhiteSpace(extraArgs))
            {
                command += " " + extraArgs.Trim();
            }
            parts.Add(command);
            return string.Join(" && ", parts);
        }

        private string ConvertPathForEnvironment(string path, string environment)
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
            using var writer = new StreamWriter(catalogPath, false, Encoding.UTF8);
            var count = 0;
            foreach (var record in records)
            {
                var sourceImage = ResolveImagePath(record);
                if (!File.Exists(sourceImage))
                {
                    AppendLog("학습 데이터 이미지 누락: " + record.ImageFile);
                    continue;
                }

                var extension = Path.GetExtension(sourceImage);
                if (string.IsNullOrWhiteSpace(extension))
                {
                    extension = ".jpg";
                }
                var exportedFileName = count.ToString("000000", CultureInfo.InvariantCulture) + "_" + Path.GetFileNameWithoutExtension(sourceImage) + extension;
                var relativeImage = Path.Combine("images", exportedFileName).Replace('\\', '/');
                File.Copy(sourceImage, Path.Combine(exportImages, exportedFileName), true);
                writer.WriteLine(BuildCatalogJsonForExport(record, relativeImage, count));
                count++;
            }

            if (count == 0)
            {
                throw new InvalidOperationException("학습 데이터셋에 복사할 수 있는 이미지가 없습니다.");
            }

            AppendLog($"학습 데이터셋 생성: {count}개 -> {exportRoot}");
            return exportRoot;
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

        private static string ConvertWindowsSharedFolderPathToVirtualBoxPath(string path)
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

        private async void btnTrain_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(dataFolder) || !Directory.Exists(dataFolder))
            {
                MessageBox.Show("먼저 DonkeyCar 데이터를 불러오세요.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var command = txtTrainCommand.Text.Trim();
            if (string.IsNullOrWhiteSpace(command))
            {
                MessageBox.Show("실행할 학습 명령을 입력하세요.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                AppendLog("> " + preparedCommand);
                var exitCode = await RunCommandAsync(preparedCommand);
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
            return command.Contains("&&", StringComparison.Ordinal) ||
                   command.Contains("||", StringComparison.Ordinal) ||
                   command.Contains("|", StringComparison.Ordinal) ||
                   command.Contains(">", StringComparison.Ordinal) ||
                   command.Contains("<", StringComparison.Ordinal) ||
                   command.TrimStart().StartsWith("call ", StringComparison.OrdinalIgnoreCase) ||
                   command.TrimStart().StartsWith("set ", StringComparison.OrdinalIgnoreCase);
        }

        private Task<int> RunCommandAsync(string command)
        {
            var workingDirectory = Directory.Exists(rootFolder) ? rootFolder : dataFolder;
            if (!RequiresShell(command) && TryCreateDirectProcessStartInfo(command, workingDirectory, out var directStartInfo))
            {
                return RunProcessAsync(directStartInfo);
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

            return RunProcessAsync(shellStartInfo);
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

        private async Task<int> RunProcessAsync(ProcessStartInfo startInfo)
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

            var outputTask = Task.Run(() => ReadProcessStream(process.StandardOutput, false));
            var errorTask = Task.Run(() => ReadProcessStream(process.StandardError, true));

            await Task.Run(() => process.WaitForExit()).ConfigureAwait(false);
            await Task.WhenAll(outputTask, errorTask).ConfigureAwait(false);
            return process.ExitCode;
        }

        private void ReadProcessStream(StreamReader reader, bool isError)
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
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
            public bool Edited { get; set; }
            public bool IsAnomaly { get; set; }
            public double? MovingAverage { get; set; }
            public double? Volatility { get; set; }
            public double AnomalyScore { get; set; }
        }
    }
}
