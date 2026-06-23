using Microsoft.AspNetCore.Mvc;
using TiendaCosmeticos.Data;
using TiendaCosmeticos.Models;

namespace TiendaCosmeticos.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _bd;

        public LoginController(ApplicationDbContext baseDatos)
        {
            _bd = baseDatos;
        }

        // 1. PANTALLA DE LOGIN (GET)
        public IActionResult Index()
        {
            return View();
        }

        // 2. PROCESO DE LOGUEARSE (POST)
        [HttpPost]
        public IActionResult Ingresar(string correo, string password)
        {
            // Buscamos si existe un usuario activo con ese correo y contraseña
            var usuarioEncontrado = _bd.Usuarios
                .FirstOrDefault(u => u.Email == correo && u.PasswordHash == password && u.Activo == true);

            if (usuarioEncontrado != null)
            {
                // ¡Éxito! Guardamos sus datos en la Sesión de la memoria web
                HttpContext.Session.SetInt32("UsuarioId", usuarioEncontrado.Id);
                HttpContext.Session.SetString("UsuarioNombre", usuarioEncontrado.NombreCompleto);

                // Buscamos el nombre de su rol para saber si es Admin o Cliente
                var rol = _bd.Roles.Find(usuarioEncontrado.RolId);
                HttpContext.Session.SetString("UsuarioRol", rol?.Nombre ?? "Cliente");

                // Lo mandamos a la tienda principal
                return RedirectToAction("Index", "Home");
            }

            // Si los datos están mal, volvemos al Login y le avisamos con un mensaje básico
            ViewBag.Error = "Correo o contraseña incorrectos.";
            return View("Index");
        }

        // 3. CERRAR SESIÓN
        public IActionResult Salir()
        {
            // Borramos la memoria de la sesión y lo mandamos al inicio
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
