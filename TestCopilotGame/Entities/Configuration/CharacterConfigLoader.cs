namespace TestCopilotGame.Entities.Configuration;

using System.Text.Json;
using Microsoft.Extensions.Logging;

/// <summary>
/// Loads character configurations from JSON files
/// </summary>
public class CharacterConfigLoader
{
    private readonly ILogger? _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public CharacterConfigLoader(ILogger? logger = null)
    {
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
    }

    /// <summary>
    /// Loads a character configuration from a JSON file
    /// </summary>
    public async Task<CharacterConfig?> LoadAsync(string filePath)
    {
        try
        {
            var fullPath = Path.GetFullPath(filePath);
            _logger?.Log(LogLevel.Information, "Loading config from: {FullPath}, Current directory: {CurrentDir}", fullPath, Directory.GetCurrentDirectory());
            
            if (!File.Exists(fullPath))
            {
                _logger?.Log(LogLevel.Warning, "Character config file not found: {Path}", fullPath);
                return null;
            }

            var json = await File.ReadAllTextAsync(fullPath);
            var config = JsonSerializer.Deserialize<CharacterConfig>(json, _jsonOptions);

            if (config is not null)
            {
                _logger?.Log(LogLevel.Information, "Loaded character config from {Path}", fullPath);
            }

            return config;
        }
        catch (Exception ex)
        {
            _logger?.Log(LogLevel.Error, ex, "Failed to load character config from {FilePath}", filePath);
            return null;
        }
    }

    /// <summary>
    /// Saves a character configuration to a JSON file
    /// </summary>
    public async Task SaveAsync(string filePath, CharacterConfig config)
    {
        try
        {
            var fullPath = Path.GetFullPath(filePath);
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(config, _jsonOptions);
            await File.WriteAllTextAsync(fullPath, json);

            _logger?.Log(LogLevel.Information, "Saved character config to {Path}", fullPath);
        }
        catch (Exception ex)
        {
            _logger?.Log(LogLevel.Error, ex, "Failed to save character config to {FilePath}", filePath);
        }
    }

    /// <summary>
    /// Creates a default character configuration
    /// </summary>
    public static CharacterConfig CreateDefault()
    {
        return new CharacterConfig
        {
            Name = "Player",
            IdleAnimation = new AnimationFrameConfig
            {
                Pattern = "Player/Idle/frame-{0}.png",
                Count = 2,
                FramesPerSecond = 1.0
            },
            RunAnimation = new AnimationFrameConfig
            {
                Pattern = "Player/running/frame-{0}.png",
                Count = 6,
                FramesPerSecond = 10.0
            },
            JumpAnimation = new AnimationFrameConfig
            {
                Pattern = "Player/Jump/frame-{0}.png",
                Count = 2,
                FramesPerSecond = 8.0
            },
            Physics = new PhysicsConfig
            {
                Gravity = 1000.0,
                JumpVelocity = 500.0,
                GroundLevel = 200.0,
                MaxRunSpeed = 300.0
            },
            Render = new RenderConfig
            {
                StartX = 100.0,
                StartY = 200.0,
                Scale = 1.0f,
                IdleZOrder = 0,
                RunZOrder = 1,
                JumpZOrder = 2
            }
        };
    }
}
