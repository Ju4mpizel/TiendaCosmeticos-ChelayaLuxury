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

        // Pagina de inicio de sesion, solo muestra el formulario vacio
        public IActionResult Index()
        {
            return View();
        }

        // Procesa el intento de login: busca al usuario por correo y contraseña.
        // Si existe y esta activo, guarda sus datos en sesion y lo manda a la tienda.
        // Si falla, regresa al login con un mensaje de error
        [HttpPost]
        public IActionResult Ingresar(string correo, string password)
        {
            var usuarioEncontrado = _bd.Usuarios
                .FirstOrDefault(u => u.Email == correo && u.PasswordHash == password && u.Activo == true);

            if (usuarioEncontrado != null)
            {
                HttpContext.Session.SetInt32("UsuarioId", usuarioEncontrado.Id);
                HttpContext.Session.SetString("UsuarioNombre", usuarioEncontrado.NombreCompleto);

                var rol = _bd.Roles.Find(usuarioEncontrado.RolId);
                HttpContext.Session.SetString("UsuarioRol", rol?.Nombre ?? "Cliente");

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Correo o contraseña incorrectos.";
            return View("Index");
        }

        // Cierra la sesion del usuario actual y lo regresa a la pagina principal
        public IActionResult Salir()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
