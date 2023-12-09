namespace Browser.Selector.Cli.UI
{
    using System.Windows.Forms;

    public class DismissibleForm : Form
    {
        public DismissibleForm() : base()
        {
            Deactivate += new System.EventHandler((a, b) => Close());
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                DialogResult = DialogResult.Abort;
                Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
