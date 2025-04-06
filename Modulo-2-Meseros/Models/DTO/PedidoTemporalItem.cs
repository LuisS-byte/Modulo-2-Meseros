namespace Modulo_2_Meseros.Models.DTO
{
    public class PedidoTemporalItem
    {
        public int IdMenu { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public string Comentarios { get; set; } = string.Empty;
        public string TipoItem { get; set; } = "Plato"; // Plato, Combo o Promo
        public decimal Subtotal => Cantidad * Precio;
    }
}
