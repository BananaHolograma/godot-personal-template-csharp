using System;
using Godot;
using Godot.Collections;
using GodotExtensions;

namespace GameRoot;

[Tool]
[GlobalClass]
public partial class MapLoader : Node3D
{

    [Export(PropertyHint.File)] public string MapFilePath = string.Empty;
    [Export] public bool MergeMeshes = true;
    [Export] public bool GenerateFloorCollisions = true;
    [Export] public bool GenerateCeilCollisions = false;
    [Export] public bool GenerateWallsCollisions = true;

    [Export]
    public bool Generate
    {
        get => _generateMap;
        set
        {
            _generateMap = false;
            if (Engine.IsEditorHint() && MapFilePath != "")
                GenerateMap();
        }
    }

    [Export]
    public bool ClearMap
    {
        get => _generateMap;
        set
        {
            _clearMap = false;
            if (Engine.IsEditorHint() && MapFilePath != "")
                ClearGeneratedMap();
        }
    }

    private bool _generateMap = false;
    private bool _clearMap = false;

    private readonly Dictionary<string, PackedScene> cachedScenes = new();
    public void GenerateMap()
    {
        if (MapFileIsValid(MapFilePath))
        {
            ClearGeneratedMap();

            PackedScene mapScene = ResourceLoader.Load<PackedScene>(MapFilePath);
            Map2D map2D = mapScene.Instantiate() as Map2D;
            TileMap tilemap = map2D.GetTileMap();

            if (tilemap == null)
            {
                GD.PushError($"The Map2D {MapFilePath} does not have a valid Tilemap to load");
                return;
            }

            int numberOfMaps = tilemap.GetLayersCount();

            Node3D mapRoot = CreateMapRootNode(map2D);

            foreach (int layer in GD.Range(0, numberOfMaps))
            {
                Node3D mapLevelRoot = CreateMapLevelRootNode(mapRoot, map2D, layer);
                Array<Vector2I> cells = tilemap.GetUsedCells(layer);



                foreach (Vector2I cell in cells)
                {
                    TileData data = tilemap.GetCellTileData(layer, cell);

                    if (layer == 1)
                        GD.Print("DATA BRO ", data);
                    if (data is not null)
                    {
                        PackedScene mapBlockScene = ObtainSceneFromCustomTileData(data);
                        if (layer == 1)
                            GD.Print("LAYER BRO ", mapBlockScene);
                        MapBlock mapBlock = mapBlockScene.Instantiate() as MapBlock;
                        mapBlock.Translate(new Vector3(cell.X * map2D.GridSize, 0, cell.Y * map2D.GridSize));
                        mapLevelRoot.AddChild(mapBlock);
                        mapBlock.UpdateFaces(GetCellNeighbours(cells, cell));

                        SetOwnerToEditedSceneRoot(mapBlock);
                    }
                }
            }
        }
        else
        {
            GD.PushError($"The map file {MapFilePath} is not valid, make sure is not empty and the path points to an existing file");
        }
    }

    private void SetOwnerToEditedSceneRoot(Node node)
    {
        if (Engine.IsEditorHint())
            node.Owner = GetTree().EditedSceneRoot;
    }
    private Dictionary<Vector2, bool> GetCellNeighbours(Array<Vector2I> cells, Vector2I cell)
    {
        return new() {
            {Vector2.Up, cells.Contains(cell + Vector2I.Up)},
            {Vector2.Down, cells.Contains (cell + Vector2I.Down)},
            {Vector2.Left, cells.Contains(cell + Vector2I.Left)},
            {Vector2.Right, cells.Contains(cell + Vector2I.Right)},
        };
    }

    private bool HasNeighbour(Array<Vector2I> cells, Vector2I cell, Vector2I direction)
    {
        return cells.Contains(cell + direction);
    }
    public void ClearGeneratedMap()
    {
        this.QueueFreeChildren();
    }

    private Node3D CreateMapRootNode(Map2D map2D)
    {
        Node3D mapRoot = new() { Name = map2D.MapName };
        AddChild(mapRoot);

        SetOwnerToEditedSceneRoot(mapRoot);

        return mapRoot;
    }

    private Node3D CreateMapLevelRootNode(Node3D mapRoot, Map2D map2D, int layer)
    {
        Node3D mapLayerRoot = new() { Name = $"{map2D.MapName}_Level{layer}" };
        mapRoot.AddChild(mapLayerRoot);

        SetOwnerToEditedSceneRoot(mapLayerRoot);

        return mapLayerRoot;
    }

    private PackedScene ObtainSceneFromCustomTileData(TileData data)
    {
        string scenePath = (string)data.GetCustomDataByLayerId(0);

        if (cachedScenes.TryGetValue(scenePath, out PackedScene mapBlockScene))
            return mapBlockScene;

        if (MapFileIsValid(scenePath))
            cachedScenes.Add(scenePath, ResourceLoader.Load<PackedScene>(scenePath));

        return cachedScenes[scenePath];
    }


    private static bool MapFileIsValid(string mapFilePath)
    {
        return mapFilePath.IsAbsolutePath() && ResourceLoader.Exists(mapFilePath);
    }
}