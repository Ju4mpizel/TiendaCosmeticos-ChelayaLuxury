using Microsoft.AspNetCore.Mvc;
using TiendaCosmeticos.Data;
using TiendaCosmeticos.Models;

namespace TiendaCosmeticos.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly ApplicationDbContext _bd;

        // El constructor solo sirve para conectar este archivo con la Base de Datos (_bd)
        public CategoriasController(ApplicationDbContext baseDatos)
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

        // 1. PANTALLA PRINCIPAL: Muestra la lista de categorías
        public IActionResult Index()
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            // Traemos la lista completa desde SQL Server
            var listaCategorias = _bd.Categorias.ToList();

            // Se la mandamos a la vista para que la dibuje en la pantalla
            return View(listaCategorias);
        }

        // 2. FORMULARIO DE CREAR (Petición GET: Solo muestra la pantalla en blanco)
        public IActionResult Create()
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            return View();
        }

        // 3. BOTÓN DE GUARDAR (Petición POST: Recibe lo que el usuario escribió)
        [HttpPost]
        public IActionResult Create(Categoria nuevaCategoria)
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            _bd.Categorias.Add(nuevaCategoria); // Lo preparamos en la lista
            _bd.SaveChanges();                  // ¡Lo guardamos en SQL Server!

            return RedirectToAction("Index");   // Volvemos a la lista principal
        }

        // 4. FORMULARIO DE EDITAR (Busca la categoría vieja y la muestra para cambiar datos)
        public IActionResult Edit(int id)
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            var categoriaEncontrada = _bd.Categorias.Find(id);

            return View(categoriaEncontrada);
        }

        // 5. BOTÓN DE ACTUALIZAR (Guarda los cambios de la edición)
        [HttpPost]
        public IActionResult Edit(Categoria categoriaModificada)
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            _bd.Categorias.Update(categoriaModificada); // Indicamos que se modificó
            _bd.SaveChanges();                          // Guardamos en SQL Server

            return RedirectToAction("Index");
        }

        // 6. BOTÓN DE ELIMINAR LÓGICO (Desactivar)
        [HttpPost]
        public IActionResult Desactivar(int id)
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            var categoria = _bd.Categorias.Find(id);

            if (categoria != null)
            {
                categoria.Activo = false; // Cambiamos el interruptor a falso (Borrado lógico)
                _bd.Categorias.Update(categoria);
                _bd.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
