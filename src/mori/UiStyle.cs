using System.Drawing;
using System.Windows.Forms;

namespace mori;

internal static class UiStyle
{
    
    public static readonly Color Navy = Color.Black;
    public static readonly Color Accent = Color.Black;
    public static readonly Color Background = Color.White;
    public static readonly Color Text = Color.Black;
    public static readonly Color Border = Color.Black;
    public static readonly Color SoftGray = Color.FromArgb(242, 242, 242);

    public static void PrepareForm(Form form, string title, int width, int height)
    {
        form.Text = title;
        form.StartPosition = FormStartPosition.CenterScreen;
        form.Size = new Size(width, height);
        form.MinimumSize = new Size(width, height);
        form.BackColor = Background;
        form.Font = new Font("Segoe UI", 10F);
        form.ForeColor = Text;
    }

    public static Button MakeButton(string text, int width = 140)
    {
        var button = new Button
        {
            Text = text,
            Width = width,
            Height = 40,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.Black,
            Font = new Font("Segoe UI", 10F, FontStyle.Regular),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };

        button.FlatAppearance.BorderColor = Color.Black;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = Color.Black;
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(55, 55, 55);

        button.MouseEnter += (_, _) => button.ForeColor = Color.White;
        button.MouseLeave += (_, _) => button.ForeColor = Color.Black;
        button.MouseDown += (_, _) => button.ForeColor = Color.White;
        button.MouseUp += (_, _) => button.ForeColor = button.ClientRectangle.Contains(button.PointToClient(Cursor.Position))
            ? Color.White
            : Color.Black;

        return button;
    }
}
