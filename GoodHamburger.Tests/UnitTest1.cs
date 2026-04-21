using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class PedidoCrudTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PedidoCrudTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DeveExecutarFluxoCompletoDoPedido()
    {
        // --- 1. CREATE (POST) ---
        var novoPedido = new { Itens = new List<string> { "X Burger", "Batata frita" } }; // 5.00 + 2.00 = 7.00
        
        var postResponse = await _client.PostAsJsonAsync("/api/pedidos", novoPedido);
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var pedidoCriado = await postResponse.Content.ReadFromJsonAsync<PedidoDto>();
        pedidoCriado.TotalFinal.Should().Be(6.30m); // 10% de desconto aplicado
        var id = pedidoCriado.Id;

        // --- 2. READ (GET BY ID) ---
        var getResponse = await _client.GetAsync($"/api/pedidos/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var pedidoConsultado = await getResponse.Content.ReadFromJsonAsync<PedidoDto>();
        pedidoConsultado.Itens.Should().HaveCount(2);

        // --- 3. UPDATE (PUT) - Alterando para combo total ---
        // Adicionando Refrigerante para subir o desconto para 20%
        var pedidoAtualizado = new { 
            Itens = new List<string> { "X Burger", "Batata frita", "Refrigerante" } 
        };
        
        var putResponse = await _client.PutAsJsonAsync($"/api/pedidos/{id}", pedidoAtualizado);
        putResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Validar se o desconto foi recalculado após o update
        var getPosUpdate = await _client.GetFromJsonAsync<PedidoDto>($"/api/pedidos/{id}");
        getPosUpdate.TotalFinal.Should().Be(7.60m); // (5+2+2.5) * 0.8 = 7.60

        // --- 4. DELETE ---
        var deleteResponse = await _client.DeleteAsync($"/api/pedidos/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Confirmar que não existe mais
        var getPosDelete = await _client.GetAsync($"/api/pedidos/{id}");
        getPosDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("Item Inexistente")]
    [InlineData("")]
    public async Task Post_DeveRetornarBadRequest_ParaItensInvalidos(string itemInvalido)
    {
        var pedidoInvalido = new { Itens = new List<string> { itemInvalido } };
        
        var response = await _client.PostAsJsonAsync("/api/pedidos", pedidoInvalido);
        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ListarTodos_DeveRetornarListaDePedidos()
    {
        // Arrange (Garantir que existe ao menos um)
        await _client.PostAsJsonAsync("/api/pedidos", new { Itens = new List<string> { "X Egg" } });

        // Act
        var response = await _client.GetAsync("/api/pedidos");
        var lista = await response.Content.ReadFromJsonAsync<List<PedidoDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        lista.Should().NotBeEmpty();
    }
}

// Data Transfer Object para os testes
public class PedidoDto {
    public int Id { get; set; }
    public List<string> Itens { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal TotalFinal { get; set; }
}
