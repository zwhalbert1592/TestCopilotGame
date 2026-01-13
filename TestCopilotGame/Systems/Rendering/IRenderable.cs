namespace TestCopilotGame.Systems.Rendering;

/// <summary>
/// Base class for renderable components
/// </summary>
public interface IRenderable
{
    void Render(double interpolationAlpha);
    int ZOrder { get; }
    bool IsVisible { get; set; }
}
