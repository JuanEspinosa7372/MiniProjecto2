namespace Desktop;

/// <summary>Colores y estilos de la aplicación de escritorio (misma identidad visual que la web).</summary>
internal static class Tema
{
    public static readonly Color Pared = ColorTranslator.FromHtml("#F2F5F1");
    public static readonly Color Puerta = ColorTranslator.FromHtml("#17466B");
    public static readonly Color PuertaOsc = ColorTranslator.FromHtml("#0F3351");
    public static readonly Color Laton = ColorTranslator.FromHtml("#C99A2E");
    public static readonly Color Guadua = ColorTranslator.FromHtml("#3F7D4B");
    public static readonly Color Cayena = ColorTranslator.FromHtml("#B23A2E");
    public static readonly Color Tinta = ColorTranslator.FromHtml("#1C2A33");
    public static readonly Color Linea = ColorTranslator.FromHtml("#D5DDD6");

    public static readonly Font Cuerpo = new("Segoe UI", 10f, FontStyle.Regular);
    public static readonly Font Negrita = new("Segoe UI", 10f, FontStyle.Bold);
    public static readonly Font Titulo = new("Segoe UI", 16f, FontStyle.Bold);

    /// <summary>Franja superior azul con filo de latón.</summary>
    public static Panel Encabezado(string texto)
    {
        var panel = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Puerta };
        panel.Controls.Add(new Label
        {
            Text = texto,
            ForeColor = Color.White,
            Font = Titulo,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(16, 0, 0, 0)
        });
        panel.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 4, BackColor = Laton });
        return panel;
    }

    /// <summary>Aplica fondo, tipografía y estilo a todos los controles de la ventana.</summary>
    public static void Aplicar(Control raiz)
    {
        raiz.BackColor = Pared;
        raiz.ForeColor = Tinta;
        raiz.Font = Cuerpo;
        foreach (Control hijo in raiz.Controls) Estilizar(hijo);
    }

    private static void Estilizar(Control control)
    {
        switch (control)
        {
            case Button boton:
                boton.FlatStyle = FlatStyle.Flat;
                boton.FlatAppearance.BorderSize = 0;
                boton.FlatAppearance.MouseOverBackColor = PuertaOsc;
                boton.UseVisualStyleBackColor = false;
                boton.Cursor = Cursors.Hand;
                boton.Padding = new Padding(8, 2, 8, 2);
                ColorearBoton(boton);
                boton.EnabledChanged += (_, _) => ColorearBoton(boton);
                break;
            case TabPage pagina:
                pagina.UseVisualStyleBackColor = false;
                pagina.BackColor = Pared;
                break;
            case DataGridView grid:
                Grid(grid);
                break;
        }

        foreach (Control hijo in control.Controls) Estilizar(hijo);
    }

    private static void ColorearBoton(Button boton)
    {
        boton.BackColor = boton.Enabled ? Puerta : Linea;
        boton.ForeColor = boton.Enabled ? Color.White : Tinta;
    }

    public static void Grid(DataGridView grid)
    {
        grid.BackgroundColor = Pared;
        grid.BorderStyle = BorderStyle.None;
        grid.GridColor = Linea;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.EnableHeadersVisualStyles = false;

        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersHeight = 36;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Puerta;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Puerta;
        grid.ColumnHeadersDefaultCellStyle.Font = Negrita;

        grid.RowTemplate.Height = 30;
        grid.DefaultCellStyle.BackColor = Color.White;
        grid.DefaultCellStyle.SelectionBackColor = Laton;
        grid.DefaultCellStyle.SelectionForeColor = Tinta;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247, 249, 246);

        grid.CellFormatting += ColorearEstado;
    }

    // Las columnas "Estado" se pintan según su valor (reservas y habitaciones).
    private static void ColorearEstado(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (sender is not DataGridView grid || e.RowIndex < 0 || e.Value is not string texto) return;
        if (grid.Columns[e.ColumnIndex].Name != "Estado") return;

        e.CellStyle.Font = Negrita;
        e.CellStyle.ForeColor = texto switch
        {
            "Confirmada" => Puerta,
            "CheckIn" or "Disponible" => Guadua,
            "Cancelada" or "Ocupada" => Cayena,
            "Mantenimiento" => Color.FromArgb(138, 106, 16),
            _ => Tinta
        };
    }
}
