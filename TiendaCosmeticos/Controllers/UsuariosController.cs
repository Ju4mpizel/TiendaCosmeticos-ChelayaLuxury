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

        // Verifica que solo un Administrador pueda ejecutar estas acciones
        private IActionResult? VerificarAdmin()
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            if (rol != "Administrador")
            {
                return RedirectToAction("Index", "Login");
            }
            return null;
        }

        // =========================================================
        // PARTE A: DASHBOARD (Solo el Administrador entra aquí)
        // =========================================================

        // 1. LISTA DE USUARIOS
        public IActionResult Index()
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            var listaUsuarios = _bd.Usuarios.Include(u => u.Rol).ToList();
            return View(listaUsuarios);
        }

        // 2. CREAR USUARIO DESDE PANEL (GET) → Redirige al formulario público de Registro
        public IActionResult Create()
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            return RedirectToAction("Registro");
        }

        // 3. GUARDAR USUARIO DESDE PANEL (POST) → Redirige al Registro público
        [HttpPost]
        public IActionResult Create(Usuario nuevoUsuario)
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            return RedirectToAction("Registro");
        }

        // 4. EDITAR USUARIO / CAMBIAR ROL (GET)
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

        // 5. ACTUALIZAR USUARIO (POST)
        [HttpPost]
        public IActionResult Edit(Usuario usuarioModificado)
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            _bd.Usuarios.Update(usuarioModificado);
            _bd.SaveChanges();
            return RedirectToAction("Index");
        }

        // 6. SUSPENDER CUENTA (Borrado Lógico)
        [HttpPost]
        public IActionResult Desactivar(int id)
        {
            var auth = VerificarAdmin();
            if (auth != null) return auth;

            var usuario = _bd.Usuarios.Find(id);
            if (usuario != null)
            {
                usuario.Activo = false; // Cuenta desactivada
                _bd.Usuarios.Update(usuario);
                _bd.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // =========================================================
        // PARTE B: PÁGINA PÚBLICA (Para cualquier persona de afuera)
        // =========================================================

        // 7. FORMULARIO DE REGISTRO PARA CLIENTES (GET)
        public IActionResult Registro()
        {
            return View();
        }

        // 8. BOTÓN DE REGISTRARSE DE LA WEB (POST)
        [HttpPost]
        public IActionResult Registro(Usuario nuevoCliente)
        {
            // Buscamos el ID del rol "Cliente" en la tabla para forzarlo
            var rolCliente = _bd.Roles.FirstOrDefault(r => r.Nombre == "Cliente");

            if (rolCliente != null)
            {
                nuevoCliente.RolId = rolCliente.Id; // Se le asigna automáticamente el rol de cliente
            }

            nuevoCliente.Activo = true;

            _bd.Usuarios.Add(nuevoCliente);
            _bd.SaveChanges();

            // Al terminar, lo mandamos a la página de bienvenida de la tienda
            return RedirectToAction("Index", "Home");
        }
    }
}
