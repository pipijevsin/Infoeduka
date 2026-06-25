namespace Infoeduka;

static class Ui
{
    public static void Try(Action action)
    {
        try
        {
            action();
        }
        catch (Exception e) when (e is ArgumentException or InvalidOperationException)
        {
            MessageBox.Show(e.Message, "Infoeduka", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    public static bool Confirm(string message)
    {
        return MessageBox.Show(message, "Infoeduka", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }

    public static Label Label(string text, int top)
    {
        return new Label { Text = text, Left = 20, Top = top + 4, AutoSize = true };
    }

    public static void SetupDialog(Form form, int width, int height)
    {
        form.ClientSize = new Size(width, height);
        form.FormBorderStyle = FormBorderStyle.FixedDialog;
        form.MaximizeBox = false;
        form.MinimizeBox = false;
        form.StartPosition = FormStartPosition.CenterParent;
    }

    public static void AddButtons(Form form, int top, Action onSave)
    {
        var save = new Button { Text = "Save", Left = form.ClientSize.Width - 190, Top = top, Width = 80 };
        var cancel = new Button { Text = "Cancel", Left = form.ClientSize.Width - 100, Top = top, Width = 80, DialogResult = DialogResult.Cancel };
        save.Click += (_, _) => Try(() =>
        {
            onSave();
            form.DialogResult = DialogResult.OK;
        });
        form.AcceptButton = save;
        form.CancelButton = cancel;
        form.Controls.Add(save);
        form.Controls.Add(cancel);
    }
}
