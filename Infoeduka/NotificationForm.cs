using Infoeduka.Core;

namespace Infoeduka;

public class NotificationForm : Form
{
    public NotificationForm(Store store, Notification? notification)
    {
        Text = notification == null ? "Add notification" : "Edit notification";
        Ui.SetupDialog(this, 420, 330);

        var course = new ComboBox
        {
            Left = 120,
            Top = 20,
            Width = 270,
            DropDownStyle = ComboBoxStyle.DropDownList,
            DataSource = store.VisibleCourses().ToList(),
            DisplayMember = "Name",
            ValueMember = "Id"
        };
        if (notification != null)
            course.SelectedValue = notification.CourseId;

        var title = new TextBox { Left = 120, Top = 55, Width = 270, Text = notification?.Title ?? "" };
        var description = new TextBox
        {
            Left = 120,
            Top = 90,
            Width = 270,
            Height = 100,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Text = notification?.Description ?? ""
        };
        var publish = new DateTimePicker { Left = 120, Top = 200, Width = 270, Format = DateTimePickerFormat.Short, Value = notification?.PublishDate ?? DateTime.Today };
        var expiry = new DateTimePicker { Left = 120, Top = 235, Width = 270, Format = DateTimePickerFormat.Short, Value = notification?.ExpiryDate ?? DateTime.Today.AddDays(7) };

        Controls.AddRange(new Control[]
        {
            Ui.Label("Course", 20), course,
            Ui.Label("Title", 55), title,
            Ui.Label("Description", 90), description,
            Ui.Label("Publish date", 200), publish,
            Ui.Label("Expiry date", 235), expiry
        });

        Ui.AddButtons(this, 280, () =>
        {
            var courseId = course.SelectedValue as int? ?? 0;
            if (notification == null)
                store.AddNotification(courseId, title.Text, description.Text, publish.Value, expiry.Value);
            else
                store.UpdateNotification(notification.Id, courseId, title.Text, description.Text, publish.Value, expiry.Value);
        });
    }
}
