using Microsoft.Extensions.Logging;

namespace TestCopilotGame.Systems.Assets;

using TestCopilotGame.Systems.Rendering;

/// <summary>
/// Manages loading and caching of game assets
/// </summary>
public class AssetManager(string assetsBasePath = "Assets", ILogger<AssetManager>? logger = null)
{
    private readonly Dictionary<string, SpriteSheet> _spriteSheets = [];
    private readonly string _assetsBasePath = assetsBasePath;
    private readonly ILogger<AssetManager>? _logger = logger;

    public void LoadSpriteSheet(string name, string relativePath, int tileWidth, int tileHeight, int columns, int rows)
    {
        string fullPath = Path.Combine(_assetsBasePath, "Sprites", relativePath);
        var spriteSheet = new SpriteSheet(name, fullPath, tileWidth, tileHeight, columns, rows);
        _spriteSheets[name] = spriteSheet;
        _logger?.LogInformation("Loaded SpriteSheet {Name} from {Path} ({Cols}x{Rows} tiles {Tw}x{Th})", name, fullPath, columns, rows, tileWidth, tileHeight);
    }
    
    public SpriteSheet? GetSpriteSheet(string name)
    {
        return _spriteSheets.TryGetValue(name, out var sheet) ? sheet : null;
    }
    
    public void UnloadSpriteSheet(string name)
    {
        if (_spriteSheets.Remove(name))
        {
            _logger?.LogInformation("Unloaded SpriteSheet {Name}", name);
        }
    }
    
    public void UnloadAll()
    {
        _spriteSheets.Clear();
        _logger?.LogInformation("Unloaded all SpriteSheets");
    }
}
