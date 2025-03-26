using System;
using System.Collections.Generic;

namespace Modulo_2_Meseros.Models;

public partial class Mesa
{
    public int MesaId { get; set; }

    public int NumeroMesa { get; set; }

    public int Capacidad { get; set; }

    public bool? Estado { get; set; }

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
