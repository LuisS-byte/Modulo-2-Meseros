﻿namespace Modulo_2_Meseros.Models.DTO
{
    public class PedidoDTO
    {
        public int IdPedido { get; set; }

        public int? IdMesa { get; set; }

        public int? EmpleadoID { get; set; }

        public int? IdEstadopedido { get; set; }
    }
}
