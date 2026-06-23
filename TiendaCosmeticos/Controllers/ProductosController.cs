using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; // Necesario para el .Include
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

        // 1. PANTALLA PRINCIPAL: Lista los productos con el nombre de su categoría
        public IActionResult Index()
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            // .Include(p => p.Categoria) une la tabla Producto con Categoría (como un JOIN en SQL)
            var listaProductos = _bd.Productos.Include(p => p.Categoria).ToList();
            return View(listaProductos);
        }

        // 2. FORMULARIO DE CREAR (GET: Pantalla en blanco con el desplegable)
        public IActionResult Create()
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            // Buscamos las categorías que no estén ocultas
            var categoriasActivas = _bd.Categorias.Where(c => c.Activo == true).ToList();

            // Guardamos la lista en el "ViewBag" configurando: (Lista, lo que se guarda, lo que el usuario lee)
            ViewBag.CategoriasDisponibles = new SelectList(categoriasActivas, "Id", "Nombre");

            return View();
        }

        // 3. BOTÓN DE GUARDAR (POST: Recibe el cosmético lleno)
        [HttpPost]
        public IActionResult Create(Producto nuevoProducto)
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            _bd.Productos.Add(nuevoProducto);
            _bd.SaveChanges();
            return RedirectToAction("Index");
        }

        // 4. FORMULARIO DE EDITAR (Busca el producto y vuelve a armar el desplegable)
        public IActionResult Edit(int id)
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            var productoEncontrado = _bd.Productos.Find(id);

            if (productoEncontrado == null) return NotFound();

            var categoriasActivas = _bd.Categorias.Where(c => c.Activo == true).ToList();
            // El último parámetro le dice al desplegable cuál es la categoría que ya tenía este producto
            ViewBag.CategoriasDisponibles = new SelectList(categoriasActivas, "Id", "Nombre", productoEncontrado.CategoriaId);

            return View(productoEncontrado);
        }

        // 5. BOTÓN DE ACTUALIZAR (Guarda los cambios de la edición)
        [HttpPost]
        public IActionResult Edit(Producto productoModificado)
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            _bd.Productos.Update(productoModificado);
            _bd.SaveChanges();
            return RedirectToAction("Index");
        }

        // 6. BOTÓN DE ELIMINAR LÓGICO (Desactivar)
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