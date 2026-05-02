using SocietiesManagementSystem.Forms;

namespace SocietiesManagementSystem;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new LoginForm());
    }
}
