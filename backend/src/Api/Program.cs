using System.Text;
using Ecommerce.Application; // ✅ AGREGAR ESTE USING
using Ecommerce.Domain;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ✅ ORDEN CORRECTO: Primero Application, luego Infrastructure
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add services to the container
builder.Services.AddDbContext<EcommerceDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionString"),
    b => b.MigrationsAssembly(typeof(EcommerceDbContext).Assembly.FullName)));

builder.Services.AddControllers(opt => // Registra los controladores de tu API
    {
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build(); // Todos los usuarios deben estar autenticados
    opt.Filters.Add(new AuthorizeFilter(policy)); // Aplica en todos los controladores
    });

IdentityBuilder identityBuilder = builder.Services.AddIdentityCore<Usuario>();
identityBuilder = new IdentityBuilder(identityBuilder.UserType, identityBuilder.Services);

identityBuilder.AddRoles<IdentityRole>().AddDefaultTokenProviders();
identityBuilder.AddClaimsPrincipalFactory<UserClaimsPrincipalFactory<Usuario, IdentityRole>>();

identityBuilder.AddEntityFrameworkStores<EcommerceDbContext>();
identityBuilder.AddSignInManager<SignInManager<Usuario>>();

builder.Services.TryAddSingleton<ISystemClock, SystemClock>();

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWTSettings:Key"]!));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder => builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Program>();

    try
    {
        var context = services.GetRequiredService<EcommerceDbContext>();
        var usuarioManager = services.GetRequiredService<UserManager<Usuario>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        logger.LogInformation("Iniciando migración de base de datos...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Migración completada exitosamente");

        logger.LogInformation("Iniciando carga de datos iniciales...");
        
        // ✅ AWAIT agregado aquí - era el problema principal
        await EcommerceDbContextData.LoadDataAsync(context, usuarioManager, roleManager, loggerFactory);
        
        logger.LogInformation("Datos iniciales cargados exitosamente");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ocurrió un error durante la migración o carga de datos iniciales");
        
        // En desarrollo, mostrar el error completo
        if (app.Environment.IsDevelopment())
        {
            logger.LogError("Detalles del error: {ErrorMessage}", ex.Message);
            logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
        }
        
        // No lanzar la excepción para permitir que la aplicación inicie
        // throw; // Comentado para evitar que la app falle completamente
    }
}

app.Run();