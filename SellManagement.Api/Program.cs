var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Usar la política de CORS antes de MapControllers
app.UseCors("AllowVueApp");

// Habilitamos el uso de controladores
app.MapControllers(); 

/* 
   Este código de ejemplo (WeatherForecast) ya no lo necesitamos, 
   pero lo dejaré comentado por si quieres revisarlo luego.
*/
/*
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
*/

app.Run();
