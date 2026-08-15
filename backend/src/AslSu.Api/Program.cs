using System.Text;
using AslSu.Application.Abstractions;
using AslSu.Application.Auth;
using AslSu.Application.BatchPolling;
using AslSu.Application.Catalog;
using AslSu.Application.ProductSync;
using AslSu.Application.Products;
using AslSu.Application.StockPrice;
using AslSu.Application.StockPriceSync;
using AslSu.Application.TrendyolSettings;
using AslSu.Infrastructure.Auth;
using AslSu.Infrastructure.BatchPolling;
using AslSu.Infrastructure.Catalog;
using AslSu.Infrastructure.Persistence;
using AslSu.Infrastructure.ProductSync;
using AslSu.Infrastructure.Products;
using AslSu.Infrastructure.StockPrice;
using AslSu.Infrastructure.StockPriceSync;
using AslSu.Infrastructure.TrendyolSettings;
using AslSu.TrendyolGo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

// Options are bound and validated lazily (ValidateOnStart runs after the host is fully
// built) rather than read eagerly here, so that test hosts / config providers layered on
// top of this builder (e.g. WebApplicationFactory) still get a chance to supply values
// before validation runs.
builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(o => !string.IsNullOrWhiteSpace(o.SigningKey),
        "Jwt:SigningKey is not configured. Set it via 'dotnet user-secrets set \"Jwt:SigningKey\" \"<value>\"' " +
        "in development, or the Jwt__SigningKey environment variable in other environments. It must never be committed to appsettings.json.")
    .ValidateOnStart();

builder.Services.AddDbContext<AslSuDbContext>((serviceProvider, options) =>
{
    var connectionString = serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("Default");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "ConnectionStrings:Default is not configured. Set it via 'dotnet user-secrets set \"ConnectionStrings:Default\" \"<value>\"' " +
            "in development, or the ConnectionStrings__Default environment variable in other environments.");
    }

    options.UseSqlServer(connectionString);
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasherAdapter>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IStockPriceService, StockPriceService>();
builder.Services.AddScoped<ITrendyolSettingsService, TrendyolSettingsService>();
builder.Services.AddScoped<IProductSyncService, ProductSyncService>();
builder.Services.AddScoped<IStockPriceSyncService, StockPriceSyncService>();
builder.Services.AddScoped<IBatchPollingService, BatchPollingService>();
builder.Services.AddTrendyolGoClient(builder.Configuration, builder.Environment);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<Microsoft.Extensions.Options.IOptions<JwtOptions>>((bearerOptions, jwtOptionsAccessor) =>
    {
        var jwt = jwtOptionsAccessor.Value;
        bearerOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorization();

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "AslSu API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a JWT access token.",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
    };
    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { { securityScheme, [] } });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var provider = scope.ServiceProvider;
    await DbInitializer.MigrateAndSeedAsync(
        provider.GetRequiredService<AslSuDbContext>(),
        provider.GetRequiredService<IPasswordHasher>(),
        provider.GetRequiredService<IConfiguration>(),
        provider.GetRequiredService<ILogger<Program>>());
}

app.UseSerilogRequestLogging();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (feature is not null)
        {
            Log.Error(feature.Error, "Unhandled exception on {Path}", context.Request.Path);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new { message = "An unexpected error occurred." });
    });
});

app.UseHttpsRedirection();

app.UseCors("Default");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

namespace AslSu.Api
{
    public partial class Program;
}
