using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StbImageSharp;

namespace TestCopilotGame.Systems.Rendering.Texture;

/// <summary>
/// Simple texture loading and caching service for MonoGame without using the MGCB content pipeline.
/// </summary>
public class TextureService
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly ILogger? _logger;
    private readonly Dictionary<string, Texture2D> _cache = new();
    private readonly Dictionary<string, byte[]> _pendingTextures = new(); // textures waiting to be created
    private readonly Dictionary<string, ImageResult> _decodedImages = new(); // decoded but not yet uploaded
    private readonly string _assetsRoot;
    private Texture2D? _placeholderTexture;
    private int _processingStage = 0; // 0 = decode, 1 = upload

    public TextureService(GraphicsDevice graphicsDevice, ILogger? logger = null, string assetsRoot = "Assets/Sprites")
    {
        _graphicsDevice = graphicsDevice;
        _logger = logger;
        _assetsRoot = assetsRoot;
    }

    /// <summary>
    /// Creates a placeholder texture for missing assets
    /// </summary>
    private Texture2D CreatePlaceholderTexture()
    {
        if (_placeholderTexture is not null)
            return _placeholderTexture;

        var texture = new Texture2D(_graphicsDevice, 32, 32);
        var data = new Color[32 * 32];
        
        // Create a magenta checkerboard pattern for easy identification
        for (int i = 0; i < data.Length; i++)
        {
            int x = i % 32;
            int y = i / 32;
            data[i] = ((x / 8 + y / 8) % 2 == 0) ? Color.Magenta : Color.Black;
        }
        
        texture.SetData(data);
        _placeholderTexture = texture;
        return texture;
    }

    /// <summary>
    /// Loads a texture from disk and caches it. Starts loading asynchronously.
    /// </summary>
    public Texture2D Load(string relativePath)
    {
        var absolutePath = Path.Combine(_assetsRoot, relativePath);
        if (_cache.TryGetValue(absolutePath, out var tex))
        {
            return tex;
        }

        // Check if already pending
        if (_pendingTextures.ContainsKey(absolutePath))
        {
            _logger?.LogInformation("Texture already pending: {Path}", absolutePath);
            if (_cache.TryGetValue(absolutePath, out var pending))
            {
                return pending; // Return the texture object we're loading into
            }
        }

        try
        {
            _logger?.LogInformation("Attempting to load texture at: {Path}", absolutePath);
            
            if (!File.Exists(absolutePath))
            {
                _logger?.LogWarning("Texture file not found: {Path}", absolutePath);
                var placeholder = CreatePlaceholderTexture();
                _cache[absolutePath] = placeholder;
                return placeholder;
            }

            _logger?.LogInformation("File exists, reading into memory: {Path}", absolutePath);
            
            // Read file into memory (this is fast)
            byte[] fileData = File.ReadAllBytes(absolutePath);
            _logger?.LogInformation("File read into memory: {Bytes} bytes", fileData.Length);
            
            // Quick decode just to get dimensions (decode happens on background or deferred)
            // We do a minimal decode here - just get width/height from PNG header
            (int width, int height) = DecodePNGDimensions(fileData);
            _logger?.LogInformation("PNG dimensions from header: {Width}x{Height}", width, height);
            
            // Create texture with correct dimensions NOW
            var texture = new Texture2D(_graphicsDevice, width, height);
            _logger?.LogInformation("Texture created: {Width}x{Height}", texture.Width, texture.Height);
            
            // Store in cache so renderer gets this object
            _cache[absolutePath] = texture;
            
            // Store file data for later pixel filling
            _pendingTextures[absolutePath] = fileData;
            
            _logger?.LogInformation("Texture queued for pixel data: {Path}", absolutePath);
            return texture;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to read texture file: {Path}", absolutePath);
            var placeholder = CreatePlaceholderTexture();
            _cache[absolutePath] = placeholder;
            return placeholder;
        }
    }

    /// <summary>
    /// Quick PNG dimension decoder - reads just the header
    /// </summary>
    private (int width, int height) DecodePNGDimensions(byte[] pngData)
    {
        // PNG signature is 8 bytes: 137 80 78 71 13 10 26 10
        if (pngData.Length < 24)
            throw new InvalidOperationException("Invalid PNG: too short");
            
        // IHDR chunk is at offset 8, dimensions are at offset 16-24
        // Width: bytes 16-19 (big-endian)
        // Height: bytes 20-23 (big-endian)
        int width = (pngData[16] << 24) | (pngData[17] << 16) | (pngData[18] << 8) | pngData[19];
        int height = (pngData[20] << 24) | (pngData[21] << 16) | (pngData[22] << 8) | pngData[23];
        
        return (width, height);
    }

    /// <summary>
    /// Must be called from the graphics thread (e.g., in Draw or Update) to create pending textures
    /// </summary>
    public void ProcessPendingTextures()
    {
        // Spread work across frames:
        // Frame N: Decode PNG -> get pixel data
        // Frame N+1: Upload pixel data to GPU
        // This prevents frame stutters from long operations
        
        // Stage 1: Decode images (CPU work, not GPU)
        if (_processingStage == 0 && _pendingTextures.Count > 0)
        {
            var kvp = _pendingTextures.First();
            var absolutePath = kvp.Key;
            var fileData = kvp.Value;

            try
            {
                // Only decode, don't upload yet
                if (!_decodedImages.ContainsKey(absolutePath))
                {
                    _logger?.LogInformation("Decoding PNG: {Path}", absolutePath);
                    ImageResult image;
                    try
                    {
                        image = ImageResult.FromMemory(fileData, ColorComponents.RedGreenBlueAlpha);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "RGBA decode failed, trying RGB");
                        image = ImageResult.FromMemory(fileData, ColorComponents.RedGreenBlue);
                    }

                    _decodedImages[absolutePath] = image;
                    _logger?.LogInformation("PNG decoded: {Width}x{Height}", image.Width, image.Height);
                    _processingStage = 1; // Next frame: upload
                    return; // Wait for next frame to upload
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to decode texture: {Path}", absolutePath);
                _pendingTextures.Remove(absolutePath);
                _processingStage = 0;
            }
        }

        // Stage 2: Upload decoded images to GPU
        if (_processingStage == 1 && _decodedImages.Count > 0)
        {
            var kvp = _decodedImages.First();
            var absolutePath = kvp.Key;
            var image = kvp.Value;

            try
            {
                _logger?.LogInformation("Uploading texture: {Path}", absolutePath);
                
                if (!_cache.TryGetValue(absolutePath, out var texture))
                {
                    _logger?.LogError("Texture not in cache: {Path}", absolutePath);
                    _decodedImages.Remove(absolutePath);
                    _pendingTextures.Remove(absolutePath);
                    _processingStage = 0;
                    return;
                }

                // Convert pixel data to MonoGame Color array
                var colorData = new Color[image.Width * image.Height];

                int bytesPerPixel = image.Data.Length / (image.Width * image.Height);

                for (int i = 0; i < colorData.Length; i++)
                {
                    int byteIndex = i * bytesPerPixel;
                    if (bytesPerPixel == 4)
                    {
                        colorData[i] = new Color((byte)image.Data[byteIndex], (byte)image.Data[byteIndex + 1], (byte)image.Data[byteIndex + 2], (byte)image.Data[byteIndex + 3]);
                    }
                    else if (bytesPerPixel == 3)
                    {
                        colorData[i] = new Color((byte)image.Data[byteIndex], (byte)image.Data[byteIndex + 1], (byte)image.Data[byteIndex + 2], (byte)255);
                    }
                    else
                    {
                        colorData[i] = Color.Magenta;
                    }
                }

                _logger?.LogInformation("Setting pixel data for texture");
                texture.SetData(colorData);
                _logger?.LogInformation("Texture upload complete: {Path}", absolutePath);

                _decodedImages.Remove(absolutePath);
                _pendingTextures.Remove(absolutePath);
                _processingStage = 0; // Back to decode stage
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to upload texture: {Path}", absolutePath);
                _decodedImages.Remove(absolutePath);
                _pendingTextures.Remove(absolutePath);
                _processingStage = 0;
            }
        }
    }

    public bool TryGet(string relativePath, out Texture2D? texture)
    {
        var absolutePath = Path.Combine(_assetsRoot, relativePath);
        var ok = _cache.TryGetValue(absolutePath, out var tex);
        texture = tex;
        return ok;
    }

    public void Unload(string relativePath)
    {
        var absolutePath = Path.Combine(_assetsRoot, relativePath);
        if (_cache.TryGetValue(absolutePath, out var tex))
        {
            tex.Dispose();
            _cache.Remove(absolutePath);
        }
        _pendingTextures.Remove(absolutePath);
    }
}
