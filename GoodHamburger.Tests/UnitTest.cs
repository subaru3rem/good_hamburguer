using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using good_hamburguer.models;
using Xunit;

public class PedidoCrudTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PedidoCrudTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
            {
                builder.UseContentRoot(Directory.GetCurrentDirectory());
            }).CreateClient();
    }

    [Fact(DisplayName = "Deve executar o fluxo completo de CRUD de um pedido (Criar, Consultar, Atualizar, Deletar)")]
    public async Task DeveExecutarFluxoCompletoDoPedido()
    {
        var novoPedido = new Pedido{
            endereco_entrega = "Rua Teste, 123",
            numero_celular_cliente = "11999999999",
            itens = new List<Item> {
                new Item { id = 2, produto = "X Burger", valor = 5.00, categoria = "Sanduiche" },
                new Item { id = 5, produto = "Batata frita", valor = 2.00, categoria = "Acompanhamento" },
            }
        };

        var postResponse = await _client.PostAsJsonAsync("/pedidos", novoPedido);
        var postContent = await postResponse.Content.ReadAsStringAsync();
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created, $"porque o pedido enviado contém itens válidos e deve ser criado com sucesso. Resposta da API: {postContent}");

        var pedidoCriado = await postResponse.Content.ReadFromJsonAsync<Pedido>();
        pedidoCriado?.valor_total.Should().Be(6.30, "porque o total precisa incluir os descontos corretos baseados nos itens escolhidos"); 

        var id = pedidoCriado?.numpedido;        
        var getResponse = await _client.GetAsync($"/pedidos/{id}");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK, $"porque o pedido foi criado anteriormente. Resposta da API: {getContent}");

        var pedidoConsultado = await getResponse.Content.ReadFromJsonAsync<Pedido>();
        pedidoConsultado?.itens.Should().HaveCount(2, "porque foram inseridos exatamente dois itens na criação original do pedido");


        var pedidoAtualizado = new Pedido{
            endereco_entrega = "Rua Teste, 123",
            numero_celular_cliente = "11999999999",
            itens = new List<Item> {
                new Item { id = 2, produto = "X Burger", valor = 5.00, categoria = "Sanduiche" },
                new Item { id = 5, produto = "Batata frita", valor = 2.00, categoria = "Acompanhamento" },
                new Item { id = 6, produto = "Refrigerante", valor = 2.50, categoria = "Acompanhamento" }
            }
        };
        var putResponse = await _client.PutAsJsonAsync($"/pedidos/{id}", pedidoAtualizado);
        var putContent = await putResponse.Content.ReadAsStringAsync();
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK, $"porque a atualização deve ser aceita. Resposta da API: {putContent}");

        var getPosUpdate = await _client.GetFromJsonAsync<Pedido>($"/pedidos/{id}");
        getPosUpdate?.valor_total.Should().Be(7.60); 

        var deleteResponse = await _client.DeleteAsync($"/pedidos/{id}");
        var deleteContent = await deleteResponse.Content.ReadAsStringAsync();
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK, $"porque a exclusão deve ser processada. Resposta da API: {deleteContent}");

        var getPosDelete = await _client.GetAsync($"/pedidos/");
        var getPosDeleteContent = await getPosDelete.Content.ReadAsStringAsync();
        getPosDelete.Content.Should().NotBe(id.ToString(), $"porque o pedido deletado não deve mais ser retornado. Resposta da API: {getPosDeleteContent}");
    }

    [Fact(DisplayName = "POST /pedidos deve retornar BadRequest (400) quando o pedido tiver um item inexistente")]
    public async Task Post_DeveRetornarBadRequest_ParaItensInvalidos()
    {
        var pedidoInvalido = new Pedido
        {
            endereco_entrega = "Rua Teste, 123",
            numero_celular_cliente = "11999999999",
            itens = new List<Item> {
                new Item { id = 55, produto = "Item Inexistente", valor = 5.00 }
            }
        };

        var response = await _client.PostAsJsonAsync("/pedidos", pedidoInvalido);
        var errorContent = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, $"porque o ID do item fornecido não existe. Resposta da API: {errorContent}");
    }

    [Fact(DisplayName = "GET /pedidos deve retornar com sucesso uma lista com todos os pedidos")]
    public async Task ListarTodos_DeveRetornarListaDePedidos()
    {
        // Arrange (Garantir que existe ao menos um)
        await _client.PostAsJsonAsync("/pedidos", new { Itens = new List<string> { "X Egg" } });

        // Act
        var response = await _client.GetAsync("/pedidos");
        var responseContent = await response.Content.ReadAsStringAsync();
        var lista = await response.Content.ReadFromJsonAsync<List<Pedido>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, $"porque nós inserimos pelo menos um pedido. Resposta da API: {responseContent}");
        lista.Should().NotBeEmpty("porque nós inserimos pelo menos um pedido na etapa de Arrange");
    }

    [Fact(DisplayName = "GET /cardapio deve retornar os itens do cardápio disponíveis para venda")]
    public async Task ListarItems()
    {
        var getCardapioResponse = await _client.GetAsync("/cardapio");
        getCardapioResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var cardapio = await getCardapioResponse.Content.ReadFromJsonAsync<List<Item>>();
        cardapio.Should().NotBeNull().And.HaveCountGreaterThan(0);
    }

    [Fact(DisplayName = "POST /pedidos deve retornar BadRequest (400) quando houver itens duplicados")]
    public async Task Post_DeveRetornarBadRequest_ParaItensDuplicados()
    {
        var pedidoDuplicado = new Pedido
        {
            endereco_entrega = "Rua Teste, 123",
            numero_celular_cliente = "11999999999",
            itens = new List<Item> {
                new Item { id = 2, produto = "X Burger", valor = 5.00, categoria = "Sanduiche" },
                new Item { id = 2, produto = "X Burger", valor = 5.00, categoria = "Sanduiche" }
            }
        };

        var response = await _client.PostAsJsonAsync("/pedidos", pedidoDuplicado);
        var errorContent = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, $"porque não é permitido enviar itens com o mesmo ID. Resposta: {errorContent}");
        errorContent.Should().Contain("Items duplicados no pedido");
    }

    [Fact(DisplayName = "POST /pedidos deve retornar BadRequest (400) quando o item tiver dados inconsistentes com o cardápio")]
    public async Task Post_DeveRetornarBadRequest_ParaItensInconsistentes()
    {
        var pedidoInconsistente = new Pedido
        {
            endereco_entrega = "Rua Teste, 123",
            numero_celular_cliente = "11999999999",
            itens = new List<Item> {
                // O item 2 é um X Burger que custa 5.00. Vamos enviar com valor errado (10.00).
                new Item { id = 2, produto = "X Burger", valor = 10.00, categoria = "Sanduiche" }
            }
        };

        var response = await _client.PostAsJsonAsync("/pedidos", pedidoInconsistente);
        var errorContent = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, $"porque o valor do item não corresponde ao cadastrado no cardápio. Resposta: {errorContent}");
        errorContent.Should().Contain("Inconsistência entre os itens do pedido e o cardápio");
    }

    [Fact(DisplayName = "PUT /pedidos/{id} deve retornar BadRequest (400) para atualização com itens inexistentes")]
    public async Task Put_DeveRetornarBadRequest_ParaItensInexistentes()
    {
        var pedidoInvalido = new Pedido
        {
            endereco_entrega = "Rua Teste, 123",
            numero_celular_cliente = "11999999999",
            itens = new List<Item> {
                new Item { id = 999, produto = "Falso", valor = 0, categoria = "Nenhuma" }
            }
        };
        
        // Usamos um ID qualquer na URL (ex: 1), pois a validação dos itens ocorre antes de olhar para o banco
        var response = await _client.PutAsJsonAsync("/pedidos/1", pedidoInvalido);
        var errorContent = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, $"porque o item 999 não existe no cardápio. Resposta: {errorContent}");
        errorContent.Should().Contain("Items inexistentes no pedido");
    }

    [Fact(DisplayName = "GET /pedidos/{id} deve retornar erro (500) para pedido inexistente devido a falta de tratamento NotFound")]
    public async Task Get_DeveRetornarErro_ParaPedidoInexistente()
    {
        var response = await _client.GetAsync("/pedidos/999999"); // Um ID que provavelmente não existe
        var errorContent = await response.Content.ReadAsStringAsync();
        
        // A API lança `throw new Exception("Pedido não encontrado")` e o ASP.NET converte isso num erro 500 genérico.
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, $"Api lança um notfound quando o pedido não existe. Resposta: {errorContent}");
    }
}
