using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TiendaCosmeticos.Data;
using TiendaCosmeticos.Models;

namespace TiendaCosmeticos.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ApplicationDbContext _bd;

        public ProductosController(ApplicationDbContext baseDatos)
        {
            _bd = baseDatos;
        }

        // Valida que solo un Administrador pueda gestionar productos
        private IActionResult? VerificarAdmin()
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            if (rol != "Administrador")
            {
                return RedirectToAction("Index", "Login");
            }
            return null;
        }

        // Lista todos los productos con el nombre de su categoria en lugar del ID,
        // asi el admin ve la informacion completa de una sola pasada
        public IActionResult Index()
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            var listaProductos = _bd.Productos.Include(p => p.Categoria).ToList();
            return View(listaProductos);
        }

        // Muestra el formulario para agregar un nuevo producto,
        // con un desplegable de categorias activas para elegir
        public IActionResult Create()
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            var categoriasActivas = _bd.Categorias.Where(c => c.Activo == true).ToList();

            ViewBag.CategoriasDisponibles = new SelectList(categoriasActivas, "Id", "Nombre");

            return View();
        }

        // Guarda el producto nuevo en la base de datos
        [HttpPost]
        public IActionResult Create(Producto nuevoProducto)
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            _bd.Productos.Add(nuevoProducto);
            _bd.SaveChanges();
            return RedirectToAction("Index");
        }

        // Carga el formulario de edicion con los datos del producto
        // y preselecciona la categoria que ya tenia asignada
        public IActionResult Edit(int id)
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            var productoEncontrado = _bd.Productos.Find(id);

            if (productoEncontrado == null) return NotFound();

            var categoriasActivas = _bd.Categorias.Where(c => c.Activo == true).ToList();
            ViewBag.CategoriasDisponibles = new SelectList(categoriasActivas, "Id", "Nombre", productoEncontrado.CategoriaId);

            return View(productoEncontrado);
        }

        // Aplica los cambios que el admin le hizo al producto
        [HttpPost]
        public IActionResult Edit(Producto productoModificado)
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            _bd.Productos.Update(productoModificado);
            _bd.SaveChanges();
            return RedirectToAction("Index");
        }

        // Desactiva un producto en lugar de borrarlo permanentemente.
        // Asi el historico de pedidos que lo incluyen no se pierde
        [HttpPost]
        public IActionResult Desactivar(int id)
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            var producto = _bd.Productos.Find(id);
            if (producto != null)
            {
                producto.Activo = false;
                _bd.Productos.Update(producto);
                _bd.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
