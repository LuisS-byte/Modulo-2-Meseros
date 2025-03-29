using System;
using System.Collections.Generic;

namespace Modulo_2_Meseros.Models;

public partial class Promocione
{
    public int PromocionId { get; set; }

    public string Descripcion { get; set; } = null!;

    public decimal Descuento { get; set; }

    public string? ImagenUrl { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public virtual ICollection<MenuItems> MenuItems { get; set; } = new List<MenuItems>();

    public virtual ICollection<PromocionesItem> PromocionesItems { get; set; } = new List<PromocionesItem>();
}
