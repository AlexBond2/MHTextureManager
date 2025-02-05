using DDSLib;
using System.Windows.Media.Imaging;

namespace MHTextureManager
{
    public partial class MainForm : Form
    {
        private TextureManifest manifest;
        private List<TreeNode> rootNodes;
        private DdsFile ddsFile;
        private TextureFileCache textureCache;
        private SettingsForm settingsForm;
        private HashSet<string> standardList;

        public const string ManifestName = "TextureFileCacheManifest.bin";
        public string ManifestPath = "";

        public MainForm()
        {
            ddsFile = new();
            manifest = new();
            rootNodes = new();
            textureCache = new();
            settingsForm = new();
            standardList = new();

            InitializeComponent();

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

            foreach (var group in groupedEntries)
            {
                var rootNode = new TreeNode(group.Key);
                rootNodes.Add(rootNode);

                Task.Run(() => AddChildNodes(rootNode, [.. group]));
            }
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
            filterBox.Text = "";
            if (rootNodes.Count == 0) return;

            manifestTreeView.BeginUpdate();
            manifestTreeView.Nodes.Clear();
            manifestTreeView.Nodes.AddRange([.. rootNodes]);
            manifestTreeView.EndUpdate();

            statusFiltered.Text = manifestTreeView.Nodes.Count.ToString();
        }

        private void manifestTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is TextureEntry entry)
            {
                textureNameLabel.Text = entry.Head.TextureName;
                textureGuidLabel.Text = entry.Head.TextureGuid.ToString();
                mipMapsLabel.Text = entry.Data.Maps.Count.ToString();
                textureFileLabel.Text = entry.Data.TextureFileName;

                UpdateMipMapBox(entry);
                LoadTextureCache(entry);
            }
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
            openFileDialog.Filter = "DDS Files (*.dds)|*.dds";
            openFileDialog.Title = "Select a DDS File";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filename = openFileDialog.FileName;
                ImportDds(filename);
            }
        }

        private void ImportDds(string filename)
        {
            var ddsHeader = new DdsFile(filename, true);

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
                
                textureCache.WriteTexture(ManifestPath, settingsForm.TextureCacheName, settingsForm.ImportType, ddsHeader);

                saveManifestToolStripMenuItem.Enabled = true;

                LoadTextureCache(entry);
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
            saveFileDialog.Filter = "DDS Files (*.dds)|*.dds";
            saveFileDialog.Title = "Save a DDS File";

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
    }
}
