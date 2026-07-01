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

        // Valida que solo un Administrador pueda entrar a esta seccion
        private IActionResult? VerificarAdmin()
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            if (rol != "Administrador")
            {
                return RedirectToAction("Index", "Login");
            }
            return null;
        }

        // Muestra el detalle de un pedido especifico: que productos compro,
        // en que cantidad, a que precio, y quien fue el cliente
        public IActionResult Index(int id)
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            var listaDetalles = _bd.DetallesPedido
                .Where(d => d.PedidoId == id)
                .Include(d => d.Producto)
                .ToList();

            var pedidoGeneral = _bd.Pedidos.Include(p => p.Usuario).FirstOrDefault(p => p.Id == id);
            ViewBag.PedidoGeneral = pedidoGeneral;

            return View(listaDetalles);
        }
    }
}
