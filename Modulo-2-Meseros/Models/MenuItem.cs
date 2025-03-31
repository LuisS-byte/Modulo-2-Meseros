using System;
using System.Collections.Generic;

namespace Modulo_2_Meseros.Models;

public partial class MenuItems
{
    public int MenuItemId { get; set; }

    public int? MenuId { get; set; }

    public int? PlatoId { get; set; }

    public int? ComboId { get; set; }

    public int? PromocionId { get; set; }

    public virtual Combo? Combo { get; set; }

    public virtual ICollection<DetallePedido> DetallePedidos { get; set; } = new List<DetallePedido>();

    public virtual Menu? Menu { get; set; }

    public virtual Platos? Platos { get; set; }

    public virtual Promocione? Promocion { get; set; }
}
