using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // 🔑 Necesario para operaciones avanzadas si expandes la BD
using System.Linq;
using TiendaCosmeticos.Data;
using TiendaCosmeticos.Models;

namespace TiendaCosmeticos.Controllers
{
    public class PagosController : Controller
    {
        private readonly ApplicationDbContext _bd;

        public PagosController(ApplicationDbContext baseDatos)
        {
            _bd = baseDatos;
        }

        // Pantalla donde el cliente ve el resumen de su pedido antes de pagar.
        // Aqui se integra el boton de PayPal para que el usuario autorice el pago
        public IActionResult ProcesarPago(int pedidoId)
        {
            var pedido = _bd.Pedidos.Find(pedidoId);

            if (pedido == null) return NotFound();

            // Si el pedido ya no esta pendiente (ya se pago o se cancelo), lo sacamos
            if (pedido.Estado != "Pendiente") return RedirectToAction("Index", "Home");

            return View(pedido);
        }

        // Lo llama el JavaScript de PayPal cuando el pago se completa exitosamente.
        // Cambia el estado del pedido a "Pagado" y guarda el registro del pago con
        // el ID de transaccion que devuelve PayPal
        [HttpPost]
        public IActionResult RegistrarPagoExitoso(int pedidoId, string transaccionId)
        {
            var pedido = _bd.Pedidos.Find(pedidoId);
            if (pedido == null) return BadRequest("El pedido no existe.");

            pedido.Estado = "Pagado";
            _bd.Pedidos.Update(pedido);

            var nuevoPago = new Pago
            {
                PedidoId = pedido.Id,
                PayPalTransactionId = transaccionId,
                Monto = pedido.Total,
                EstadoPago = "Approved",
                FechaPago = System.DateTime.Now
            };

            _bd.Pagos.Add(nuevoPago);
            _bd.SaveChanges();

            return Json(new { success = true, mensaje = "Pago registrado en el sistema correctamente." });
        }

        // 🔄 NUEVA ACCIÓN: Devuelve el stock si la pasarela es cancelada o da error
        [HttpPost]
        public IActionResult CancelarPedido(int pedidoId)
        {
            var pedido = _bd.Pedidos.Find(pedidoId);

            // Si el pedido no existe o ya cambió de estado, evitamos procesarlo dos veces
            if (pedido == null || pedido.Estado != "Pendiente")
                return Json(new { success = false, mensaje = "El pedido no puede ser cancelado." });

            // Buscamos todos los productos que pertenecían a esta orden
            var detalles = _bd.DetallesPedido.Where(d => d.PedidoId == pedidoId).ToList();

            foreach (var item in detalles)
            {
                var producto = _bd.Productos.Find(item.ProductoId);
                if (producto != null)
                {
                    // Regresamos las unidades guardadas en el carrito al stock general
                    producto.Stock += item.Cantidad;
                    _bd.Productos.Update(producto);
                }
            }

            // Cambiamos el estado de 'Pendiente' a 'Cancelado'
            pedido.Estado = "Cancelado";
            _bd.Pedidos.Update(pedido);

            // Guardamos todos los cambios juntos en SQL Server
            _bd.SaveChanges();

            return Json(new { success = true, mensaje = "Pedido cancelado y stock restablecido con éxito." });
        }
    }
}
