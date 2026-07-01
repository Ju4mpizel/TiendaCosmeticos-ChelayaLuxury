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

        // Valida que solo un Administrador pueda ver los pedidos
        private IActionResult? VerificarAdmin()
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            if (rol != "Administrador")
            {
                return RedirectToAction("Index", "Login");
            }
            return null;
        }

        // Lista todas las ordenes de compra que se han generado en la tienda,
        // incluyendo el cliente que las hizo. Sirve como panel de control para el admin
        public IActionResult Index()
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            var listaPedidos = _bd.Pedidos.Include(p => p.Usuario).ToList();
            return View(listaPedidos);
        }
    }
}
