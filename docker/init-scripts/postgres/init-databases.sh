#!/bin/bash
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    -- Create databases for each service
    CREATE DATABASE "UserDb";
    CREATE DATABASE "OrderDb";
    CREATE DATABASE "PaymentDb";
    
    -- Grant privileges
    GRANT ALL PRIVILEGES ON DATABASE "UserDb" TO postgres;
    GRANT ALL PRIVILEGES ON DATABASE "OrderDb" TO postgres;
    GRANT ALL PRIVILEGES ON DATABASE "PaymentDb" TO postgres;
    
    -- Display created databases
    \l
EOSQL

echo "✅ Databases created successfully!"
