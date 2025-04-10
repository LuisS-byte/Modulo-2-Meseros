﻿using Microsoft.AspNetCore.Authorization;
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
    
    public class MeseroController : Controller
    {
        private readonly AppDbContext _context;

        public MeseroController(AppDbContext context)
        {
            _context = context;
        }

      
        

       
        [HttpPost]
        public async Task<IActionResult> CambiarEstadoMesa(int id)
        {
            var mesa = await _context.Mesas.FirstOrDefaultAsync(x => x.MesaId == id);
            if (mesa == null) return NotFound();

            mesa.Estado = !mesa.Estado;
            await _context.SaveChangesAsync();

            return RedirectToAction("EstadoMesas");
        }

       
        public async Task<IActionResult> VisualizarMenuOnlyPlatos(int idMesa, bool? esNuevo)
        {
            ViewBag.IdMesa = idMesa;
            ViewBag.EsNuevo = esNuevo;
            try
            {
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
                ViewBag.ErrorMessage = ex.Message;
                return View(new List<dynamic>());
            }
        }

        
        public async Task<IActionResult> VisualizarMenuOnlyCombos(int idMesa, bool? esNuevo)
        {
            ViewBag.IdMesa = idMesa;
            ViewBag.EsNuevo = esNuevo;
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



        [HttpGet]
        public async Task<IActionResult> VerDetallePedido(int idMesa, bool nuevoPedido)
        {
            if (nuevoPedido)
            {

                return View("DetallePedidoView", null);
            }

            var pedidoActivo = await _context.Pedidos
            .Where(p => p.IdMesa == idMesa && (p.IdEstadopedido == 2))
            .OrderByDescending(p => p.IdPedido)
            .FirstOrDefaultAsync();
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


        public async Task<IActionResult> VisualizarMenuOnlyPromociones(int idMesa, bool? esNuevo)
        {
            ViewBag.IdMesa = idMesa;
            ViewBag.EsNuevo = esNuevo;
            try
            {
                var currentDate = DateOnly.FromDateTime(DateTime.Now);

                var promociones = await _context.Promociones
                    .Where(p => p.FechaInicio <= currentDate && p.FechaFin >= currentDate)
                    .ToListAsync();

                var result = new List<object>();

                foreach (var promocion in promociones)
                {
                    var items = new List<object>();

                    var platosEnPromocion = await _context.PromocionesItems
                        .Where(pi => pi.PromocionId == promocion.PromocionId)
                        .Join(_context.Platos,
                              pi => pi.PlatoId,
                              p => p.PlatoId,
                              (pi, p) => new { PromocionItem = pi, Plato = p })
                        .Where(x => x.Plato != null)
                        .Select(x => new
                        {
                            Tipo = "Plato",
                            ID = x.Plato.PlatoId,
                            Nombre = x.Plato.Nombre,
                            PrecioOriginal = x.Plato.Precio,
                            PrecioConDescuento = x.Plato.Precio * (1 - (promocion.Descuento / 100m)),
                            PromocionId = promocion.PromocionId
                        })
                        .ToListAsync();

                    items.AddRange(platosEnPromocion);

                    var combosEnPromocion = await _context.PromocionesItems
                        .Where(pi => pi.PromocionId == promocion.PromocionId)
                        .Join(_context.Combos,
                              pi => pi.ComboId,
                              c => c.ComboId,
                              (pi, c) => new { PromocionItem = pi, Combo = c })
                        .Where(x => x.Combo != null)
                        .Select(x => new
                        {
                            Tipo = "Combo",
                            ID = x.Combo.ComboId,
                            Nombre = x.Combo.Nombre,
                            PrecioOriginal = x.Combo.Precio,
                            PrecioConDescuento = x.Combo.Precio * (1 - (promocion.Descuento / 100m)),
                            PromocionId = promocion.PromocionId
                        })
                        .ToListAsync();

                    items.AddRange(combosEnPromocion);

                    if (items.Any())
                    {
                        result.Add(new
                        {
                            PromocionID = promocion.PromocionId,
                            Descripcion = promocion.Descripcion,
                            Descuento = promocion.Descuento,
                            FechaInicio = promocion.FechaInicio,
                            FechaFin = promocion.FechaFin,
                            Items = items
                        });
                    }
                }

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(result);
                }

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
                }

                return View(new List<dynamic>());
            }
        }














        public async Task<IActionResult> EstadoMesas()
        {
            var mesas = await _context.Mesas.ToListAsync();
            return View(mesas);
        }



        [HttpPost]
        public async Task<IActionResult> AgregarPedido([FromForm] PedidoDTO request)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var mesa = await _context.Mesas.FindAsync(request.IdMesa.Value);
                if (mesa == null) return NotFound("Mesa no encontrada.");

                if (mesa.Estado == false)
                    return BadRequest("La mesa ya está ocupada.");

                mesa.Estado = false;

                var pedido = new Pedido
                {
                    IdMesa = request.IdMesa.Value,
                    IdMesero = 1,
                    IdEstadopedido = 2
                };

                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync();

                var itemsTemporales = HttpContext.Session.GetObjectFromJson<List<PedidoTemporalItem>>(SessionPedido);
                if (itemsTemporales == null || !itemsTemporales.Any())
                {
                    TempData["Error"] = "No hay productos en el pedido temporal.";
                    return RedirectToAction("PreDetallePedido", new { idMesa = request.IdMesa });
                }

                decimal precio = 0;
                foreach (var item in itemsTemporales)
                {
                    var menuItem = await _context.MenuItems
                        .FirstOrDefaultAsync(mi => mi.MenuItemId == item.MenuItemId);

                    if (menuItem == null)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest($"El MenuItemId {item.MenuItemId} no existe.");
                    }

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
                        IdMenu = item.MenuItemId,
                        DetCantidad = item.Cantidad,
                        DetPrecio = precio,
                        DetSubtotal = item.Cantidad * precio,
                        DetComentarios = item.Comentarios ?? string.Empty,
                        IdEstadopedido = item.IdEstadoPedido
                    };

                    _context.DetallePedidos.Add(detalle);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction("EstadoMesas");


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


        private async Task<decimal> ObtenerPrecioMenuItem(int menuItemId)
        {
            var menuItem = await _context.MenuItems
                .Include(mi => mi.Platos)
                .Include(mi => mi.Combo)
                .Include(mi => mi.Promocion)
                .FirstOrDefaultAsync(mi => mi.MenuItemId == menuItemId);

            if (menuItem?.Platos != null) return menuItem.Platos.Precio;
            if (menuItem?.Combo != null) return menuItem.Combo.Precio;
            if (menuItem?.Promocion != null) return menuItem.Promocion.Descuento;

            return 0;
        }



        
        [HttpPost]
        public async Task<IActionResult> AgregarDetallePedido(int idMesa)
        {
            var pedidoExistente = await _context.Pedidos
                .FirstOrDefaultAsync(p => p.IdMesa == idMesa && p.IdEstadopedido == 2);

            if (pedidoExistente == null)
            {
                TempData["Error"] = "No se encontró un pedido activo para esta mesa.";
                return RedirectToAction("PreDetallePedido", new { idMesa });
            }

            var itemsTemporales = HttpContext.Session.GetObjectFromJson<List<PedidoTemporalItem>>(SessionPedido);
            if (itemsTemporales == null || !itemsTemporales.Any())
            {
                TempData["Error"] = "No hay productos en el pedido temporal.";
                return RedirectToAction("PreDetallePedido", new { idMesa });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in itemsTemporales)
                {
                    var menuItem = await _context.MenuItems
                        .Include(mi => mi.Platos)
                        .Include(mi => mi.Combo)
                        .Include(mi => mi.Promocion)
                        .FirstOrDefaultAsync(mi => mi.MenuItemId == item.MenuItemId);

                    if (menuItem == null) continue;

                    decimal precio = 0;
                    if (menuItem.Platos != null)
                    {
                        precio = menuItem.Platos.Precio;
                    }
                    else if (menuItem.Combo != null)
                    {
                        precio = menuItem.Combo.Precio;
                    }
                    else if (menuItem.Promocion != null)
                    {
                        precio = menuItem.Promocion.Descuento;
                    }

                    var detalleExistente = await _context.DetallePedidos
                        .FirstOrDefaultAsync(d =>
                            d.IdPedido == pedidoExistente.IdPedido &&
                            d.IdMenu == item.MenuItemId &&
                            d.IdEstadopedido == 1);

                    if (detalleExistente != null)
                    {
                        detalleExistente.DetCantidad += item.Cantidad;
                        detalleExistente.DetSubtotal = detalleExistente.DetCantidad * precio;

                        if (!string.IsNullOrEmpty(item.Comentarios))
                        {
                            detalleExistente.DetComentarios = item.Comentarios;
                        }
                    }
                    else
                    {
                        var nuevoDetalle = new DetallePedido
                        {
                            IdPedido = pedidoExistente.IdPedido,
                            IdMenu = item.MenuItemId,
                            DetCantidad = item.Cantidad,
                            DetPrecio = item.Precio,
                            DetSubtotal = item.Cantidad * precio,
                            DetComentarios = item.Comentarios ?? string.Empty,
                            IdEstadopedido = 1
                        };
                        _context.DetallePedidos.Add(nuevoDetalle);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                HttpContext.Session.Remove(SessionPedido);
                TempData["Success"] = "¡Ítems agregados al pedido exitosamente!";
                return RedirectToAction("VerDetallePedido", new { idMesa });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Error al procesar el pedido: {ex.Message}";
                return RedirectToAction("PreDetallePedido", new { idMesa });
            }
        }


     
        [HttpPost]
        public async Task<IActionResult> CambiarEstadoDetallePedido(int idDetallePedido, int idMenu, int IdEstadoDetallePedido)
        {
            var detalle = await _context.DetallePedidos
                 .Include(dp => dp.IdPedidoNavigation)
                 .FirstOrDefaultAsync(dp => dp.IdPedido == idDetallePedido && dp.IdMenu == idMenu);



            if (detalle == null)
                return NotFound();

            if (IdEstadoDetallePedido == 5 &&
                (detalle.IdEstadopedido == 2 || detalle.IdEstadopedido == 3))
            {
                TempData["Error"] = "No se puede cancelar un plato en proceso o finalizado.";
                return RedirectToAction("VerDetallePedido", new { idMesa = detalle.IdPedidoNavigation.IdMesa });
            }

            detalle.IdEstadopedido = IdEstadoDetallePedido;
            await _context.SaveChangesAsync();

            return RedirectToAction("VerDetallePedido", new { idMesa = detalle.IdPedidoNavigation.IdMesa });
        }

        private const string SessionPedido = "PedidoTemporal";

        
        public IActionResult PreDetallePedido(int idMesa, bool? esNuevo)
        {
            var pedidoTemporal = HttpContext.Session.GetObjectFromJson<List<PedidoTemporalItem>>(SessionPedido) ?? new List<PedidoTemporalItem>();
            ViewBag.IdMesa = idMesa;
            ViewBag.EsNuevo = esNuevo;
            return View(pedidoTemporal);
        }

        [HttpPost]
        public IActionResult AgregarItemTemporal(int idMesa, bool? esNuevo, PedidoTemporalItem item)
        {
            if (item.Precio >= 10) 
            {
                item.Precio /= 100m; 
            }
            if (item.PlatoId != 0)
            {
                var menuitens = _context.MenuItems.Where(x => x.PlatoId == item.PlatoId).ToList();
                item.MenuItemId = menuitens.FirstOrDefault().MenuItemId;
            }
            if (item.ComboId != 0)
            {
                var menuitens = _context.MenuItems.Where(x => x.ComboId == item.ComboId).ToList();
                item.MenuItemId = menuitens.FirstOrDefault().MenuItemId;
            }
            if (item.PromocionId != 0)
            {
                if (item.TipoItem == "Plato")
                {
                    var menuitens = _context.MenuItems.Where(x => x.PlatoId == item.PromocionId).ToList();
                    item.MenuItemId = menuitens.FirstOrDefault().MenuItemId;
                }
                else if (item.TipoItem == "Combo")
                {
                    var menuitens = _context.MenuItems.Where(x => x.ComboId == item.PromocionId).ToList();
                    item.MenuItemId = menuitens.FirstOrDefault().MenuItemId;
                }

            }
            var pedidoTemporal = HttpContext.Session.GetObjectFromJson<List<PedidoTemporalItem>>(SessionPedido) ?? new List<PedidoTemporalItem>();
            pedidoTemporal.Add(item);
            HttpContext.Session.SetObjectAsJson(SessionPedido, pedidoTemporal);
            return RedirectToAction("PreDetallePedido", new { idMesa, esNuevo });
        }

        
        

        [HttpPost]
        public IActionResult CancelarPreparacion()
        {
            HttpContext.Session.Remove("PedidoTemporal");
            return RedirectToAction("EstadoMesas");
        }
       

        [HttpPost]
        public IActionResult EliminarItemTemporal(int idMenu)
        {
            var pedidoTemporal = HttpContext.Session.GetObjectFromJson<List<PedidoTemporalItem>>(SessionPedido);

            if (pedidoTemporal != null)
            {
                var item = pedidoTemporal.FirstOrDefault(x => x.MenuItemId == idMenu);
                if (item != null)
                {
                    pedidoTemporal.Remove(item);
                    HttpContext.Session.SetObjectAsJson(SessionPedido, pedidoTemporal);
                }
            }

            int idMesa = ViewBag.IdMesa ?? 0; 
            return RedirectToAction("PreDetallePedido", new { idMesa });
        }

        [HttpPost]
        public async Task<IActionResult> FinalizarOrden(int idMesa)
        {
            var pedido = await _context.Pedidos
                .Where(p => p.IdMesa == idMesa && p.IdEstadopedido == 2)
                .OrderByDescending(p => p.IdPedido)
                .FirstOrDefaultAsync();

            if (pedido == null)
            {
                TempData["Error"] = "No se encontró un pedido activo para esta mesa.";
                return RedirectToAction("EstadoMesas");
            }

            var detalles = await _context.DetallePedidos
                .Where(dp => dp.IdPedido == pedido.IdPedido)
                .ToListAsync();

            bool puedeFinalizar = detalles.All(dp =>
                dp.IdEstadopedido == 4 || dp.IdEstadopedido == 5);

            if (!puedeFinalizar)
            {
                TempData["Error"] = "La orden no puede ser finalizada. Hay platos aún pendientes.";
                return RedirectToAction("VerDetallePedido", new { idMesa });
            }

            pedido.IdEstadopedido = 3;

            var mesa = await _context.Mesas.FindAsync(idMesa);
            if (mesa != null)
                mesa.Estado = true;

            await _context.SaveChangesAsync();

            TempData["Success"] = "La orden fue finalizada correctamente.";
            return RedirectToAction("EstadoMesas");
        }

        [HttpPost]
        public async Task<IActionResult> EditarDetallePedido(DetallePeiddoDTOEd dto)
        {
            var detalle = await _context.DetallePedidos
             .Include(d => d.IdPedidoNavigation)
            .FirstOrDefaultAsync(d => d.IdPedido == dto.IdPedido && d.IdMenu == dto.IdMenu);


            if (detalle == null)
                return NotFound();

            detalle.DetCantidad = dto.DetCantidad;
            detalle.DetComentarios = dto.DetComentarios;
            detalle.DetSubtotal = detalle.DetCantidad * detalle.DetPrecio;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Detalle actualizado correctamente.";
            return RedirectToAction("VerDetallePedido", new { idMesa = detalle.IdPedidoNavigation.IdMesa });
        }

    }
}
