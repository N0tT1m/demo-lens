using Microsoft.EntityFrameworkCore;
using CS2DemoParser.Data;
using CS2DemoParser.Services;
using CS2DemoParserWeb.Services;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddControllers();

// Add Entity Framework - make it optional for testing
var configConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var envConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
var sqlPassword = Environment.GetEnvironmentVariable("SQL_PASSWORD");

Console.WriteLine($"Config connection string: {(string.IsNullOrEmpty(configConnectionString) ? "NULL/EMPTY" : "SET")}");
Console.WriteLine($"Environment CONNECTION_STRING: {(string.IsNullOrEmpty(envConnectionString) ? "NULL/EMPTY" : "SET")}");
Console.WriteLine($"Environment SQL_PASSWORD: {(string.IsNullOrEmpty(sqlPassword) ? "NULL/EMPTY" : "SET")}");

var connectionString = configConnectionString ?? envConnectionString;

if (!string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Configuring Entity Framework with connection string");
    builder.Services.AddDbContext<CS2DemoContext>(options =>
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
            sqlOptions.CommandTimeout(30);
        }));
}
else
{
    Console.WriteLine("WARNING: No connection string found. Database features will be disabled.");
}

// Add CS2DemoParser service - make optional for testing
try
{
    builder.Services.AddScoped<CorrectedDemoParserService>();
    // Add Heatmap service
    builder.Services.AddScoped<IHeatmapService, HeatmapService>();
    builder.Services.AddScoped<IHeatmapImageService, HeatmapImageService>();
}
catch (Exception ex)
{
    Console.WriteLine($"Service configuration failed: {ex.Message}");
}

// Add memory cache for heatmap caching
builder.Services.AddMemoryCache();

// Configure file upload limits
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 1_000_000_000; // 1GB
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 1_000_000_000; // 1GB
    options.ValueLengthLimit = 1_000_000_000; // 1GB
    options.ValueCountLimit = 10000;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 1_000_000_000; // 1GB
});

var app = builder.Build();

// Run database migrations on startup in production
if (app.Environment.IsProduction())
{
    try
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetService<CS2DemoContext>();
        
        if (context != null)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Running database migrations on startup...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations completed successfully");
        }
        else
        {
            Console.WriteLine("Skipping database migrations - no DbContext configured");
        }
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetService<ILogger<Program>>();
        if (logger != null)
        {
            logger.LogError(ex, "Failed to run database migrations on startup");
        }
        else
        {
            Console.WriteLine($"Failed to run database migrations on startup: {ex.Message}");
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
app.MapControllers();

// Add health check endpoint for Docker
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
