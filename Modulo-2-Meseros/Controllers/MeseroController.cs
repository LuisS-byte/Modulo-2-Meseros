using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using Modulo_2_Meseros.Context;
using Modulo_2_Meseros.Models;

namespace Modulo_2_Meseros.Controllers
{
    [Authorize(Roles = "Mesero")]
    public class MeseroController : Controller
    {
        private readonly AppDbContext _context;

        public MeseroController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Mesero")]
        public async Task<IActionResult> Index()
        {
            var mesas = await _context.Mesas.ToListAsync();
            return View(mesas);
        }

        [Authorize(Roles = "Mesero")]
        public async Task<IActionResult> EstadoMesas()
        {
            var mesas = await _context.Mesas.ToListAsync();
            return View(mesas);
        }

        [Authorize(Roles = "Mesero")]
        [HttpPost]
        public async Task<IActionResult> CambiarEstadoMesa(int id)
        {
            var mesa = await _context.Mesas.FirstOrDefaultAsync(x => x.MesaId == id);
            if (mesa == null) return NotFound();

            mesa.Estado = !mesa.Estado;
            await _context.SaveChangesAsync();

            return RedirectToAction("EstadoMesas");
        }

        [Authorize(Roles = "Mesero")]
        public async Task<IActionResult> VisualizarMenuOnlyPlatos()
        {
            var platos = await (from p in _context.Platos
                                join M in _context.MenuItems on p.PlatoId equals M.PlatoId
                                join c in _context.Categorias on p.CategoriaId equals c.CategoriaId
                                where M.PlatoId != null
                                select new
                                {
                                    PlatoId = M.PlatoId,
                                    NombrePlato = p.Nombre,
                                    Precio = p.Precio,
                                    Descripcion = p.Descripcion,
                                    ImagenURL = p.ImagenUrl,
                                    NombreCategoria = c.Nombre
                                }).ToListAsync();

            return View(platos);
        }

        [Authorize(Roles = "Mesero")]
        public async Task<IActionResult> VisualizarMenuOnlyCombos()
        {
            var combos = await (from p in _context.Combos
                                join M in _context.MenuItems on p.ComboId equals M.ComboId
                                where M.ComboId != null
                                select new
                                {
                                    ComboId = M.ComboId,
                                    NombreCombo = p.Nombre,
                                    Precio = p.Precio,
                                    Descripcion = p.Descripcion,
                                    ImagenURL = p.ImagenUrl
                                }).ToListAsync();

            return View(combos);
        }

        [Authorize(Roles = "Mesero")]
        public async Task<IActionResult> VisualizarMenuOnlyPromociones()
        {
            var promociones = await (from p in _context.Promociones
                                     join M in _context.MenuItems on p.PromocionId equals M.PromocionId
                                     where M.ComboId != null && M.PromocionId != null
                                     select new { }).ToListAsync();

            return View(promociones);
        }

        [Authorize(Roles = "Mesero")]
        public IActionResult CrearPedido()
        {
            return View();
        }

        [Authorize(Roles = "Mesero")]
        [HttpPost]
        public async Task<IActionResult> AgregarPedido([FromForm] PedidoCreacion request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var pedido = new Pedido
                {
                    IdPedido = request.Pedido.IdPedido,
                    IdMesa = request.Pedido.IdMesa,
                    IdMesero = request.Pedido.IdMesero,
                    IdEstadopedido = request.Pedido.IdEstadopedido
                };

                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync();

                var detallePedido = new DetallePedido
                {
                    IdPedido = pedido.IdPedido,
                    IdMenu = request.DetallePedido.IdMenu,
                    DetCantidad = request.DetallePedido.DetCantidad,
                    DetPrecio = request.DetallePedido.DetPrecio,
                    DetSubtotal = request.DetallePedido.DetSubtotal,
                    DetComentarios = request.DetallePedido.DetComentarios,
                    IdEstadopedido = request.DetallePedido.IdEstadopedido
                };

                _context.DetallePedidos.Add(detallePedido);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return RedirectToAction("VerDetallePedido", new { idMesa = pedido.IdMesa });
            }
            catch
            {
                await transaction.RollbackAsync();
                ViewBag.Error = "Error al crear el pedido";
                return View("CrearPedido");
            }
        }

        [Authorize(Roles = "Mesero")]
        public async Task<IActionResult> VerDetallePedido(int idMesa)
        {
            var detallePedido = await (from D in _context.DetallePedidos
                                       join P in _context.Pedidos on D.IdPedido equals P.IdPedido
                                       where P.IdMesa == idMesa
                                       select D).ToListAsync();

            return View(detallePedido);
        }

        [Authorize(Roles = "Mesero")]
        [HttpPost]
        public async Task<IActionResult> AgregarDetallePedido(int idMesa, DetallePedido detallePedido)
        {
            _context.DetallePedidos.Add(detallePedido);
            await _context.SaveChangesAsync();

            return RedirectToAction("VerDetallePedido", new { idMesa });
        }

        [Authorize(Roles = "Mesero")]   
        [HttpPost]
        public async Task<IActionResult> CambiarEstadoDetallePedido(int idDetallePedido, int IdEstadoDetallePedido)
        {
            var pedido = await _context.Pedidos.FirstOrDefaultAsync(x => x.IdPedido == idDetallePedido);
            if (pedido == null) return NotFound();

            pedido.IdEstadopedido = IdEstadoDetallePedido;
            await _context.SaveChangesAsync();

            return RedirectToAction("VerDetallePedido", new { idMesa = pedido.IdMesa });
        }
    }
}
