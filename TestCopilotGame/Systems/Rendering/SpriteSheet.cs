namespace TestCopilotGame.Systems.Rendering;

/// <summary>
/// Represents sprite sheet data for rendering
/// </summary>
public class SpriteSheet
{
    public string Name { get; set; }
    public string FilePath { get; set; }
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }
    public int Columns { get; set; }
    public int Rows { get; set; }
    
    public SpriteSheet(string name, string filePath, int tileWidth, int tileHeight, int columns, int rows)
    {
        Name = name;
        FilePath = filePath;
        TileWidth = tileWidth;
        TileHeight = tileHeight;
        Columns = columns;
        Rows = rows;
    }
    
    public (int x, int y) GetTilePosition(int tileIndex)
    {
        int row = tileIndex / Columns;
        int col = tileIndex % Columns;
        return (col * TileWidth, row * TileHeight);
    }
}
