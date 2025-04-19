using DDSLib;
using System.Text.Json;
using System.Windows.Media.Imaging;
using System.Text;
using System.Diagnostics;

namespace MHTextureManager
{
    public partial class MainForm : Form
    {
        private TextureManifest manifest;
        private List<TreeNode> rootNodes;
        private readonly object rootNodesLock = new();

        private DdsFile ddsFile;
        private TextureFileCache textureCache;
        private SettingsForm settingsForm;
        private HashSet<string> standardList;
        private MultiStateCheckedListBox modListBox;

        public const string ManifestName = "TextureFileCacheManifest.bin";
        public string ManifestPath = "";
        public string ModsPath = "";
        private TreeNode lastEntryNode;

        public MainForm()
        {
            ddsFile = new();
            manifest = new();
            rootNodes = new();
            textureCache = new();
            settingsForm = new();
            standardList = new();

            InitializeComponent();

            modListBox = new();

            ModsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mods");
            if (!Directory.Exists(ModsPath)) Directory.CreateDirectory(ModsPath);

            tabPage2.Controls.Add(modListBox);
            modListBox.Dock = DockStyle.Fill;
            modListBox.ContextMenuStrip = contextMenuStrip1;

            string tfclist = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "TFCLIst.txt");
            if (File.Exists(tfclist))
                foreach (string line in File.ReadLines(tfclist))
                    standardList.Add(line.Trim());
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Texture Manifest (*.bin)|" + ManifestName;
            openFileDialog.Title = "Select " + ManifestName;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFile = Path.GetFileName(openFileDialog.FileName);

                if (selectedFile != ManifestName)
                {
                    MessageBox.Show("Please select the correct file: " + ManifestName,
                                    "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string filePath = openFileDialog.FileName;
                ManifestPath = Path.GetDirectoryName(filePath) ?? "";

                manifestTreeView.Nodes.Clear();
                lastEntryNode = null;

                Task.Run(() =>
                {
                    var entries = manifest.LoadManifest(filePath);
                    // CheckAllTextures(entries);
                    int totalEntries = entries.Count;

                    Invoke(new Action(() => totalTexturesStatus.Text = totalEntries.ToString()));

                    BuildTree(entries);

                    BeginInvoke(new Action(() =>
                    {
                        manifestTreeView.BeginUpdate();
                        manifestTreeView.Nodes.AddRange([.. rootNodes]);
                        manifestTreeView.EndUpdate();

                        statusFiltered.Text = manifestTreeView.Nodes.Count.ToString();
                        saveManifestToolStripMenuItem.Enabled = false;

                        ReloadMods();
                    }));
                });
            }
        }

        private void CheckAllTextures(List<TextureEntry> entries)
        {
            //string notfound = "NotFound.tsv";
            //string found = "FoundInfo.tsv";
            string notLoad = "NotLoad.tsv";

            Invoke(new Action(() => progressBar.Maximum = entries.Count));

            foreach (var entry in entries)
            {
                var mipmap = entry.Data.OverrideMipMap;
                if (mipmap.ImageData != null)
                {
                    if (entry.Data.Maps.Count == 0) continue;
                    int index = mipmap.ImageData[0];
                    string tfcPath = Path.Combine(ManifestPath, entry.Data.TextureFileName + ".tfc");

                    if (textureCache.LoadFromFile(tfcPath, entry))
                    {
                        /*if (mipmap.ImageData[0] == 10)
                            AddToFile($"{entry.Head.TextureGUID}\t{index}\t{mipmap.Width}\t{mipmap.Height}\t{mipmap.OverrideFormat}", found);*/

                        /*
                        var stream = textureCache.Texture2D.GetObjectStream(0);
                        if (stream != null)
                        {
                            string filename = Path.Combine("Out", entry.Head.TextureName + ".dds");
                            using var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
                            stream.Seek(0, SeekOrigin.Begin);
                            stream.CopyTo(fileStream);
                        }*/
                    }
                    else
                        AddToFile($"{entry.Head.TextureName}\t{entry.Head.TextureGuid}\t{index}\t{mipmap.Width}\t{mipmap.Height}\t{mipmap.OverrideFormat}", notLoad);
                }/*
                else
                {
                    AddToFile($"{entry.Head.TextureName}\t{entry.Head.TextureGUID}\t{entry.Data.Maps.Count}", notfound);
                }*/
                BeginInvoke(new Action(() => progressBar.Value++));
            }
        }

        private void AddToFile(string message, string filePath)
        {
            using var writer = new StreamWriter(filePath, true);
            writer.WriteLine(message);
        }

        private void BuildTree(List<TextureEntry> entries)
        {
            rootNodes.Clear();

            var groupedEntries = entries.GroupBy(entry => entry.Head.TextureName.Split('.')[0]).ToList();

            List<Task> tasks = [];

            foreach (var group in groupedEntries)
            {
                var task = Task.Run(() =>
                {
                    var rootNode = new TreeNode(group.Key);
                    AddChildNodes(rootNode, [..group]);

                    lock (rootNodesLock)
                    {
                        rootNodes.Add(rootNode);
                    }
                });

                tasks.Add(task);
            }

            Task.WaitAll([.. tasks]);
        }

        private void AddChildNodes(TreeNode parentNode, List<TextureEntry> entries)
        {
            if (entries.Count == 0) return;

            var groupedEntries = entries.GroupBy(entry =>
            {
                var parts = entry.Head.TextureName.Split('.');
                return parts.Length > parentNode.Level + 1 ? parts[parentNode.Level + 1] : null;
            }).Where(g => g.Key != null).ToList();

            foreach (var group in groupedEntries)
            {
                var childNode = new TreeNode(group.Key);
                childNode.Tag = group.FirstOrDefault(entry => entry.Head.TextureName.Split('.').Length == parentNode.Level + 2);
                parentNode.Nodes.Add(childNode);

                AddChildNodes(childNode, [.. group]);
            }
        }

        private void filterBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (FilterTree(filterBox.Text))
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
                statusFiltered.Text = manifestTreeView.Nodes.Count.ToString();
            }
        }

        private bool FilterTree(string filterText)
        {
            filterText = filterText.Trim().ToLower();
            if (filterText.Length < 3 || rootNodes.Count == 0) return false;

            manifestTreeView.BeginUpdate();
            manifestTreeView.Nodes.Clear();
            lastEntryNode = null;

            foreach (TreeNode rootNode in rootNodes)
                if (FilterNode(rootNode, filterText))
                    manifestTreeView.Nodes.Add(rootNode);

            manifestTreeView.EndUpdate();

            return manifestTreeView.Nodes.Count > 0;
        }

        private static bool FilterNode(TreeNode node, string filterText)
        {
            if (node.Text.ToLower().Contains(filterText))
                return true;

            foreach (TreeNode childNode in node.Nodes)
                if (FilterNode(childNode, filterText))
                    return true;

            return false;
        }

        private void filterClear_Click(object sender, EventArgs e)
        {
            ResetManifestTreeView();
        }

        private void ResetManifestTreeView()
        {
            filterBox.Text = "";
            if (rootNodes.Count == 0) return;

            manifestTreeView.BeginUpdate();
            manifestTreeView.Nodes.Clear();
            lastEntryNode = null;
            manifestTreeView.Nodes.AddRange([.. rootNodes]);
            manifestTreeView.EndUpdate();

            statusFiltered.Text = manifestTreeView.Nodes.Count.ToString();
        }

        private void manifestTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ReloadTextureFromNode(e.Node);
        }

        private void ReloadTextureFromNode(TreeNode node)
        {
            if (node?.Tag is TextureEntry entry)
            {
                lastEntryNode = node;
                ReloadTextureView(entry);
            }
        }

        public void ReloadTextureView(TextureEntry entry)
        {
            textureNameLabel.Text = entry.Head.TextureName;
            textureGuidLabel.Text = entry.Head.TextureGuid.ToString();
            mipMapsLabel.Text = entry.Data.Maps.Count.ToString();
            textureFileLabel.Text = entry.Data.TextureFileName;

            UpdateMipMapBox(entry);
            textureCache.Reset();
            LoadTextureCache(entry);
        }

        private void LoadTextureCache(TextureEntry entry, int index = 0)
        {
            string tfcPath = Path.Combine(ManifestPath, entry.Data.TextureFileName + ".tfc");
            if (entry.Data.Maps.Count == 0) return;
            if (textureCache.LoadFromFile(tfcPath, entry) && textureCache.Texture2D.MipMaps.Count > 0)
            {
                UpdateTextureInfo(index);
                var stream = textureCache.Texture2D.GetObjectStream(index);
                ddsFile.Load(stream);
                textureView.Image = BitmapSourceToBitmap(ddsFile.BitmapSource);
                CenterTexture();

                importDDSToolStripMenuItem.Enabled = true;
                exportDDSToolStripMenuItem.Enabled = true;
            }
            else MessageBox.Show($"Can't Load TFC: {entry.Head.TextureName}\nFile: {tfcPath}",
                                 "Error load", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void UpdateTextureInfo(int mipmapIndex)
        {
            var mipMap = textureCache.Texture2D.MipMaps[mipmapIndex];
            formatLabel.Text = mipMap.OverrideFormat.ToString();
            widthLabel.Text = $"{mipMap.Width} x {mipMap.Height}";
            mipMapBox.SelectedIndex = mipmapIndex;
        }

        private void UpdateMipMapBox(TextureEntry entry)
        {
            mipMapBox.Items.Clear();

            foreach (var mipMap in entry.Data.Maps)
                mipMapBox.Items.Add(mipMap);
        }

        private void importDDSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textureCache == null || textureCache.Entry == null)
            {
                MessageBox.Show("Current texture is empty!", "Texture Empty", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "DDS Files (*.dds)|*.dds|PNG Files (*.png)|*.png";
            openFileDialog.Title = "Select a Texture File";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filename = openFileDialog.FileName;
                DdsFile ddsHeader;

                bool isPNG = filename.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
                if (isPNG)
                {
                    var entry = textureCache.Entry;
                    var mipmap = textureCache.Texture2D.MipMaps[0];
                    ddsHeader = new DdsFile();
                    ddsHeader.BuildFromPng(filename, mipmap.OverrideFormat, mipmap.Width, mipmap.Height, entry.Data.Maps.Count);
                }
                else
                {
                    ddsHeader = new DdsFile(filename, true);
                }

                ImportHeaderDds(ddsHeader);
            }
        }

        private void ImportHeaderDds(DdsFile ddsHeader)
        {
            if (ddsHeader == null)
            {
                MessageBox.Show("Wrong dds format!", "DDS Format", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var entry = textureCache.Entry;
            var mipmap = textureCache.Texture2D.MipMaps[0];

            if (ddsHeader.Width != mipmap.Width && ddsHeader.Height != mipmap.Height)
            {
                MessageBox.Show($"DDS should be {mipmap.Width} x {mipmap.Height}, your size {ddsHeader.Width} x {ddsHeader.Height}",
                    "DDS Format", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (ddsHeader.FileFormat != mipmap.OverrideFormat)
            {
                MessageBox.Show($"DDS format should be {mipmap.OverrideFormat}, your format is {ddsHeader.FileFormat}",
                    "DDS Format", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var textureFileName = entry.Data.TextureFileName;
            settingsForm.TextureCacheName = textureFileName;
            settingsForm.TextureCachePath = ManifestPath;

            bool canChange = IsStandardCache(textureFileName) == false;
            settingsForm.CanChanged = canChange;

            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                if (ddsHeader.MipMaps.Count < entry.Data.Maps.Count)
                    ddsHeader.RegenMipMaps(entry.Data.Maps.Count);

                var info = new TextureMipMapsInfo(entry);

                var result = textureCache.WriteTexture(ManifestPath, settingsForm.TextureCacheName, settingsForm.ImportType, ddsHeader);

                switch (result)
                {
                    case WriteResult.MipMapError:
                        MessageBox.Show("Error while writing MipMap data",
                            "Compression Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;

                    case WriteResult.SizeReplaceError:
                        MessageBox.Show("Compressed data is too large to replace",
                            "Compression Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                }

                if (result == WriteResult.Success)
                    info.Update(entry.Data);

                // if (canChange == false)
                info.SaveBackup(ModsPath);

                saveManifestToolStripMenuItem.Enabled = true;
                ReloadTextureView(entry);
                ReloadMods();
            }
        }

        private bool IsStandardCache(string textureFileName)
        {
            return standardList.Contains(textureFileName);
        }

        private static Bitmap BitmapSourceToBitmap(BitmapSource bitmapSource)
        {
            Bitmap bitmap;

            using (MemoryStream outStream = new())
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(outStream);
                bitmap = new Bitmap(outStream);
            }

            return bitmap;
        }

        private static MemoryStream BitmapSourceToPng(BitmapSource bitmapSource)
        {
            MemoryStream outStream = new();

            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(outStream);

            return outStream;
        }

        private void texturePanel_Resize(object sender, EventArgs e)
        {
            CenterTexture();
        }

        private void CenterTexture()
        {
            if (textureView.Image != null)
            {
                int x = (texturePanel.ClientSize.Width - textureView.Width) / 2;
                int y = (texturePanel.ClientSize.Height - textureView.Height) / 2;

                textureView.Location = new Point(Math.Max(x, 0), Math.Max(y, 0));
            }
        }

        private void exportDDSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textureCache.Texture2D.MipMaps.Count == 0) return;

            using var saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = textureNameLabel.Text + ".dds";
            saveFileDialog.Filter = "DDS Files (*.dds)|*.dds|PNG Files (*.png)|*.png";
            saveFileDialog.Title = "Save a Texture File";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filename = saveFileDialog.FileName;

                if (textureCache.Loaded == false)
                {
                    var entry = textureCache.Entry;
                    string tfcPath = Path.Combine(ManifestPath, entry.Data.TextureFileName + ".tfc");
                    textureCache.LoadFromFile(tfcPath, entry);
                }

                var stream = textureCache.Texture2D.GetMipMapsStream();
                if (stream == null) return;

                bool isPNG = filename.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
                if (isPNG)
                {
                    ddsFile.Load(stream);
                    stream = BitmapSourceToPng(ddsFile.BitmapSource);
                }

                using var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }
        }

        private void mipMapBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (mipMapBox.SelectedItem is TextureMipMap mipMap)
            {
                var entry = mipMap.Entry;
                offsetLabel.Text = mipMap.Offset.ToString();
                sizeLabel.Text = mipMap.Size.ToString();

                int index = entry.Data.Maps.IndexOf(mipMap);
                LoadTextureCache(mipMap.Entry, index);
            }
        }

        private void saveManifestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = ManifestPath;
            saveFileDialog.FileName = ManifestName;
            saveFileDialog.Filter = "Texture Manifest (*.bin)|" + ManifestName;
            saveFileDialog.Title = "Save " + ManifestName;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
                manifest.SaveManifest(saveFileDialog.FileName);
        }

        public static TextureMipMapsInfo[] LoadMods(string filePath)
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return JsonSerializer.Deserialize<TextureMipMapsInfo[]>(fileStream);
        }

        private void ReloadMods()
        {
            string selectedItem = modListBox.SelectedIndex != -1 ? modListBox.SelectedItem.ToString() : null;
            modListBox.Items.Clear();

            try
            {
                if (Directory.Exists(ModsPath))
                {
                    string[] jsonFiles = Directory.GetFiles(ModsPath, "*.json");
                    foreach (string filePath in jsonFiles)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(filePath);
                        var index = modListBox.Items.Add(fileName);
                        CheckMod(filePath, index);
                    }

                    if (selectedItem != null)
                    {
                        int indexToSelect = modListBox.Items.IndexOf(selectedItem);
                        if (indexToSelect >= 0)
                            modListBox.SelectedIndex = indexToSelect;
                    }
                }
                else
                {
                    MessageBox.Show("Folder Mods not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error mod loading:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CheckMod(string filename, int index)
        {
            var mods = LoadMods(filename);
            bool modApplied = true;

            foreach (var mod in mods)
            {
                ModResult status;

                if (IsStandardCache(mod.Original.TextureFileName))
                    status = manifest.GetStatus(mod, ManifestPath);
                else
                    status = ModResult.TexutureNotFound;

                if (status != ModResult.Success)
                {
                    if (status == ModResult.TexutureNotFound || status == ModResult.NotMatch)
                    {
                        modListBox.SetItemCheckState(index, CheckState.Indeterminate);
                        return;
                    }

                    modApplied = false;
                    break;
                }
            }

            modListBox.SetItemCheckState(index, modApplied ? CheckState.Checked : CheckState.Unchecked);
        }

        private void reloadModsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ReloadMods();
        }

        private void applyModToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> errorMessages = [];
            bool anyModApplied = false;

            foreach (int index in modListBox.SelectedIndices)
            {
                string modName = modListBox.Items[index].ToString();
                string filename = Path.Combine(ModsPath, $"{modName}.json");

                if (!File.Exists(filename))
                {
                    errorMessages.Add($"Mod file not found: {modName}");
                    continue;
                }

                try
                {
                    var mods = LoadMods(filename);
                    foreach (var mod in mods)
                    {
                        ModResult result;
                        if (IsStandardCache(mod.Original.TextureFileName))
                            result = manifest.ApplyMod(mod, ManifestPath);
                        else
                            result = ModResult.Reset;

                        switch (result)
                        {
                            case ModResult.Success:
                                anyModApplied = true;
                                break;

                            case ModResult.TexutureNotFound:
                                errorMessages.Add($"[{mod.Head.TextureName}] {mod.Updated.TextureFileName}.tfc not found");
                                break;

                            case ModResult.NotMatch:
                                errorMessages.Add($"[{mod.Head.TextureName}] MipMaps count mismatch");
                                break;

                            case ModResult.Reset:
                                errorMessages.Add($"[{mod.Head.TextureName}] Reset impossible");
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"Failed to process mod '{modName}': {ex.Message}");
                }
            }

            if (anyModApplied)
            {
                ReloadMods();
                ReloadTextureFromNode(lastEntryNode);
                saveManifestToolStripMenuItem.Enabled = true;
            }

            if (errorMessages.Count > 0)
            {
                string errorText = "The following errors occurred:\n\n" + string.Join("\n\n", errorMessages);
                MessageBox.Show(errorText,
                              "Wrong Mods",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Information);
            }
            else if (!anyModApplied)
            {
                MessageBox.Show("No mods were applied",
                              "Information",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Information);
            }
        }

        private void resetModToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> errorMessages = [];
            bool anyModReset = false;

            foreach (int index in modListBox.SelectedIndices)
            {
                string modName = modListBox.Items[index].ToString();
                string filename = Path.Combine(ModsPath, $"{modName}.json");

                if (!File.Exists(filename))
                {
                    errorMessages.Add($"Mod file not found: {modName}");
                    continue;
                }

                var mods = LoadMods(filename);
                foreach (var mod in mods)
                {
                    var result = manifest.ResetMod(mod, ManifestPath);
                    if (result == ModResult.NotMatch)
                    {
                        errorMessages.Add($"[{mod.Head.TextureName}] Wrong Original Texture");
                        break;
                    }
                    else if (result == ModResult.Success)
                    {
                        anyModReset = true;
                    }
                }
            }

            if (anyModReset)
            {
                ReloadMods();
                ReloadTextureFromNode(lastEntryNode);
                saveManifestToolStripMenuItem.Enabled = true;
            }

            if (errorMessages.Count > 0)
            {
                string errorText = "The following errors occurred:\n\n" + string.Join("\n\n", errorMessages);
                MessageBox.Show(errorText,
                              "Wrong Mods",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Information);
            }
            else if (!anyModReset)
            {
                MessageBox.Show("Mods were not reset",
                              "Information",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Information);
            }
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool flag = manifest.Entries.Count > 0;
            int selected = modListBox.SelectedIndices.Count;

            reloadModsToolStripMenuItem.Enabled = flag;

            int checkedCount = 0;
            foreach (int index in modListBox.SelectedIndices)
                if (modListBox.GetItemCheckState(index) == CheckState.Checked)
                    checkedCount++;

            applyModToolStripMenuItem.Enabled = flag && selected > 0 && checkedCount == 0;

            resetModToolStripMenuItem.Enabled = flag && selected > 0 && checkedCount == selected;

            modInfoToolStripMenuItem.Enabled = flag && selected == 1;

            saveModsAsToolStripMenuItem.Enabled = flag && selected > 1;

            if (saveModsAsToolStripMenuItem.Enabled)
            {
                foreach (int index in modListBox.SelectedIndices)
                    if (modListBox.GetItemCheckState(index) == CheckState.Indeterminate)
                    {
                        saveModsAsToolStripMenuItem.Enabled = false;
                        break;
                    }
            }

            deleteToolStripMenuItem.Enabled = flag && selected > 0 && checkedCount == 0;

            if (flag && selected == 1)
            {
                int index = modListBox.SelectedIndex;
                string modName = modListBox.Items[index].ToString();
                string filename = Path.Combine(ModsPath, $"{modName}.json");

                if (!File.Exists(filename)) return;

                modInfoToolStripMenuItem.DropDownItems.Clear();

                var mods = LoadMods(filename);
                foreach (var mod in mods)
                {
                    var menuItem = new ToolStripMenuItem
                    {
                        Text = mod.Head.TextureName,
                        Tag = mod
                    };
                    menuItem.Click += textureSelectToolStripMenuItem_Click;
                    modInfoToolStripMenuItem.DropDownItems.Add(menuItem);
                }
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Confirmation dialog
            DialogResult result = MessageBox.Show(
                "Are you sure you want to delete the selected mods?",
                "Confirm Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2); // Default to "No"

            if (result != DialogResult.Yes) return;

            int successCount = 0;
            int failCount = 0;
            List<object> itemsToRemove = [];
            var errorMessages = new StringBuilder();

            foreach (int index in modListBox.SelectedIndices)
            {
                try
                {
                    string modName = modListBox.Items[index].ToString();
                    string filePath = Path.Combine(ModsPath, $"{modName}.json");

                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        itemsToRemove.Add(modListBox.Items[index]);
                        successCount++;
                    }
                    else
                    {
                        failCount++;
                        errorMessages.AppendLine($"• File not found: {modName}.json");
                    }
                }
                catch (Exception ex)
                {
                    failCount++;
                    errorMessages.AppendLine($"• Failed to delete {modListBox.Items[index]}: {ex.Message}");
                }
            }

            foreach (var item in itemsToRemove)
                modListBox.Items.Remove(item);

            if (failCount > 0)
            {
                MessageBox.Show($"Deleted {successCount} mod(s) successfully.\n\n" +
                    $"Failed to delete {failCount} mod(s):\n" + errorMessages.ToString(),
                    "Deletion Results",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else if (successCount > 0)
            {
                MessageBox.Show(
                    $"Successfully deleted {successCount} mod(s).",
                    "Operation Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void openModsFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string args = $"\"{ModsPath}\"";

                if (modListBox.SelectedIndex != -1)
                {
                    string selectedMod = modListBox.SelectedItem.ToString();
                    string filePath = Path.Combine(ModsPath, $"{selectedMod}.json");

                    if (File.Exists(filePath))
                        args = $"/select,\"{filePath}\"";
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = args,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open Mods folder:\n{ex.Message}",
                              "Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
        }

        private void textureSelectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem && menuItem.Tag is TextureMipMapsInfo mod)
            {
                SelectTexture(mod);
            }
        }

        private void SelectTexture(TextureMipMapsInfo mod)
        {
            var entry = manifest.GetTextureEntry(mod);
            if (entry == null) return;

            TreeNode foundNode = FindNodeByTag(manifestTreeView.Nodes, entry);
            if (foundNode != null)
            {
                tabControl1.SelectedTab = tabPage1;
                manifestTreeView.SelectedNode = foundNode;
                manifestTreeView.Focus();
            }
            else
            {
                DialogResult result = MessageBox.Show($"Texture [{mod.Head.TextureName}] not found.\n\n" +
                    "Remove the filter and try again?",
                    "Not Found",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2); // Default to "No"

                if (result != DialogResult.Yes) return;

                ResetManifestTreeView();
                SelectTexture(mod);
            }
        }

        private TreeNode FindNodeByTag(TreeNodeCollection nodes, object targetTag)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag == targetTag)
                    return node;

                var childResult = FindNodeByTag(node.Nodes, targetTag);
                if (childResult != null)
                    return childResult;
            }
            return null;
        }

        private void saveModsAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = ModsPath;
            saveFileDialog.Filter = "Mod Files (*.json)|*.json";
            saveFileDialog.Title = "Save a Mod File";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                List<TextureMipMapsInfo> merged = [];
                foreach (int index in modListBox.SelectedIndices)
                {
                    string modName = modListBox.Items[index].ToString();
                    string filename = Path.Combine(ModsPath, $"{modName}.json");

                    if (!File.Exists(filename)) return;
                    merged.AddRange(LoadMods(filename));
                }

                if (TextureMipMapsInfo.SaveMods(merged, saveFileDialog.FileName) == false)
                {
                    MessageBox.Show("Found duplicate texture", "Duplicate Textures", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    ReloadMods();
                }
            }
        }
    }
}
