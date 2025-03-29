using System;
using System.Collections.Generic;

namespace Modulo_2_Meseros.Models;

public partial class Menu
{
    public int MenuId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual ICollection<MenuItems> MenuItems { get; set; } = new List<MenuItems>();
}
