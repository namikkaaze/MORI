using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace mori;

public class MainForm : Form
{
    private readonly UserSession _session;
    private readonly DataGridView _grid = new();
    private Image? _placeholderImage;
    private readonly List<Image> _loadedImages = new();

    public MainForm(UserSession session)
    {
        _session = session;
        UiStyle.PrepareForm(this, "MORI — каталог обуви", 1280, 760);
        MinimumSize = new Size(1050, 650);
        BuildInterface();
        Load += (_, _) => LoadCatalog();
        FormClosed += (_, _) => DisposeImages();
    }

    private void BuildInterface()
    {
        var header = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Black,
            Padding = new Padding(22, 12, 22, 12)
        };

        var title = new Label
        {
            Text = "MORI  |  Каталог обуви",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(22, 20)
        };

        var userLabel = new Label
        {
            Text = "Пользователь: " + _session.FullName,
            ForeColor = Color.White,
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(760, 28)
        };

        var logoutButton = UiStyle.MakeButton("Выйти", 100);
        logoutButton.Height = 36;
        logoutButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        logoutButton.Location = new Point(1090, 20);
        logoutButton.Click += (_, _) => Close();

        header.Resize += (_, _) =>
        {
            logoutButton.Left = header.ClientSize.Width - logoutButton.Width - 22;
            userLabel.Left = logoutButton.Left - userLabel.Width - 25;
        };
        header.Controls.Add(title);
        header.Controls.Add(userLabel);
        header.Controls.Add(logoutButton);

        var intro = new Panel { Dock = DockStyle.Fill, Padding = new Padding(22, 10, 22, 5) };
        var introTitle = new Label
        {
            Text = "Список товаров",
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(22, 8)
        };
        var introDescription = new Label
        {
            Text = "Цена рассчитывается с учётом заказов за предыдущий календарный месяц.",
            ForeColor = Color.FromArgb(80, 80, 80),
            AutoSize = true,
            Location = new Point(22, 37)
        };
        var refreshButton = UiStyle.MakeButton("Обновить", 115);
        refreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        refreshButton.Location = new Point(1090, 16);
        refreshButton.Click += (_, _) => LoadCatalog();
        intro.Resize += (_, _) => refreshButton.Left = intro.ClientSize.Width - refreshButton.Width - 22;
        intro.Controls.Add(introTitle);
        intro.Controls.Add(introDescription);
        intro.Controls.Add(refreshButton);

        ConfigureGrid();

        var footer = new Label
        {
            Text = "Наличие: «Много» — больше 5 пар, «Мало» — 5 пар или меньше. Товары с остатком 3 пары или меньше выделены светло-серым цветом.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(18, 0, 0, 0),
            ForeColor = Color.FromArgb(80, 80, 80),
            BackColor = Color.White
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = UiStyle.Background
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 86F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 68F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(intro, 0, 1);
        layout.Controls.Add(_grid, 0, 2);
        layout.Controls.Add(footer, 0, 3);
        Controls.Add(layout);
    }

    private void ConfigureGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.BackgroundColor = Color.White;
        _grid.BorderStyle = BorderStyle.None;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AllowUserToResizeRows = false;
        _grid.ReadOnly = true;
        _grid.MultiSelect = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.RowHeadersVisible = false;
        _grid.AutoGenerateColumns = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.ColumnHeadersHeight = 42;
        _grid.RowTemplate.Height = 125;
        _grid.EnableHeadersVisualStyles = false;
        _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.Black;
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _grid.GridColor = Color.FromArgb(205, 205, 205);
        _grid.DefaultCellStyle.ForeColor = Color.Black;
        _grid.DefaultCellStyle.BackColor = Color.White;
        _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(55, 55, 55);
        _grid.DefaultCellStyle.SelectionForeColor = Color.White;
        _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);

        var imageColumn = new DataGridViewImageColumn
        {
            Name = "ProductImage",
            HeaderText = "Фото",
            ImageLayout = DataGridViewImageCellLayout.Zoom,
            FillWeight = 15
        };
        _grid.Columns.Add(imageColumn);
        AddTextColumn("ProductName", "Наименование", 24);
        AddTextColumn("CategoryName", "Категория", 16);
        AddTextColumn("Manufacturer", "Производство", 17);
        AddTextColumn("Composition", "Состав", 20);
        AddTextColumn("DiscountPrice", "Цена со скидкой", 13);
        AddTextColumn("QuantityLabel", "Количество пар", 12);
    }

    private void AddTextColumn(string name, string header, float weight)
    {
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = name,
            HeaderText = header,
            DataPropertyName = name,
            FillWeight = weight,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
    }

    private void LoadCatalog()
    {
        try
        {
            _grid.Rows.Clear();
            DisposeImages();

            string placeholderPath = Path.Combine(AppContext.BaseDirectory, "Resources", "picture.png");
            if (File.Exists(placeholderPath))
            {
                using var source = Image.FromFile(placeholderPath);
                _placeholderImage = new Bitmap(source);
            }

            using var connection = Db.CreateConnection();
            connection.Open();
            using var command = new SqlCommand(
                @"SELECT ProductID, Article, ProductName, CategoryName, Manufacturer,
                         Composition, DiscountPrice, QuantityLabel, LowStock, ImagePath
                  FROM dbo.vw_ProductCatalog
                  ORDER BY ProductName;", connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                string? imagePath = reader.IsDBNull(9) ? null : reader.GetString(9);
                Image image = LoadProductImage(imagePath);
                decimal price = reader.GetDecimal(6);

                int rowIndex = _grid.Rows.Add(
                    image,
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetString(4),
                    reader.GetString(5),
                    price.ToString("N2") + " ₽",
                    reader.GetString(7));

                if (reader.GetBoolean(8))
                {
                    _grid.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(232, 232, 232);
                }
            }

            if (_grid.Rows.Count == 0)
            {
                MessageBox.Show("В каталоге пока нет товаров.", "Каталог пуст", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Не удалось загрузить каталог. Проверь создание базы и наличие представления vw_ProductCatalog.\n\n" + ex.Message,
                "Ошибка загрузки каталога", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private Image LoadProductImage(string? imagePath)
    {
        if (!string.IsNullOrWhiteSpace(imagePath))
        {
            
            
            string fullPath = Path.IsPathRooted(imagePath)
                ? imagePath
                : Path.Combine(AppContext.BaseDirectory, imagePath.Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(fullPath))
            {
                try
                {
                    using var source = Image.FromFile(fullPath);
                    var copy = new Bitmap(source);
                    _loadedImages.Add(copy);
                    return copy;
                }
                catch
                {
                    
                }
            }
        }

        if (_placeholderImage != null) return _placeholderImage;
        return SystemIcons.Information.ToBitmap();
    }

    private void DisposeImages()
    {
        foreach (Image image in _loadedImages) image.Dispose();
        _loadedImages.Clear();
        _placeholderImage?.Dispose();
        _placeholderImage = null;
    }
}
