#!/bin/bash

# Carrega as variáveis do arquivo .env se ele existir
if [ -f .env ]; then
  echo "Carregando variáveis de ambiente do arquivo .env..."
  export $(grep -v '^#' .env | xargs)
fi

# Defina as variáveis de conexão (ou deixe que leiam das variáveis de ambiente do container)
DB_HOST="${DB_HOST:-postgres}"
DB_PORT="${DB_PORT:-5432}"
DB_USER="${DB_USERNAME:-postgres}"
DB_PASSWORD="${DB_PASSWORD:-postgres}"
DB_NAME="${DB_NAME:-good_hamburguer}"

sleep 2 # Aguarda um pouco para garantir que o banco de dados esteja pronto

echo "Conectando ao banco de dados '$DB_NAME' no host '$DB_HOST'..."

# Exporta a senha para que o psql não exija digitação interativa
export PGPASSWORD="$DB_PASSWORD"

# Executa o psql conectando via rede ao serviço do banco de dados
psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -f schema.sql

if [ $? -eq 0 ]; then
    echo "Migrations (tabelas e dados) aplicadas com sucesso!"
else
    echo "Ocorreu um erro ao tentar rodar o schema no banco de dados."
fi
