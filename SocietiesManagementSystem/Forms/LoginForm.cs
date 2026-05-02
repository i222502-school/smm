using SocietiesManagementSystem.Services;

namespace SocietiesManagementSystem.Forms;

public class LoginForm : Form
{
    readonly TextBox _txtUser = new() { PlaceholderText = "Username", Left = 24, Top = 24, Width = 280 };
    readonly TextBox _txtPass = new() { PlaceholderText = "Password", Left = 24, Top = 64, Width = 280, UseSystemPasswordChar = true };
    readonly Button _btnLogin = new() { Text = "Log in", Left = 24, Top = 110, Width = 120 };
    readonly Button _btnRegister = new() { Text = "Register (student)", Left = 160, Top = 110, Width = 144 };
    readonly Label _lblErr = new() { Left = 24, Top = 150, Width = 320, Height = 48, ForeColor = Color.DarkRed };

    public LoginForm()
    {
        Text = "Societies Management — Login";
        ClientSize = new Size(360, 220);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        Controls.AddRange(new Control[] { _txtUser, _txtPass, _btnLogin, _btnRegister, _lblErr });

        _btnLogin.Click += (_, _) => TryLogin();
        _btnRegister.Click += (_, _) =>
        {
            Hide();
            using var reg = new RegisterForm();
            reg.ShowDialog();
            Show();
        };
        AcceptButton = _btnLogin;
    }

    void TryLogin()
    {
        _lblErr.Text = "";
        var auth = new AuthService();
        var user = auth.Login(_txtUser.Text.Trim(), _txtPass.Text);
        if (user == null)
        {
            _lblErr.Text = "Invalid credentials or inactive account.";
            return;
        }

        AppSession.CurrentUser = user;
        Hide();

        Form next = user.UserType == "Admin"
            ? new AdminDashboardForm()
            : new StudentPortalForm();

        next.FormClosed += (_, _) =>
        {
            AppSession.SignOut();
            _txtPass.Clear();
            Show();
        };
        next.Show();
    }
}
