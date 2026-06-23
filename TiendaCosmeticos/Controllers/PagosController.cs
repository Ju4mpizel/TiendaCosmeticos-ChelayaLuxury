using Microsoft.AspNetCore.Mvc;
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

        // 1. PANTALLA DE PAGO (GET)
        // Muestra el total a pagar y aquí es donde se dibujará el botón amarillo de PayPal
        public IActionResult ProcesarPago(int pedidoId)
        {
            // Buscamos el pedido en la base de datos para saber cuánto cobrar
            var pedido = _bd.Pedidos.Find(pedidoId);

            if (pedido == null) return NotFound();

            // Si el pedido ya fue pagado o cancelado, lo botamos al inicio
            if (pedido.Estado != "Pendiente") return RedirectToAction("Index", "Home");

            // Le mandamos el pedido completo a la vista (para sacar el ID y el Monto Total en el HTML)
            return View(pedido);
        }

        // 2. RECIBIR CONFIRMACIÓN DE PAYPAL (POST)
        // Este método será llamado automáticamente desde el JavaScript de PayPal cuando la transacción sea exitosa
        [HttpPost]
        public IActionResult RegistrarPagoExitoso(int pedidoId, string transaccionId)
        {
            var pedido = _bd.Pedidos.Find(pedidoId);
            if (pedido == null) return BadRequest("El pedido no existe.");

            // PASO A: Cambiamos el estado del Pedido original a 'Pagado'
            pedido.Estado = "Pagado";
            _bd.Pedidos.Update(pedido);

            // PASO B: Llenamos la séptima tabla (Pagos) con los datos del simulador
            var nuevoPago = new Pago
            {
                PedidoId = pedido.Id,
                PayPalTransactionId = transaccionId, // El código largo que nos da PayPal (Ej: PAYID-L7...)
                Monto = pedido.Total,
                EstadoPago = "Approved", // Estado aprobado por el simulador
                FechaPago = System.DateTime.Now
            };

            _bd.Pagos.Add(nuevoPago);

            // PASO C: Guardamos todos los cambios juntos en SQL Server
            _bd.SaveChanges();

            // Le respondemos un mensaje de éxito al JavaScript (un OK plano)
            return Json(new { success = true, mensaje = "Pago registrado en el sistema correctamente." });
        }
    }
}
