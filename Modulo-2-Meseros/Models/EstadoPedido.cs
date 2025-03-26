using System;
using System.Collections.Generic;

namespace Modulo_2_Meseros.Models;

public partial class EstadoPedido
{
    public int IdEstadopedido { get; set; }

    public string? EstadoNombre { get; set; }

    public virtual ICollection<DetallePedido> DetallePedidos { get; set; } = new List<DetallePedido>();

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
