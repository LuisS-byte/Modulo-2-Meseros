using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using Modulo_2_Meseros.Context;
using Modulo_2_Meseros.Models;
using Microsoft.EntityFrameworkCore.Storage;
using Modulo_2_Meseros.Models.DTO;

namespace Modulo_2_Meseros.Controllers
{
    //[Authorize(Roles = "Mesero")]
    public class MeseroController : Controller
    {
        private readonly AppDbContext _context;

        public MeseroController(AppDbContext context)
        {
            _context = context;
        }

        //[Authorize(Roles = "Mesero")]
        public async Task<IActionResult> Index()
        {
            var mesas = await _context.Mesas.ToListAsync();
            return View(mesas);
        }

        //[Authorize(Roles = "Mesero")]
        public async Task<IActionResult> EstadoMesas()
        {
            var mesas = await _context.Mesas.ToListAsync();
            return View(mesas);
        }

        // [Authorize(Roles = "Mesero")]
        [HttpPost]
        public async Task<IActionResult> CambiarEstadoMesa(int id)
        {
            var mesa = await _context.Mesas.FirstOrDefaultAsync(x => x.MesaId == id);
            if (mesa == null) return NotFound();

            mesa.Estado = !mesa.Estado;
            await _context.SaveChangesAsync();

            return RedirectToAction("EstadoMesas");
        }

        //[Authorize(Roles = "Mesero")]
        public async Task<IActionResult> VisualizarMenuOnlyPlatos()
        {
            try
            {
                // Consulta con join para obtener el nombre de la categoría
                var platos = await _context.Platos
                    .Include(p => p.Categoria)
                    .Select(p => new
                    {
                        PlatoId = p.PlatoId,
                        NombrePlato = p.Nombre,
                        Precio = p.Precio,
                        Descripcion = p.Descripcion,
                        ImagenURL = p.ImagenUrl,
                        NombreCategoria = p.Categoria != null ? p.Categoria.Nombre : "Sin categoría"
                    })
                    .ToListAsync();

                return View(platos);
            }
            catch (Exception ex)
            {
                // Manejo de errores
                ViewBag.ErrorMessage = ex.Message;
                return View(new List<dynamic>());
            }
        }

        //[Authorize(Roles = "Mesero")]
        public async Task<IActionResult> VisualizarMenuOnlyCombos()
        {
            var today = DateTime.Today;

            var combos = await (from c in _context.Combos
                                join mi in _context.MenuItems on c.ComboId equals mi.ComboId
                                join m in _context.Menus on mi.MenuId equals m.MenuId
                                where mi.ComboId != null
                                && m.FechaInicio <= today
                                && m.FechaFin >= today
                                select new
                                {
                                    ComboId = c.ComboId,
                                    NombreCombo = c.Nombre,
                                    Precio = c.Precio,
                                    Descripcion = c.Descripcion,
                                    ImagenURL = c.ImagenUrl,
                                    FechaValidoDesde = m.FechaInicio,
                                    FechaValidoHasta = m.FechaFin
                                }).ToListAsync();

            return View(combos);
        }

        //[Authorize(Roles = "Mesero")]
        public async Task<IActionResult> VisualizarMenuOnlyPromociones()
        {
            var currentDate = DateOnly.FromDateTime(DateTime.Now);

            var promociones = await (from p in _context.Promociones
                                     join pi in _context.PromocionesItems on p.PromocionId equals pi.PromocionId
                                     where p.FechaInicio <= currentDate
                                        && p.FechaFin >= currentDate
                                     select new
                                     {
                                         PromocionID = p.PromocionId,
                                         Descripcion = p.Descripcion,
                                         Descuento = p.Descuento,
                                         FechaInicio = p.FechaInicio,
                                         FechaFin = p.FechaFin,
                                         Items = (pi.PlatoId != null) ?
                                             _context.Platos.Where(pl => pl.PlatoId == pi.PlatoId)
                                                 .Select(pl => new
                                                 {
                                                     Tipo = "Plato",
                                                     ID = pl.PlatoId,
                                                     Nombre = pl.Nombre,
                                                     PrecioOriginal = pl.Precio,
                                                     PrecioConDescuento = pl.Precio * (1 - (p.Descuento / 100))
                                                 }).FirstOrDefault() :
                                             _context.Combos.Where(co => co.ComboId == pi.ComboId)
                                                 .Select(co => new
                                                 {
                                                     Tipo = "Combo",
                                                     ID = co.ComboId,
                                                     Nombre = co.Nombre,
                                                     PrecioOriginal = co.Precio,
                                                     PrecioConDescuento = co.Precio * (1 - (p.Descuento / 100))
                                                 }).FirstOrDefault()
                                     }).ToListAsync();

            // Group by promotion since one promotion can have multiple items
            var result = promociones.GroupBy(p => p.PromocionID)
                .Select(g => new
                {
                    PromocionID = g.Key,
                    Descripcion = g.First().Descripcion,
                    Descuento = g.First().Descuento,
                    FechaInicio = g.First().FechaInicio,
                    FechaFin = g.First().FechaFin,
                    Items = g.Select(i => i.Items).Where(i => i != null).ToList()
                }).ToList();

            return Ok(result);
        }

        //[Authorize(Roles = "Mesero")]
        public IActionResult CrearPedido()
        {
            return View();
        }

        // [Authorize(Roles = "Mesero")]
        [HttpPost]
        public async Task<IActionResult> AgregarPedido([FromForm] PedidoCreacion request)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var mesa = await _context.Mesas.FindAsync(request.Pedido.IdMesa.Value);
                if (mesa == null) return NotFound("Mesa no encontrada.");

                if (mesa.Estado == true)
                    return BadRequest("La mesa ya está ocupada.");

                mesa.Estado = true;

                var pedido = new Pedido
                {
                    IdMesa = request.Pedido.IdMesa.Value,
                    IdMesero = request.Pedido.EmpleadoID.Value,
                    IdEstadopedido = request.Pedido.IdEstadopedido.Value
                };

                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync();

                var menuItem = await _context.MenuItems
                    .FirstOrDefaultAsync(mi => mi.MenuItemId == request.DetallePedido.IdMenu);

                if (menuItem == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest($"El MenuItemId {request.DetallePedido.IdMenu} no existe.");
                }

                decimal precio = 0;
                if (menuItem.PlatoId.HasValue)
                {
                    precio = await _context.Platos
                        .Where(p => p.PlatoId == menuItem.PlatoId.Value)
                        .Select(p => p.Precio)
                        .FirstOrDefaultAsync();
                }
                else if (menuItem.ComboId.HasValue)
                {
                    precio = await _context.Combos
                        .Where(c => c.ComboId == menuItem.ComboId.Value)
                        .Select(c => c.Precio)
                        .FirstOrDefaultAsync();
                }

                var detalle = new DetallePedido
                {
                    IdPedido = pedido.IdPedido,
                    IdMenu = request.DetallePedido.IdMenu,
                    DetCantidad = request.DetallePedido.DetCantidad.Value,
                    DetPrecio = precio,
                    DetSubtotal = request.DetallePedido.DetCantidad.Value * precio,
                    DetComentarios = request.DetallePedido.DetComentarios ?? string.Empty,
                    IdEstadopedido = request.DetallePedido.IdEstadopedido.Value
                };

                _context.DetallePedidos.Add(detalle);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new PedidoResponseDTO
                {
                    IdPedido = pedido.IdPedido,
                    IdMesa = (int)pedido.IdMesa,
                    IdMesero = (int)pedido.IdMesero,
                    Detalle = new DetallePedidoResponseDTO
                    {
                        IdMenu = detalle.IdMenu,
                        Cantidad = (int)detalle.DetCantidad,
                        Precio = (decimal)detalle.DetPrecio,
                        Comentarios = detalle.DetComentarios
                    }
                });
            }
            catch (Exception ex)
            {
                if (transaction.GetDbTransaction().Connection != null)
                {
                    await transaction.RollbackAsync();
                }

                return StatusCode(500, new
                {
                    mensaje = "Error al crear el pedido.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        //[Authorize(Roles = "Mesero")]
        public async Task<IActionResult> VerDetallePedido(int idMesa)
        {


            var pedidoActivo = await _context.Pedidos
            .Where(p => p.IdMesa == idMesa && (p.IdEstadopedido == 2)) // Ajustá los IDs de estados según tu DB
            .OrderByDescending(p => p.IdPedido) // opcional, por si hubiera más de uno con estado activo
            .FirstOrDefaultAsync();

           /* if (pedidoActivo == null)
            {
                TempData["Error"] = "Esta mesa no tiene un pedido activo.";
                return RedirectToAction("EstadoMesas"); // O la vista donde el mesero decide qué hacer
            }*/

            // Traer detalles del pedido activo
            var detallePedido = await _context.DetallePedidos
                .Include(dp => dp.IdPedidoNavigation)
                    .ThenInclude(p => p.IdMeseroNavigation)
                .Include(dp => dp.IdEstadopedidoNavigation)
                .Include(dp => dp.IdMenuNavigation)
                    .ThenInclude(mi => mi.Platos)
                .Include(dp => dp.IdMenuNavigation)
                    .ThenInclude(mi => mi.Combo)
                .Include(dp => dp.IdMenuNavigation)
                    .ThenInclude(mi => mi.Promocion)
                .Where(dp => dp.IdPedido == pedidoActivo.IdPedido)
                .ToListAsync();

            return View("DetallePedidoView", detallePedido);
        }


        //[Authorize(Roles = "Mesero")]
        [HttpPost]
        public async Task<IActionResult> AgregarDetallePedido(DetallePedidoDTO detallePedidoDTO)
        {
            var detallePedido = new DetallePedido
            {
                IdPedido = detallePedidoDTO.IdPedido,
                IdMenu = detallePedidoDTO.IdMenu,
                DetCantidad = detallePedidoDTO.DetCantidad,
                DetPrecio = detallePedidoDTO.DetPrecio,
                DetSubtotal = detallePedidoDTO.DetSubtotal,
                DetComentarios = detallePedidoDTO.DetComentarios,
                IdEstadopedido = detallePedidoDTO.IdEstadopedido
            };

            _context.DetallePedidos.Add(detallePedido);
            await _context.SaveChangesAsync();
            return Ok(detallePedido);
        }

        //[Authorize(Roles = "Mesero")]   
        [HttpPost]
        public async Task<IActionResult> CambiarEstadoDetallePedido(int idDetallePedido, int IdEstadoDetallePedido)
        {
            var pedido = await _context.Pedidos.FirstOrDefaultAsync(x => x.IdPedido == idDetallePedido);
            if (pedido == null) return NotFound();

            pedido.IdEstadopedido = IdEstadoDetallePedido;
            await _context.SaveChangesAsync();

            return RedirectToAction("VerDetallePedido", new { idMesa = pedido.IdMesa });
        }

        public IActionResult DetallePedidoView()
        {

            return View();
        }
    }
}
