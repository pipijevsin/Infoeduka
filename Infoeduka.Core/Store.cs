using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Infoeduka.Core;

public class Store
{
    private class Data
    {
        public List<User> Users { get; set; } = new();
        public List<Course> Courses { get; set; } = new();
        public List<Notification> Notifications { get; set; } = new();
    }

    private readonly string _path;
    private readonly Data _data;

    public User? CurrentUser { get; private set; }
    public List<Course> Courses => _data.Courses;
    public List<Notification> Notifications => _data.Notifications;
    public IEnumerable<User> Lecturers => _data.Users.Where(u => u.Role == Role.Lecturer);

    public Store(string path)
    {
        _path = path;
        _data = File.Exists(path)
            ? JsonSerializer.Deserialize<Data>(File.ReadAllText(path)) ?? new Data()
            : new Data();

        if (_data.Users.Count == 0)
        {
            _data.Users.Add(new User
            {
                Id = 1,
                FirstName = "System",
                LastName = "Administrator",
                Email = "admin@algebra.hr",
                PasswordHash = Hash("admin"),
                Role = Role.Administrator
            });
            Save();
        }
    }

    public static string Hash(string password)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
    }

    private void Save()
    {
        File.WriteAllText(_path, JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static int NextId(IEnumerable<int> ids)
    {
        return ids.DefaultIfEmpty(0).Max() + 1;
    }

    public bool Login(string email, string password)
    {
        var user = _data.Users.FirstOrDefault(u =>
            u.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase) && u.PasswordHash == Hash(password));
        CurrentUser = user;
        return user != null;
    }

    public void Logout()
    {
        CurrentUser = null;
    }

    private User RequireLogin()
    {
        return CurrentUser ?? throw new InvalidOperationException("You must be logged in.");
    }

    private void RequireAdmin()
    {
        if (RequireLogin().Role != Role.Administrator)
            throw new InvalidOperationException("Only an administrator can do this.");
    }

    public string LecturerName(int lecturerId)
    {
        return _data.Users.FirstOrDefault(u => u.Id == lecturerId)?.FullName ?? "";
    }

    public string CourseName(int courseId)
    {
        return _data.Courses.FirstOrDefault(c => c.Id == courseId)?.Name ?? "";
    }

    public User AddLecturer(string firstName, string lastName, string email, string password)
    {
        RequireAdmin();
        ValidateUser(firstName, lastName, email, null);
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is required.");

        var user = new User
        {
            Id = NextId(_data.Users.Select(u => u.Id)),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.Trim(),
            PasswordHash = Hash(password),
            Role = Role.Lecturer
        };
        _data.Users.Add(user);
        Save();
        return user;
    }

    public void UpdateLecturer(int id, string firstName, string lastName, string email, string? newPassword)
    {
        RequireAdmin();
        ValidateUser(firstName, lastName, email, id);
        var user = Lecturers.First(u => u.Id == id);
        user.FirstName = firstName.Trim();
        user.LastName = lastName.Trim();
        user.Email = email.Trim();
        if (!string.IsNullOrWhiteSpace(newPassword))
            user.PasswordHash = Hash(newPassword);
        Save();
    }

    public void DeleteLecturer(int id)
    {
        RequireAdmin();
        if (_data.Courses.Any(c => c.LecturerId == id))
            throw new ArgumentException("Lecturer still has courses assigned.");
        _data.Users.RemoveAll(u => u.Id == id && u.Role == Role.Lecturer);
        Save();
    }

    private void ValidateUser(string firstName, string lastName, string email, int? existingId)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("First and last name are required.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("A valid email is required.");
        if (_data.Users.Any(u => u.Id != existingId && u.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("A user with this email already exists.");
    }

    public IEnumerable<Course> VisibleCourses()
    {
        var user = RequireLogin();
        return user.Role == Role.Administrator
            ? _data.Courses
            : _data.Courses.Where(c => c.LecturerId == user.Id);
    }

    public Course AddCourse(string name, int lecturerId)
    {
        RequireAdmin();
        ValidateCourse(name, lecturerId);
        var course = new Course { Id = NextId(_data.Courses.Select(c => c.Id)), Name = name.Trim(), LecturerId = lecturerId };
        _data.Courses.Add(course);
        Save();
        return course;
    }

    public void UpdateCourse(int id, string name, int lecturerId)
    {
        RequireAdmin();
        ValidateCourse(name, lecturerId);
        var course = _data.Courses.First(c => c.Id == id);
        course.Name = name.Trim();
        course.LecturerId = lecturerId;
        Save();
    }

    public void DeleteCourse(int id)
    {
        RequireAdmin();
        _data.Courses.RemoveAll(c => c.Id == id);
        _data.Notifications.RemoveAll(n => n.CourseId == id);
        Save();
    }

    private void ValidateCourse(string name, int lecturerId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Course name is required.");
        if (!Lecturers.Any(u => u.Id == lecturerId))
            throw new ArgumentException("A lecturer must be selected.");
    }

    public IEnumerable<Notification> VisibleNotifications()
    {
        var courseIds = VisibleCourses().Select(c => c.Id).ToHashSet();
        return _data.Notifications.Where(n => courseIds.Contains(n.CourseId));
    }

    public Notification AddNotification(int courseId, string title, string description, DateTime publishDate, DateTime expiryDate)
    {
        var user = RequireLogin();
        RequireCourseAccess(courseId);
        ValidateNotification(title, publishDate, expiryDate);
        var notification = new Notification
        {
            Id = NextId(_data.Notifications.Select(n => n.Id)),
            CourseId = courseId,
            Title = title.Trim(),
            Description = description.Trim(),
            PublishDate = publishDate.Date,
            ExpiryDate = expiryDate.Date,
            CreatedById = user.Id
        };
        _data.Notifications.Add(notification);
        Save();
        return notification;
    }

    public void UpdateNotification(int id, int courseId, string title, string description, DateTime publishDate, DateTime expiryDate)
    {
        var notification = _data.Notifications.First(n => n.Id == id);
        RequireCourseAccess(notification.CourseId);
        RequireCourseAccess(courseId);
        ValidateNotification(title, publishDate, expiryDate);
        notification.CourseId = courseId;
        notification.Title = title.Trim();
        notification.Description = description.Trim();
        notification.PublishDate = publishDate.Date;
        notification.ExpiryDate = expiryDate.Date;
        Save();
    }

    public void DeleteNotification(int id)
    {
        var notification = _data.Notifications.First(n => n.Id == id);
        RequireCourseAccess(notification.CourseId);
        _data.Notifications.Remove(notification);
        Save();
    }

    private void RequireCourseAccess(int courseId)
    {
        if (!VisibleCourses().Any(c => c.Id == courseId))
            throw new InvalidOperationException("You can only manage notifications for your own courses.");
    }

    private static void ValidateNotification(string title, DateTime publishDate, DateTime expiryDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.");
        if (expiryDate.Date < publishDate.Date)
            throw new ArgumentException("Expiry date cannot be before the publish date.");
    }
}
