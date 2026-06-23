using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaCosmeticos.Data;

namespace TiendaCosmeticos.Controllers
{
    public class DetallePedidoController : Controller
    {
        private readonly ApplicationDbContext _bd;

        public DetallePedidoController(ApplicationDbContext baseDatos)
        {
            _bd = baseDatos;
        }

        // 🔒 Verifica que solo un Administrador pueda ejecutar estas acciones
        private IActionResult? VerificarAdmin()
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            if (rol != "Administrador")
            {
                return RedirectToAction("Index", "Login");
            }
            return null;
        }

        // 1. LEER DETALLES DE UN PEDIDO (Index)
        // Recibe el ID del pedido que el administrador seleccionó
        public IActionResult Index(int id)
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            // Buscamos solo los renglones que correspondan a ese número de pedido
            // .Include(d => d.Producto) nos da acceso al Nombre y Precio del cosmético
            var listaDetalles = _bd.DetallesPedido
                .Where(d => d.PedidoId == id)
                .Include(d => d.Producto)
                .ToList();

            // Guardamos el pedido general para mostrar el Total de la compra arriba en la pantalla
            var pedidoGeneral = _bd.Pedidos.Include(p => p.Usuario).FirstOrDefault(p => p.Id == id);
            ViewBag.PedidoGeneral = pedidoGeneral;

            return View(listaDetalles);
        }
    }
}
