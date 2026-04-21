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
    public class Cardapio : Controller
    {
        [HttpGet]
        public IActionResult Itens()
        {
            var dados = new Dados();
            var itens = dados.ObterItens();
            return Ok(itens);
        }
    }
}
