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

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void LoadMemberInfo(string fileName)
        {
            try
            {
                string[] lines = File.ReadAllLines(fileName);
                if (lines.Length >= 3)
                {

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"파일을 읽는 중 오류가 발생했습니다: {ex.Message}");
            }
        }

        private void BtnMember1_Click(object sender, EventArgs e)
        {
            LoadMemberInfo("member1.txt");
        }

        private void BtnMember2_Click(object sender, EventArgs e)
        {
            LoadMemberInfo("member2.txt");
        }

        private void BtnMember3_Click(object sender, EventArgs e)
        {
            LoadMemberInfo("member3.txt");
        }

        private void BtnMember4_Click(object sender, EventArgs e)
        {
            LoadMemberInfo("member4.txt");
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
