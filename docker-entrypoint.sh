#!/bin/bash

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to be ready..."
until /opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P "$SQL_PASSWORD" -Q "SELECT 1" > /dev/null 2>&1
do
  echo "SQL Server is unavailable - sleeping"
  sleep 1
done

echo "SQL Server is up - starting application"

# Start the application
dotnet CS2DemoParserWeb.dll &
APP_PID=$!

# Wait a bit for EF migrations to complete
echo "Waiting for application to complete EF migrations..."
sleep 30

# Apply performance indexes after tables are created
echo "Applying performance indexes..."
/opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P "$SQL_PASSWORD" -d demos -i /usr/config/database_performance_indexes.sql

echo "Performance indexes applied successfully"

# Wait for the application process
wait $APP_PID