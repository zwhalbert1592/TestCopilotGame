using Microsoft.Extensions.Logging;
using TestCopilotGame.Rendering.MonoGame;
using TestCopilotGame.Systems.Rendering;
using TestCopilotGame.Systems.Tick;

// Setup logging
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Information)
        .AddSimpleConsole(options =>
        {
            options.IncludeScopes = false;
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
        });
});

var hostLogger = loggerFactory.CreateLogger<MonoGameHost>();
var tickLogger = loggerFactory.CreateLogger<TickManager>();
var renderLogger = loggerFactory.CreateLogger<RenderManager>();

// Create managers
var tickManager = new TickManager(tickLogger);
var renderManager = new RenderManager(renderLogger);

hostLogger.LogInformation("Game starting...");

// Run MonoGame host (blocking on the main thread)
try
{
    using var host = new MonoGameHost(hostLogger) 
    { 
        RenderManager = renderManager,
        TickManager = tickManager
    };
    hostLogger.LogInformation("MonoGame host created, running...");
    host.Run();
    hostLogger.LogInformation("MonoGame host stopped");
}
catch (Exception ex)
{
    hostLogger.LogError(ex, "Game error");
}

hostLogger.LogInformation("Game ended");
