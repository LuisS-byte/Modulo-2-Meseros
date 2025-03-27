using Microsoft.AspNetCore.Mvc;
using Modulo_2_Meseros.Models;
using Modulo_2_Meseros.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Modulo_2_Meseros.Custom;
using Modulo_2_Meseros.Context;

namespace Modulo_2_Meseros.Controllers
{
    public class AccesoController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly Utilidades _utilidades;

        public AccesoController(AppDbContext dbContext, Utilidades utilidades)
        {
            _dbContext = dbContext;
            _utilidades = utilidades;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO objeto)
        {
            var usuario = await _dbContext.Empleados
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u =>
                    u.Email == objeto.Correo &&
                    u.Contrasena == _utilidades.encriptarSHA256(objeto.Clave));

            if (usuario == null)
            {
                ViewBag.Error = "Correo o contraseña incorrectos.";
                return View("Index"); 
            }

            HttpContext.Session.SetString("Usuario", usuario.Email);
            HttpContext.Session.SetString("Rol", usuario.Rol?.Nombre ?? "");

            //cambiar cuando la vista este creada
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); 
            return RedirectToAction("Index");
        }
    }
}
