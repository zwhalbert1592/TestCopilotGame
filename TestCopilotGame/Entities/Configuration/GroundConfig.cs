namespace TestCopilotGame.Entities.Configuration;

/// <summary>
/// Configuration for ground/level rendering
/// </summary>
public class GroundConfig
{
    //TODO: The ground texture should ideally be a tileset with multiple variations to avoid repetition, but for simplicity we'll use a single texture for now
    //TODO: The ground texture isn't actually loading and rendering correctly, investigate why. It may be related to the content pipeline or how the texture is being loaded in the GroundRenderer.
    public string TexturePath { get; set; } = "Environment/ground.png";
    public double TileWidth { get; set; } = 32.0;
    public double TileHeight { get; set; } = 32.0;
    public double GroundY { get; set; } = 200.0; // Y position where ground starts
    public int ZOrder { get; set; } = -1; // Draw behind player
}
