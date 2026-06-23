using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaCosmeticos.Data;

namespace TiendaCosmeticos.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _bd;

        // El constructor conecta este controlador a la base de datos
        public HomeController(ApplicationDbContext baseDatos)
        {
            _bd = baseDatos;
        }

        // Esta acción maneja la vitrina o escaparate principal de la web
        public IActionResult Index()
        {
            // Buscamos los productos activos e incluimos su categoría (un JOIN en SQL)
            var productosTienda = _bd.Productos
                .Where(p => p.Activo == true)
                .Include(p => p.Categoria)
                .ToList();

            // Enviamos la lista de cosméticos a la vista pública
            return View(productosTienda);
        }

        // 🔑 CORREGIDO: Acción encargada de manejar y renderizar la pantalla de errores (Punto #3)
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
