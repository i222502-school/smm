using SocietiesManagementSystem.Services;

namespace SocietiesManagementSystem.Forms;

public class RegisterForm : Form
{
    readonly TextBox _txtUser = new() { PlaceholderText = "Username", Left = 20, Top = 16, Width = 280 };
    readonly TextBox _txtEmail = new() { PlaceholderText = "Email", Left = 20, Top = 52, Width = 280 };
    readonly TextBox _txtName = new() { PlaceholderText = "Full name", Left = 20, Top = 88, Width = 280 };
    readonly TextBox _txtPass = new() { PlaceholderText = "Password (min 6)", Left = 20, Top = 124, Width = 280, UseSystemPasswordChar = true };
    readonly Button _btnOk = new() { Text = "Create account", Left = 20, Top = 170, Width = 140 };
    readonly Button _btnCancel = new() { Text = "Cancel", Left = 170, Top = 170, Width = 130 };
    readonly Label _lbl = new() { Left = 20, Top = 210, Width = 320, Height = 60 };

    public RegisterForm()
    {
        Text = "Student registration";
        ClientSize = new Size(340, 290);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Controls.AddRange(new Control[] { _txtUser, _txtEmail, _txtName, _txtPass, _btnOk, _btnCancel, _lbl });

        _btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        _btnOk.Click += (_, _) =>
        {
            var auth = new AuthService();
            var (ok, msg) = auth.RegisterStudent(_txtUser.Text, _txtPass.Text, _txtEmail.Text, _txtName.Text);
            _lbl.ForeColor = ok ? Color.DarkGreen : Color.DarkRed;
            _lbl.Text = msg;
            if (ok) DialogResult = DialogResult.OK;
        };
    }
}
