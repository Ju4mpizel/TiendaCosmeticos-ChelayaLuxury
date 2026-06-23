using System;

namespace TiendaCosmeticos.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; } // Quién hizo la compra
        public DateTime FechaPedido { get; set; } = DateTime.Now; // Fecha y hora actual
        public decimal Total { get; set; }
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Pagado o Cancelado

        // Propiedad de navegación para saber los datos del cliente
        public Usuario? Usuario { get; set; }
    }
}