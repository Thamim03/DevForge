using Serilog;
using DevForge.Application;
using DevForge.Infrastructure;
using DevForge.API.Middleware;

var isTesting = AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName?.StartsWith("xunit", StringComparison.OrdinalIgnoreCase) == true);

if (!isTesting)
{
    // Configure bootstrap logging first
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .CreateBootstrapLogger();
}

try
{
    if (!isTesting)
    {
        Log.Information("Starting DevForge API bootstrapping...");
    }
    
    var builder = WebApplication.CreateBuilder(args);

    // Integrate Serilog structured logging
    if (!isTesting)
    {
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                path: "logs/devforge-.txt", 
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"));
    }

    // Add API Controllers and JSON options
    builder.Services.AddControllers();

    // Register Clean Architecture layers
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // Register JWT Authentication
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "DevForgeAPI",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "DevForgeWeb",
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "DefaultSecretKeyPlaceholder1234567890!"))
        };
    });

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
    if (!isTesting)
    {
        app.UseSerilogRequestLogging();
    }

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

    app.UseAuthentication();

    app.UseAuthorization();

    // Map health check endpoints
    app.MapHealthChecks("/health");

    app.MapControllers();

    // Initialize and seed database
    using (var scope = app.Services.CreateScope())
    {
        var initializer = scope.ServiceProvider.GetRequiredService<DevForge.Infrastructure.Persistence.DbContextInitializer>();
        initializer.InitializeAsync().GetAwaiter().GetResult();
        initializer.SeedAsync().GetAwaiter().GetResult();
    }

    if (!isTesting)
    {
        Log.Information("Host built and successfully configured. Starting app...");
    }
    app.Run();
}
catch (Exception ex) when (ex.GetType().Name is not "HostAbortedException")
{
    if (!isTesting)
    {
        Log.Fatal(ex, "Host terminated unexpectedly during bootstrapping.");
    }
    throw;
}
finally
{
    if (!isTesting)
    {
        Log.CloseAndFlush();
    }
}

public partial class Program { }
