using Microsoft.Extensions.Logging;

namespace TestCopilotGame.Systems.Tick;

/// <summary>
/// Manages all tickable game objects
/// </summary>
public class TickManager(ILogger<TickManager>? logger = null, bool enableDebugLogging = false, int sampleEveryNTicks = 60)
{
    private readonly List<ITickable> _tickables = new();
    private readonly object _sync = new();
    private readonly ILogger<TickManager>? _logger = logger;
    private readonly bool _enableDebugLogging = enableDebugLogging;
    private readonly int _sampleEveryNTicks = sampleEveryNTicks;
    private long _tickCounter;
    
    public void Register(ITickable tickable)
    {
        lock (_sync)
        {
            if (!_tickables.Contains(tickable))
            {
                _tickables.Add(tickable);
                _logger?.LogDebug("Registered tickable {Type}", tickable.GetType().Name);
            }
        }
    }
    
    public void Unregister(ITickable tickable)
    {
        lock (_sync)
        {
            _tickables.Remove(tickable);
            _logger?.LogDebug("Unregistered tickable {Type}", tickable.GetType().Name);
        }
    }
    
    public void Update(double deltaTime)
    {
        ITickable[] snapshot;
        lock (_sync)
        {
            snapshot = _tickables.ToArray();
        }

        // Update all registered tickables
        for (int i = snapshot.Length - 1; i >= 0; i--)
        {
            snapshot[i].Tick(deltaTime);
        }
        
        _tickCounter++;
        if (_enableDebugLogging && _logger is not null && _tickCounter % _sampleEveryNTicks == 0)
        {
            lock (_sync)
            {
                _logger.LogDebug("Tick sample: count={Count}, tickables={Tickables}", _tickCounter, _tickables.Count);
            }
        }
    }
}
