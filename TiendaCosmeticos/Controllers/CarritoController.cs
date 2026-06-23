using Microsoft.AspNetCore.Mvc;
using System.Text.Json; // Herramienta simple para guardar listas en la sesión
using TiendaCosmeticos.Data;
using TiendaCosmeticos.Models;

namespace TiendaCosmeticos.Controllers
{
    public class CarritoController : Controller
    {
        private readonly ApplicationDbContext _bd;

        public CarritoController(ApplicationDbContext baseDatos)
        {
            _bd = baseDatos;
        }

        // AUXILIAR: Método interno que saca el carrito actual de la memoria Session
        private List<CarritoItem> ObtenerCarritoDeSesion()
        {
            var carritoJson = HttpContext.Session.GetString("Carrito");
            if (string.IsNullOrEmpty(carritoJson))
            {
                return new List<CarritoItem>(); // Si está vacío, devuelve una lista nueva
            }
            return JsonSerializer.Deserialize<List<CarritoItem>>(carritoJson)!;
        }

        // AUXILIAR: Método interno que guarda el carrito modificado en la memoria Session
        private void GuardarCarritoEnSesion(List<CarritoItem> carrito)
        {
            var carritoJson = JsonSerializer.Serialize(carrito);
            HttpContext.Session.SetString("Carrito", carritoJson);
        }

        // 1. VER EL CARRITO
        public IActionResult Index()
        {
            var carrito = ObtenerCarritoDeSesion();
            // Calculamos la suma de todos los subtotales para mostrar el total general en la pantalla
            ViewBag.TotalCarrito = carrito.Sum(item => item.Subtotal);
            return View(carrito);
        }

        // 2. AGREGAR UN PRODUCTO AL CARRITO
        [HttpPost]
        [ValidateAntiForgeryToken] // 🔒 Protección CSRF
        public IActionResult Agregar(int productoId, int cantidad)
        {
            var producto = _bd.Productos.Find(productoId);
            if (producto == null) return NotFound();

            var carrito = ObtenerCarritoDeSesion();

            // Revisamos si el cosmético ya estaba en el carrito
            var itemExistente = carrito.FirstOrDefault(i => i.ProductoId == productoId);

            if (itemExistente != null)
            {
                itemExistente.Cantidad += cantidad; // Si ya estaba, solo sumamos la cantidad
            }
            else
            {
                // Si es nuevo, creamos un nuevo renglón
                carrito.Add(new CarritoItem
                {
                    ProductoId = producto.Id,
                    NombreProducto = producto.Nombre,
                    Precio = producto.Precio,
                    Cantidad = cantidad
                });
            }

            GuardarCarritoEnSesion(carrito);
            return RedirectToAction("Index");
        }

        // 3. QUITAR UN PRODUCTO DEL CARRITO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int productoId)
        {
            var carrito = ObtenerCarritoDeSesion();
            var item = carrito.FirstOrDefault(i => i.ProductoId == productoId);

            if (item != null)
            {
                carrito.Remove(item); // Lo sacamos de la lista temporal
            }

            GuardarCarritoEnSesion(carrito);
            return RedirectToAction("Index");
        }

        // 4. TRANSFORMAR EL CARRITO TEMPORAL EN UN PEDIDO REAL EN SQL Y DESCONTAR STOCK
        [HttpPost]
        [ValidateAntiForgeryToken] // 🔒 Protección CSRF
        public IActionResult ConfirmarCompra()
        {
            // PASO REQUISITO: Validamos si hay un usuario logueado en el sistema
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                // Si no ha iniciado sesión, lo mandamos a registrarse o loguearse primero
                return RedirectToAction("Index", "Login");
            }

            var carrito = ObtenerCarritoDeSesion();
            if (carrito.Count == 0) return RedirectToAction("Index", "Home");

            // PASO A: Creamos la Cabecera del Pedido
            var nuevoPedido = new Pedido
            {
                UsuarioId = usuarioId.Value,
                Total = carrito.Sum(item => item.Subtotal),
                Estado = "Pendiente" // Esperando el pago simulado de PayPal
            };

            _bd.Pedidos.Add(nuevoPedido);
            _bd.SaveChanges(); // SQL genera el ID automático para este pedido aquí

            // PASO B: Recorremos el carrito, guardamos cada renglón y DESCONTAMOS STOCK
            foreach (var item in carrito)
            {
                // 1. Guardamos el registro en DetallesPedido
                var detalle = new DetallePedido
                {
                    PedidoId = nuevoPedido.Id, // El ID que SQL acaba de generar arriba
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.Precio
                };
                _bd.DetallesPedido.Add(detalle);

                // 2. 🔑 DESCUENTO DE STOCK (Punto 7): Buscamos el producto real en SQL Server
                var productoBD = _bd.Productos.Find(item.ProductoId);
                if (productoBD != null)
                {
                    // Restamos la cantidad que el cliente agregó a su carrito
                    productoBD.Stock -= item.Cantidad;

                    // Validación de seguridad para que el inventario nunca baje de 0
                    if (productoBD.Stock < 0)
                    {
                        productoBD.Stock = 0;
                    }

                    _bd.Productos.Update(productoBD);
                }
            }

            _bd.SaveChanges(); // Guardamos todos los detalles y el nuevo stock en la base de datos

            // PASO C: Vaciamos el carrito de la memoria porque ya se procesó
            HttpContext.Session.Remove("Carrito");

            // Pasamos el ID del pedido a la siguiente pantalla (La de pagos de PayPal)
            return RedirectToAction("ProcesarPago", "Pagos", new { pedidoId = nuevoPedido.Id });
        }
    }
}
