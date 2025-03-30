namespace Modulo_2_Meseros.Models.DTO
{
    public class DetallePedidoResponseDTO
    {
        public int IdMenu { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Subtotal { get; set; }
        public string Comentarios { get; set; }
    }
}
