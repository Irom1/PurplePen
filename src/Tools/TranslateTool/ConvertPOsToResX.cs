using System;
using System.IO;
using System.Windows.Forms;

namespace TranslateTool
{
    public partial class ConvertPOsToResX : Form
    {
        public ConvertPOsToResX() {
            InitializeComponent();
        }

        public string ResXFileName {
            get {
                return textBoxResXFile.Text;
            }
        }

        public string PODirectory {
            get {
                return textBoxPODirectory.Text;
            }
        }

        private void buttonSelectResXFile_Click(object sender, EventArgs e) {
            if (textBoxResXFile.Text.Length > 0 && File.Exists(textBoxResXFile.Text)) {
                openFileDialog.InitialDirectory = Path.GetDirectoryName(textBoxResXFile.Text);
                openFileDialog.FileName = Path.GetFileName(textBoxResXFile.Text);
            }
            else {
                Uri uri = new Uri(typeof(OpenDirectory).Assembly.CodeBase);
                openFileDialog.InitialDirectory = Path.GetFullPath(Path.GetDirectoryName(uri.LocalPath) + @"\..\..\..\..");
                openFileDialog.FileName = "";
            }

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                textBoxResXFile.Text = openFileDialog.FileName;
        }

        private void buttonSelectPODirectory_Click(object sender, EventArgs e) {
            if (textBoxPODirectory.Text.Length > 0)
                folderBrowserDialog.SelectedPath = textBoxPODirectory.Text;
            else {
                Uri uri = new Uri(typeof(OpenDirectory).Assembly.CodeBase);
                folderBrowserDialog.SelectedPath = Path.GetFullPath(Path.GetDirectoryName(uri.LocalPath) + @"\..\..\..\..");
            }

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                textBoxPODirectory.Text = folderBrowserDialog.SelectedPath;
        }
    }
}
