using Godot;
using GodotExtensions;

namespace GameRoot;

[Tool]
public partial class Map2D : Node2D
{
    [Export] public string MapName;
    [Export] public int GridSize = 2;

    public TileMap GetTileMap() => this.FirstNodeOfType<TileMap>();
}