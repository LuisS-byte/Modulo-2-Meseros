using System;
using System.Collections.Generic;

namespace Modulo_2_Meseros.Models;

public partial class DetallePedido
{
    public int IdPedido { get; set; }

    public int IdMenu { get; set; }

    public int? DetCantidad { get; set; }

    public decimal? DetPrecio { get; set; }

    public decimal? DetSubtotal { get; set; }

    public string? DetComentarios { get; set; }

    public int? IdEstadopedido { get; set; }

    public virtual EstadoPedido? IdEstadopedidoNavigation { get; set; }

    public virtual MenuItem IdMenuNavigation { get; set; } = null!;

    public virtual Pedido IdPedidoNavigation { get; set; } = null!;
}
