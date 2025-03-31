using System;
using System.Collections.Generic;

namespace Modulo_2_Meseros.Models;

public partial class Categorias
{
    public int CategoriaId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public virtual ICollection<Platos> Platos { get; set; } = new List<Platos>();
}
