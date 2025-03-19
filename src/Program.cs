using System.Net.WebSockets;
using netnitel.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure WebSockets
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};
app.UseWebSockets(webSocketOptions);

app.MapControllers();

app.Run();

