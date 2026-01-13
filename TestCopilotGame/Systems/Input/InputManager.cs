namespace TestCopilotGame.Systems.Input;

/// <summary>
/// Manages keyboard and other input events
/// </summary>
public class InputManager
{
    private readonly HashSet<ConsoleKey> _pressedKeys = new();
    private readonly HashSet<ConsoleKey> _justPressedKeys = new();
    private readonly HashSet<ConsoleKey> _justReleasedKeys = new();
    
    public void Update()
    {
        _justPressedKeys.Clear();
        _justReleasedKeys.Clear();
        
        // Note: This is a simple implementation
        // For real game input, consider using a proper game framework
        if (Console.KeyAvailable)
        {
            var keyInfo = Console.ReadKey(true);
            if (!_pressedKeys.Contains(keyInfo.Key))
            {
                _pressedKeys.Add(keyInfo.Key);
                _justPressedKeys.Add(keyInfo.Key);
            }
        }
    }
    
    public bool IsKeyPressed(ConsoleKey key) => _pressedKeys.Contains(key);
    public bool IsKeyJustPressed(ConsoleKey key) => _justPressedKeys.Contains(key);
    public bool IsKeyJustReleased(ConsoleKey key) => _justReleasedKeys.Contains(key);
    
    public void ReleaseKey(ConsoleKey key)
    {
        if (_pressedKeys.Remove(key))
        {
            _justReleasedKeys.Add(key);
        }
    }
}
