using Core.Constantes;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Desktop;

/// <summary>Inicio de sesión con ASP.NET Core Identity (solo Admin y Recepcionista).</summary>
public class LoginForm : Form
{
    private readonly IServiceProvider _proveedor;
    private readonly TextBox _txtCorreo = new TextBox { Width = 240 };
    private readonly TextBox _txtClave = new TextBox { Width = 240, UseSystemPasswordChar = true };
    private readonly Button _btnIngresar = new Button { Text = "Ingresar", AutoSize = true };
    private readonly Label _lblError = new Label { AutoSize = true, ForeColor = Color.Firebrick };

    public bool EsAdmin { get; private set; }
    public string Usuario { get; private set; } = string.Empty;

    public LoginForm(IServiceProvider proveedor)
    {
        _proveedor = proveedor;

        Text = "Hotel - Iniciar sesión";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        ClientSize = new Size(300, 290);

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(20)
        };
        panel.Controls.Add(new Label { Text = "Correo", AutoSize = true });
        panel.Controls.Add(_txtCorreo);
        panel.Controls.Add(new Label { Text = "Contraseña", AutoSize = true });
        panel.Controls.Add(_txtClave);
        panel.Controls.Add(_btnIngresar);
        panel.Controls.Add(_lblError);
        Controls.Add(panel);
        Controls.Add(Tema.Encabezado("Hotel"));
        Tema.Aplicar(this);

        AcceptButton = _btnIngresar;
        _btnIngresar.Click += async (_, _) => await IngresarAsync();
    }

    private async Task IngresarAsync()
    {
        _lblError.Text = string.Empty;

        try
        {
            using var alcance = _proveedor.CreateScope();
            var userManager = alcance.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var usuario = await userManager.FindByEmailAsync(_txtCorreo.Text.Trim());
            if (usuario is null || !await userManager.CheckPasswordAsync(usuario, _txtClave.Text))
            {
                _lblError.Text = "Correo o contraseña incorrectos.";
                return;
            }

            bool esAdmin = await userManager.IsInRoleAsync(usuario, Roles.Admin);
            bool esRecepcionista = await userManager.IsInRoleAsync(usuario, Roles.Recepcionista);

            if (!esAdmin && !esRecepcionista)
            {
                _lblError.Text = "Tu rol no tiene acceso a la aplicación de escritorio.";
                return;
            }

            EsAdmin = esAdmin;
            Usuario = usuario.Email ?? usuario.UserName ?? string.Empty;
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            _lblError.Text = "Error al iniciar sesión: " + ex.Message;
        }
    }
}
