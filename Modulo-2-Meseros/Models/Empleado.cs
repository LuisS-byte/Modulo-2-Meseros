using System;
using System.Collections.Generic;

namespace Modulo_2_Meseros.Models;

public partial class Empleado
{
    public int EmpleadoId { get; set; }

    public string? UrlFotoEmpleado { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Telefono { get; set; }

    public string Contrasena { get; set; } = null!;

    public int? RolId { get; set; }

    public DateOnly? FechaIngreso { get; set; }

    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

    public virtual Role? Rol { get; set; }
}
