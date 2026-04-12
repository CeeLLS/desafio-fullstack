#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
BACKEND_DIR="$ROOT_DIR/backend"

echo "Iniciando PostgreSQL via Docker..."
cd "$SCRIPT_DIR"
docker compose up -d

echo "Aguardando PostgreSQL..."
until docker compose exec postgres pg_isready -U adm -d taskmanager_db > /dev/null 2>&1; do
  sleep 2
done

echo -e "${BLUE}3. Aplicando migrations...${NC}"
cd "$BACKEND_DIR"
dotnet ef database update \
  --project src/TaskManager.Infrastructure \
  --startup-project src/TaskManager.API

echo "✅ Setup concluído."
echo "  Host:     localhost:5432"
echo "  Database: taskmanager_db"
echo "  User:     adm"
echo "  Password: 123"
