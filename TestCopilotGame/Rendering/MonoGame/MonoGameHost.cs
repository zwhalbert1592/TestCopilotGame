using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TestCopilotGame.Core;
using TestCopilotGame.Entities;
using TestCopilotGame.Entities.Configuration;
using TestCopilotGame.Systems.Input;
using TestCopilotGame.Systems.Rendering;
using TestCopilotGame.Systems.Rendering.Texture;
using TestCopilotGame.Systems.Tick;

namespace TestCopilotGame.Rendering.MonoGame;

/// <summary>
/// Hosts a MonoGame Game to drive rendering and game logic.
/// Replaces the custom GameLoop with MonoGame's built-in fixed timestep.
/// </summary>
public class MonoGameHost : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch? _spriteBatch;
    private readonly ILogger<MonoGameHost>? _logger;

    // Managers
    public RenderManager? RenderManager { get; set; }
    public TickManager? TickManager { get; set; }

    private Player? _player;
    private KeyboardState _previousKeyboardState;
    private bool _drawLoggedOnce;
    private bool _playerInitialized;
    private TextureService? _textureService;
    private GroundRenderer? _groundRenderer;

    public MonoGameHost(ILogger<MonoGameHost>? logger = null)
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0); // 60 FPS fixed timestep
        _graphics.SynchronizeWithVerticalRetrace = false;
        
        _graphics.PreferredBackBufferWidth = 1920;
        _graphics.PreferredBackBufferHeight = 1080;
        
        _logger = logger;

        Window.Title = "Test Copilot Game";
        Window.AllowUserResizing = true;
        Window.IsBorderless = false;
        
        _graphics.ApplyChanges();

        _logger?.LogInformation("MonoGameHost created");
    }

    protected override void Initialize()
    {
        try
        {
            _logger?.LogInformation("Initialize() called");
            base.Initialize();
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _logger?.LogInformation("MonoGame initialized: {Adapter}", GraphicsAdapter.DefaultAdapter.Description);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in Initialize()");
            throw;
        }
    }

    private async Task InitializePlayerAsync()
    {
        try
        {
            if (RenderManager is null || TickManager is null)
            {
                _logger?.LogError("RenderManager or TickManager is null");
                return;
            }

            var configLoader = new CharacterConfigLoader(_logger);
            var configPath = "Config/character.json";

            var config = await configLoader.LoadAsync(configPath) ?? CharacterConfigLoader.CreateDefault();

            if (config is null)
            {
                _logger?.LogError("Failed to load or create character configuration");
                return;
            }

            await configLoader.SaveAsync(configPath, config);

            var textures = new TextureService(GraphicsDevice, _logger);
            _textureService = textures;

            IReadOnlyList<Texture2D> LoadFrames(string pattern, int count)
            {
                var list = new List<Texture2D>(capacity: count);
                for (int i = 1; i <= count; i++)
                {
                    var path = string.Format(pattern, i);
                    _logger?.LogInformation("Loading texture: {Path}", path);
                    list.Add(textures.Load(path));
                }
                return list;
            }

            _logger?.LogInformation("Loading idle frames from {Pattern}", config.IdleAnimation.Pattern);
            var idleFrames = LoadFrames(config.IdleAnimation.Pattern, config.IdleAnimation.Count);
            
            _logger?.LogInformation("Loading run frames from {Pattern}", config.RunAnimation.Pattern);
            var runFrames = LoadFrames(config.RunAnimation.Pattern, config.RunAnimation.Count);
            
            _logger?.LogInformation("Loading jump frames from {Pattern}", config.JumpAnimation.Pattern);
            var jumpFrames = LoadFrames(config.JumpAnimation.Pattern, config.JumpAnimation.Count);

            var idleRenderer = new AnimatedSpriteRenderer(idleFrames, zOrder: config.Render.IdleZOrder)
            {
                Position = new Vector2((float)config.Render.StartX, (float)config.Render.StartY),
                FramesPerSecond = config.IdleAnimation.FramesPerSecond,
                Scale = new Vector2(config.Render.Scale)
            };
            
            var runRenderer = new AnimatedSpriteRenderer(runFrames, zOrder: config.Render.RunZOrder)
            {
                Position = new Vector2((float)config.Render.StartX, (float)config.Render.StartY),
                FramesPerSecond = config.RunAnimation.FramesPerSecond,
                Scale = new Vector2(config.Render.Scale)
            };
            
            var jumpRenderer = new AnimatedSpriteRenderer(jumpFrames, zOrder: config.Render.JumpZOrder)
            {
                Position = new Vector2((float)config.Render.StartX, (float)config.Render.StartY),
                FramesPerSecond = config.JumpAnimation.FramesPerSecond,
                Scale = new Vector2(config.Render.Scale)
            };

            _player = new Player(config)
            {
                IdleRenderer = idleRenderer,
                RunRenderer = runRenderer,
                JumpRenderer = jumpRenderer
            };

            TickManager.Register(_player);
            RenderManager.Register(idleRenderer);

            // Load and setup ground
            _logger?.LogInformation("Loading ground texture...");
            var groundConfig = new GroundConfig
            {
                GroundY = config.Physics.GroundLevel
            };
            _groundRenderer = new GroundRenderer(
                GraphicsDevice,
                groundConfig.TileWidth,
                groundConfig.TileHeight,
                groundConfig.GroundY,
                groundConfig.ZOrder
            );
            RenderManager.Register(_groundRenderer);
            _logger?.LogInformation("Ground initialized");

            _logger?.LogInformation("Player initialized from config");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error initializing player");
        }
    }

    protected override void Update(GameTime gameTime)
    {
        // Initialize player on first update
        if (!_playerInitialized && RenderManager is not null && TickManager is not null)
        {
            _logger?.LogInformation("First Update() - initializing player");
            _playerInitialized = true;
            _ = InitializePlayerAsync();
        }

        var keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Keys.Escape))
            Exit();

        // Handle player input
        if (_player is not null)
        {
            bool movingLeft = keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A);
            bool movingRight = keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D);
            bool jumping = keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W);
            
            if (movingLeft && !movingRight)
                _player.Move(-200.0);
            else if (movingRight && !movingLeft)
                _player.Move(200.0);
            else
                _player.StopMoving();
            
            if (jumping)
                _player.Jump();
        }

        // Update game logic (fixed timestep)
        double deltaSeconds = gameTime.ElapsedGameTime.TotalSeconds;
        TickManager?.Update(deltaSeconds);

        // Update animations
        if (RenderManager is not null)
        {
            foreach (var renderable in RenderManager.GetRenderablesSnapshot())
            {
                if (renderable is AnimatedSpriteRenderer asr)
                {
                    asr.Update(deltaSeconds);
                }
            }
        }

        // Handle window resizing
        HandleWindowResize(keyboardState);

        _previousKeyboardState = keyboardState;

        base.Update(gameTime);
    }

    private void HandleWindowResize(KeyboardState keyboardState)
    {
        bool IsKeyJustPressed(Keys key) => 
            keyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);

        if (IsKeyJustPressed(Keys.D1) && keyboardState.IsKeyDown(Keys.LeftControl))
            ResizeWindow(640, 360);
        else if (IsKeyJustPressed(Keys.D2) && keyboardState.IsKeyDown(Keys.LeftControl))
            ResizeWindow(1280, 720);
        else if (IsKeyJustPressed(Keys.D3) && keyboardState.IsKeyDown(Keys.LeftControl))
            ResizeWindow(1920, 1080);
        else if (IsKeyJustPressed(Keys.D4) && keyboardState.IsKeyDown(Keys.LeftControl))
            ResizeWindow(2560, 1440);
        else if (IsKeyJustPressed(Keys.F) && keyboardState.IsKeyDown(Keys.LeftControl))
        {
            _graphics.IsFullScreen = !_graphics.IsFullScreen;
            _graphics.ApplyChanges();
            _logger?.LogInformation("Fullscreen toggled: {FullScreen}", _graphics.IsFullScreen);
        }
    }

    private void ResizeWindow(int width, int height)
    {
        _graphics.PreferredBackBufferWidth = width;
        _graphics.PreferredBackBufferHeight = height;
        _graphics.ApplyChanges();
        _logger?.LogInformation("Window resized to {Width}x{Height}", width, height);
    }

    protected override void Draw(GameTime gameTime)
    {
        try
        {
            if (!_drawLoggedOnce)
            {
                _logger?.LogInformation("Draw() called for first time");
                _drawLoggedOnce = true;
            }

            // Use dark green background to make ground level visible
            GraphicsDevice.Clear(Color.DarkGreen);
            
            if (_spriteBatch is not null && RenderManager is not null)
            {
                _textureService?.ProcessPendingTextures();

                if (_player is not null)
                    UpdatePlayerRenderer();

                _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp);
                foreach (var renderable in RenderManager.GetRenderablesSnapshot())
                {
                    if (renderable.IsVisible)
                    {
                        if (renderable is SpriteRenderer sr)
                            sr.Draw(_spriteBatch);
                        else if (renderable is AnimatedSpriteRenderer asr)
                            asr.Draw(_spriteBatch);
                        else if (renderable is GroundRenderer gr)
                            gr.Draw(_spriteBatch);
                    }
                }
                _spriteBatch.End();
            }
            base.Draw(gameTime);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in Draw()");
        }
    }

    private void UpdatePlayerRenderer()
    {
        if (_player is null || RenderManager is null)
            return;

        IRenderable? targetRenderer = _player.CurrentState switch
        {
            Player.PlayerState.Idle => _player.IdleRenderer,
            Player.PlayerState.Running => _player.RunRenderer,
            Player.PlayerState.Jumping => _player.JumpRenderer,
            _ => _player.IdleRenderer
        };

        if (_player.CurrentState != _player.PreviousState && targetRenderer is not null)
        {
            if (_player.PreviousState == Player.PlayerState.Idle && _player.IdleRenderer is not null)
                RenderManager.Unregister(_player.IdleRenderer);
            else if (_player.PreviousState == Player.PlayerState.Running && _player.RunRenderer is not null)
                RenderManager.Unregister(_player.RunRenderer);
            else if (_player.PreviousState == Player.PlayerState.Jumping && _player.JumpRenderer is not null)
                RenderManager.Unregister(_player.JumpRenderer);

            RenderManager.Register(targetRenderer);
        }

        if (targetRenderer is AnimatedSpriteRenderer asr)
        {
            asr.Position = new Vector2((float)_player.X, (float)_player.Y);
            asr.FlipHorizontally = _player.FacingDirection < 0;
        }
        else if (targetRenderer is SpriteRenderer sr)
        {
            sr.Position = new Vector2((float)_player.X, (float)_player.Y);
        }
    }
}
