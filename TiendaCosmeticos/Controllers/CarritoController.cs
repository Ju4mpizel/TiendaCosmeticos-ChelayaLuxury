using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
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

        // Lee el carrito desde la sesion del usuario.
        // Si no hay nada guardado, devuelve una lista vacia para empezar de cero
        private List<CarritoItem> ObtenerCarritoDeSesion()
        {
            var carritoJson = HttpContext.Session.GetString("Carrito");
            if (string.IsNullOrEmpty(carritoJson))
            {
                return new List<CarritoItem>();
            }
            return JsonSerializer.Deserialize<List<CarritoItem>>(carritoJson)!;
        }

        // Guarda el carrito actual en la sesion serializado como JSON.
        // Se llama despues de cualquier cambio (agregar, quitar, etc.)
        private void GuardarCarritoEnSesion(List<CarritoItem> carrito)
        {
            var carritoJson = JsonSerializer.Serialize(carrito);
            HttpContext.Session.SetString("Carrito", carritoJson);
        }

        // Muestra el contenido del carrito de compras con el total acumulado
        public IActionResult Index()
        {
            var carrito = ObtenerCarritoDeSesion();
            ViewBag.TotalCarrito = carrito.Sum(item => item.Subtotal);
            return View(carrito);
        }

        // Agrega un producto al carrito. Si ya estaba, solo aumenta la cantidad
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Agregar(int productoId, int cantidad)
        {
            var producto = _bd.Productos.Find(productoId);
            if (producto == null) return NotFound();

            var carrito = ObtenerCarritoDeSesion();

            var itemExistente = carrito.FirstOrDefault(i => i.ProductoId == productoId);

            if (itemExistente != null)
            {
                itemExistente.Cantidad += cantidad;
            }
            else
            {
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

        // Quita un producto completo del carrito, no solo una unidad
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int productoId)
        {
            var carrito = ObtenerCarritoDeSesion();
            var item = carrito.FirstOrDefault(i => i.ProductoId == productoId);

            if (item != null)
            {
                carrito.Remove(item);
            }

            GuardarCarritoEnSesion(carrito);
            return RedirectToAction("Index");
        }

        // Convierte el carrito en un pedido real en la base de datos.
        // Por cada producto en el carrito descuenta el stock correspondiente.
        // Requiere que el usuario haya iniciado sesion, si no, lo redirige al login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmarCompra()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var carrito = ObtenerCarritoDeSesion();
            if (carrito.Count == 0) return RedirectToAction("Index", "Home");

            // Creamos la cabecera del pedido con el total calculado
            var nuevoPedido = new Pedido
            {
                UsuarioId = usuarioId.Value,
                Total = carrito.Sum(item => item.Subtotal),
                Estado = "Pendiente"
            };

            _bd.Pedidos.Add(nuevoPedido);
            _bd.SaveChanges();

            // Guardamos cada producto del carrito como detalle del pedido
            // y aprovechamos para descontar el stock en la misma pasada
            foreach (var item in carrito)
            {
                var detalle = new DetallePedido
                {
                    PedidoId = nuevoPedido.Id,
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.Precio
                };
                _bd.DetallesPedido.Add(detalle);

                var productoBD = _bd.Productos.Find(item.ProductoId);
                if (productoBD != null)
                {
                    productoBD.Stock -= item.Cantidad;

                    // Seguro por si se vende mas de lo que hay en teoria
                    if (productoBD.Stock < 0)
                    {
                        productoBD.Stock = 0;
                    }

                    _bd.Productos.Update(productoBD);
                }
            }

            _bd.SaveChanges();

            // Limpiamos el carrito de la sesion y mandamos al flujo de pago
            HttpContext.Session.Remove("Carrito");

            return RedirectToAction("ProcesarPago", "Pagos", new { pedidoId = nuevoPedido.Id });
        }
    }
}
