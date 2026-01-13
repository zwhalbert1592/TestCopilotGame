using Microsoft.Extensions.Logging;

namespace TestCopilotGame.Systems.Rendering;

/// <summary>
/// Manages rendering of all renderable objects
/// </summary>
public class RenderManager(ILogger<RenderManager>? logger = null, bool enableDebugLogging = false)
{
    private readonly List<IRenderable> _renderables = new();
    private readonly object _sync = new();
    private readonly ILogger<RenderManager>? _logger = logger;
    private readonly bool _enableDebugLogging = enableDebugLogging;
    
    public void Register(IRenderable renderable)
    {
        lock (_sync)
        {
            if (!_renderables.Contains(renderable))
            {
                _renderables.Add(renderable);
                _renderables.Sort((a, b) => a.ZOrder.CompareTo(b.ZOrder));
                if (_enableDebugLogging)
                {
                    _logger?.LogDebug("Registered renderable z={Z} count={Count}", renderable.ZOrder, _renderables.Count);
                }
            }
        }
    }
    
    public void Unregister(IRenderable renderable)
    {
        lock (_sync)
        {
            _renderables.Remove(renderable);
            if (_enableDebugLogging)
            {
                _logger?.LogDebug("Unregistered renderable count={Count}", _renderables.Count);
            }
        }
    }

    /// <summary>
    /// Returns a snapshot of renderables in Z-order for external drawing.
    /// </summary>
    public IReadOnlyList<IRenderable> GetRenderablesSnapshot()
    {
        lock (_sync)
        {
            return _renderables.ToArray();
        }
    }
    
    public void Render(double interpolationAlpha)
    {
        IRenderable[] snapshot;
        lock (_sync)
        {
            snapshot = _renderables.ToArray();
        }
        foreach (var renderable in snapshot)
        {
            if (renderable.IsVisible)
            {
                renderable.Render(interpolationAlpha);
            }
        }
    }
}
