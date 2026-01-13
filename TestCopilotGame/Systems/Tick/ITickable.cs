namespace TestCopilotGame.Systems.Tick;

/// <summary>
/// Interface for objects that need to update each game tick
/// </summary>
public interface ITickable
{
    void Tick(double deltaTime);
}
