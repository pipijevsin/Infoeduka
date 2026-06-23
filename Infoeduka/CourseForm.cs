using Infoeduka.Core;

namespace Infoeduka;

public class CourseForm : Form
{
    public CourseForm(Store store, Course? course)
    {
        Text = course == null ? "Add course" : "Edit course";
        Ui.SetupDialog(this, 380, 150);

        var name = new TextBox { Left = 120, Top = 20, Width = 230, Text = course?.Name ?? "" };
        var lecturer = new ComboBox
        {
            Left = 120,
            Top = 55,
            Width = 230,
            DropDownStyle = ComboBoxStyle.DropDownList,
            DataSource = store.Lecturers.ToList(),
            DisplayMember = "FullName",
            ValueMember = "Id"
        };
        if (course != null)
            lecturer.SelectedValue = course.LecturerId;

        Controls.AddRange(new Control[]
        {
            Ui.Label("Name", 20), name,
            Ui.Label("Lecturer", 55), lecturer
        });

        Ui.AddButtons(this, 100, () =>
        {
            var lecturerId = lecturer.SelectedValue as int? ?? 0;
            if (course == null)
                store.AddCourse(name.Text, lecturerId);
            else
                store.UpdateCourse(course.Id, name.Text, lecturerId);
        });
    }
}
