

using LoteriaWorkerWeb;

var builder = WebApplication.CreateBuilder(args);

// Registrar tu Worker como HostedService
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

// Endpoint mínimo para Render
app.MapGet("/", () => "Worker running OK");

// Ejecutar la aplicación
app.Run();
