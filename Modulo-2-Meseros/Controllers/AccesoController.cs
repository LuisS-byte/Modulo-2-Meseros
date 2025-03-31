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

        public IActionResult Index(string token)
        {
            ViewData["Token"] = token;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDTO objeto)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", objeto);
            }

            var usuario = await _dbContext.Empleados
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u =>
                    u.Email == objeto.Correo &&
                    u.Contrasena == _utilidades.encriptarSHA256(objeto.Clave));

            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
                return View("Index", objeto);
            }

            var token = _utilidades.GenerarToken(usuario);

            // Pasar el token como parámetro de consulta
            return RedirectToAction("Index", "Acceso", new { token });
        }


    }
}
