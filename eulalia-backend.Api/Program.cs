using Microsoft.AspNetCore.Authentication.JwtBearer;
using EFCore.NamingConventions;
using Microsoft.IdentityModel.Tokens;
using eulalia_backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using eulalia_backend.Api.Settings;
using System.Text;
using Microsoft.OpenApi.Models;
using eulalia_backend.Application.Interfaces;
using eulalia_backend.Application.Services;
using eulalia_backend.Infrastructure.Repositories;
using eulalia_backend.Infrastructure.Options;
using eulalia_backend.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// JWT config from appsettings.json
builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("Jwt"));
var jwt = builder.Configuration.GetSection("Jwt").Get<AuthSettings>();
Console.WriteLine($"[DEBUG] Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"[DEBUG] JWT Object is null: {jwt == null}");
if (jwt != null) {
    Console.WriteLine($"[DEBUG] JWT Key Length: {jwt.Key?.Length}");
    Console.WriteLine($"[DEBUG] JWT Issuer: {jwt.Issuer}");
}
if (jwt == null || string.IsNullOrEmpty(jwt.Key)) throw new Exception("JWT Configuration is missing or Key is empty in appsettings.json");

// Identus Options
builder.Services.Configure<IdentusOptions>(builder.Configuration.GetSection("Identus"));
builder.Services.Configure<CardanoOptions>(builder.Configuration.GetSection("Cardano"));

// Identus HttpClient
builder.Services.AddHttpClient<IIdentusClient, IdentusClient>();
builder.Services.AddHttpClient<ICardanoClient, CardanoClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CardanoOptions>>().Value;
    if (Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri))
        client.BaseAddress = new Uri(baseUri.ToString().TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(Math.Max(1, options.TimeoutSeconds));
});

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
        };
    });

// Add Authorization
builder.Services.AddAuthorization();

// Add Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Eulalia API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,           
        Scheme = "bearer",                        
        BearerFormat = "JWT"                      
    });

   c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

});

// Add Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// PostgreSQL
builder.Services.AddDbContext<EulaliaContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseSnakeCaseNamingConvention()
           .EnableSensitiveDataLogging()
           .LogTo(Console.WriteLine, LogLevel.Information));

// Register Application Services
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ICiudadanoService, CiudadanoService>();
builder.Services.AddScoped<IAfiliacionService, AfiliacionService>();
builder.Services.AddScoped<IOrganizacionService, OrganizacionService>();
builder.Services.AddScoped<IProvinciaService, ProvinciaService>();
builder.Services.AddScoped<ISSIService, SSIService>();
builder.Services.AddScoped<IOrganizacionIdentityService, OrganizacionIdentityService>();
builder.Services.AddScoped<BiometriaService>(); 

//CORS
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy
                .WithOrigins("http://localhost:5173") // URL de tu frontend
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


var app = builder.Build();

// Dev bootstrap for transactional DB compatibility with legacy schemas.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EulaliaContext>();
    db.Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS ssi_issuances (
            id integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
            cedula character varying(20) NOT NULL,
            holder_did text NULL,
            issuer_did text NULL,
            schema_id text NULL,
            invitation_id text NULL,
            invitation_url text NULL,
            credential_record_id text NULL,
            status integer NOT NULL,
            created_at timestamp with time zone NOT NULL,
            updated_at timestamp with time zone NOT NULL,
            error_message text NULL
        );
    ");

    // Biometria table is required for mobile self-enrollment; ensure it exists.
    db.Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS biometriaciudadano (
            biometriaid integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
            cedula character varying(50) NOT NULL,
            templatecifrado bytea NOT NULL,
            hashtemplate character varying(256) NOT NULL,
            fecharegistro timestamp with time zone NOT NULL,
            estadoverificacion character varying(20) NOT NULL,
            CONSTRAINT fk_biometria_ciudadano_cedula
                FOREIGN KEY (cedula) REFERENCES ciudadano(cedula) ON DELETE CASCADE
        );
        CREATE INDEX IF NOT EXISTS ix_biometriaciudadano_cedula ON biometriaciudadano(cedula);
    ");

    db.Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS organizacion_identidad (
            id integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
            organizacion_id integer NOT NULL UNIQUE,
            client_did text NULL,
            holder_connection_id text NULL,
            long_form_did text NULL,
            published_did text NULL,
            estado character varying(32) NOT NULL,
            publication_operation_id text NULL,
            issuer_reference text NULL,
            credential_record_id text NULL,
            credential_status text NULL,
            created_at timestamp with time zone NOT NULL,
            updated_at timestamp with time zone NOT NULL,
            error_message text NULL,
            CONSTRAINT fk_organizacion_identidad_organizacion
                FOREIGN KEY (organizacion_id) REFERENCES organizacion(organizacion_id) ON DELETE CASCADE
        );
        ALTER TABLE organizacion_identidad ADD COLUMN IF NOT EXISTS client_did text NULL;
            ALTER TABLE organizacion_identidad ADD COLUMN IF NOT EXISTS holder_connection_id text NULL;
    ");

    db.Database.ExecuteSqlRaw(@"
        ALTER TABLE afiliacion ADD COLUMN IF NOT EXISTS cardano_affiliation_event_id text NULL;
        ALTER TABLE afiliacion ADD COLUMN IF NOT EXISTS cardano_affiliation_record_hash text NULL;
        ALTER TABLE afiliacion ADD COLUMN IF NOT EXISTS cardano_affiliation_tx_hash text NULL;
        ALTER TABLE afiliacion ADD COLUMN IF NOT EXISTS cardano_affiliation_status text NULL;
        ALTER TABLE afiliacion ADD COLUMN IF NOT EXISTS cardano_cancellation_event_id text NULL;
        ALTER TABLE afiliacion ADD COLUMN IF NOT EXISTS cardano_cancellation_record_hash text NULL;
        ALTER TABLE afiliacion ADD COLUMN IF NOT EXISTS cardano_cancellation_tx_hash text NULL;
        ALTER TABLE afiliacion ADD COLUMN IF NOT EXISTS cardano_cancellation_status text NULL;
    ");

    // Replace the legacy full cedula uniqueness rule with the business rule:
    // only one non-annulled affiliation may exist for a cedula.
    db.Database.ExecuteSqlRaw(@"
        ALTER TABLE afiliacion DROP CONSTRAINT IF EXISTS afiliacion_cedula_key;
        CREATE UNIQUE INDEX IF NOT EXISTS ux_afiliacion_cedula_active
            ON afiliacion (cedula)
            WHERE estado <> 'Anulado';
    ");
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication(); // JWT auth must be BEFORE authorization
app.UseAuthorization();

app.MapControllers();

app.Run();
