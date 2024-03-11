using Godot;
using GodotExtensions;

namespace GameRoot;

[Tool]
public partial class Map2D : Node2D
{
    [Export] public string MapName;
    [Export] public Vector2 GridSize = new(2, 2);
    [Export] public int Height = 3;
    public TileMap GetTileMap() => this.FirstNodeOfType<TileMap>();
}