
# Good Hamburguer API

Sistema de registro de pedidos de uma lanchonete com foco principal na venda de hambúrgueres

Api desenvolvida em .net 6.0, usando banco de dados postgresql e testes com xunit.
## API Reference

#### Get all items

```http
  GET /cardapio
```

Metodo que retorna todos os itens cadastrados no sistema com seus valores e categorias.


#### Post Pedidos

```http
  POST /pedidos
```

| Body      | Type     | Description                       |
| :-------- | :------- | :-------------------------------- |
| `endereco_entrega`      | `string` | **Required**. Endereço onde o pedido deve ser enviado |
| `numero_celular_cliente`| `string` | **Required**. Número de celular do cliente |
| `itens`                 | `objeto` | **Required**. Itens do pedido |

Rota que gera um novo pedido

#### Get pedidos
```
  GET /pedidos
```

Rota que retorna todos os pedidos do sistema

#### Get Pedido

```
  GET /pedidos/{numpedido}
```

Rota que retorna informações sobre um pedido especifico

#### Put Pedido

```
  PUT /pedidos/{numpedido}
```
Rota que faz alterações no pedido

####  DELETE /pedidos/{numpedido}

```
  DELETE /pedidos/{numpedido}
```

Rota que deleta um pedido

## Deployment

#### Rodando localmente

Deve se clonar esse projeto na sua maquina local.

```bash
  git clone https://github.com/subaru3rem/good_hamburguer.git
```

Entrar na pasta do projeto

```bash
  cd good_hamburger
```

Restaurar o projeto

```bash
  dotnet restore
```

Alterar as variaveis de ambiente para sua conexão no postgres e rodar o migration.sh

```bash
  ./migration.sh
```

Rodar o projeto

```bash
  dotnet run
```


#### Rodando com docker


Deve se clonar esse projeto na sua maquina local.

```bash
  git clone https://github.com/subaru3rem/good_hamburguer.git
```

Entrar na pasta do projeto

```bash
  cd good_hamburger
```

Alterar as variaveis de ambiente para sua conexão no postgres

Subir o docker compose

```bash
  docker compose up
```



## Running Tests

Para rodar os testes basta fazer o processo de de criação do banco de dados como descrido no rodar localmente e em seguidar rodar os testes no .net local

```bash
  dotnet test
```


## Features

- Crud de pedidos completo
- Espaço para expansão da api para gerencia de cardapio 
- Docker para subir facilmente com um db ou sem o dotnet na maquina
## Observation

Para essa api eu decidi deixar uma estrutura um pouco aberta para novos itens e de facil alteração para clientes e outros dados adicionais, com os dados em banco de dados, basta alguma alterações e a estrutura pode seguir de forma parecida, porém as regras de negocios são em codigo para apenas com atenção e gerencia da equipe de desenvolvimento possam ser alteradas tanto em logica quanto em valores.

Decidi implementar o docker e deixar maneira mais proxima ao que subiria para o ambiente de produção caso a api fosse ser realmente usada.

Tive um pouco de demora e dificuldade para a implementação dos testes com a ferramenta nativa do dotnet 6.0, o que me deixou impossibilitado de aprender para desenvolver um front-end em Blazor que é outra ferramenta que não tenho dominio, acredito que caso fosse um front-end em react ou jquery simples, eu poderia ter entregue com um pouco mais de tempo.
