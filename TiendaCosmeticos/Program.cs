using Microsoft.EntityFrameworkCore;
using TiendaCosmeticos.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de la conexión a SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConexionCosmeticos")));

// 2. Add services to the container (Controladores y Vistas)
builder.Services.AddControllersWithViews();

// 🔑 PASO A: REGISTRAR LOS SERVICIOS DE SESIÓN
builder.Services.AddDistributedMemoryCache(); // Necesario para almacenar las sesiones en memoria
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // La sesión expira tras 30 minutos de inactividad
    options.Cookie.HttpOnly = true; // Mayor seguridad para que no se acceda por JS del navegador
    options.Cookie.IsEssential = true; // Esencial para el funcionamiento correcto de la app
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// 🔑 PASO B: ENCIENDE EL MIDDLEWARE DE SESIONES (¡Obligatorio justo aquí!)
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
