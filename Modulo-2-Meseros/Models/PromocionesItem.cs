using System;
using System.Collections.Generic;

namespace Modulo_2_Meseros.Models;

public partial class PromocionesItem
{
    public int PromocionId { get; set; }

    public int PlatoId { get; set; }

    public int ComboId { get; set; }

    public virtual Combo Combo { get; set; } = null!;

    public virtual Platos Platos { get; set; } = null!;

    public virtual Promocione Promocion { get; set; } = null!;
}
