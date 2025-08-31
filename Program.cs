var builder = WebApplication.CreateBuilder(args);

// ----------------------------
// 1️⃣ Services - Servisleri ekle
// ----------------------------
builder.Services.AddControllers();              // Controller desteği
builder.Services.AddEndpointsApiExplorer();    // Swagger için endpoint keşfi
builder.Services.AddSwaggerGen();              // Swagger UI

var app = builder.Build();

// ----------------------------
// 2️⃣ Middleware - HTTP pipeline
// ----------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();                           // Swagger JSON
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CRM API V1");
    });
}

// HTTPS yönlendirmesini kapatıyoruz geliştirme için
// app.UseHttpsRedirection();

app.UseAuthorization();

// ----------------------------
// 3️⃣ Map Controllers
// ----------------------------
app.MapControllers();

// ----------------------------
// 4️⃣ Minimal API örneği (opsiyonel)
// ----------------------------
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
.WithName("GetWeatherForecast");

app.Run();

// ----------------------------
// 5️⃣ Record
// ----------------------------
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
