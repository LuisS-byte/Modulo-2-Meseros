namespace Modulo_2_Meseros.Models.DTO
{
    public class PedidoResponseDTO
    {
        public int IdPedido { get; set; }
        public int IdMesa { get; set; }
        public int IdMesero { get; set; }
        public List<DetallePedidoResponseDTO> Detalle { get; set; }
    }

}
