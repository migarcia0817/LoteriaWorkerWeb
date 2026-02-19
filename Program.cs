using LoteriaWorkerWeb;

var builder = WebApplication.CreateBuilder(args);

// Registrar tu Worker como HostedService
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

// Endpoint mínimo para Render
app.MapGet("/", () => "Worker running OK");

// Endpoint de salud para monitoreo
app.MapGet("/health", () => "OK");

// Ejecutar la aplicación
app.Run();
