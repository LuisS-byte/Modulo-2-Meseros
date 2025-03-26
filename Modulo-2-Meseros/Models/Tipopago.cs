using System;
using System.Collections.Generic;

namespace Modulo_2_Meseros.Models;

public partial class Tipopago
{
    public int Id { get; set; }

    public string Tipo { get; set; } = null!;

    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();
}
