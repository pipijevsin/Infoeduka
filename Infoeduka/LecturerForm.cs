using Infoeduka.Core;

namespace Infoeduka;

public class LecturerForm : Form
{
    public LecturerForm(Store store, User? lecturer)
    {
        Text = lecturer == null ? "Add lecturer" : "Edit lecturer";
        Ui.SetupDialog(this, 380, 220);

        var firstName = new TextBox { Left = 120, Top = 20, Width = 230, Text = lecturer?.FirstName ?? "" };
        var lastName = new TextBox { Left = 120, Top = 55, Width = 230, Text = lecturer?.LastName ?? "" };
        var email = new TextBox { Left = 120, Top = 90, Width = 230, Text = lecturer?.Email ?? "" };
        var password = new TextBox { Left = 120, Top = 125, Width = 230, UseSystemPasswordChar = true };

        Controls.AddRange(new Control[]
        {
            Ui.Label("First name", 20), firstName,
            Ui.Label("Last name", 55), lastName,
            Ui.Label("Email", 90), email,
            Ui.Label(lecturer == null ? "Password" : "New password", 125), password
        });

        Ui.AddButtons(this, 170, () =>
        {
            if (lecturer == null)
                store.AddLecturer(firstName.Text, lastName.Text, email.Text, password.Text);
            else
                store.UpdateLecturer(lecturer.Id, firstName.Text, lastName.Text, email.Text, password.Text);
        });
    }
}
