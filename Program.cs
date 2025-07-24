var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
// Serve archivos estáticos (como index.html en wwwroot)
app.UseStaticFiles();
// Swagger solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthorization();
// Mapear rutas de controllers
app.MapControllers();
// Ruta por defecto (opcional: sirve index.html si entras sin ruta)
app.MapFallbackToFile("index.html");
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://*:{port}");
app.Run();
