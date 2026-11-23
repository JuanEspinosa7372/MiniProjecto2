using Core.Entidades;
using Core.Enums;
using Core.Excepciones;
using Core.Interfaces;
using Core.Servicios;
using Microsoft.Extensions.DependencyInjection;

namespace Desktop;

// Filas que se muestran en las tablas (DataGridView).
public record FilaReserva(int Id, string Huesped, string Habitacion, string Entrada, string Salida, string Estado, decimal Total);
public record FilaHabitacion(int Id, string Numero, string Tipo, int Capacidad, decimal PrecioNoche, string Estado);

/// <summary>Ventana principal del escritorio. La interfaz se construye por código (no requiere el diseñador).</summary>
public class MainForm : Form
{
    private readonly IServiceProvider _proveedor;
    private readonly bool _esAdmin;

    // Pestaña "Reservas"
    private readonly DataGridView _gridReservas = CrearGrid();

    // Pestaña "Nueva reserva"
    private readonly TextBox _txtDocumento = new TextBox { Width = 130 };
    private readonly TextBox _txtNombre = new TextBox { Width = 180 };
    private readonly TextBox _txtCorreo = new TextBox { Width = 180 };
    private readonly TextBox _txtTelefono = new TextBox { Width = 120 };
    private readonly DateTimePicker _dtpEntrada = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 100, Value = DateTime.Today };
    private readonly DateTimePicker _dtpSalida = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 100, Value = DateTime.Today.AddDays(1) };
    private readonly NumericUpDown _nudPersonas = new NumericUpDown { Minimum = 1, Maximum = 10, Value = 1, Width = 60 };
    private readonly Button _btnBuscar = new Button { Text = "Buscar disponibles", AutoSize = true };
    private readonly Button _btnReservar = new Button { Text = "Reservar", AutoSize = true };
    private readonly Label _lblTotal = new Label { AutoSize = true, Text = "Total estimado: -", Padding = new Padding(10, 8, 0, 0) };
    private readonly DataGridView _gridDisponibles = CrearGrid();
    private IReadOnlyList<Habitacion> _disponibles = Array.Empty<Habitacion>();

    // Pestaña "Habitaciones"
    private readonly DataGridView _gridHabitaciones = CrearGrid();

    // Barra de estado (se actualiza con los eventos del servicio)
    private readonly ToolStripStatusLabel _lblEstado = new ToolStripStatusLabel("Listo");

    public MainForm(IServiceProvider proveedor, bool esAdmin, string usuario)
    {
        _proveedor = proveedor;
        _esAdmin = esAdmin;

        Text = $"Hotel - {(esAdmin ? "Administrador" : "Recepcionista")} ({usuario})";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1000, 640);

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(ConstruirTabReservas());
        tabs.TabPages.Add(ConstruirTabNuevaReserva());
        tabs.TabPages.Add(ConstruirTabHabitaciones());

        var estado = new StatusStrip();
        estado.Items.Add(_lblEstado);

        Controls.Add(tabs);
        Controls.Add(estado);
        Controls.Add(Tema.Encabezado($"Hotel · {(esAdmin ? "Administrador" : "Recepción")}"));
        Tema.Aplicar(this);

        Load += async (_, _) =>
        {
            await CargarReservasAsync();
            await CargarHabitacionesAsync();
        };
    }

    // ---------------------------------------------------------------- construcción de la interfaz

    private TabPage ConstruirTabReservas()
    {
        var tab = new TabPage("Reservas");

        var barra = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(6) };
        var btnRefrescar = new Button { Text = "Refrescar", AutoSize = true };
        var btnCheckIn = new Button { Text = "Check-in", AutoSize = true };
        var btnCheckOut = new Button { Text = "Check-out", AutoSize = true };
        var btnCancelar = new Button { Text = "Cancelar reserva", AutoSize = true };
        barra.Controls.AddRange(new Control[] { btnRefrescar, btnCheckIn, btnCheckOut, btnCancelar });

        btnRefrescar.Click += async (_, _) => await CargarReservasAsync();
        btnCheckIn.Click += async (_, _) => await EjecutarAccionAsync((s, id) => s.CheckInAsync(id), "Check-in realizado.");
        btnCheckOut.Click += async (_, _) => await EjecutarAccionAsync((s, id) => s.CheckOutAsync(id), "Check-out realizado.");
        btnCancelar.Click += async (_, _) => await EjecutarAccionAsync((s, id) => s.CancelarAsync(id), "Reserva cancelada.");

        tab.Controls.Add(_gridReservas); // el control que ocupa el centro se agrega primero
        tab.Controls.Add(barra);
        return tab;
    }

    private TabPage ConstruirTabNuevaReserva()
    {
        var tab = new TabPage("Nueva reserva");

        var campos = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(6) };
        campos.Controls.AddRange(new Control[]
        {
            Campo("Documento", _txtDocumento),
            Campo("Nombre completo", _txtNombre),
            Campo("Correo", _txtCorreo),
            Campo("Teléfono", _txtTelefono),
            Campo("Entrada", _dtpEntrada),
            Campo("Salida", _dtpSalida),
            Campo("Personas", _nudPersonas),
            Campo(" ", _btnBuscar)
        });

        var pie = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 48, Padding = new Padding(6) };
        pie.Controls.Add(_btnReservar);
        pie.Controls.Add(_lblTotal);

        _btnBuscar.Click += async (_, _) => await BuscarDisponiblesAsync();
        _btnReservar.Click += async (_, _) => await ReservarAsync();
        _gridDisponibles.SelectionChanged += (_, _) => ActualizarTotal();
        _dtpEntrada.ValueChanged += (_, _) => ActualizarTotal();
        _dtpSalida.ValueChanged += (_, _) => ActualizarTotal();

        tab.Controls.Add(_gridDisponibles);
        tab.Controls.Add(campos);
        tab.Controls.Add(pie);
        return tab;
    }

    private TabPage ConstruirTabHabitaciones()
    {
        var tab = new TabPage("Habitaciones");

        var barra = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(6) };
        var btnEstado = new Button
        {
            Text = "Alternar mantenimiento (solo Admin)",
            AutoSize = true,
            Enabled = _esAdmin // control por rol: el Recepcionista no puede usarlo
        };
        barra.Controls.Add(btnEstado);
        btnEstado.Click += async (_, _) => await AlternarMantenimientoAsync();

        tab.Controls.Add(_gridHabitaciones);
        tab.Controls.Add(barra);
        return tab;
    }

    private static DataGridView CrearGrid() => new DataGridView
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        MultiSelect = false,
        RowHeadersVisible = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };

    private static Control Campo(string etiqueta, Control control)
    {
        var panel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(4)
        };
        panel.Controls.Add(new Label { Text = etiqueta, AutoSize = true });
        panel.Controls.Add(control);
        return panel;
    }

    private static void FormatearGrid(DataGridView grid)
    {
        if (grid.Columns.Contains("Id")) grid.Columns["Id"]!.Visible = false;
        foreach (var nombre in new[] { "Total", "PrecioNoche" })
        {
            if (grid.Columns.Contains(nombre)) grid.Columns[nombre]!.DefaultCellStyle.Format = "C0";
        }
    }

    private static int? IdSeleccionado(DataGridView grid) =>
        grid.CurrentRow?.Cells["Id"].Value as int?;

    private static void Aviso(string mensaje, MessageBoxIcon icono = MessageBoxIcon.Information) =>
        MessageBox.Show(mensaje, "Hotel", MessageBoxButtons.OK, icono);

    // ---------------------------------------------------------------- carga de datos

    private async Task CargarReservasAsync()
    {
        using var alcance = _proveedor.CreateScope();
        var repositorio = alcance.ServiceProvider.GetRequiredService<IRepositorio<Reserva>>();

        var reservas = await repositorio.ObtenerTodosAsync(r => r.Huesped!, r => r.Habitacion!);

        _gridReservas.DataSource = reservas
            .OrderByDescending(r => r.FechaEntrada)
            .Select(r => new FilaReserva(
                r.Id,
                r.Huesped?.Nombre ?? "-",
                r.Habitacion?.Numero ?? "-",
                r.FechaEntrada.ToString("dd/MM/yyyy"),
                r.FechaSalida.ToString("dd/MM/yyyy"),
                r.Estado.ToString(),
                r.Total))
            .ToList();

        FormatearGrid(_gridReservas);
    }

    private async Task CargarHabitacionesAsync()
        {
            using var alcance = _proveedor.CreateScope();
            var repoHab = alcance.ServiceProvider.GetRequiredService<IRepositorio<Habitacion>>();
            var repoRes = alcance.ServiceProvider.GetRequiredService<IRepositorio<Reserva>>();
        
            var habitaciones = await repoHab.ObtenerTodosAsync();
            var ocupadas = (await repoRes.BuscarAsync(r => r.Estado == EstadoReserva.CheckIn))
                .Select(r => r.HabitacionId)
                .ToHashSet();
        
            _gridHabitaciones.DataSource = habitaciones
                .OrderBy(h => h.Numero)
                .Select(h => new FilaHabitacion(
                    h.Id, h.Numero, h.Tipo.ToString(), h.Capacidad, h.PrecioNoche,
                    h.Estado == EstadoHabitacion.Mantenimiento ? "Mantenimiento"
                    : ocupadas.Contains(h.Id) ? "Ocupada"
                    : "Disponible"))
                .ToList();
        
            FormatearGrid(_gridHabitaciones);
        }

    // ---------------------------------------------------------------- acciones

    private async Task EjecutarAccionAsync(Func<ReservaServicio, int, Task> accion, string mensajeOk)
    {
        var id = IdSeleccionado(_gridReservas);
        if (id is null)
        {
            Aviso("Selecciona una reserva de la lista.", MessageBoxIcon.Warning);
            return;
        }

        try
        {
            using var alcance = _proveedor.CreateScope();
            var servicio = alcance.ServiceProvider.GetRequiredService<ReservaServicio>();

            await accion(servicio, id.Value);
            Aviso(mensajeOk);
            await CargarReservasAsync();
        }
        catch (ReglaNegocioException ex)
        {
            Aviso(ex.Message, MessageBoxIcon.Warning);
        }
    }

    private async Task BuscarDisponiblesAsync()
    {
        try
        {
            using var alcance = _proveedor.CreateScope();
            var servicio = alcance.ServiceProvider.GetRequiredService<ReservaServicio>();

            _disponibles = await servicio.BuscarDisponiblesAsync(
                _dtpEntrada.Value, _dtpSalida.Value, (int)_nudPersonas.Value);

            _gridDisponibles.DataSource = _disponibles
                .Select(h => new FilaHabitacion(h.Id, h.Numero, h.Tipo.ToString(), h.Capacidad, h.PrecioNoche, h.Estado.ToString()))
                .ToList();
            FormatearGrid(_gridDisponibles);
            ActualizarTotal();

            if (_disponibles.Count == 0)
                Aviso("No hay habitaciones disponibles para esas fechas.", MessageBoxIcon.Warning);
        }
        catch (ReglaNegocioException ex)
        {
            Aviso(ex.Message, MessageBoxIcon.Warning);
        }
    }

    private void ActualizarTotal()
    {
        var id = IdSeleccionado(_gridDisponibles);
        var habitacion = _disponibles.FirstOrDefault(h => h.Id == id);

        if (habitacion is null || _dtpSalida.Value.Date <= _dtpEntrada.Value.Date)
        {
            _lblTotal.Text = "Total estimado: -";
            return;
        }

        using var alcance = _proveedor.CreateScope();
        var servicio = alcance.ServiceProvider.GetRequiredService<ReservaServicio>();
        decimal total = servicio.CalcularTotal(habitacion.PrecioNoche, _dtpEntrada.Value, _dtpSalida.Value);
        _lblTotal.Text = $"Total estimado: {total:C0}";
    }

    private async Task ReservarAsync()
    {
        var habitacionId = IdSeleccionado(_gridDisponibles);
        if (habitacionId is null)
        {
            Aviso("Busca disponibilidad y selecciona una habitación.", MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(_txtDocumento.Text) ||
            string.IsNullOrWhiteSpace(_txtNombre.Text) ||
            string.IsNullOrWhiteSpace(_txtCorreo.Text))
        {
            Aviso("Documento, nombre y correo del huésped son obligatorios.", MessageBoxIcon.Warning);
            return;
        }

        try
        {
            using var alcance = _proveedor.CreateScope();
            var huespedes = alcance.ServiceProvider.GetRequiredService<IRepositorio<Huesped>>();
            var servicio = alcance.ServiceProvider.GetRequiredService<ReservaServicio>();

            // Evento del servicio: actualiza la barra de estado.
            servicio.ReservaCreada += (_, e) =>
                _lblEstado.Text = $"Reserva #{e.Reserva.Id} creada a las {DateTime.Now:t}";

            // Busca al huésped por documento; si no existe, lo crea.
            string documento = _txtDocumento.Text.Trim();
            var existentes = await huespedes.BuscarAsync(h => h.Documento == documento);
            var huesped = existentes.FirstOrDefault() ?? await huespedes.AgregarAsync(new Huesped
            {
                Documento = documento,
                Nombre = _txtNombre.Text.Trim(),
                Email = _txtCorreo.Text.Trim(),
                Telefono = string.IsNullOrWhiteSpace(_txtTelefono.Text) ? null : _txtTelefono.Text.Trim()
            });

            var reserva = await servicio.CrearReservaAsync(
                huesped.Id, habitacionId.Value, _dtpEntrada.Value, _dtpSalida.Value, (int)_nudPersonas.Value);

            Aviso($"Reserva creada. Total a pagar: {reserva.Total:C0}");

            _txtDocumento.Clear();
            _txtNombre.Clear();
            _txtCorreo.Clear();
            _txtTelefono.Clear();

            await BuscarDisponiblesAsync();
            await CargarReservasAsync();
        }
        catch (ReglaNegocioException ex)
        {
            Aviso(ex.Message, MessageBoxIcon.Warning);
        }
    }

    private async Task AlternarMantenimientoAsync()
    {
        var id = IdSeleccionado(_gridHabitaciones);
        if (id is null)
        {
            Aviso("Selecciona una habitación.", MessageBoxIcon.Warning);
            return;
        }

        using var alcance = _proveedor.CreateScope();
        var repositorio = alcance.ServiceProvider.GetRequiredService<IRepositorio<Habitacion>>();

        var habitacion = await repositorio.ObtenerPorIdAsync(id.Value);
        if (habitacion is null) return;

        habitacion.Estado = habitacion.Estado == EstadoHabitacion.Disponible
            ? EstadoHabitacion.Mantenimiento
            : EstadoHabitacion.Disponible;

        await repositorio.ActualizarAsync(habitacion);
        await CargarHabitacionesAsync();
    }
}
