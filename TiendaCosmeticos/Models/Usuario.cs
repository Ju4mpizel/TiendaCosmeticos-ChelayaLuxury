namespace TiendaCosmeticos.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public int RolId { get; set; }
        public bool Activo { get; set; } = true;
        public Rol? Rol { get; set; }
    }
}
