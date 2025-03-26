using System;
using System.Collections.Generic;

namespace Modulo_2_Meseros.Models;

public partial class MetodosPago
{
    public int MetodoId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }
}
