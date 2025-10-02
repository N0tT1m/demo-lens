# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy all source code first
COPY . .

# Let dotnet handle all dependencies automatically by building from the web project
WORKDIR "/src/CS2DemoParserWeb"
RUN dotnet restore "CS2DemoParserWeb.csproj" --verbosity minimal
RUN dotnet publish "CS2DemoParserWeb.csproj" -c Release -o /app/publish --verbosity minimal

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN MKDIR -p /https
COPY https/fullchain.pem /https/fullchain.pem
COPY https/privkey.pem /https/privkey.pem

# Install SkiaSharp dependencies for Linux, wget for health checks, and SQL tools
RUN apt-get update && apt-get install -y \
    libfontconfig1 \
    wget \
    curl \
    gnupg \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Install SQL Server command line tools
RUN curl -fsSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o /usr/share/keyrings/microsoft-prod.gpg \
    && echo "deb [arch=amd64,arm64,armhf signed-by=/usr/share/keyrings/microsoft-prod.gpg] https://packages.microsoft.com/ubuntu/20.04/prod focal main" > /etc/apt/sources.list.d/msprod.list \
    && apt-get update \
    && ACCEPT_EULA=Y apt-get install -y mssql-tools unixodbc-dev \
    && rm -rf /var/lib/apt/lists/*

# Add SQL tools to PATH
ENV PATH="$PATH:/opt/mssql-tools/bin"

# Create uploads directory
RUN mkdir -p /app/uploads

# Copy published app
COPY --from=build /app/publish .

# Copy the startup script and fix line endings
COPY docker-entrypoint.sh /usr/local/bin/docker-entrypoint.sh
RUN chmod +x /usr/local/bin/docker-entrypoint.sh \
    && sed -i 's/\r$//' /usr/local/bin/docker-entrypoint.sh

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=https://demo-lens.duocore.dev:80

# Expose port
EXPOSE 80

# Use custom entrypoint that applies database optimizations
ENTRYPOINT ["/usr/local/bin/docker-entrypoint.sh"]