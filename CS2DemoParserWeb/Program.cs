using Microsoft.EntityFrameworkCore;
using CS2DemoParser.Data;
using CS2DemoParser.Services;
using CS2DemoParserWeb.Services;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Add Entity Framework - make it optional for testing
try 
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
        Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? 
        throw new InvalidOperationException("No connection string configured. Set CONNECTION_STRING environment variable or configure DefaultConnection in appsettings.json");

    builder.Services.AddDbContext<CS2DemoContext>(options =>
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
            sqlOptions.CommandTimeout(30);
        }));
}
catch (Exception ex)
{
    Console.WriteLine($"Database configuration failed: {ex.Message}");
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
        var context = scope.ServiceProvider.GetRequiredService<CS2DemoContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Running database migrations on startup...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations completed successfully");
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Failed to run database migrations on startup");
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
