using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaCosmeticos.Data;

namespace TiendaCosmeticos.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _bd;

        public HomeController(ApplicationDbContext baseDatos)
        {
            _bd = baseDatos;
        }

        // Página principal de la tienda — muestra todos los productos activos
        // en una especie de vitrina o catalogo para que el cliente vea que hay
        public IActionResult Index()
        {
            var productosTienda = _bd.Productos
                .Where(p => p.Activo == true)
                .Include(p => p.Categoria)
                .ToList();

            return View(productosTienda);
        }

        // Pantalla generica de error, se usa cuando algo sale mal en la app
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
