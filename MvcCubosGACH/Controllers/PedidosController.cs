using Microsoft.AspNetCore.Mvc;
using MvcCubosGACH.Filters;
using MvcCubosGACH.Models;
using MvcCubosGACH.Services;

namespace MvcCubosGACH.Controllers {
    public class PedidosController : Controller {

        private ServiceCubos service;

        public PedidosController(ServiceCubos service) {
            this.service = service;
        }

        [AuthorizeUsers]
        public async Task<IActionResult> GetPedidos() {
            List<Pedido> pedidos = await this.service.GetPedidos();
            return View(pedidos);
        }

        [AuthorizeUsers]
        public async Task<IActionResult> InsertPedido(int idCubo) {
            await this.service.InsertPedidoAsync(idCubo);
            return RedirectToAction("GetPedidos");
        }

    }
}
