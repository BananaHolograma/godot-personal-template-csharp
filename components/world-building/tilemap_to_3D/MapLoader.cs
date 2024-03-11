using Godot;
using Godot.Collections;
using GodotExtensions;

namespace GameRoot;

public record MapBlockSize(Vector2 Size, int Height);

[Tool]
[GlobalClass]
public partial class MapLoader : Node3D
{
    [Export(PropertyHint.File)] public string MapFilePath = string.Empty;
    [Export] public bool MergeMeshes = true;
    [Export] public bool GenerateFloorCollisions = true;
    [Export] public bool GenerateCeilCollisions = false;
    [Export] public bool GenerateWallsCollisions = true;
    [Export] public Vector2 DefaultGridPlaneSize = new(1, 1);
    [Export] public int DefaultGridPlaneHeight = 3;
    [Export] public Vector3 DefaultGridBoxSize = new(1, 3, 1);
    [Export] public int DefaultGridBoxHeight = 3;

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

    public enum MapBlockMode
    {
        PLANE,
        BOX
    }
    internal enum TileDataLayer
    {
        MAPBLOCK_SCENE,
        MAPBLOCK_GRID_SIZE,
        MAPBLOCK_HEIGHT
    }
    private bool _generateMap = false;
    private bool _clearMap = false;
    private readonly Dictionary<string, PackedScene> cachedScenes = new();
    private readonly System.Collections.Generic.Dictionary<string, SingleMeshMerged> singleMeshes = new();


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
                singleMeshes.Clear();

                Node3D mapLevelRoot = CreateMapLevelRootNode(mapRoot, map2D, layer);
                Array<Vector2I> cells = tilemap.GetUsedCells(layer);

                foreach (Vector2I cell in cells)
                {
                    TileData data = tilemap.GetCellTileData(layer, cell);

                    if (data is not null)
                    {
                        PackedScene mapBlockScene = ObtainSceneFromCustomTileData(data);

                        if (mapBlockScene == null)
                            continue;

                        MapBlockSize mapBlockSize = ObtainMapBlockSizeFromCustomTileData(data);
                        MapBlock mapBlock = mapBlockScene.Instantiate() as MapBlock;

                        if (layer == 1)
                            GD.Print(mapBlockSize.Size, mapBlockSize.Height);
                        mapBlock.Translate(new Vector3(cell.X * mapBlockSize.Size.X, 0, cell.Y * mapBlockSize.Size.Y));
                        mapLevelRoot.AddChild(mapBlock);
                        mapBlock.ChangeSize(mapBlockSize.Size, mapBlockSize.Height);
                        mapBlock.UpdateFaces(GetCellNeighbours(cells, cell));
                        mapBlock.SetOwnerToEditedSceneRoot();

                        if (MergeMeshes)
                        {
                            foreach (MeshInstance3D meshToMerge in mapBlock.VisibleMeshes())
                            {
                                string type = GetKeyTypeFromMesh(meshToMerge);

                                if (!singleMeshes.ContainsKey(type))
                                    singleMeshes.Add(type, new SingleMeshMerged(type));

                                if (singleMeshes.TryGetValue(type, out SingleMeshMerged singleMeshMerged))
                                    singleMeshMerged.MergeMesh(meshToMerge);

                            }
                        }
                    }
                }

                if (mapLevelRoot.GetChildCount() == 0)
                {
                    mapLevelRoot.QueueFree();
                    continue;
                }

                if (MergeMeshes)
                {
                    mapLevelRoot.QueueFreeChildren();

                    foreach (SingleMeshMerged singleMeshMerged in singleMeshes.Values)
                        singleMeshMerged.AddToTree(mapLevelRoot, GenerateCollisionsOnType(singleMeshMerged.GetMeshType()));
                }
            }
        }
        else
        {
            GD.PushError($"The map file {MapFilePath} is not valid, make sure is not empty and the path points to an existing file");
        }
    }

    private Node3D CreateMapRootNode(Map2D map2D)
    {
        Node3D mapRoot = new() { Name = map2D.MapName };
        AddChild(mapRoot);

        mapRoot.SetOwnerToEditedSceneRoot();

        return mapRoot;
    }

    private Node3D CreateMapLevelRootNode(Node3D mapRoot, Map2D map2D, int layer)
    {
        Node3D mapLayerRoot = new() { Name = $"{map2D.MapName}_Level{layer}" };
        mapRoot.AddChild(mapLayerRoot);

        mapLayerRoot.SetOwnerToEditedSceneRoot();

        return mapLayerRoot;
    }

    private PackedScene ObtainSceneFromCustomTileData(TileData data)
    {
        string scenePath = (string)data.GetCustomDataByLayerId((int)TileDataLayer.MAPBLOCK_SCENE);

        if (cachedScenes.TryGetValue(scenePath, out PackedScene mapBlockScene))
            return mapBlockScene;

        if (MapFileIsValid(scenePath))
        {
            cachedScenes.Add(scenePath, ResourceLoader.Load<PackedScene>(scenePath));
            return cachedScenes[scenePath];
        }

        return null;
    }

    private MapBlockSize ObtainMapBlockSizeFromCustomTileData(TileData data)
    {
        Vector2 mapBlockSize = (Vector2)data.GetCustomDataByLayerId((int)TileDataLayer.MAPBLOCK_GRID_SIZE);
        int mapBlockHeight = (int)data.GetCustomDataByLayerId((int)TileDataLayer.MAPBLOCK_HEIGHT);

        if (mapBlockSize.IsZeroApprox())
            mapBlockSize = DefaultGridPlaneSize;

        if (mapBlockHeight == 0)
            mapBlockHeight = DefaultGridPlaneHeight;

        return new MapBlockSize(mapBlockSize, mapBlockHeight);
    }

    private static bool MapFileIsValid(string mapFilePath)
    {
        return mapFilePath.IsAbsolutePath() && ResourceLoader.Exists(mapFilePath);
    }

    private static string GetKeyTypeFromMesh(MeshInstance3D mesh)
    {
        string name = mesh.Name.ToString().Trim().ToLower();

        return name.Contains("wall") ? "walls" : name;
    }

    private bool GenerateCollisionsOnType(string name)
    {
        return name switch
        {
            "floor" => GenerateFloorCollisions,
            "ceil" => GenerateCeilCollisions,
            "walls" => GenerateWallsCollisions,
            _ => false,
        };
    }
    private static Dictionary<Vector2, bool> GetCellNeighbours(Array<Vector2I> cells, Vector2I cell)
    {
        return new() {
            {Vector2.Up, cells.Contains(cell + Vector2I.Up)},
            {Vector2.Down, cells.Contains (cell + Vector2I.Down)},
            {Vector2.Left, cells.Contains(cell + Vector2I.Left)},
            {Vector2.Right, cells.Contains(cell + Vector2I.Right)},
        };
    }

    private static bool HasNeighbour(Array<Vector2I> cells, Vector2I cell, Vector2I direction)
    {
        return cells.Contains(cell + direction);
    }
    public void ClearGeneratedMap()
    {
        this.QueueFreeChildren();
    }
}

public class SingleMeshMerged
{
    public string MeshName { get; private set; }
    public ArrayMesh ArrayMesh { get; set; }
    public SurfaceTool SurfaceTool { get; private set; }
    private string Type { get; }

    public SingleMeshMerged(string meshName)
    {
        Type = meshName.Trim().ToLower();
        MeshName = $"{meshName.Capitalize()}SingleMesh";
        ArrayMesh = new ArrayMesh();
        SurfaceTool = new SurfaceTool();
    }

    public string GetMeshType() => Type;
    public void MergeMesh(MeshInstance3D meshToMerge)
    {
        SurfaceTool.AppendFrom(meshToMerge.Mesh, 0, meshToMerge.GlobalTransform);
        ArrayMesh = SurfaceTool.Commit();
    }

    public void AddToTree(Node3D parent, bool generateCollisions = true)
    {
        MeshInstance3D mesh = new() { Name = MeshName };
        parent.AddChild(mesh);
        mesh.Mesh = ArrayMesh;
        mesh.SetOwnerToEditedSceneRoot();

        if (generateCollisions)
        {
            StaticBody3D body = new() { Name = $"{MeshName}StaticBody" };
            CollisionShape3D collision = new() { Name = $"{MeshName}Collision", Shape = mesh.Mesh.CreateTrimeshShape() };
            body.AddChild(collision);
            mesh.AddChild(body);
            body.SetOwnerToEditedSceneRoot();
            collision.SetOwnerToEditedSceneRoot();
        }
    }
}