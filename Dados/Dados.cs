using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using good_hamburguer.models;
using Npgsql;
using System.Data;
using good_hamburguer.Controllers;


namespace good_hamburguer.dados
{
    public class Conexao
    {
        private readonly IDbConnection _dbConnection;

        public Conexao()
        {
            string connectionString = "Host=" + Environment.GetEnvironmentVariable("DB_HOST") +
            ";Port=" + Environment.GetEnvironmentVariable("DB_PORT") +
            ";Database=" + Environment.GetEnvironmentVariable("DB_NAME") +
            ";Username=" + Environment.GetEnvironmentVariable("DB_USERNAME") +
            ";Password=" + Environment.GetEnvironmentVariable("DB_PASSWORD");
            _dbConnection = new NpgsqlConnection(connectionString);
        }

        private void OpenConnection()
        {
            if (_dbConnection.State == ConnectionState.Closed)
            {
                _dbConnection.Open();
            }
        }

        private void CloseConnection()
        {
            if (_dbConnection.State == ConnectionState.Open)
            {
                _dbConnection.Close();
            }
        }

        private IDbCommand CreateCommand(string query)
        {
            var command = _dbConnection.CreateCommand();
            command.CommandText = query;
            return command;
        }

        public T? ExecuteScalar<T>(string query)
        {
            using (var command = CreateCommand(query))
            {
                OpenConnection();
                var result = command.ExecuteScalar();
                CloseConnection();
                return result != null ? (T)result : default;
            }
        }

        public List<T> GetData<T>(string query) where T : new()
        {
            var itens = new List<T>();
            using (var command = CreateCommand(query))
            {
                OpenConnection();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        T obj = new T();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            // Pega a propriedade correspondente ao nome da coluna ignorando maiúsculas e minúsculas
                            var property = typeof(T).GetProperty(reader.GetName(i), System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                            if (property != null && !reader.IsDBNull(i))
                            {
                                // Converte o valor retornado pelo banco para o tipo da propriedade
                                property.SetValue(obj, Convert.ChangeType(reader.GetValue(i), property.PropertyType));
                            }
                        }
                        itens.Add(obj);
                    }
                }
                CloseConnection();
            }
            return itens;
        }

        public IDbConnection DbConnection => _dbConnection;
    }
    public class Dados
    {
        public List<Item> ObterCardapio()
        {
            var conexao = new Conexao();
            string query = "SELECT id_item AS id, nome_produto AS produto, valor_unitario as valor, categoria FROM itens";

            return conexao.GetData<Item>(query);
        }
        
        public Pedido InserirPedido(Pedido pedido)
        {
            pedido = CalcularDesconto(pedido);

            var conexao = new Conexao();
            string insertPedidoQuery = $"INSERT INTO pedidos (endereco_entrega, numero_celular_cliente, valor_total, desconto, valor_total_sem_desconto, cancelado) VALUES ('{pedido.endereco_entrega}', '{pedido.numero_celular_cliente}', {pedido.valor_total}, {pedido.desconto}, {pedido.valor_total_sem_desconto}, '0') RETURNING id_pedido";
            int idPedido = conexao.ExecuteScalar<int>(insertPedidoQuery);

            if (idPedido == 0)
                throw new Exception("Erro ao inserir pedido");

            pedido.numpedido = idPedido;

            foreach (var item in pedido.itens)
            {
                string insertItemPedidoQuery = $"INSERT INTO item_pedido (id_pedido, id_item) VALUES ({idPedido}, {item.id})";
                conexao.ExecuteScalar<int>(insertItemPedidoQuery);
            }

            return pedido;
        }
    
        public Pedido CalcularDesconto(Pedido pedido)
        {
            double valorTotal = pedido.itens.Sum(i => i.valor);
            pedido.desconto = 0; // É uma boa prática inicializar o desconto.
            pedido.valor_total = valorTotal; // Inicializa o valor total antes de aplicar o desconto.
            pedido.valor_total_sem_desconto = valorTotal; // Inicializa o valor total sem desconto.

            if (pedido.itens.Count < 2)
                return pedido;
            
            if (!pedido.itens.Any(i => i.categoria.ToLower() == "sanduiche"))
                return pedido;
            
            bool batataCheck = pedido.itens.Any(i => i.produto.ToLower().Contains("batata"));
            bool refriCheck = pedido.itens.Any(i => i.produto.ToLower().Contains("refrigerante"));

            if (batataCheck && refriCheck)
                pedido.desconto = 0.2;
            else if (batataCheck)
                pedido.desconto = 0.1;
            else if (refriCheck)
                pedido.desconto = 0.15;

            pedido.valor_total_sem_desconto = valorTotal;
            pedido.valor_total = valorTotal * (1 - pedido.desconto);

            return pedido;
        }
    
        public List<Pedido> ObterPedidos()
        {
            var conexao = new Conexao();
            string query = "SELECT id_pedido AS numpedido, cancelado, endereco_entrega, numero_celular_cliente, valor_total, desconto, valor_total_sem_desconto FROM pedidos WHERE cancelado = '0'";

            List<Pedido> pedidos = conexao.GetData<Pedido>(query);

            foreach (var pedido in pedidos)
                pedido.itens = ObterItensPedido(pedido.numpedido);

            return pedidos;
        }

        public List<Item> ObterItensPedido(int numpedido)
        {
            var conexao = new Conexao();
            string query = $"SELECT i.id_item AS id, i.nome_produto AS produto, i.valor_unitario AS valor FROM itens i JOIN item_pedido ip ON i.id_item = ip.id_item WHERE ip.id_pedido = {numpedido}";

            return conexao.GetData<Item>(query);
        }
    
        public Pedido? ObterPedido(int numpedido)
        {
            var conexao = new Conexao();
            string query = $"SELECT id_pedido AS numpedido, cancelado, endereco_entrega, numero_celular_cliente, valor_total, desconto, valor_total_sem_desconto FROM pedidos WHERE id_pedido = {numpedido}";

            List<Pedido> pedidos = conexao.GetData<Pedido>(query);

            if (pedidos.Count == 0)
                return null;

            Pedido pedido = pedidos[0];
            pedido.itens = ObterItensPedido(pedido.numpedido);

            return pedido;
        }

        public Pedido EditarPedido(Pedido pedido)
        {
            pedido = CalcularDesconto(pedido);

            var conexao = new Conexao();
            string updatePedidoQuery = $"UPDATE pedidos SET endereco_entrega = '{pedido.endereco_entrega}', numero_celular_cliente = '{pedido.numero_celular_cliente}', valor_total = {pedido.valor_total}, desconto = {pedido.desconto}, valor_total_sem_desconto = {pedido.valor_total_sem_desconto} WHERE id_pedido = {pedido.numpedido}";
            conexao.ExecuteScalar<int>(updatePedidoQuery);

            string deleteItensPedidoQuery = $"DELETE FROM item_pedido WHERE id_pedido = {pedido.numpedido}";
            conexao.ExecuteScalar<int>(deleteItensPedidoQuery);

            foreach (var item in pedido.itens)
            {
                string insertItemPedidoQuery = $"INSERT INTO item_pedido (id_pedido, id_item) VALUES ({pedido.numpedido}, {item.id})";
                conexao.ExecuteScalar<int>(insertItemPedidoQuery);
            }

            return pedido;
        }

        public void CancelarPedido(int numpedido)
        {
            var conexao = new Conexao();
            string deletePedidoQuery = $"UPDATE pedidos SET cancelado = '1' WHERE id_pedido = {numpedido}";
            conexao.ExecuteScalar<int>(deletePedidoQuery);
        }
    
        public List<Item> ObterItens()
        {
            var conexao = new Conexao();
            string query = "SELECT id_item AS id, nome_produto AS produto, valor_unitario AS valor, categoria FROM itens";

            return conexao.GetData<Item>(query);
        }
    }
}
