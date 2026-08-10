using Serilog;
using DevForge.Application;
using DevForge.Infrastructure;
using DevForge.API.Middleware;

// Configure bootstrap logging first
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting DevForge API bootstrapping...");
    
    var builder = WebApplication.CreateBuilder(args);

    // Integrate Serilog structured logging
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/devforge-.txt", 
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"));

    // Add API Controllers and JSON options
    builder.Services.AddControllers();

    // Register Clean Architecture layers
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // Register Global Exception Handling with ProblemDetails
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // Configure CORS using settings from Configuration
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy", policy =>
        {
            if (allowedOrigins.Length > 0)
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            }
            else
            {
                policy.AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowAnyOrigin(); // Fallback for dev ease, though overridden by config
            }
        });
    });

    // Configure Swagger/OpenAPI
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    app.UseExceptionHandler();
    
    // Serilog Request Logging middleware
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "DevForge API v1");
        });
    }

    app.UseHttpsRedirection();

    app.UseCors("CorsPolicy");

    app.UseAuthorization();

    // Map health check endpoints
    app.MapHealthChecks("/health");

    app.MapControllers();

    Log.Information("Host built and successfully configured. Starting app...");
    app.Run();
}
catch (Exception ex) when (ex.GetType().Name is not "HostAbortedException")
{
    Log.Fatal(ex, "Host terminated unexpectedly during bootstrapping.");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
