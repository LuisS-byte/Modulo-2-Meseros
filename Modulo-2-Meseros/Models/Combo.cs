using System;
using System.Collections.Generic;

namespace Modulo_2_Meseros.Models;

public partial class Combo
{
    public int ComboId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string? ImagenUrl { get; set; }

    public decimal Precio { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

    public virtual ICollection<PromocionesItem> PromocionesItems { get; set; } = new List<PromocionesItem>();

    public virtual ICollection<Plato> Platos { get; set; } = new List<Plato>();
}
