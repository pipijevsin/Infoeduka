namespace Infoeduka.Core;

public enum Role
{
    Administrator,
    Lecturer
}

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public Role Role { get; set; }
    public string FullName => $"{FirstName} {LastName}";
}

public class Course
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int LecturerId { get; set; }
}

public class Notification
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime PublishDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int CreatedById { get; set; }
}
