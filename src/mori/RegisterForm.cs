using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace mori;

public class RegisterForm : Form
{
    private readonly TextBox _fullNameTextBox = new();
    private readonly TextBox _loginTextBox = new();
    private readonly TextBox _phoneTextBox = new();

    public string CreatedLogin { get; private set; } = string.Empty;

    public RegisterForm()
    {
        UiStyle.PrepareForm(this, "MORI — регистрация", 520, 440);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BuildInterface();
    }

    private void BuildInterface()
    {
        var title = new Label
        {
            Text = "Регистрация пользователя",
            Font = new Font("Segoe UI", 17F, FontStyle.Bold),
            ForeColor = Color.Black,
            AutoSize = true,
            Location = new Point(30, 22)
        };

        AddField("Фамилия, имя и отчество", _fullNameTextBox, 85, "Например: Иванов Иван Иванович");
        AddField("Логин для входа", _loginTextBox, 165, "Латинские буквы, цифры, точка или _");
        AddField("Телефон (необязательно)", _phoneTextBox, 245, "+7 ...");

        var info = new Label
        {
            Text = "Для входа используется логин. Пароль в этом задании не требуется.",
            AutoSize = false,
            Location = new Point(30, 300),
            Size = new Size(455, 35),
            ForeColor = Color.FromArgb(85, 85, 85)
        };

        var saveButton = UiStyle.MakeButton("Зарегистрироваться", 210);
        saveButton.Location = new Point(275, 345);
        saveButton.Click += SaveButton_Click;

        Controls.Add(title);
        Controls.Add(info);
        Controls.Add(saveButton);
        AcceptButton = saveButton;
    }

    private void AddField(string labelText, TextBox textBox, int top, string placeholder)
    {
        var label = new Label { Text = labelText, AutoSize = true, Location = new Point(30, top) };
        textBox.Location = new Point(30, top + 25);
        textBox.Size = new Size(455, 29);
        textBox.PlaceholderText = placeholder;
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.BackColor = Color.White;
        textBox.ForeColor = Color.Black;
        Controls.Add(label);
        Controls.Add(textBox);
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        string fullName = _fullNameTextBox.Text.Trim();
        string login = _loginTextBox.Text.Trim();
        string phone = _phoneTextBox.Text.Trim();

        if (fullName.Length < 5)
        {
            MessageBox.Show("Укажи полное ФИО.", "Проверь ФИО", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _fullNameTextBox.Focus();
            return;
        }

        if (login.Length < 3 || login.Length > 50 ||
            login.Any(ch => !char.IsLetterOrDigit(ch) && ch != '.' && ch != '_' && ch != '-'))
        {
            MessageBox.Show("Логин должен содержать от 3 до 50 символов: буквы, цифры, точку, дефис или подчёркивание.",
                "Некорректный логин", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _loginTextBox.Focus();
            return;
        }

        try
        {
            using var connection = Db.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                using (var checkCommand = new SqlCommand(
                    "SELECT COUNT(1) FROM dbo.Users WHERE Login = @Login;", connection, transaction))
                {
                    checkCommand.Parameters.AddWithValue("@Login", login);
                    int count = Convert.ToInt32(checkCommand.ExecuteScalar());
                    if (count > 0)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Этот логин уже занят. Придумай другой.",
                            "Логин занят", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        _loginTextBox.Focus();
                        return;
                    }
                }

                int customerId;
                using (var customerCommand = new SqlCommand(
                    @"INSERT INTO dbo.Customers(FullName, Phone)
                      VALUES(@FullName, @Phone);
                      SELECT CAST(SCOPE_IDENTITY() AS INT);", connection, transaction))
                {
                    customerCommand.Parameters.AddWithValue("@FullName", fullName);
                    customerCommand.Parameters.AddWithValue("@Phone", string.IsNullOrWhiteSpace(phone) ? DBNull.Value : phone);
                    customerId = Convert.ToInt32(customerCommand.ExecuteScalar());
                }

                using (var userCommand = new SqlCommand(
                    "INSERT INTO dbo.Users(Login, CustomerID) VALUES(@Login, @CustomerID);",
                    connection, transaction))
                {
                    userCommand.Parameters.AddWithValue("@Login", login);
                    userCommand.Parameters.AddWithValue("@CustomerID", customerId);
                    userCommand.ExecuteNonQuery();
                }

                transaction.Commit();
                CreatedLogin = login;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            MessageBox.Show("Такой логин уже существует. Выбери другое имя для входа.",
                "Логин занят", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Не удалось сохранить данные регистрации.\n\n" + ex.Message,
                "Ошибка регистрации", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
