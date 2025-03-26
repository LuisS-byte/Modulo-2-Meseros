using System;
using System.Collections.Generic;

namespace Modulo_2_Meseros.Models;

public partial class Plato
{
    public int PlatoId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public string? ImagenUrl { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public int? CategoriaId { get; set; }

    public virtual Categoria? Categoria { get; set; }

    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

    public virtual ICollection<PromocionesItem> PromocionesItems { get; set; } = new List<PromocionesItem>();

    public virtual ICollection<Combo> Combos { get; set; } = new List<Combo>();
}
