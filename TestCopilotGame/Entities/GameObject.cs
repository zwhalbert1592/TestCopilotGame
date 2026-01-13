namespace TestCopilotGame.Entities;

using TestCopilotGame.Systems.Tick;

/// <summary>
/// Base class for all game entities
/// </summary>
public class GameObject : ITickable
{
    public string Name { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public bool IsActive { get; set; } = true;
    
    public GameObject(string name, double x, double y)
    {
        Name = name;
        X = x;
        Y = y;
    }
    
    public virtual void Tick(double deltaTime)
    {
        if (!IsActive) return;
        
        // Update position based on velocity
        X += VelocityX * deltaTime;
        Y += VelocityY * deltaTime;
    }
}
