namespace TestCopilotGame.Entities.Configuration;

using System.Text.Json.Serialization;

/// <summary>
/// Configuration for animation frames within a character state
/// </summary>
public class AnimationFrameConfig
{
    [JsonPropertyName("pattern")]
    public string Pattern { get; set; } = "";

    [JsonPropertyName("count")]
    public int Count { get; set; } = 1;

    [JsonPropertyName("framesPerSecond")]
    public double FramesPerSecond { get; set; } = 6.0;
}

/// <summary>
/// Configuration for a character's physics
/// </summary>
public class PhysicsConfig
{
    [JsonPropertyName("gravity")]
    public double Gravity { get; set; } = 1000.0;

    [JsonPropertyName("jumpVelocity")]
    public double JumpVelocity { get; set; } = 500.0;

    [JsonPropertyName("groundLevel")]
    public double GroundLevel { get; set; } = 200.0;

    [JsonPropertyName("maxRunSpeed")]
    public double MaxRunSpeed { get; set; } = 300.0;
}

/// <summary>
/// Configuration for rendering a character
/// </summary>
public class RenderConfig
{
    [JsonPropertyName("startX")]
    public double StartX { get; set; } = 100.0;

    [JsonPropertyName("startY")]
    public double StartY { get; set; } = 200.0;

    [JsonPropertyName("scale")]
    public float Scale { get; set; } = 1.0f;

    [JsonPropertyName("idleZOrder")]
    public int IdleZOrder { get; set; } = 0;

    [JsonPropertyName("runZOrder")]
    public int RunZOrder { get; set; } = 1;

    [JsonPropertyName("jumpZOrder")]
    public int JumpZOrder { get; set; } = 2;
}

/// <summary>
/// Complete character configuration
/// </summary>
public class CharacterConfig
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "Player";

    [JsonPropertyName("idle")]
    public AnimationFrameConfig IdleAnimation { get; set; } = new();

    [JsonPropertyName("run")]
    public AnimationFrameConfig RunAnimation { get; set; } = new();

    [JsonPropertyName("jump")]
    public AnimationFrameConfig JumpAnimation { get; set; } = new();

    [JsonPropertyName("physics")]
    public PhysicsConfig Physics { get; set; } = new();

    [JsonPropertyName("render")]
    public RenderConfig Render { get; set; } = new();
}
