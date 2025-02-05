
namespace MHTextureManager
{
    public partial class SettingsForm : Form
    {
        public string TextureCachePath;
        public string TextureCacheName;
        public bool CanChanged;
        public ImportType ImportType;

        public SettingsForm()
        {
            TextureCachePath = "";
            TextureCacheName = "";
            InitializeComponent();
        }

        private void CheckEnabled()
        {
            importButton.Enabled = false;
            createOpen.Enabled = radioNew.Checked;
            selectOpen.Enabled = radioAdd.Checked;
        }

        private void radioNew_CheckedChanged(object sender, EventArgs e)
        {
            // Check New
            CheckEnabled();
            if (radioNew.Checked)
                importButton.Enabled = checkFileName(createBox.Text, true);
        }

        private void radioAdd_CheckedChanged(object sender, EventArgs e)
        {
            // Check Add
            CheckEnabled();
            if (radioAdd.Checked)
                importButton.Enabled = checkFileName(addBox.Text);
        }

        private void radioReplace_CheckedChanged(object sender, EventArgs e)
        {
            // Check Replace
            CheckEnabled();
            if (radioReplace.Checked)
                importButton.Enabled = checkFileName(replaceBox.Text);
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            // Select possible States
            radioNew.Checked = true;
            importButton.Enabled = false;
            replaceBox.Text = TextureCacheName;
        }

        private static string JustName(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return string.Empty;

            string fileName = Path.GetFileName(filePath);
            return Path.GetFileNameWithoutExtension(fileName);
        }

        private bool checkFileName(string newChache, bool isNew = false)
        {
            if (newChache == string.Empty) return false;

            string tfcPath = Path.Combine(TextureCachePath, newChache + ".tfc");
            if (isNew && File.Exists(tfcPath))
            {
                MessageBox.Show($"Please select a new file. File {Path.GetFileName(tfcPath)} exist!",
                                 "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (isNew == false && !File.Exists(tfcPath))
            {
                MessageBox.Show($"Please select an existing file. File {Path.GetFileName(tfcPath)} is not exist!",
                                 "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (CanChanged == false && newChache == TextureCacheName)
            {
                MessageBox.Show($"{newChache} is already reserved for the standard tfc. Please choose another one!",
                                 "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void createOpen_Click(object sender, EventArgs e)
        {
            using var saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = TextureCachePath;
            saveFileDialog.Filter = "TFC files (*.tfc)|*.tfc";
            saveFileDialog.Title = "Select a new TFC file";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string newFile = saveFileDialog.FileName;
                string newChache = JustName(newFile);

                createBox.Text = newChache;
                importButton.Enabled = checkFileName(newChache, true);
            }
        }

        private void selectOpen_Click(object sender, EventArgs e)
        {
            using var selectFileDialog = new OpenFileDialog();
            selectFileDialog.InitialDirectory = TextureCachePath;

            selectFileDialog.Filter = "TFC files (*.tfc)|*.tfc";
            selectFileDialog.Title = "Select a TFC file";

            if (selectFileDialog.ShowDialog() == DialogResult.OK)
            {
                string newFile = selectFileDialog.FileName;
                string newChache = JustName(newFile);

                addBox.Text = newChache;
                importButton.Enabled = checkFileName(newChache);
            }
        }

        private void importButton_Click(object sender, EventArgs e)
        {
            if (radioNew.Checked)
            {
                ImportType = ImportType.New;
                TextureCacheName = createBox.Text;
            }
            else if (radioAdd.Checked)
            {
                ImportType = ImportType.Add;
                TextureCacheName = addBox.Text;
            }
            else if (radioAdd.Checked) 
            { 
                ImportType = ImportType.Replace;
                TextureCacheName = replaceBox.Text;
            }
        }
    }
}
