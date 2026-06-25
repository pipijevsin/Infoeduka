using Infoeduka.Core;

namespace Infoeduka;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        var store = new Store(Path.Combine(AppContext.BaseDirectory, "data.json"));

        while (true)
        {
            using var login = new LoginForm(store);
            if (login.ShowDialog() != DialogResult.OK)
                return;

            Application.Run(new MainForm(store));
            store.Logout();
        }
    }
}
