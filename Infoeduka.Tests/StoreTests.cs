using Infoeduka.Core;

namespace Infoeduka.Tests;

public class StoreTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"infoeduka-{Guid.NewGuid()}.json");
    private readonly Store _store;

    public StoreTests()
    {
        _store = new Store(_path);
        _store.Login("admin@algebra.hr", "admin");
    }

    public void Dispose()
    {
        File.Delete(_path);
    }

    private User AddLecturer(string email = "ana@algebra.hr")
    {
        return _store.AddLecturer("Ana", "Anić", email, "pass");
    }

    [Fact]
    public void DefaultAdminCanLogin()
    {
        var store = new Store(_path);
        Assert.True(store.Login("admin@algebra.hr", "admin"));
        Assert.Equal(Role.Administrator, store.CurrentUser!.Role);
    }

    [Fact]
    public void WrongPasswordFailsLogin()
    {
        var store = new Store(_path);
        Assert.False(store.Login("admin@algebra.hr", "wrong"));
        Assert.Null(store.CurrentUser);
    }

    [Fact]
    public void PasswordIsStoredHashed()
    {
        var lecturer = AddLecturer();
        Assert.NotEqual("pass", lecturer.PasswordHash);
        Assert.Equal(Store.Hash("pass"), lecturer.PasswordHash);
    }

    [Fact]
    public void LecturerCanLoginAfterBeingAdded()
    {
        AddLecturer();
        Assert.True(_store.Login("ana@algebra.hr", "pass"));
        Assert.Equal(Role.Lecturer, _store.CurrentUser!.Role);
    }

    [Fact]
    public void DuplicateEmailIsRejected()
    {
        AddLecturer();
        Assert.Throws<ArgumentException>(() => AddLecturer());
    }

    [Fact]
    public void EmptyNameIsRejected()
    {
        Assert.Throws<ArgumentException>(() => _store.AddLecturer("", "Anić", "x@algebra.hr", "pass"));
    }

    [Fact]
    public void LecturerWithCoursesCannotBeDeleted()
    {
        var lecturer = AddLecturer();
        _store.AddCourse("Math", lecturer.Id);
        Assert.Throws<ArgumentException>(() => _store.DeleteLecturer(lecturer.Id));
    }

    [Fact]
    public void CourseRequiresNameAndLecturer()
    {
        var lecturer = AddLecturer();
        Assert.Throws<ArgumentException>(() => _store.AddCourse("", lecturer.Id));
        Assert.Throws<ArgumentException>(() => _store.AddCourse("Math", 999));
    }

    [Fact]
    public void LecturerCannotManageCourses()
    {
        var lecturer = AddLecturer();
        _store.Login("ana@algebra.hr", "pass");
        Assert.Throws<InvalidOperationException>(() => _store.AddCourse("Math", lecturer.Id));
    }

    [Fact]
    public void LecturerSeesOnlyOwnCourses()
    {
        var ana = AddLecturer();
        var ivo = AddLecturer("ivo@algebra.hr");
        _store.AddCourse("Math", ana.Id);
        _store.AddCourse("Physics", ivo.Id);

        _store.Login("ana@algebra.hr", "pass");
        Assert.Equal(new[] { "Math" }, _store.VisibleCourses().Select(c => c.Name));
    }

    [Fact]
    public void AdminSeesAllCourses()
    {
        var ana = AddLecturer();
        var ivo = AddLecturer("ivo@algebra.hr");
        _store.AddCourse("Math", ana.Id);
        _store.AddCourse("Physics", ivo.Id);
        Assert.Equal(2, _store.VisibleCourses().Count());
    }

    [Fact]
    public void LecturerCannotAddNotificationToOtherCourse()
    {
        var ivo = AddLecturer("ivo@algebra.hr");
        AddLecturer();
        var physics = _store.AddCourse("Physics", ivo.Id);

        _store.Login("ana@algebra.hr", "pass");
        Assert.Throws<InvalidOperationException>(() =>
            _store.AddNotification(physics.Id, "Exam", "", DateTime.Today, DateTime.Today));
    }

    [Fact]
    public void LecturerCanAddNotificationToOwnCourse()
    {
        var ana = AddLecturer();
        var math = _store.AddCourse("Math", ana.Id);

        _store.Login("ana@algebra.hr", "pass");
        var notification = _store.AddNotification(math.Id, "Exam", "Room 101", DateTime.Today, DateTime.Today.AddDays(3));

        Assert.Equal(ana.Id, notification.CreatedById);
        Assert.Single(_store.VisibleNotifications());
    }

    [Fact]
    public void ExpiryBeforePublishIsRejected()
    {
        var ana = AddLecturer();
        var math = _store.AddCourse("Math", ana.Id);
        Assert.Throws<ArgumentException>(() =>
            _store.AddNotification(math.Id, "Exam", "", DateTime.Today, DateTime.Today.AddDays(-1)));
    }

    [Fact]
    public void DeletingCourseDeletesItsNotifications()
    {
        var ana = AddLecturer();
        var math = _store.AddCourse("Math", ana.Id);
        _store.AddNotification(math.Id, "Exam", "", DateTime.Today, DateTime.Today);

        _store.DeleteCourse(math.Id);
        Assert.Empty(_store.Notifications);
    }

    [Fact]
    public void DataSurvivesRestart()
    {
        var ana = AddLecturer();
        var math = _store.AddCourse("Math", ana.Id);
        _store.AddNotification(math.Id, "Exam", "Room 101", DateTime.Today, DateTime.Today);

        var reloaded = new Store(_path);
        reloaded.Login("admin@algebra.hr", "admin");

        Assert.Single(reloaded.Lecturers);
        Assert.Equal("Math", reloaded.Courses.Single().Name);
        Assert.Equal("Room 101", reloaded.Notifications.Single().Description);
    }
}
