using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TestCopilotGame.Systems.Rendering;

namespace TestCopilotGame.Entities;

/// <summary>
/// Renders tiled ground texture at ground level
/// </summary>
public class GroundRenderer : IRenderable
{
    private readonly Texture2D _texture;
    private readonly double _tileWidth;
    private readonly double _tileHeight;
    private readonly double _groundY;

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

    public void Render(double interpolationAlpha)
    {
        // Ground doesn't animate, no-op
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible || _texture == null)
            return;

        // Draw tiles across the screen width
        // Assume screen width is 1280 (standard resolution)
        int screenWidth = 1280;
        int screenHeight = 720;
        
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
                spriteBatch.Draw(_texture, destRect, Color.White);
            }
        }
    }
}
