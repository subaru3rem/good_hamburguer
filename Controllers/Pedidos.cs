using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using good_hamburguer.models;
using good_hamburguer.dados;

namespace good_hamburguer.Controllers
{
    [Route("[controller]")]
    public class Pedidos : Controller
    {
        [HttpPost]
        public IActionResult Pedido([FromBody] Pedido pedido)
        {
            if (!VerificaDuplicados(pedido.itens))
                return BadRequest("Items duplicados no pedido");

            var dados = new Dados();
            pedido = dados.InserirPedido(pedido);
            return Ok(pedido);
        }

        [HttpGet]
        public IActionResult ListarPedidos()
        {
            var dados = new Dados();
            var pedidos = dados.ObterPedidos();
            return Ok(pedidos);
        }

        [HttpGet("{id}")]
        public IActionResult ObterPedido(int id)
        {
            var dados = new Dados();
            var pedido = dados.ObterPedido(id);
            return Ok(pedido);
        }

        [HttpDelete("{id}")]
        public IActionResult CancelarPedido(int id)
        {
            var dados = new Dados();
            dados.CancelarPedido(id);
            return Ok();
        }

        [HttpPut("{id}")]
        public IActionResult AlterarPedido([FromBody] Pedido pedido)
        {
            if (!VerificaDuplicados(pedido.itens))
                return BadRequest("Items duplicados no pedido");

            var dados = new Dados();
            pedido = dados.EditarPedido(pedido);
            return Ok(pedido);
        }

        private bool VerificaDuplicados(List<Item> itens)
        {
            var codigos = new List<int>();

            foreach(Item item in itens)
            {
                if (codigos.Contains(item.id))
                    return false;

                codigos.Add(item.id);
            }

            return true;
        }
    }
}
