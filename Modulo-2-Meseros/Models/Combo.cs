﻿using System;
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

    public virtual ICollection<MenuItems> MenuItems { get; set; } = new List<MenuItems>();

    public virtual ICollection<PromocionesItem> PromocionesItems { get; set; } = new List<PromocionesItem>();

    public virtual ICollection<Platos> Platos { get; set; } = new List<Platos>();
}
