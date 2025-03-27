namespace Modulo_2_Meseros.Models.DTO
{
    public class DetallePedidoDTO
    {
        public int IdPedido { get; set; }

        public int IdMenu { get; set; }

        public int? DetCantidad { get; set; }

        public decimal? DetPrecio { get; set; }

        public decimal? DetSubtotal { get; set; }

        public string? DetComentarios { get; set; }

        public int? IdEstadopedido { get; set; }
    }
}
