using System;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Windows.Forms;

namespace TeamApp
{
    public partial class Form1 : Form
    {
        // 파싱된 카탈로그 데이터: 각 항목은 [index, sessionId, timestampMs, imageFile, angle, mode, throttle]
        private readonly System.Collections.Generic.List<string[]> data = new System.Collections.Generic.List<string[]>();
        // 현재 선택된 폴더의 이미지 디렉터리
        private string imagesDir = string.Empty;
        // 마지막 삭제 작업을 저장해두어 undo 가능하게 함
        private DeletionRecord? lastDeletion = null;

        private record DeletionRecord(int Index, string ImagePath, string? CatalogContent, System.Collections.Generic.List<string[]> DataSnapshot);

        public Form1()
        {
            InitializeComponent();

            // 선택 변경 핸들러 연결 및 PictureBox 출력 모드 설정
            lstIndexList.SelectedIndexChanged += lstIndexList_SelectedIndexChanged;
            trbChangeIndex.Scroll += trbChangeIndex_Scroll;
            btnDelete.Click += btnDelete_Click;
            btnSave.Click += btnSave_Click;
            // undo 버튼의 Click 이벤트를 한 번만 연결
            var undoBtn = this.Controls.Find("btnUndo", true).FirstOrDefault() as Button;
            if (undoBtn != null)
            {
                // 먼저 기존 핸들러를 제거한 뒤 다시 추가하여 중복 연결을 방지
                undoBtn.Click -= btnUndo_Click;
                undoBtn.Click += btnUndo_Click;
            }
            picCurrentIndexImage.SizeMode = PictureBoxSizeMode.Zoom;
        }
        private void btnSave_Click(object? sender, EventArgs e)
        {
            var sel = lstIndexList.SelectedIndex;
            if (sel < 0)
            {
                MessageBox.Show("저장할 항목을 선택하세요.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // read inputs
            var newThrottleText = txtChangeThrottle.Text?.Trim() ?? string.Empty;
            var newAngleText = txtChangeAngle.Text?.Trim() ?? string.Empty;

            // normalize numeric formatting if possible
            if (double.TryParse(newThrottleText, NumberStyles.Any, CultureInfo.InvariantCulture, out var thr))
                newThrottleText = thr.ToString(CultureInfo.InvariantCulture);
            if (double.TryParse(newAngleText, NumberStyles.Any, CultureInfo.InvariantCulture, out var ang))
                newAngleText = ang.ToString(CultureInfo.InvariantCulture);

            // update internal data if present
            if (data.Count > sel)
            {
                var row = data[sel];
                row[4] = newAngleText;   // angle
                row[6] = newThrottleText; // throttle

                // persist to catalog if it exists
                var catalogPath = Path.Combine(Path.GetDirectoryName(imagesDir) ?? string.Empty, "catalog_0.catalog");
                if (File.Exists(catalogPath))
                {
                    try
                    {
                        using var sw = new StreamWriter(catalogPath, false);
                        foreach (var r in data)
                        {
                            var idxVal = int.TryParse(r[0], out var idxn) ? idxn : 0;
                            var sess = r[1] ?? string.Empty;
                            var ts = long.TryParse(r[2], out var tsl) ? tsl : 0L;
                            var img = r[3] ?? string.Empty;
                            var a = double.TryParse(r[4], NumberStyles.Any, CultureInfo.InvariantCulture, out var av) ? av : 0.0;
                            var mode = r[5] ?? string.Empty;
                            var t = double.TryParse(r[6], NumberStyles.Any, CultureInfo.InvariantCulture, out var tv) ? tv : 0.0;

                            var line = $"{{\"_index\": {idxVal}, \"_session_id\": \"{sess.Replace("\\", "\\\\")}\", \"_timestamp_ms\": {ts}, \"cam/image_array\": \"{img}\", \"user/angle\": {a.ToString(CultureInfo.InvariantCulture)}, \"user/mode\": \"{mode}\", \"user/throttle\": {t.ToString(CultureInfo.InvariantCulture)} }}";
                            sw.WriteLine(line);
                        }

                        // update labels
                        lblCurrentThrottle.Text = $"현재 throttle : {newThrottleText}";
                        lblCurrentAngle.Text = $"현재 angle : {newAngleText}";
                        MessageBox.Show("저장되었습니다.", "저장", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"저장 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    // catalog 없음: 변경은 메모리에서만 적용
                    lblCurrentThrottle.Text = $"현재 throttle : {newThrottleText}";
                    lblCurrentAngle.Text = $"현재 angle : {newAngleText}";
                    MessageBox.Show("catalog_0.catalog 파일이 없어 메모리에서만 변경되었습니다.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                // no data entry for this index (fallback listing)
                lblCurrentThrottle.Text = $"현재 throttle : {newThrottleText}";
                lblCurrentAngle.Text = $"현재 angle : {newAngleText}";
                MessageBox.Show("해당 인덱스에 적용할 데이터 항목이 없습니다.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
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
            allFrames.Clear();
            visibleFrames.Clear();
            lastDeleted = null;
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

                    txtName.Text = lines[0];
                    txtSchool.Text = lines[1];
                    txtClass.Text = lines[2];
                    txtGGa.Text = lines[3];
                }

                allFrames.Sort((a, b) => a.GlobalOrder.CompareTo(b.GlobalOrder));
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

            visibleFrames.Clear();
            visibleFrames.AddRange(allFrames.Where(record => !record.Deleted && PassesFilter(record)).OrderBy(record => record.GlobalOrder));

            lstFrames.BeginUpdate();
            lstFrames.Items.Clear();
            foreach (var record in visibleFrames)
            {
                lstFrames.Items.Add(ToListText(record));
            }
            lstFrames.EndUpdate();

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

        private void lstFrames_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }

            e.DrawBackground();
            var text = lstFrames.Items[e.Index]?.ToString() ?? string.Empty;
            var isAnomaly = e.Index < visibleFrames.Count && visibleFrames[e.Index].IsAnomaly;
            var foreColor = isAnomaly ? Color.Red : e.ForeColor;
            using var brush = new SolidBrush(foreColor);
            e.Graphics.DrawString(text, e.Font ?? Font, brush, e.Bounds);
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
                var oldImage = picFrame.Image;
                picFrame.Image = copy;
                oldImage?.Dispose();
                picFrame.Invalidate();
            }
            catch (Exception ex)
            {
                loadedImagePath = string.Empty;
                DrawPlaceholder(picFrame, "이미지를 여는 중 오류가 발생했습니다.\n" + ex.Message);
            }
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
            var record = CurrentRecord();
            if (record == null)
            {
                MessageBox.Show("삭제할 프레임을 선택하세요.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                "선택한 프레임을 catalog에서 제거하고 이미지 파일을 백업 폴더로 이동할까요?\n삭제 취소 버튼으로 한 번 복구할 수 있습니다.",
                "삭제 확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            var originalImagePath = ResolveImagePath(record);
            string? backupImagePath = null;

            try
            {
                if (File.Exists(originalImagePath))
                {
                    var backupFolder = Path.Combine(dataFolder, "_deleted_backup");
                    Directory.CreateDirectory(backupFolder);
                    backupImagePath = Path.Combine(backupFolder, $"{DateTime.Now:yyyyMMddHHmmssfff}_{Path.GetFileName(originalImagePath)}");
                    File.Move(originalImagePath, backupImagePath);
                }

                lastDeleted = new DeletedRecord
                {
                    Record = record,
                    OriginalVisibleIndex = currentVisibleIndex,
                    OriginalImagePath = originalImagePath,
                    BackupImagePath = backupImagePath
                };

                record.Deleted = true;
                RewriteCatalogs();
                RecalculateAnomaliesIfNeeded();

                var nextIndex = currentVisibleIndex;
                ApplyFilters(null);
                if (visibleFrames.Count > 0)
                {
                    lstFrames.SelectedIndex = Math.Min(nextIndex, visibleFrames.Count - 1);
                }

                AppendLog($"삭제: index={record.Index}, image={record.ImageFile}");
            }
            catch (Exception ex)
            {
                record.Deleted = false;
                lastDeleted = null;
                if (!string.IsNullOrWhiteSpace(backupImagePath) && File.Exists(backupImagePath) && !File.Exists(originalImagePath))
                {
                    File.Move(backupImagePath, originalImagePath);
                }
                ApplyFilters(record.GlobalOrder);
                MessageBox.Show($"삭제 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUndo_Click(object? sender, EventArgs e)
        {
            if (lastDeleted == null)
            {
                MessageBox.Show("되돌릴 삭제 작업이 없습니다.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(lastDeleted.BackupImagePath) &&
                    !string.IsNullOrWhiteSpace(lastDeleted.OriginalImagePath) &&
                    File.Exists(lastDeleted.BackupImagePath))
                {
                    var originalDirectory = Path.GetDirectoryName(lastDeleted.OriginalImagePath);
                    if (!string.IsNullOrWhiteSpace(originalDirectory))
                    {
                        Directory.CreateDirectory(originalDirectory);
                    }

                    if (File.Exists(lastDeleted.OriginalImagePath))
                    {
                        File.Delete(lastDeleted.OriginalImagePath);
                    }

                    File.Move(lastDeleted.BackupImagePath, lastDeleted.OriginalImagePath);
                }

                lastDeleted.Record.Deleted = false;
                RewriteCatalogs();
                RecalculateAnomaliesIfNeeded();
                var targetOrder = lastDeleted.Record.GlobalOrder;
                var targetVisibleIndex = Math.Max(0, lastDeleted.OriginalVisibleIndex);
                lastDeleted = null;
                ApplyFilters(targetOrder);

                if (visibleFrames.Count > 0 && lstFrames.SelectedIndex < 0)
                {
                    lstFrames.SelectedIndex = Math.Min(targetVisibleIndex, visibleFrames.Count - 1);
                }

                AppendLog("삭제 취소 완료");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"삭제 취소 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    .Where(record => !record.Deleted && string.Equals(record.CatalogPath, catalogPath, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(record => record.OriginalLineNumber)
                    .ThenBy(record => record.GlobalOrder)
                    .ToList();

                using var writer = new StreamWriter(catalogPath, false, Encoding.UTF8);
                foreach (var record in rows)
                {
                    var json = BuildCatalogJson(record);
                    writer.WriteLine(json);
                    record.RawJson = json;
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

            for (var i = 0; i < active.Count; i++)
            {
                var record = active[i];
                if (!record.Angle.HasValue)
                {
                    continue;
                }

                var start = Math.Max(0, i - window);
                var end = Math.Min(active.Count - 1, i + window);
                var neighbors = new List<double>();
                for (var j = start; j <= end; j++)
                {
                    if (j == i || !active[j].Angle.HasValue)
                    {
                        continue;
                    }
                    neighbors.Add(active[j].Angle!.Value);
                }

                if (neighbors.Count < 3)
                {
                    continue;
                }

                var mean = neighbors.Average();
                var variance = neighbors.Select(value => Math.Pow(value - mean, 2)).Average();
                var std = Math.Sqrt(Math.Max(variance, 0));
                var deviation = Math.Abs(record.Angle.Value - mean);
                var band = Math.Max(sigma * std, minDelta);

                double? previousDelta = null;
                var previous = active.Take(i).LastOrDefault(item => item.Angle.HasValue);
                if (previous != null && previous.Angle.HasValue)
                {
                    previousDelta = Math.Abs(record.Angle.Value - previous.Angle.Value);
                }

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

            var count = active.Count(record => record.IsAnomaly);
            lblAnomalyStatus.Text = $"감지 결과: 이상 주행 {count}개 / 전체 {active.Count}개";
            if (writeLog)
            {
                AppendLog($"이상 주행 탐지 완료: {count}개, window={window}, sigma={sigma:0.##}, minJump={minDelta:0.##}");
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

            var color = cmbMaskMode.Text switch
            {
                "검정" => Color.Black,
                "흰색" => Color.White,
                "평균색" => CalculateAverageColor(bitmap, rect),
                _ => Color.Gray
            };

            using (var graphics = Graphics.FromImage(bitmap))
            using (var brush = new SolidBrush(color))
            {
                graphics.FillRectangle(brush, rect);
            }

            imageDirty = true;
            UpdateSelectionLabel();
            picFrame.Invalidate();
            AppendLog($"이미지 영역 가리기 적용: {Path.GetFileName(loadedImagePath)} {rect}");
            SaveCurrentImageEdit(false);
        }

        private void btnReplaceRegion_Click(object? sender, EventArgs e)
        {
            if (!TryGetSelectedBitmapAndRect(out var bitmap, out var rect))
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

            try
            {
                using var replacement = Image.FromFile(dlg.FileName);
                using var graphics = Graphics.FromImage(bitmap);
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(replacement, rect);

                imageDirty = true;
                UpdateSelectionLabel();
                picFrame.Invalidate();
                AppendLog($"이미지 영역 교체 적용: {Path.GetFileName(loadedImagePath)} <- {Path.GetFileName(dlg.FileName)}");
                SaveCurrentImageEdit(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"교체 이미지를 적용하는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                bitmap.Save(tempPath, GetImageFormat(loadedImagePath));
                File.Copy(tempPath, loadedImagePath, true);
                File.Delete(tempPath);

                imageDirty = false;
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
            if (string.IsNullOrWhiteSpace(loadedImagePath))
            {
                MessageBox.Show("복구할 이미지가 없습니다.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var backupPath = GetEditedBackupPath(loadedImagePath);
            if (!File.Exists(backupPath))
            {
                MessageBox.Show("저장된 원본 백업이 없습니다. 이미지 편집 저장 후에만 원본 복구가 가능합니다.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                File.Copy(backupPath, loadedImagePath, true);
                var record = CurrentRecord();
                if (record != null)
                {
                    LoadImage(record);
                }
                AppendLog($"이미지 원본 복구: {loadedImagePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"원본 복구 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

            command = command
                .Replace("{ROOT_FOLDER}", rootFolder)
                .Replace("{DATA_FOLDER}", dataFolder)
                .Replace("{IMAGES_FOLDER}", imagesFolder)
                .Replace("{ROOT}", rootFolder)
                .Replace("{DATA}", dataFolder)
                .Replace("{IMAGES}", imagesFolder);

            btnTrain.Enabled = false;
            AppendLog("> " + command);

            try
            {
                var exitCode = await RunShellCommandAsync(command);
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

        private Task<int> RunShellCommandAsync(string command)
        {
            var completion = new TaskCompletionSource<int>();
            var shell = Environment.GetEnvironmentVariable("ComSpec");
            if (string.IsNullOrWhiteSpace(shell))
            {
                shell = "cmd.exe";
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = shell,
                    Arguments = "/C " + command,
                    WorkingDirectory = Directory.Exists(rootFolder) ? rootFolder : dataFolder,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            process.OutputDataReceived += (_, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.Data))
                {
                    AppendLog(args.Data);
                }
            };
            process.ErrorDataReceived += (_, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.Data))
                {
                    AppendLog("ERR: " + args.Data);
                }
            };
            process.Exited += (_, _) =>
            {
                try
                {
                    completion.TrySetResult(process.ExitCode);
                }
                finally
                {
                    process.Dispose();
                }
            };

            if (!process.Start())
            {
                process.Dispose();
                throw new InvalidOperationException("프로세스를 시작하지 못했습니다.");
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            return completion.Task;
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
            var anomaly = record.IsAnomaly ? "!" : " ";
            return $"{anomaly} {record.Index,6} | a={FormatForInput(record.Angle),8} | t={FormatForInput(record.Throttle),8} | {Path.GetFileName(record.ImageFile)}";
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
            public bool IsAnomaly { get; set; }
            public double? MovingAverage { get; set; }
            public double? Volatility { get; set; }
            public double AnomalyScore { get; set; }
        }

        private sealed class DeletedRecord
        {
            public FrameRecord Record { get; set; } = null!;
            public int OriginalVisibleIndex { get; set; }
            public string? OriginalImagePath { get; set; }
            public string? BackupImagePath { get; set; }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "mycar 폴더를 선택하세요";
                dlg.UseDescriptionForTitle = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    var selectedPath = dlg.SelectedPath;
                    txtSelectedFolder.Text = selectedPath;

                    // Look for ./data/images under the selected folder
                    imagesDir = Path.Combine(selectedPath, "data", "images");

                    lstIndexList.Items.Clear();

                    if (!Directory.Exists(imagesDir))
                    {
                        MessageBox.Show("선택한 폴더에 ./data/images 폴더가 없습니다.", "폴더 없음", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    try
                    {
                        // First try to read catalog_0.catalog under ./data
                        var catalogPath = Path.Combine(selectedPath, "data", "catalog_0.catalog");
                        data.Clear();

                        if (File.Exists(catalogPath))
                        {
                            // 각 줄을 읽어 JSON으로 파싱
                            foreach (var line in File.ReadLines(catalogPath))
                            {
                                var trimmed = line.Trim();
                                if (string.IsNullOrEmpty(trimmed))
                                    continue;

                                try
                                {
                                    using var doc = JsonDocument.Parse(trimmed);
                                    var root = doc.RootElement;

                                    // 안전한 검사로 필드 추출
                                    var index = root.TryGetProperty("_index", out var pIndex) && pIndex.ValueKind == JsonValueKind.Number ? pIndex.GetInt32() : -1;
                                    var sessionId = root.TryGetProperty("_session_id", out var pSess) && pSess.ValueKind == JsonValueKind.String ? pSess.GetString() ?? string.Empty : string.Empty;
                                    var timestamp = root.TryGetProperty("_timestamp_ms", out var pTs) && pTs.ValueKind == JsonValueKind.Number ? pTs.GetInt64().ToString(CultureInfo.InvariantCulture) : string.Empty;
                                    var camImage = root.TryGetProperty("cam/image_array", out var pImg) && pImg.ValueKind == JsonValueKind.String ? pImg.GetString() ?? string.Empty : string.Empty;
                                    var angle = root.TryGetProperty("user/angle", out var pAngle) && (pAngle.ValueKind == JsonValueKind.Number || pAngle.ValueKind == JsonValueKind.String) ? pAngle.GetRawText() : string.Empty;
                                    var mode = root.TryGetProperty("user/mode", out var pMode) && pMode.ValueKind == JsonValueKind.String ? pMode.GetString() ?? string.Empty : string.Empty;
                                    var throttle = root.TryGetProperty("user/throttle", out var pThr) && (pThr.ValueKind == JsonValueKind.Number || pThr.ValueKind == JsonValueKind.String) ? pThr.GetRawText() : string.Empty;

                                    // 숫자형 텍스트 값 정규화
                                    if (double.TryParse(angle, NumberStyles.Any, CultureInfo.InvariantCulture, out var ang))
                                        angle = ang.ToString(CultureInfo.InvariantCulture);
                                    if (double.TryParse(throttle, NumberStyles.Any, CultureInfo.InvariantCulture, out var thr))
                                        throttle = thr.ToString(CultureInfo.InvariantCulture);

                                    data.Add(new[] { index.ToString(CultureInfo.InvariantCulture), sessionId, timestamp, camImage, angle, mode, throttle });
                                }
                                catch
                                {
                                    // 잘못된 라인은 건너뜀
                                    continue;
                                }
                            }

                            if (data.Count == 0)
                            {
                                MessageBox.Show("catalog_0.catalog 파일에서 유효한 항목을 찾을 수 없습니다.", "데이터 없음", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                // index 기준 정렬
                                var orderedData = data.OrderBy(d => int.TryParse(d[0], out var n) ? n : int.MaxValue).ToList();
                                lstIndexList.Items.Clear();
                                foreach (var row in orderedData)
                                {
                                    lstIndexList.Items.Add(row[3]); // cam/image_array
                                }

                                // 내부 데이터를 정렬된 목록으로 교체
                                data.Clear();
                                data.AddRange(orderedData);
                                // 트랙바 범위 설정
                                trbChangeIndex.Minimum = 0;
                                trbChangeIndex.Maximum = Math.Max(0, data.Count - 1);
                                trbChangeIndex.TickFrequency = 1;
                                trbChangeIndex.LargeChange = 1;
                                trbChangeIndex.SmallChange = 1;
                            }
                        }
                        else
                        {
                            // 대체 동작: imagesDir의 파일 나열
                            var files = Directory.GetFiles(imagesDir);
                            if (files.Length == 0)
                            {
                                MessageBox.Show("./data/images 폴더에 파일이 없습니다.", "파일 없음", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }

                            // Sort files by leading numeric value (e.g., "0.jpg", "1.png")
                            var fileNames = files.Select(Path.GetFileName).ToList();
                            var ordered = fileNames
                                .OrderBy(name =>
                                {
                                    var m = Regex.Match(name, "^(\\d+)");
                                    if (m.Success && int.TryParse(m.Groups[1].Value, out var n))
                                        return n;
                                    if (int.TryParse(Path.GetFileNameWithoutExtension(name), out n))
                                        return n;
                                    return int.MaxValue;
                                })
                                .ThenBy(name => name, StringComparer.OrdinalIgnoreCase);

                            foreach (var name in ordered)
                            {
                                lstIndexList.Items.Add(name);
                            }

                            // configure trackbar range for fallback
                            trbChangeIndex.Minimum = 0;
                            trbChangeIndex.Maximum = Math.Max(0, lstIndexList.Items.Count - 1);
                            trbChangeIndex.TickFrequency = 1;
                            trbChangeIndex.LargeChange = 1;
                            trbChangeIndex.SmallChange = 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"이미지 파일을 불러오는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    // auto-select index 0 if any items exist
                    if (lstIndexList.Items.Count > 0)
                    {
                        lstIndexList.SelectedIndex = 0;
                    }
                }
            }
        }

        private void lstIndexList_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (lstIndexList.SelectedIndex < 0)
                return;

            var sel = lstIndexList.SelectedIndex;

            string imageFileName = lstIndexList.Items[sel].ToString() ?? string.Empty;
            string angle = string.Empty;
            string throttle = string.Empty;

            if (data.Count > sel)
            {
                var row = data[sel];
                // row structure: index, sessionId, timestamp, imageFile, angle, mode, throttle
                imageFileName = row[3];
                angle = row[4];
                throttle = row[6];
            }

            // try load image from imagesDir
            if (!string.IsNullOrEmpty(imagesDir))
            {
                var fullPath = Path.Combine(imagesDir, imageFileName);
                if (File.Exists(fullPath))
                {
                    try
                    {
                        using var fs = File.OpenRead(fullPath);
                        using var src = Image.FromStream(fs);
                        // dispose previous image
                        picCurrentIndexImage.Image?.Dispose();
                        picCurrentIndexImage.Image = new Bitmap(src);
                    }
                    catch
                    {
                        picCurrentIndexImage.Image = null;
                    }
                }
                else
                {
                    picCurrentIndexImage.Image = null;
                }
            }

            // update separate labels: current index, throttle and angle
            lblCurrentIndex.Text = $"현재 인덱스 : {sel}";
            lblCurrentThrottle.Text = $"현재 throttle : {throttle}";
            lblCurrentAngle.Text = $"현재 angle : {angle}";

            // keep trackbar in sync
            if (trbChangeIndex.Minimum <= sel && sel <= trbChangeIndex.Maximum)
            {
                trbChangeIndex.Value = sel;
            }
        }

        private void trbChangeIndex_Scroll(object? sender, EventArgs e)
        {
            var v = trbChangeIndex.Value;
            if (v >= 0 && v < lstIndexList.Items.Count)
            {
                lstIndexList.SelectedIndex = v;
            }
        }

        private void btnBefore_Click(object? sender, EventArgs e)
        {
            var idx = lstIndexList.SelectedIndex;
            if (idx > 0)
            {
                lstIndexList.SelectedIndex = idx - 1;
            }
        }

        private void btnAfter_Click(object? sender, EventArgs e)
        {
            var idx = lstIndexList.SelectedIndex;
            if (idx < lstIndexList.Items.Count - 1)
            {
                lstIndexList.SelectedIndex = idx + 1;
            }
        }

        private void btnDelete_Click(object? sender, EventArgs e)
        {
            var sel = lstIndexList.SelectedIndex;
            if (sel < 0)
                return;

            // dispose displayed image first
            picCurrentIndexImage.Image?.Dispose();
            picCurrentIndexImage.Image = null;

            // helper for safe renames: move old -> temp then later temp -> final
            string tempPrefix() => Guid.NewGuid().ToString();

            // If we have catalog data (data list corresponds to catalog)
            var catalogPath = Path.Combine(Path.GetDirectoryName(imagesDir) ?? string.Empty, "catalog_0.catalog");
            var catalogExists = File.Exists(catalogPath) && data.Count > 0;

            try
            {
                // 저장하기 전에 확인 다이얼로그
                var confirm = MessageBox.Show("선택된 항목을 삭제하시겠습니까?", "삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (confirm != DialogResult.Yes)
                    return;

                if (catalogExists)
                {
                    // Delete selected image file
                    var delName = data[sel][3];
                    var delPath = Path.Combine(imagesDir, delName);
                    // 저장된 삭제 정보 생성
                    var catalogText = File.Exists(catalogPath) ? File.ReadAllText(catalogPath) : null;
                    lastDeletion = new DeletionRecord(sel, delPath, catalogText, new System.Collections.Generic.List<string[]>(data.Select(r => (string[])r.Clone())));

                    if (File.Exists(delPath))
                        File.Delete(delPath);

                    // For subsequent entries (sel+1 ... end), rename files to shift indices left by 1
                    var tempMoves = new System.Collections.Generic.List<(string tempPath, string finalPath, int newIndex, int dataIndex)>();
                    for (int i = sel + 1; i < data.Count; i++)
                    {
                        var row = data[i];
                        var oldName = row[3];
                        var m = Regex.Match(oldName, "^(\\d+)");
                        if (!m.Success)
                            continue;

                        var rest = oldName.Substring(m.Groups[1].Length);
                        var newIndex = i - 1; // shift left
                        var newName = newIndex + rest;
                        var oldPath = Path.Combine(imagesDir, oldName);
                        var finalPath = Path.Combine(imagesDir, newName);
                        if (File.Exists(oldPath))
                        {
                            var temp = Path.Combine(imagesDir, tempPrefix() + ".tmp");
                            File.Move(oldPath, temp);
                            tempMoves.Add((temp, finalPath, newIndex, i));
                        }
                        else
                        {
                            // file missing; update data row directly
                            row[0] = newIndex.ToString(CultureInfo.InvariantCulture);
                            row[3] = newName;
                        }
                    }

                    // Move temp files to final names and update data rows
                    foreach (var mv in tempMoves)
                    {
                        if (File.Exists(mv.finalPath))
                            File.Delete(mv.finalPath);
                        File.Move(mv.tempPath, mv.finalPath);
                        var row = data[mv.dataIndex];
                        row[0] = mv.newIndex.ToString(CultureInfo.InvariantCulture);
                        row[3] = Path.GetFileName(mv.finalPath);
                    }

                    // Remove the selected data entry
                    if (sel < data.Count)
                        data.RemoveAt(sel);

                    // Rebuild catalog file from data
                    using (var sw = new StreamWriter(catalogPath, false))
                    {
                        foreach (var row in data)
                        {
                            // row: index, sessionId, timestamp, imageFile, angle, mode, throttle
                            var idxVal = int.TryParse(row[0], out var idxn) ? idxn : 0;
                            var sess = row[1] ?? string.Empty;
                            var ts = long.TryParse(row[2], out var tsl) ? tsl : 0L;
                            var img = row[3] ?? string.Empty;
                            var ang = double.TryParse(row[4], NumberStyles.Any, CultureInfo.InvariantCulture, out var angv) ? angv : 0.0;
                            var mode = row[5] ?? string.Empty;
                            var thr = double.TryParse(row[6], NumberStyles.Any, CultureInfo.InvariantCulture, out var thv) ? thv : 0.0;

                            var line = $"{{\"_index\": {idxVal}, \"_session_id\": \"{sess.Replace("\\", "\\\\")}\", \"_timestamp_ms\": {ts}, \"cam/image_array\": \"{img}\", \"user/angle\": {ang.ToString(CultureInfo.InvariantCulture)}, \"user/mode\": \"{mode}\", \"user/throttle\": {thr.ToString(CultureInfo.InvariantCulture)} }}";
                            sw.WriteLine(line);
                        }
                    }

                    // Update listbox items from data
                    lstIndexList.Items.Clear();
                    foreach (var row in data)
                        lstIndexList.Items.Add(row[3]);

                    // Adjust trackbar
                    trbChangeIndex.Maximum = Math.Max(0, data.Count - 1);
                    if (data.Count == 0)
                    {
                        lblCurrentIndex.Text = "";
                        lblCurrentThrottle.Text = "";
                        lblCurrentAngle.Text = "";
                    }
                    else
                    {
                        var newSel = Math.Min(sel, data.Count - 1);
                        lstIndexList.SelectedIndex = newSel;
                    }
                }
                else
                {
                    // no catalog: operate on files listed in listbox
                    var selName = lstIndexList.Items[sel].ToString() ?? string.Empty;
                    var delPath = Path.Combine(imagesDir, selName);
                    if (File.Exists(delPath))
                        File.Delete(delPath);

                    // rename subsequent files with leading numbers
                    var temps = new System.Collections.Generic.List<(string temp, string final)>();
                    for (int i = sel + 1; i < lstIndexList.Items.Count; i++)
                    {
                        var oldName = lstIndexList.Items[i].ToString() ?? string.Empty;
                        var m = Regex.Match(oldName, "^(\\d+)");
                        if (!m.Success)
                            continue;
                        var rest = oldName.Substring(m.Groups[1].Length);
                        var newIndex = i - 1;
                        var newName = newIndex + rest;
                        var oldPath = Path.Combine(imagesDir, oldName);
                        var finalPath = Path.Combine(imagesDir, newName);
                        if (File.Exists(oldPath))
                        {
                            var temp = Path.Combine(imagesDir, tempPrefix() + ".tmp");
                            File.Move(oldPath, temp);
                            temps.Add((temp, finalPath));
                        }
                    }

                    // 저장된 삭제 정보 생성 (카탈로그 없을 때는 이미지 파일만 백업)
                    var catalogText = File.Exists(catalogPath) ? File.ReadAllText(catalogPath) : null;
                    lastDeletion = new DeletionRecord(sel, delPath, catalogText, new System.Collections.Generic.List<string[]>(data.Select(r => (string[])r.Clone())));

                    foreach (var mv in temps)
                    {
                        if (File.Exists(mv.final))
                            File.Delete(mv.final);
                        File.Move(mv.temp, mv.final);
                    }

                    // rebuild listbox
                    var items = Directory.GetFiles(imagesDir).Select(Path.GetFileName).ToList();
                    var ordered = items.OrderBy(name =>
                    {
                        var m = Regex.Match(name, "^(\\d+)");
                        if (m.Success && int.TryParse(m.Groups[1].Value, out var n))
                            return n;
                        if (int.TryParse(Path.GetFileNameWithoutExtension(name), out var n2))
                            return n2;
                        return int.MaxValue;
                    }).ThenBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();

                    lstIndexList.Items.Clear();
                    foreach (var nm in ordered)
                        lstIndexList.Items.Add(nm);

                    trbChangeIndex.Maximum = Math.Max(0, lstIndexList.Items.Count - 1);
                    if (lstIndexList.Items.Count == 0)
                    {
                        lblCurrentIndex.Text = "";
                        lblCurrentThrottle.Text = "";
                        lblCurrentAngle.Text = "";
                    }
                    else
                    {
                        var newSel = Math.Min(sel, lstIndexList.Items.Count - 1);
                        lstIndexList.SelectedIndex = newSel;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"삭제 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUndo_Click(object? sender, EventArgs e)
        {
            if (lastDeletion == null)
            {
                MessageBox.Show("되돌릴 삭제 작업이 없습니다.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // 복원: 카탈로그 원본이 있으면 복원
                var catalogPath = Path.Combine(Path.GetDirectoryName(imagesDir) ?? string.Empty, "catalog_0.catalog");
                if (lastDeletion.CatalogContent != null)
                {
                    File.WriteAllText(catalogPath, lastDeletion.CatalogContent);
                }

                // 이미지 파일 복원은 간단히 불가능할 수 있음(파일을 삭제했기 때문에).
                // 우리는 삭제 전에 전체 data 스냅샷을 저장했으므로, 카탈로그가 복원되면 리스트와 내부 data를 복원한다.
                data.Clear();
                data.AddRange(lastDeletion.DataSnapshot);

                lstIndexList.Items.Clear();
                foreach (var row in data)
                    lstIndexList.Items.Add(row[3]);

                trbChangeIndex.Minimum = 0;
                trbChangeIndex.Maximum = Math.Max(0, data.Count - 1);

                if (data.Count > 0)
                {
                    var sel = Math.Min(lastDeletion.Index, data.Count - 1);
                    lstIndexList.SelectedIndex = sel;
                }

                lastDeletion = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"되돌리는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
