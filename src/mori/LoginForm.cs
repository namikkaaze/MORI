using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace mori;

public class LoginForm : Form
{
    private readonly TextBox _loginTextBox = new();

    public LoginForm()
    {
        UiStyle.PrepareForm(this, "MORI — вход в систему", 480, 360);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BuildInterface();
    }

    private void BuildInterface()
    {
        var logo = new Label
        {
            Text = "MORI",
            Font = new Font("Segoe UI", 28F, FontStyle.Bold),
            ForeColor = Color.Black,
            AutoSize = true,
            Location = new Point(35, 25)
        };

        var subtitle = new Label
        {
            Text = "Магазин обуви",
            AutoSize = true,
            ForeColor = Color.FromArgb(85, 85, 85),
            Location = new Point(40, 78)
        };

        var divider = new Panel
        {
            BackColor = Color.Black,
            Location = new Point(40, 112),
            Size = new Size(380, 1)
        };

        var loginLabel = new Label
        {
            Text = "Логин пользователя",
            AutoSize = true,
            Location = new Point(40, 135)
        };

        _loginTextBox.Location = new Point(40, 162);
        _loginTextBox.Size = new Size(380, 30);
        _loginTextBox.PlaceholderText = "Введите свой логин";
        _loginTextBox.BorderStyle = BorderStyle.FixedSingle;
        _loginTextBox.BackColor = Color.White;
        _loginTextBox.ForeColor = Color.Black;

        
        var loginButton = UiStyle.MakeButton("Войти", 380);
        loginButton.Location = new Point(40, 215);
        loginButton.Click += LoginButton_Click;

        var registerButton = UiStyle.MakeButton("Регистрация нового пользователя", 380);
        registerButton.Location = new Point(40, 266);
        registerButton.Click += RegisterButton_Click;

        Controls.Add(logo);
        Controls.Add(subtitle);
        Controls.Add(divider);
        Controls.Add(loginLabel);
        Controls.Add(_loginTextBox);
        Controls.Add(loginButton);
        Controls.Add(registerButton);
        AcceptButton = loginButton;
    }

    private void LoginButton_Click(object? sender, EventArgs e)
    {
        string login = _loginTextBox.Text.Trim();
        if (login.Length == 0)
        {
            MessageBox.Show("Введи логин. Если у тебя ещё нет учётной записи, сначала пройди регистрацию.",
                "Не заполнено поле", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            using var connection = Db.CreateConnection();
            connection.Open();
            using var command = new SqlCommand(
                @"SELECT u.UserID, u.Login, c.FullName
                  FROM dbo.Users u
                  INNER JOIN dbo.Customers c ON c.CustomerID = u.CustomerID
                  WHERE u.Login = @Login;", connection);
            command.Parameters.AddWithValue("@Login", login);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                MessageBox.Show("Пользователь с таким логином не найден. Проверь логин или нажми «Регистрация нового пользователя».",
                    "Пользователь не найден", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var session = new UserSession(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2));
            reader.Close();

            Hide();
            using (var mainForm = new MainForm(session))
            {
                mainForm.ShowDialog(this);
            }
            Show();
            _loginTextBox.Clear();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Не удалось выполнить вход. Проверь, что база moridb_v2 создана и SQL Server LocalDB запущен.\n\n" + ex.Message,
                "Ошибка входа", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RegisterButton_Click(object? sender, EventArgs e)
    {
        using var registerForm = new RegisterForm();
        if (registerForm.ShowDialog(this) == DialogResult.OK)
        {
            _loginTextBox.Text = registerForm.CreatedLogin;
            MessageBox.Show("Регистрация завершена. Теперь нажми «Войти».",
                "Пользователь создан", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}

public sealed record UserSession(int UserID, string Login, string FullName);
