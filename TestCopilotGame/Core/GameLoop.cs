using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TestCopilotGame.Core;

/// <summary>
/// Manages the main game loop with fixed timestep updates
/// </summary>
public class GameLoop
{
    private bool _isRunning;
    private readonly double _targetTicksPerSecond;
    private readonly double _tickInterval;
    
    public event Action<double>? OnUpdate;
    public event Action<double>? OnRender;
    
    public double DeltaTime { get; private set; }
    public long TotalTicks { get; private set; }

    private readonly ILogger<GameLoop>? _logger;
    private readonly bool _enableDiagnostics;
    private readonly int _sampleEveryNTicks;
    private readonly Stopwatch _stopwatch = new();
    private double _lastUpdateDurationMs;
    private double _lastRenderDurationMs;
    
    public GameLoop(double targetTicksPerSecond = 60.0, ILogger<GameLoop>? logger = null, bool enableDiagnostics = false, int sampleEveryNTicks = 60)
    {
        _targetTicksPerSecond = targetTicksPerSecond;
        _tickInterval = 1.0 / targetTicksPerSecond;
        _logger = logger;
        _enableDiagnostics = enableDiagnostics;
        _sampleEveryNTicks = sampleEveryNTicks;
    }
    
    public void Start()
    {
        _isRunning = true;
        Run();
    }
    
    public void Stop()
    {
        _isRunning = false;
    }
    
    private void Run()
    {
        double accumulator = 0.0;
        long lastTime = Environment.TickCount64;
        
        while (_isRunning)
        {
            long currentTime = Environment.TickCount64;
            double frameTime = (currentTime - lastTime) / 1000.0;
            lastTime = currentTime;
            
            accumulator += frameTime;
            
            // Fixed timestep updates
            while (accumulator >= _tickInterval)
            {
                DeltaTime = _tickInterval;
                if (_enableDiagnostics)
                {
                    _stopwatch.Restart();
                }
                OnUpdate?.Invoke(DeltaTime);
                if (_enableDiagnostics)
                {
                    _stopwatch.Stop();
                    _lastUpdateDurationMs = _stopwatch.Elapsed.TotalMilliseconds;
                }
                TotalTicks++;
                accumulator -= _tickInterval;
            }
            
            // Render with interpolation value
            double alpha = accumulator / _tickInterval;
            if (_enableDiagnostics)
            {
                _stopwatch.Restart();
            }
            OnRender?.Invoke(alpha);
            if (_enableDiagnostics)
            {
                _stopwatch.Stop();
                _lastRenderDurationMs = _stopwatch.Elapsed.TotalMilliseconds;
            }
            
            if (_enableDiagnostics && _logger is not null && TotalTicks % _sampleEveryNTicks == 0)
            {
                _logger.LogInformation("Diagnostics sample: tick={Tick}, updateMs={UpdateMs:F3}, renderMs={RenderMs:F3}, accumulator={Accumulator:F4}", TotalTicks, _lastUpdateDurationMs, _lastRenderDurationMs, accumulator);
            }
            
            // Sleep to prevent busy waiting
            Thread.Sleep(1);
        }
    }
}
