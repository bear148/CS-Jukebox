using System;
using System.IO;
using System.Windows.Forms;

namespace CS_Jukebox
{
    public partial class GamePathForm : Form
    {
        private bool dirValid;

        public GamePathForm()
        {
            InitializeComponent();
            MaximizeBox = false;
            MinimizeBox = false;
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                dirTextBox.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private static bool IsValidCs2Root(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                return false;

            var candidates = new[]
            {
                Path.Combine(path, "game", "bin", "win64"),
                Path.Combine(path, "game", "csgo"),
                Path.Combine(path, "csgo", "cfg")
            };

            foreach (var candidate in candidates)
            {
                if (Directory.Exists(candidate))
                    return true;
            }

            return false;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (dirValid)
            {
                Properties.SaveProperties();
                Close();
            }
        }

        private void dirTextBox_TextChanged(object sender, EventArgs e)
        {
            dirValid = IsValidCs2Root(dirTextBox.Text);

            if (dirValid)
            {
                Properties.GameDir = dirTextBox.Text.TrimEnd('\\', '/');
                errorLabel.Visible = false;
                okButton.Enabled = true;
            }
            else
            {
                Properties.GameDir = string.Empty;
                errorLabel.Visible = true;
                okButton.Enabled = false;
            }
        }
    }
}
