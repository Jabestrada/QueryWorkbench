using System;
using System.Windows.Forms;

namespace QueryWorkbenchUI.UserForms {
    public partial class TextInputDialog : Form {
        public TextInputDialog(string labelCaption = "", 
                               string defaultValue = "", 
                               string formCaption = "") {
            InitializeComponent();

            AssignTextIfNotNull(formCaption, () => Text = formCaption);
            AssignTextIfNotNull(labelCaption, () => lblFieldLabel.Text = labelCaption);
            AssignTextIfNotNull(defaultValue, () => txtInput.Text = defaultValue);

            ActiveControl = txtInput;
            txtInput.SelectAll();
        }

        private void AssignTextIfNotNull(string input, Action assignAction) {
            if (!string.IsNullOrWhiteSpace(input)) {
                assignAction();
            }
        }

        public string Input {
            get {
                return txtInput.Text.Trim();
            }
        }
        private void btnOk_Click(object sender, EventArgs e) {
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            Close();
        }
    }
}
