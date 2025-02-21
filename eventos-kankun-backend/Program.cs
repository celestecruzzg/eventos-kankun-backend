using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using EventosAPI.Data;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configurar DbContext con SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar Controllers con opciones de JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Manejar referencias circulares en las relaciones entre entidades
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        // Mantener la capitalización original de las propiedades
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Configurar Swagger con más detalles
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Eventos API",
        Version = "v1",
        Description = "API para la gestión de eventos e inscripciones en ASP.NET Core",
        Contact = new OpenApiContact
        {
            Name = "Tu Nombre",
            Email = "tu.email@ejemplo.com"
        }
    });

    // Agregar descripciones de respuestas comunes
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Agrupar endpoints por controlador
    c.TagActionsBy(api => new[] { api.GroupName });
});

// Configurar compresión de respuesta
builder.Services.AddResponseCompression();

var app = builder.Build();

// Configurar middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Eventos API v1");
        c.RoutePrefix = string.Empty; // Para acceder a Swagger en la raíz
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.DefaultModelsExpandDepth(-1); // Ocultar esquemas por defecto
    });
}
else
{
    // Agregar middleware de seguridad en producción
    app.UseHsts();
}

// Habilitar CORS
app.UseCors("AllowAll");

// Habilitar compresión
app.UseResponseCompression();

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// Middleware de manejo de errores global
app.UseExceptionHandler("/error");

app.MapControllers();

// Asegurar que la base de datos está creada
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al crear la base de datos.");
    }
}

app.Run();