using Infoeduka.Core;

namespace Infoeduka;

public class LoginForm : Form
{
    public LoginForm(Store store)
    {
        Text = "Infoeduka - Login";
        Ui.SetupDialog(this, 340, 150);
        StartPosition = FormStartPosition.CenterScreen;

        var email = new TextBox { Left = 110, Top = 20, Width = 200 };
        var password = new TextBox { Left = 110, Top = 55, Width = 200, UseSystemPasswordChar = true };
        var login = new Button { Text = "Login", Left = 110, Top = 95, Width = 200, Height = 30 };

        login.Click += (_, _) =>
        {
            if (store.Login(email.Text, password.Text))
                DialogResult = DialogResult.OK;
            else
                MessageBox.Show("Wrong email or password.", "Infoeduka", MessageBoxButtons.OK, MessageBoxIcon.Error);
        };

        AcceptButton = login;
        Controls.AddRange(new Control[]
        {
            Ui.Label("Email", 20), email,
            Ui.Label("Password", 55), password,
            login
        });
    }
}
