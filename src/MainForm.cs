using System.Collections.Generic;
using System.Windows.Forms;

namespace MHTextureManager
{
    public partial class MainForm : Form
    {
        private TextureManifest manifest;
        private List<TreeNode> rootNodes;

        public MainForm()
        {
            manifest = new();
            rootNodes = new();
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

                        Invoke(new Action(() =>
                        {
                            manifestTreeView.Nodes.AddRange([.. rootNodes]);
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

                Invoke(new Action(() =>
                {
                    manifestProgressBar.Value++;
                }));
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

            manifestTreeView.Nodes.Clear();

            foreach (TreeNode rootNode in rootNodes)
                if (FilterNode(rootNode, filterText))
                    manifestTreeView.Nodes.Add(rootNode);

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

            manifestTreeView.Nodes.Clear();
            manifestTreeView.Nodes.AddRange([.. rootNodes]);
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
            }
        }

        private void UpdateTexturesTree(TextureEntry entry)
        {
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
        }
    }
}
