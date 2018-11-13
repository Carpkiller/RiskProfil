using System;
using System.Windows.Forms;

namespace RiskProfil
{
    public partial class InputDialogBox : Form
    {
        public string ReturnValue1 { get; set; }
        public InputDialogBox()
        {
            InitializeComponent();
            textBox1.Focus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.ReturnValue1 = textBox1.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
