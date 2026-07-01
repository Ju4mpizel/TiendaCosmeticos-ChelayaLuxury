using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TiendaCosmeticos.Data;
using TiendaCosmeticos.Models;

namespace TiendaCosmeticos.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _bd;

        public UsuariosController(ApplicationDbContext baseDatos)
        {
            _bd = baseDatos;
        }

        // Valida que solo un Administrador pueda gestionar usuarios
        private IActionResult? VerificarAdmin()
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            if (rol != "Administrador")
            {
                return RedirectToAction("Index", "Login");
            }
            return null;
        }

        // Lista todos los usuarios registrados, incluyendo el nombre del rol
        // en lugar de solo mostrar el ID numerico
        public IActionResult Index()
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            var listaUsuarios = _bd.Usuarios.Include(u => u.Rol).ToList();
            return View(listaUsuarios);
        }

        // Redirige al formulario de registro publico para crear un usuario
        public IActionResult Create()
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            return RedirectToAction("Registro");
        }

        // Redirige al registro publico (por si alguien intenta POST directo)
        [HttpPost]
        public IActionResult Create(Usuario nuevoUsuario)
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            return RedirectToAction("Registro");
        }

        // Muestra el formulario para editar un usuario, con un desplegable
        // para asignarle un rol diferente
        public IActionResult Edit(int id)
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            var usuarioEncontrado = _bd.Usuarios.Find(id);
            if (usuarioEncontrado == null) return NotFound();

            var todosLosRoles = _bd.Roles.ToList();
            ViewBag.RolesDisponibles = new SelectList(todosLosRoles, "Id", "Nombre", usuarioEncontrado.RolId);

            return View(usuarioEncontrado);
        }

        // Guarda los cambios que el admin le hizo al usuario (nombre, rol, etc.)
        [HttpPost]
        public IActionResult Edit(Usuario usuarioModificado)
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            _bd.Usuarios.Update(usuarioModificado);
            _bd.SaveChanges();
            return RedirectToAction("Index");
        }

        // Suspende la cuenta de un usuario marcandola como inactiva.
        // No se borra de la base para no perder el historial de sus pedidos
        [HttpPost]
        public IActionResult Desactivar(int id)
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            var usuario = _bd.Usuarios.Find(id);
            if (usuario != null)
            {
                usuario.Activo = false;
                _bd.Usuarios.Update(usuario);
                _bd.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // Formulario publico de registro para que cualquier persona
        // pueda crear una cuenta nueva en la tienda
        public IActionResult Registro()
        {
            return View();
        }

        // Procesa el registro de un nuevo cliente: le asigna el rol
        // "Cliente" automaticamente y lo guarda como usuario activo
        [HttpPost]
        public IActionResult Registro(Usuario nuevoCliente)
        {
            var rolCliente = _bd.Roles.FirstOrDefault(r => r.Nombre == "Cliente");

            if (rolCliente != null)
            {
                nuevoCliente.RolId = rolCliente.Id;
            }

            nuevoCliente.Activo = true;

            _bd.Usuarios.Add(nuevoCliente);
            _bd.SaveChanges();

            return RedirectToAction("Index", "Home");
        }
    }
}
