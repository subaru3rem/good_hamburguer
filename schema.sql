-- Criação da tabela de itens (produtos do cardápio)
CREATE TABLE
	itens (
		id_item SERIAL PRIMARY KEY,
		nome_produto VARCHAR(100) NOT NULL,
		valor_unitario DECIMAL(10, 2) NOT NULL,
		categoria VARCHAR(50) NOT NULL
	);

-- Criação da tabela de pedidos
CREATE TABLE
	pedidos (
		id_pedido SERIAL PRIMARY KEY,
		data_criacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
		endereco_entrega VARCHAR(255) NOT NULL,
		numero_celular_cliente VARCHAR(20) NOT NULL,
		valor_total DECIMAL(10, 2) NOT NULL,
		cancelado BOOLEAN DEFAULT FALSE,
		desconto DECIMAL(10, 2) NOT NULL,
		valor_total_sem_desconto DECIMAL(10, 2) NOT NULL
	);

-- Criação da tabela de relacionamento entre pedidos e itens
CREATE TABLE
	item_pedido (
		id_pedido INT,
		id_item INT,
		PRIMARY KEY (id_pedido, id_item),
		FOREIGN KEY (id_pedido) REFERENCES pedidos (id_pedido) ON DELETE CASCADE,
		FOREIGN KEY (id_item) REFERENCES itens (id_item) ON DELETE RESTRICT
	);

insert into
	public.itens (nome_produto, valor_unitario, categoria)
values
	('X Burger', 5, 'Sanduiche'),
	('X Egg', 4.5, 'Sanduiche'),
	('X Bacon', 7, 'Sanduiche'),
	('Batata frita', 2, 'Acompanhamento'),
	('Refrigerante', 2.5, 'Acompanhamento');
