using Infoeduka.Core;

namespace Infoeduka;

public class MainForm : Form
{
    private readonly Store _store;
    private readonly DataGridView _courses = NewGrid();
    private readonly DataGridView _notifications = NewGrid();
    private readonly DataGridView _lecturers = NewGrid();

    public MainForm(Store store)
    {
        _store = store;
        var user = store.CurrentUser!;

        Text = "Infoeduka";
        ClientSize = new Size(900, 520);
        MinimumSize = new Size(700, 400);
        StartPosition = FormStartPosition.CenterScreen;

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(MakeTab("Courses", _courses, user.Role == Role.Administrator,
            () => new CourseForm(_store, null),
            id => new CourseForm(_store, _store.Courses.First(c => c.Id == id)),
            id => _store.DeleteCourse(id)));
        tabs.TabPages.Add(MakeTab("Notifications", _notifications, true,
            () => new NotificationForm(_store, null),
            id => new NotificationForm(_store, _store.Notifications.First(n => n.Id == id)),
            id => _store.DeleteNotification(id)));
        if (user.Role == Role.Administrator)
            tabs.TabPages.Add(MakeTab("Lecturers", _lecturers, true,
                () => new LecturerForm(_store, null),
                id => new LecturerForm(_store, _store.Lecturers.First(u => u.Id == id)),
                id => _store.DeleteLecturer(id)));

        var top = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(10, 7, 10, 7) };
        var logout = new Button { Text = "Logout", Width = 90, Dock = DockStyle.Right };
        logout.Click += (_, _) => Close();
        top.Controls.Add(new Label { Text = $"Logged in as {user.FullName} ({user.Role})", Left = 10, Top = 12, AutoSize = true });
        top.Controls.Add(logout);

        Controls.Add(tabs);
        Controls.Add(top);
        RefreshData();
    }

    private static DataGridView NewGrid()
    {
        return new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            RowHeadersVisible = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = SystemColors.Window
        };
    }

    private TabPage MakeTab(string title, DataGridView grid, bool canEdit, Func<Form> addForm, Func<int, Form> editForm, Action<int> delete)
    {
        var page = new TabPage(title);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 36, Padding = new Padding(4) };

        var add = new Button { Text = "Add" };
        var edit = new Button { Text = "Edit" };
        var remove = new Button { Text = "Delete" };

        add.Click += (_, _) => ShowForm(addForm());
        edit.Click += (_, _) =>
        {
            if (SelectedId(grid) is int id)
                ShowForm(editForm(id));
        };
        remove.Click += (_, _) =>
        {
            if (SelectedId(grid) is int id && Ui.Confirm("Delete the selected item?"))
                Ui.Try(() =>
                {
                    delete(id);
                    RefreshData();
                });
        };
        grid.CellDoubleClick += (_, _) => edit.PerformClick();

        buttons.Controls.AddRange(new Control[] { add, edit, remove });
        page.Controls.Add(grid);
        if (canEdit)
            page.Controls.Add(buttons);
        return page;
    }

    private void ShowForm(Form form)
    {
        using (form)
        {
            if (form.ShowDialog(this) == DialogResult.OK)
                RefreshData();
        }
    }

    private static int? SelectedId(DataGridView grid)
    {
        return grid.CurrentRow?.Cells["Id"].Value as int?;
    }

    private void RefreshData()
    {
        _courses.DataSource = _store.VisibleCourses()
            .Select(c => new { c.Id, c.Name, Lecturer = _store.LecturerName(c.LecturerId) })
            .ToList();

        _notifications.DataSource = _store.VisibleNotifications()
            .OrderByDescending(n => n.PublishDate)
            .Select(n => new
            {
                n.Id,
                Course = _store.CourseName(n.CourseId),
                n.Title,
                n.Description,
                Published = n.PublishDate.ToShortDateString(),
                Expires = n.ExpiryDate.ToShortDateString(),
                CreatedBy = _store.LecturerName(n.CreatedById)
            })
            .ToList();

        _lecturers.DataSource = _store.Lecturers
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.Email })
            .ToList();

        foreach (var grid in new[] { _courses, _notifications, _lecturers })
            if (grid.Columns["Id"] != null)
                grid.Columns["Id"]!.Visible = false;
    }
}
