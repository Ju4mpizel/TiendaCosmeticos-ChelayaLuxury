using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaCosmeticos.Data;

namespace TiendaCosmeticos.Controllers
{
    public class PedidoController : Controller
    {
        private readonly ApplicationDbContext _bd;

        public PedidoController(ApplicationDbContext baseDatos)
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

        // 1. LEER PEDIDOS (Index)
        // Muestra en una tabla del Dashboard todas las órdenes de compra de la tienda
        public IActionResult Index()
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            // .Include(p => p.Usuario) une el pedido con el cliente para saber quién compró
            var listaPedidos = _bd.Pedidos.Include(p => p.Usuario).ToList();
            return View(listaPedidos);
        }
    }
}
