using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- CONFIGURACIÓN DE JWT ---
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Habilitar CORS para permitir que Vue (puerto 5173) se comunique con el backend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        builder =>
        {
            builder.WithOrigins("http://localhost:5173")
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

// Agregamos nuestro servicio de base de datos para que pueda ser usado en toda la aplicación
builder.Services.AddScoped<SellManagement.Api.Services.DatabaseService>();
builder.Services.AddControllers(); // Habilitamos el uso de controladores (Controllers)

var app = builder.Build();

// Inicializar base de datos
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
try 
{
    SellManagement.Api.DatabaseInitializer.Initialize(connectionString);
}
catch (Exception ex)
{
    Console.WriteLine($"Error inicializando BD: {ex.Message}");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Usar la política de CORS antes de Authentication
app.UseCors("AllowVueApp");

// --- ACTIVAR AUTENTICACIÓN Y AUTORIZACIÓN ---
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<SellManagement.Api.Middlewares.ErrorHandlerMiddleware>(); // <-- AGREGAR ESTO

app.MapControllers(); 

app.Run();
