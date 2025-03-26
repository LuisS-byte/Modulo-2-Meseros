using System;
using System.Collections.Generic;

namespace Modulo_2_Meseros.Models;

public partial class Detallefactura
{
    public int Id { get; set; }

    public int? PlatoId { get; set; }

    public int? ComboId { get; set; }

    public int? FacturaId { get; set; }

    public decimal? Subtotal { get; set; }

    public virtual Factura? Factura { get; set; }
}
