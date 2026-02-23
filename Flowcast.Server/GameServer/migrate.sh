#!/bin/bash
set -e

cd "$(dirname "$0")"

echo "🚀 Starting Database Migrations..."

docker compose up -d db

docker compose run --rm --name flowcast-migrator -e MIGRATE_ONLY=true flowcast

echo "✅ Migrations applied successfully!"