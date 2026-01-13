namespace TestCopilotGame.Entities;

using TestCopilotGame.Systems.Rendering;
using TestCopilotGame.Entities.Configuration;

/// <summary>
/// Player character with state management for Idle, Running, and Jumping
/// </summary>
public class Player(CharacterConfig config) : GameObject(config.Name, config.Render.StartX, config.Render.StartY)
{
    public enum PlayerState
    {
        Idle,
        Running,
        Jumping
    }

    private PlayerState _currentState = PlayerState.Idle;
    private PlayerState _previousState = PlayerState.Idle;
    private int _facingDirection = 1; // 1 for right, -1 for left

    public PlayerState CurrentState => _currentState;
    public PlayerState PreviousState => _previousState;
    public int FacingDirection => _facingDirection;

    // Renderers for each state
    public IRenderable? IdleRenderer { get; set; }
    public IRenderable? RunRenderer { get; set; }
    public IRenderable? JumpRenderer { get; set; }

    public override void Tick(double deltaTime)
    {
        if (!IsActive) return;

        // Apply gravity
        if (Y < config.Physics.GroundLevel)
        {
            VelocityY += config.Physics.Gravity * deltaTime;
        }
        else
        {
            Y = config.Physics.GroundLevel;
            VelocityY = 0;
        }

        // Update position
        X += VelocityX * deltaTime;
        Y += VelocityY * deltaTime;

        // Clamp to ground
        if (Y >= config.Physics.GroundLevel)
        {
            Y = config.Physics.GroundLevel;
            VelocityY = 0;
        }

        // Update facing direction based on velocity
        if (VelocityX > 0)
        {
            _facingDirection = 1; // Moving right
        }
        else if (VelocityX < 0)
        {
            _facingDirection = -1; // Moving left
        }

        // Update state
        _previousState = _currentState;

        if (VelocityY != 0)
        {
            _currentState = PlayerState.Jumping;
        }
        else if (VelocityX != 0)
        {
            _currentState = PlayerState.Running;
        }
        else
        {
            _currentState = PlayerState.Idle;
        }
    }

    public void Jump()
    {
        if (Y >= config.Physics.GroundLevel)
        {
            VelocityY = -config.Physics.JumpVelocity;
        }
    }

    public void Move(double velocityX)
    {
        // Clamp velocity to max run speed
        VelocityX = Math.Clamp(velocityX, -config.Physics.MaxRunSpeed, config.Physics.MaxRunSpeed);
    }

    public void StopMoving()
    {
        VelocityX = 0;
    }
}
