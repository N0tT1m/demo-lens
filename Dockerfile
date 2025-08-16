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

# Install SkiaSharp dependencies for Linux and wget for health checks
RUN apt-get update && apt-get install -y \
    libfontconfig1 \
    wget \
    && rm -rf /var/lib/apt/lists/*

# Create uploads directory
RUN mkdir -p /app/uploads

# Copy published app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Expose port
EXPOSE 80

# Start the application
ENTRYPOINT ["dotnet", "CS2DemoParserWeb.dll"]