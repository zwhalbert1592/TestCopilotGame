using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TestCopilotGame.Systems.Rendering;

namespace TestCopilotGame.Entities;

/// <summary>
/// Renders tiled ground texture at ground level
/// </summary>
public class GroundRenderer : IRenderable
{
    private readonly Texture2D? _texture;
    private readonly double _tileWidth;
    private readonly double _tileHeight;
    private readonly double _groundY;
    private readonly GraphicsDevice? _graphicsDevice;
    private Texture2D? _proceduralTile;

    public int ZOrder { get; }
    public bool IsVisible { get; set; } = true;

    public GroundRenderer(Texture2D texture, double tileWidth, double tileHeight, double groundY, int zOrder = -1)
    {
        _texture = texture;
        _tileWidth = tileWidth;
        _tileHeight = tileHeight;
        _groundY = groundY;
        ZOrder = zOrder;
    }

    /// <summary>
    /// Constructor that creates procedural ground when no texture is provided
    /// </summary>
    public GroundRenderer(GraphicsDevice graphicsDevice, double tileWidth, double tileHeight, double groundY, int zOrder = -1)
    {
        _graphicsDevice = graphicsDevice;
        _tileWidth = tileWidth;
        _tileHeight = tileHeight;
        _groundY = groundY;
        ZOrder = zOrder;
        CreateProceduralTile();
    }

    private void CreateProceduralTile()
    {
        if (_graphicsDevice == null)
            return;

        int tileSize = 32;
        _proceduralTile = new Texture2D(_graphicsDevice, tileSize, tileSize);
        var data = new Color[tileSize * tileSize];

        // Create a checkerboard pattern for ground
        Color color1 = new Color(34, 139, 34); // Forest green
        Color color2 = new Color(46, 175, 46); // Lighter green

        for (int i = 0; i < data.Length; i++)
        {
            int x = i % tileSize;
            int y = i / tileSize;
            data[i] = ((x / 8 + y / 8) % 2 == 0) ? color1 : color2;
        }

        _proceduralTile.SetData(data);
    }

    public void Render(double interpolationAlpha)
    {
        // Ground doesn't animate, no-op
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Texture2D? textureToUse = _texture ?? _proceduralTile;
        if (!IsVisible || textureToUse == null)
            return;

        // Draw tiles across the screen width
        // Assume screen width is 1920 (default resolution)
        int screenWidth = 1920;
        int screenHeight = 1080;
        
        // Calculate number of tiles needed
        int tilesX = (int)Math.Ceiling(screenWidth / _tileWidth) + 1;
        int tilesY = (int)Math.Ceiling(screenHeight / _tileHeight) + 1;

        // Draw tiled ground
        for (int x = 0; x < tilesX; x++)
        {
            for (int y = 0; y < tilesY; y++)
            {
                var destRect = new Rectangle(
                    (int)(x * _tileWidth),
                    (int)(_groundY + y * _tileHeight),
                    (int)_tileWidth,
                    (int)_tileHeight
                );
                spriteBatch.Draw(textureToUse, destRect, Color.White);
            }
        }
    }
}
