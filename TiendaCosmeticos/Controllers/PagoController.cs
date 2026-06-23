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

        // 1. LEER HISTORIAL DE PAGOS (Index)
        // Genera un reporte completo cruzando datos de tres tablas
        public IActionResult Index()
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            // Explicación para el grupo:
            // .Include(p => p.Pedido) jala los datos de la compra
            // .ThenInclude(ped => ped.Usuario) jala los datos del cliente que pagó
            var listaPagos = _bd.Pagos
                .Include(p => p.Pedido)
                    .ThenInclude(ped => ped.Usuario)
                .ToList();

            return View(listaPagos);
        }
    }
}
