using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaCosmeticos.Data;

namespace TiendaCosmeticos.Controllers
{
    public class PagoController : Controller
    {
        private readonly ApplicationDbContext _bd;

        public PagoController(ApplicationDbContext baseDatos)
        {
            _bd = baseDatos;
        }

        // Valida que solo un Administrador pueda acceder a estas pantallas
        private IActionResult? VerificarAdmin()
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            if (rol != "Administrador")
            {
                return RedirectToAction("Index", "Login");
            }
            return null;
        }

        // Muestra el historial completo de pagos registrados en el sistema.
        // Incluye datos del pedido y del usuario que hizo la compra para tener contexto
        public IActionResult Index()
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            var listaPagos = _bd.Pagos
                .Include(p => p.Pedido)
                    .ThenInclude(ped => ped.Usuario)
                .ToList();

            return View(listaPagos);
        }
    }
}
