namespace Modulo_2_Meseros.Models.DTO
{
    public class PedidoTemporalItem
    {
        public int MenuItemId { get; set; }
        public int PlatoId { get; set; }
        public int ComboId { get; set; }
        public int PromocionId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public string Comentarios { get; set; } = string.Empty;
        public string TipoItem { get; set; } 
        public int IdEstadoPedido { get; set; } = 1; // Valor por defecto (1 = Solicitado)
        public decimal Subtotal => Cantidad * Precio;
    }
}
