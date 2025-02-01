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

        public string ManifestPath;

        public MainForm()
        {
            ddsFile = new();
            manifest = new();
            rootNodes = new();
            textureCache = new();
            InitializeComponent();
            manifestProgressBar.Visible = false;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Texture Manifest (*.bin)|TextureFileCacheManifest.bin";
                openFileDialog.Title = "Select TextureFileCacheManifest.bin";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFile = Path.GetFileName(openFileDialog.FileName);

                    if (selectedFile != "TextureFileCacheManifest.bin")
                    {
                        MessageBox.Show("Please select the correct file: TextureFileCacheManifest.bin",
                                        "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string filePath = openFileDialog.FileName;
                    ManifestPath = Path.GetDirectoryName(filePath) ?? "";

                    manifestTreeView.Nodes.Clear();
                    manifestProgressBar.Visible = true;
                    manifestProgressBar.Value = 0;

                    Task.Run(() =>
                    {
                        var entries = manifest.LoadManifestFromFile(filePath);
                        int totalEntries = entries.Count;

                        Invoke(new Action(() =>
                        {
                            manifestProgressBar.Maximum = totalEntries;
                            totalTexturesStatus.Text = totalEntries.ToString();
                        }));

                        BuildTree(entries);

                        BeginInvoke(new Action(() =>
                        {
                            manifestTreeView.BeginUpdate();
                            manifestTreeView.Nodes.AddRange([.. rootNodes]);
                            manifestTreeView.EndUpdate();
                            manifestProgressBar.Value = manifestProgressBar.Maximum;
                        }));

                        Invoke(new Action(() =>
                        {
                            statusFiltered.Text = manifestTreeView.Nodes.Count.ToString();
                            manifestProgressBar.Visible = false;
                        }));
                    });
                }
            }
        }

        private void BuildTree(List<TextureEntry> entries)
        {
            rootNodes.Clear();
            int count = 0;

            foreach (var entry in entries)
            {
                string[] parts = entry.Head.TextureName.Split('.');
                TreeNode? currentNode = null;

                foreach (var part in parts)
                {
                    if (currentNode == null)
                    {
                        currentNode = rootNodes.FirstOrDefault(n => n.Text == part);

                        if (currentNode == null)
                        {
                            currentNode = new TreeNode(part);
                            rootNodes.Add(currentNode);
                        }
                    }
                    else
                    {
                        TreeNode? childNode = currentNode.Nodes.Cast<TreeNode>()
                                                              .FirstOrDefault(n => n.Text == part);

                        if (childNode == null)
                        {
                            childNode = new TreeNode(part);
                            currentNode.Nodes.Add(childNode);
                        }

                        currentNode = childNode;
                    }

                    if (Array.IndexOf(parts, part) == parts.Length - 1)
                        currentNode.Tag = entry;
                }

                if (++count % 100 == 0)
                    Invoke(new Action(() => manifestProgressBar.Value += 100));
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
                textureGuidLabel.Text = entry.Head.TextureGUID.ToString();
                mipMapsLabel.Text = entry.Data.Maps.Count.ToString();
                textureFileLabel.Text = entry.Data.TextureFileName;

                UpdateTexturesTree(entry);
                LoadTextureCache(entry);
            }
        }

        private void LoadTextureCache(TextureEntry entry)
        {
            string tfcPath = Path.Combine(ManifestPath, entry.Data.TextureFileName + ".tfc");
            if (entry.Data.Maps.Count == 0) return;
            if (textureCache.LoadFromFile(tfcPath, entry) && textureCache.Texture2D.MipMaps.Count > 0)
            {
                var stream = textureCache.Texture2D.GetObjectStream(0);
                ddsFile.Load(stream);
                textureView.Image = BitmapSourceToBitmap(ddsFile.BitmapSource);
                CenterTexture();
            }
            else MessageBox.Show($"Can't Load TFC: {entry.Head.TextureName}\nFile: {tfcPath}",
                                 "Error load", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void UpdateTexturesTree(TextureEntry entry)
        {
            texturesTree.BeginUpdate();
            texturesTree.Nodes.Clear();

            var rootNode = new TreeNode(entry.Data.TextureFileName);

            foreach (var mipMap in entry.Data.Maps)
            {
                var mipNode = new TreeNode($"MipMap[{mipMap.Index}]") { Tag = mipMap };

                var offsetNode = new TreeNode($"Offset = {mipMap.Offset}");
                var sizeNode = new TreeNode($"Size = {mipMap.Size}");

                mipNode.Nodes.Add(offsetNode);
                mipNode.Nodes.Add(sizeNode);

                rootNode.Nodes.Add(mipNode);
            }

            texturesTree.Nodes.Add(rootNode);
            texturesTree.ExpandAll();
            texturesTree.EndUpdate();
        }

        private void importDDSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "DDS Files (*.dds)|*.dds";
                openFileDialog.Title = "Select a DDS File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filename = openFileDialog.FileName;
                    ImportDds(filename);
                }
            }
            
        }

        private void ImportDds(string filename)
        {
            ddsFile = new DdsFile(filename);
            textureView.Image = BitmapSourceToBitmap(ddsFile.BitmapSource);
            CenterTexture();
            MessageBox.Show("Import not ready!", "TODO", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.FileName = textureNameLabel.Text + ".dds";
                saveFileDialog.Filter = "DDS Files (*.dds)|*.dds";
                saveFileDialog.Title = "Save a DDS File";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filename = saveFileDialog.FileName; 
                    var stream = textureCache.Texture2D.GetObjectStream(0);
                    if (stream == null) return;

                    using var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);
                }
            }
        }
    }
}
