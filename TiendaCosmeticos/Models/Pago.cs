using System;

namespace TiendaCosmeticos.Models
{
    public class Pago
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public string PayPalTransactionId { get; set; }
        public DateTime FechaPago { get; set; } = DateTime.Now;
        public decimal Monto { get; set; }
        public string EstadoPago { get; set; }
        public Pedido? Pedido { get; set; }
    }
}
