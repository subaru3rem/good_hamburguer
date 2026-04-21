using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace good_hamburguer.models
{
    public class Pedido
    {
        public int numpedido {get; set;}
        public string endereco_entrega {get; set;}
        public string numero_celular_cliente {get; set;}
        public double valor_total {get; set;}
        public double desconto {get; set;}
        public double valor_total_sem_desconto {get; set;}
        public List<Item> itens {get; set;}

        public Pedido()
        {
            this.numpedido = 0;
            this.endereco_entrega = "";
            this.numero_celular_cliente = "";
            this.itens = new List<Item>();
        }
    }

    public class Item
    {
        public int id {get; set;}
        public string produto {get; set;}
        public double valor {get; set;}
        public string categoria {get; set;}

        public Item()
        {
            this.id = 0;
            this.produto = "";
            this.valor = 0;
            this.categoria = "";

        }
    }
}
