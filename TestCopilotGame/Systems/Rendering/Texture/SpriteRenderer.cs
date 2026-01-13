using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TestCopilotGame.Systems.Rendering;

namespace TestCopilotGame.Systems.Rendering.Texture;

/// <summary>
/// Basic sprite renderer that draws a texture region.
/// </summary>
public class SpriteRenderer : IRenderable
{
    private readonly Texture2D _texture;
    private readonly Rectangle _sourceRect;
    public int ZOrder { get; }
    public bool IsVisible { get; set; } = true;

    public Vector2 Position { get; set; }
    public Vector2 Scale { get; set; } = Vector2.One;
    public float Rotation { get; set; } = 0f;
    public Vector2 Origin { get; set; } = Vector2.Zero;
    public Color Color { get; set; } = Color.White;

    public SpriteRenderer(Texture2D texture, Rectangle sourceRect, int zOrder = 0)
    {
        _texture = texture;
        _sourceRect = sourceRect;
        ZOrder = zOrder;
    }

    public void Render(double interpolationAlpha)
    {
        // Rendering is driven by MonoGameHost; this class needs access to a SpriteBatch.
        // We will provide an adapter shortly to route drawing.
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            _texture,
            Position,
            _sourceRect,
            Color,
            Rotation,
            Origin,
            Scale,
            SpriteEffects.None,
            0f);
    }
}
