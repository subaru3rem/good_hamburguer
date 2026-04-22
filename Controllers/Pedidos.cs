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
        {;
            if (pedido == null)
                return BadRequest("O pedido deve ser fornecido no corpo da requisição");
            if (!VerificaDuplicados(pedido.itens))
                return BadRequest("Items duplicados no pedido");

            if (!VerificaItensExistentes(pedido.itens))
                return BadRequest("Items inexistentes no pedido");

            if (!VerificaConsistenciaItens(pedido))
                return BadRequest("Inconsistência entre os itens do pedido e o cardápio");

            var dados = new Dados();
            pedido = dados.InserirPedido(pedido);
            if (pedido == null)
                return BadRequest("Erro ao inserir pedido");
            return StatusCode(201, pedido);
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
            if (pedido == null)
                return NotFound("Pedido não encontrado");
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
        public IActionResult AlterarPedido([FromBody] Pedido pedido, int id)
        {
            if (!VerificaDuplicados(pedido.itens))
                return BadRequest("Items duplicados no pedido");

            if (!VerificaItensExistentes(pedido.itens))
                return BadRequest("Items inexistentes no pedido");

            if (!VerificaConsistenciaItens(pedido))
                return BadRequest("Inconsistência entre os itens do pedido e o cardápio");

            pedido.numpedido = id;
            var dados = new Dados();
            pedido = dados.EditarPedido(pedido);
            return Ok(pedido);
        }

        private bool VerificaDuplicados(List<Item> itens)
        {
            var codigos = new List<int>();

            foreach (Item item in itens)
            {
                if (codigos.Contains(item.id))
                    return false;

                codigos.Add(item.id);
            }

            return true;
        }

        private bool VerificaItensExistentes(List<Item> itens)
        {
            var dados = new Dados();
            var cardapio = dados.ObterCardapio();

            foreach (var item in itens)
            {
                if (!cardapio.Any(i => i.id == item.id))
                    return false;
            }

            return true;
        }

        private bool VerificaConsistenciaItens(Pedido pedido)
        {
            var dados = new Dados();
            var cardapio = dados.ObterCardapio();

            foreach (var item in pedido.itens)
            {
                if (!cardapio.Any(i => i.produto == item.produto && i.valor == item.valor && i.categoria == item.categoria))
                    return false;
            }

            return true;
        }
    }
}
