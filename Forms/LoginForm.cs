using PersonalFinance.Controllers;
using PersonalFinance.Services;

namespace PersonalFinance.Forms
{
    /// <summary>Форма авторизации пользователя при запуске приложения.</summary>
    public class LoginForm : Form
    {
        private readonly AuthController _authController;
        private TextBox _loginBox;
        private TextBox _passwordBox;
        private Label _hintLabel;

        public LoginForm()
        {
            _authController = new AuthController(new DataService());
            BuildUi();
        }

        private void BuildUi()
        {
            Text = "Вход в систему — Учёт личных финансов";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(380, 230);
            Font = new Font("Segoe UI", 9f);

            var title = new Label
            {
                Text = "Учёт личных финансов",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 50
            };

            var loginLabel = new Label { Text = "Логин:", Location = new Point(30, 65), AutoSize = true };
            _loginBox = new TextBox { Location = new Point(150, 62), Width = 190, Text = "admin" };

            var passwordLabel = new Label { Text = "Пароль:", Location = new Point(30, 100), AutoSize = true };
            _passwordBox = new TextBox { Location = new Point(150, 97), Width = 190, UseSystemPasswordChar = true, Text = "admin" };

            var loginButton = new Button { Text = "Войти", Location = new Point(150, 140), Width = 90, DialogResult = DialogResult.None };
            loginButton.Click += LoginButton_Click;

            var cancelButton = new Button { Text = "Отмена", Location = new Point(250, 140), Width = 90 };
            cancelButton.Click += (s, e) => Application.Exit();

            _hintLabel = new Label
            {
                Text = "Демо-доступ: admin / admin  •  user / 123456",
                ForeColor = Color.Gray,
                Location = new Point(30, 185),
                AutoSize = true
            };

            AcceptButton = loginButton;
            Controls.AddRange(new Control[]
            {
                title, loginLabel, _loginBox, passwordLabel, _passwordBox,
                loginButton, cancelButton, _hintLabel
            });
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            var user = _authController.Login(_loginBox.Text.Trim(), _passwordBox.Text);
            if (user == null)
            {
                MessageBox.Show("Неверный логин или пароль.", "Ошибка входа",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Hide();
            using (var main = new MainForm(user))
            {
                main.ShowDialog();
            }
            Application.Exit();
        }
    }
}
