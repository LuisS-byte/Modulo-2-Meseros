using System;
using System.Collections.Generic;

namespace Modulo_2_Meseros.Models;

public partial class Factura
{
    public int Id { get; set; }

    public int PedidoId { get; set; }

    public DateOnly? Fecha { get; set; }

    public decimal? Total { get; set; }

    public int? TipopagoId { get; set; }

    public int? EmpleadoId { get; set; }

    public virtual ICollection<Detallefactura> Detallefacturas { get; set; } = new List<Detallefactura>();

    public virtual Empleado? Empleado { get; set; }

    public virtual Pedido Pedido { get; set; } = null!;

    public virtual Tipopago? Tipopago { get; set; }
}
