using Modulo_2_Meseros.Models.DTO;

namespace Modulo_2_Meseros.Models
{
    public class PedidoCreacion
    {
        public PedidoDTO? Pedido { get; set; }
        public DetallePedidoDTO? DetallePedido { get; set; }
    }
}
