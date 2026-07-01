using Microsoft.AspNetCore.Mvc;
using TiendaCosmeticos.Data;
using TiendaCosmeticos.Models;

namespace TiendaCosmeticos.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly ApplicationDbContext _bd;
        public CategoriasController(ApplicationDbContext baseDatos)
        {
            _bd = baseDatos;
        }

        // Valida que el usuario logueado tenga rol de Administrador.
        // Si no cumple, lo redirige al login sin dejarle hacer nada
        private IActionResult? VerificarAdmin()
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            if (rol != "Administrador")
            {
                return RedirectToAction("Index", "Login");
            }
            return null;
        }

        // Lista todas las categorias registradas en el sistema
        public IActionResult Index()
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            var listaCategorias = _bd.Categorias.ToList();
            return View(listaCategorias);
        }

        // Muestra el formulario vacio para crear una categoria nueva
        public IActionResult Create()
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            return View();
        }

        // Guarda la categoria que el admin acaba de escribir
        [HttpPost]
        public IActionResult Create(Categoria nuevaCategoria)
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            _bd.Categorias.Add(nuevaCategoria);
            _bd.SaveChanges();

            return RedirectToAction("Index");
        }

        // Carga el formulario de edicion con los datos de la categoria seleccionada
        public IActionResult Edit(int id)
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            var categoriaEncontrada = _bd.Categorias.Find(id);

            return View(categoriaEncontrada);
        }

        // Aplica los cambios que el admin le hizo a la categoria
        [HttpPost]
        public IActionResult Edit(Categoria categoriaModificada)
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            _bd.Categorias.Update(categoriaModificada);
            _bd.SaveChanges();

            return RedirectToAction("Index");
        }

        // Desactiva una categoria en lugar de borrarla de la base de datos
        // Asi no se pierde el historial de productos que tenian esa categoria
        [HttpPost]
        public IActionResult Desactivar(int id)
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            var categoria = _bd.Categorias.Find(id);

            if (categoria != null)
            {
                categoria.Activo = false;
                _bd.Categorias.Update(categoria);
                _bd.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
