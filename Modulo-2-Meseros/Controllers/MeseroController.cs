using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using Modulo_2_Meseros.Context;
using Modulo_2_Meseros.Models;
using Microsoft.EntityFrameworkCore.Storage;
using Modulo_2_Meseros.Models.DTO;
using Modulo_2_Meseros.Custom;

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
        public async Task<IActionResult> VisualizarMenuOnlyPlatos(int idMesa)
        {
            ViewBag.IdMesa = idMesa;
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
        public async Task<IActionResult> VisualizarMenuOnlyCombos(int idMesa)
        {
            ViewBag.IdMesa = idMesa;
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
        public async Task<IActionResult> VisualizarMenuOnlyPromociones(int idMesa)
        {
            ViewBag.IdMesa = idMesa;
            try
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

                // Para solicitudes AJAX devolvemos JSON
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(result);
                }

                // Para solicitudes normales devolvemos la vista
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;

                // Para solicitudes AJAX devolvemos un error JSON
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return StatusCode(500, new { error = ex.Message });
                }

                return View(new List<dynamic>());
            }
        }

        //[Authorize(Roles = "Mesero")]
        public IActionResult CrearPedido()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AgregarPedido([FromForm] PedidoCreacion request)
        {
            if (request?.Pedido == null || request.DetallePedido == null)
            {
                return BadRequest("Datos de pedido incompletos");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Validar mesa
                var mesa = await _context.Mesas.FindAsync(request.Pedido.IdMesa.Value);
                if (mesa == null) return NotFound("Mesa no encontrada.");
                if (mesa.Estado==true) return BadRequest("La mesa ya está ocupada.");

                // 2. Crear pedido SIN especificar IdPedido
                var pedido = new Pedido
                {
                    // NO incluir IdPedido aquí
                    IdMesa = request.Pedido.IdMesa.Value,
                    IdMesero = request.Pedido.EmpleadoID.Value,
                    IdEstadopedido = request.Pedido.IdEstadopedido.Value
                };

                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync(); // Esto generará el ID automáticamente

                // 3. Obtener precio del ítem
                var menuItem = await _context.MenuItems
                    .Include(mi => mi.Platos)
                    .Include(mi => mi.Combo)
                    .FirstOrDefaultAsync(mi => mi.MenuItemId == request.DetallePedido.IdMenu);

                if (menuItem == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest("Ítem del menú no encontrado");
                }

                decimal precio = menuItem.Platos?.Precio ?? menuItem.Combo?.Precio ?? 0;

                // 4. Crear detalle
                var detalle = new DetallePedido
                {
                    IdMenu = request.DetallePedido.IdMenu,
                    DetCantidad = request.DetallePedido.DetCantidad ?? 1,
                    DetPrecio = precio,
                    DetSubtotal = (request.DetallePedido.DetCantidad ?? 1) * precio,
                    DetComentarios = request.DetallePedido.DetComentarios ?? string.Empty,
                    IdEstadopedido = request.DetallePedido.IdEstadopedido.Value
                };

                _context.DetallePedidos.Add(detalle);

                // 5. Actualizar mesa
                mesa.Estado = true;
                _context.Mesas.Update(mesa);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new PedidoResponseDTO
                {
                    IdPedido = pedido.IdPedido,
                    IdMesa = pedido.IdMesa.Value,
                    IdMesero = pedido.IdMesero.Value,
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
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = "Error interno al procesar el pedido" });
            }
        }

        //[Authorize(Roles = "Mesero")]
        public async Task<IActionResult> VerDetallePedido(int idMesa, bool nuevoPedido)
        {
            if (nuevoPedido)
            {
                
            return View("DetallePedidoView",    null);
            }

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

        private const string SessionPedido = "PedidoTemporal";

        // GET
        public IActionResult PreDetallePedido(int idMesa)
        {
            var pedidoTemporal = HttpContext.Session.GetObjectFromJson<List<PedidoTemporalItem>>(SessionPedido) ?? new List<PedidoTemporalItem>();
            ViewBag.IdMesa = idMesa;
            return View(pedidoTemporal);
        }

        // POST para agregar plato (similar para combos/promos)
        [HttpPost]
        public IActionResult AgregarItemTemporal(int idMesa, PedidoTemporalItem item)
        {
            var pedidoTemporal = HttpContext.Session.GetObjectFromJson<List<PedidoTemporalItem>>(SessionPedido) ?? new List<PedidoTemporalItem>();
            pedidoTemporal.Add(item);
            HttpContext.Session.SetObjectAsJson(SessionPedido, pedidoTemporal);
            return RedirectToAction("PreDetallePedido", new { idMesa });
        }

        // POST final: crear pedido y detallePedido real
        [HttpPost]
        public async Task<IActionResult> EnviarPedidoTemporal(int idMesa)
        {
            var pedidoTemporal = HttpContext.Session.GetObjectFromJson<List<PedidoTemporalItem>>(SessionPedido);
            if (pedidoTemporal == null || !pedidoTemporal.Any())
                return RedirectToAction("PreDetallePedido", new { idMesa });

            // Crear pedido, luego detallePedido en la DB...
            // (Podemos ayudarte a armarlo según tu estructura actual)

            HttpContext.Session.Remove(SessionPedido); // Limpiar carrito
            return RedirectToAction("VerDetallePedido", new { idMesa });
        }

        [HttpPost]
        public async Task<IActionResult> EnviarPedido(int idMesa)
        {
            var pedidoTemporal = HttpContext.Session.GetObjectFromJson<List<PedidoTemporalItem>>(SessionPedido);
            if (pedidoTemporal == null || !pedidoTemporal.Any())
            {
                TempData["Error"] = "No hay elementos para enviar.";
                return RedirectToAction("PreDetallePedido", new { idMesa });
            }

            // Crear el Pedido
            var pedido = new Pedido
            {
                IdMesa = idMesa,
                IdEstadopedido = 2, // En Proceso
                IdMesero = HttpContext.Session.GetInt32("IdMesero") ?? 0, // Tu lógica aquí
                                                      // FechaPedido = DateTime.Now, si tienes el campo
            };

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            // Insertar los detalles
            foreach (var item in pedidoTemporal)
            {
                _context.DetallePedidos.Add(new DetallePedido
                {
                    IdPedido = pedido.IdPedido,
                    IdMenu = item.IdMenu,
                    DetCantidad = item.Cantidad,
                    DetPrecio = item.Precio,
                    DetSubtotal = item.Subtotal,
                    DetComentarios = item.Comentarios,
                    IdEstadopedido = 1 // Solicitado
                });
            }

            await _context.SaveChangesAsync();
            HttpContext.Session.Remove(SessionPedido);

            return RedirectToAction("VerDetallePedido", new { idMesa });
        }


        [HttpPost]
        public IActionResult CancelarPreparacion()
        {
            HttpContext.Session.Remove("PedidoTemporal");
            return RedirectToAction("EstadoMesas");
        }
        // Paso 1: Agregar un nuevo método en el controlador MeseroController

        [HttpPost]
        public IActionResult EliminarItemTemporal(int idMenu)
        {
            var pedidoTemporal = HttpContext.Session.GetObjectFromJson<List<PedidoTemporalItem>>(SessionPedido);

            if (pedidoTemporal != null)
            {
                var item = pedidoTemporal.FirstOrDefault(x => x.IdMenu == idMenu);
                if (item != null)
                {
                    pedidoTemporal.Remove(item);
                    HttpContext.Session.SetObjectAsJson(SessionPedido, pedidoTemporal);
                }
            }

            int idMesa = ViewBag.IdMesa ?? 0; // Alternativamente podrías pasarlo como parámetro si lo tienes en el form
            return RedirectToAction("PreDetallePedido", new { idMesa });
        }

    }
}
