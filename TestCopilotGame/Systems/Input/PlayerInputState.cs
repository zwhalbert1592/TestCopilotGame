namespace TestCopilotGame.Systems.Input;

/// <summary>
/// Holds the current player input state - updated from MonoGame main thread, read by game loop
/// </summary>
public class PlayerInputState
{
    public double MoveDirection { get; set; } = 0.0; // -1 (left), 0 (none), 1 (right)
    public bool WantsToJump { get; set; } = false;
    
    public void Reset()
    {
        MoveDirection = 0.0;
        WantsToJump = false;
    }
}
