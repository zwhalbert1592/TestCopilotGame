# TestCopilotGame

A tick-based game engine built with .NET 9.

## Project Structure

```
TestCopilotGame/
??? Core/                      # Core game systems
?   ??? GameLoop.cs           # Main game loop with fixed timestep
?
??? Entities/                  # Game objects and entities
?   ??? GameObject.cs         # Base game object class
?
??? Systems/                   # Game systems
?   ??? Assets/               # Asset management
?   ?   ??? AssetManager.cs   # Loads and manages game assets
?   ??? Input/                # Input handling
?   ?   ??? InputManager.cs   # Keyboard/input management
?   ??? Rendering/            # Rendering system
?   ?   ??? IRenderable.cs    # Interface for renderable objects
?   ?   ??? RenderManager.cs  # Manages rendering pipeline
?   ?   ??? SpriteSheet.cs    # Sprite sheet data structure
?   ??? Tick/                 # Tick/update system
?       ??? ITickable.cs      # Interface for tickable objects
?       ??? TickManager.cs    # Manages tick updates
?
??? Assets/                    # Game assets
?   ??? Sprites/              # Sprite sheets and images
?   ??? Audio/                # Sound effects and music
?   ??? Data/                 # Game data files
?   ??? README.md             # Asset organization guide
?
??? Program.cs                # Entry point

```

## Architecture

### Game Loop
- Fixed timestep updates (60 ticks/second by default)
- Separate update and render phases
- Interpolation support for smooth rendering

### Systems
- **TickManager**: Updates all game objects each tick
- **RenderManager**: Handles rendering with Z-order sorting
- **InputManager**: Processes keyboard input
- **AssetManager**: Loads and caches game assets

### Entities
- Implement `ITickable` for game logic updates
- Implement `IRenderable` for rendering
- Base `GameObject` class provides common functionality

## Getting Started

1. Create game objects by extending `GameObject`
2. Register them with `TickManager` and `RenderManager`
3. Load assets using `AssetManager`
4. Run the game loop!

## Next Steps

- Add a graphics rendering library (Raylib, MonoGame, Silk.NET)
- Implement sprite rendering
- Add collision detection
- Create game-specific entities (Player, Enemy, etc.)
