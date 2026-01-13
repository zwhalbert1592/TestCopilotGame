using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TestCopilotGame.Systems.Rendering;

namespace TestCopilotGame.Systems.Rendering.Texture;

/// <summary>
/// Renders an animation made from a sequence of textures (PNG frames).
/// </summary>
public class AnimatedSpriteRenderer : IRenderable
{
    private readonly IReadOnlyList<Texture2D> _frames;
    private readonly Rectangle? _sourceRect; // optional per-frame source
    private int _currentFrame;

    public int ZOrder { get; }
    public bool IsVisible { get; set; } = true;

    public Vector2 Position { get; set; }
    public Vector2 Scale { get; set; } = Vector2.One;
    public float Rotation { get; set; } = 0f;
    public Vector2 Origin { get; set; } = Vector2.Zero;
    public Color Color { get; set; } = Color.White;
    public bool FlipHorizontally { get; set; } = false;

    public double FramesPerSecond { get; set; } = 10.0; // default playback speed

    private double _accumulator;
    private double _lastInterpolationAlpha;

    public AnimatedSpriteRenderer(IReadOnlyList<Texture2D> frames, Rectangle? sourceRect = null, int zOrder = 0)
    {
        _frames = frames;
        _sourceRect = sourceRect;
        ZOrder = zOrder;
    }

    public void Render(double interpolationAlpha)
    {
        // Calculate actual delta time from interpolation alpha
        // interpolationAlpha ranges from 0 to 1 within a fixed tick
        // We need to track the delta between frames to get proper timing
        double deltaTime = interpolationAlpha - _lastInterpolationAlpha;
        
        // Handle wrap-around (when alpha resets from ~1.0 back to 0)
        if (deltaTime < 0)
        {
            deltaTime += 1.0;
        }
        
        // Assuming fixed tick rate of 60 Hz (0.01667 seconds per tick)
        // Convert interpolation delta to actual seconds
        const double FixedTickDelta = 1.0 / 60.0;
        deltaTime *= FixedTickDelta;
        
        _lastInterpolationAlpha = interpolationAlpha;

        // Advance frame timing
        _accumulator += deltaTime * FramesPerSecond;
        while (_accumulator >= 1.0)
        {
            _accumulator -= 1.0;
            _currentFrame = (_currentFrame + 1) % _frames.Count;
        }
    }

    /// <summary>
    /// Update animation for fixed timestep (called from MonoGame Update)
    /// </summary>
    public void Update(double deltaSeconds)
    {
        // Advance frame timing
        _accumulator += deltaSeconds * FramesPerSecond;
        while (_accumulator >= 1.0)
        {
            _accumulator -= 1.0;
            _currentFrame = (_currentFrame + 1) % _frames.Count;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var tex = _frames[_currentFrame];
        var src = _sourceRect ?? new Rectangle(0, 0, tex.Width, tex.Height);
        var effects = FlipHorizontally ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        spriteBatch.Draw(tex, Position, src, Color, Rotation, Origin, Scale, effects, 0f);
    }
}
