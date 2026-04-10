#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
BACKEND_DIR="$ROOT_DIR/backend"

GREEN='\033[0;32m'
BLUE='\033[0;34m'
RED='\033[0;31m'
NC='\033[0m'

echo -e "${BLUE}1. Iniciando PostgreSQL via Docker...${NC}"
cd "$SCRIPT_DIR"
docker compose up -d

echo -e "${BLUE}2. Aguardando PostgreSQL ficar pronto...${NC}"
until docker compose exec postgres pg_isready -U adm -d taskmanager_db > /dev/null 2>&1; do
  echo "   aguardando..."
  sleep 2
done
echo -e "${GREEN}   PostgreSQL pronto.${NC}"

echo -e "${BLUE}3. Aplicando migrations...${NC}"
cd "$BACKEND_DIR"
dotnet ef database update \
  --project src/TaskManager.Infrastructure \
  --startup-project src/TaskManager.API

echo ""
echo -e "${GREEN}✅ Setup concluído.${NC}"
echo ""
echo "Conexão:"
echo "  Host:     localhost:5432"
echo "  Database: taskmanager_db"
echo "  User:     adm"
echo "  Password: 123"
echo ""
echo "Comandos úteis:"
echo "  Parar banco:    docker compose down          (em db/)"
echo "  Logs:           docker compose logs postgres"
echo "  Nova migration: dotnet ef migrations add <Nome> --project src/TaskManager.Infrastructure --startup-project src/TaskManager.API"