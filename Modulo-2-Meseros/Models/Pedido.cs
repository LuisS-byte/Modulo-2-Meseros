using System;
using System.Collections.Generic;

namespace Modulo_2_Meseros.Models;

public partial class Pedido
{
    public int IdPedido { get; set; }

    public int? IdMesa { get; set; }

    public int? IdMesero { get; set; }

    public int? IdEstadopedido { get; set; }

    public virtual ICollection<DetallePedido> DetallePedidos { get; set; } = new List<DetallePedido>();

    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    public virtual EstadoPedido? IdEstadopedidoNavigation { get; set; }

    public virtual Mesa? IdMesaNavigation { get; set; }

    public virtual Empleado? IdMeseroNavigation { get; set; }
}
