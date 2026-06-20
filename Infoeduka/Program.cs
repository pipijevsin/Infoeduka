using Infoeduka.Core;

namespace Infoeduka;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        var store = new Store(Path.Combine(AppContext.BaseDirectory, "data.json"));
        using var login = new LoginForm(store);
        login.ShowDialog();
    }
}
