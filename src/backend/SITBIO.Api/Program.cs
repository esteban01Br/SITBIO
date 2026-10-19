using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SITBIO.Api.Auth;
using SITBIO.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// Configuración JWT (ADSO-ARQ-01 §6): HS256, 30 minutos,
// claims id_usuario / tenant_id / rol.
// ============================================================
var jwt = builder.Configuration.GetSection("Jwt").Get<OpcionesJwt>()
    ?? throw new InvalidOperationException("Falta la sección \"Jwt\" en appsettings.json.");
if (string.IsNullOrWhiteSpace(jwt.Clave) || jwt.Clave.Length < 32)
    throw new InvalidOperationException("Jwt:Clave debe tener al menos 32 caracteres.");
builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton<ServicioJwt>();

// ============================================================
// Persistencia: PostgreSQL 16 + Entity Framework Core 8
// ============================================================
var cadena = builder.Configuration.GetConnectionString("Sitbio")
    ?? throw new InvalidOperationException("Falta ConnectionStrings:Sitbio en appsettings.json.");
builder.Services.AddDbContext<SITBIOContext>(opciones => opciones.UseNpgsql(cadena));

// ============================================================
// Autenticación JWT + autorización (RBAC revalidado en servidor)
// ============================================================
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.MapInboundClaims = false;
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Clave)),
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            RoleClaimType = "rol",
            NameClaimType = "nombre"
        };
    });
builder.Services.AddAuthorization();

// ============================================================
// Controladores + Swagger (documentación OpenAPI para pruebas)
// ============================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SITBIO API",
        Version = "v1",
        Description = "API REST de SITBIO (ADSO-ARQ-01): PostgreSQL 16 + EF Core, " +
                      "JWT HS256 y RBAC multi-tenant. Rutas bajo /api/v1."
    });
    opciones.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pegue el token que devuelve POST /api/v1/auth/login"
    });
    opciones.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// CORS amplio: el cliente WPF no lo requiere, pero permite probar desde el navegador.
builder.Services.AddCors(opciones => opciones.AddPolicy("libre",
    p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// ============================================================
// Al arrancar: migración (crear tablas) + siembra de datos demo.
// La siembra solo se ejecuta si la base está vacía.
// ============================================================
using (var alcance = app.Services.CreateScope())
{
    var db = alcance.ServiceProvider.GetRequiredService<SITBIOContext>();
    await db.Database.MigrateAsync();
    await SemillaDatos.SembrarAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("libre");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
