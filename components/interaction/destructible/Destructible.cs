using System;
using System.Linq;
using Godot;
using Godot.Collections;
using GodotExtensions;

namespace GameRoot;

[GlobalClass]
public partial class Destructible : Node
{
    [Signal]
    public delegate void DestroyedEventHandler(int amount);

    #region Exports
    [Export] MeshInstance3D Target;
    [Export] Node3D ShardsContainer;
    [Export] public string GroupName = "shards";
    [Export] public ShardTypes ShardType = ShardTypes.BRICK;
    [Export] public int AmountOfShards = 100;
    [Export] public float MinShardSize = .01f;
    [Export] public float MaxShardSize = .05f;
    [Export] public ExplosionModes ExplosionMode = ExplosionModes.ALL_DIRECTIONS;
    [Export] public float MinExplosionPower = 4.5f;
    [Export] public float MaxExplosionPower = 7f;
    [Export(PropertyHint.Range, "-8, 8, 0.001")] public float ShardsGravityScale = .75f;
    [Export(PropertyHint.Range, "0.01, 1000, 0.01")] public float ShardMass = .9f;

    #endregion

    public enum ShardTypes
    {
        BOX,
        BRICK
    }
    public enum ExplosionModes
    {
        ALL_DIRECTIONS,
        HORIZONTAL,
        FORWARD,
        OPPOSITE_TO_CAMERA,
        TO_THE_CAMERA,
        LEFT,
        ASCENDANT,
        LANDSLIDE
    }

    public CollisionShape3D cachedCollision;
    public Mesh cachedMesh;
    public StandardMaterial3D TargetMaterial;

    private readonly RandomNumberGenerator rng = new();
    private Array<Mesh> MeshShapes = new() { new BoxMesh(), new CapsuleMesh(), new SphereMesh(), new CylinderMesh() };
    private readonly Array<Shard> Pool = new();
    public override void _Ready()
    {
        Destroyed += OnDestroyed;

        if (Target != null)
            TargetMaterial = (StandardMaterial3D)Target.Mesh.SurfaceGetMaterial(0);

        for (int i = 0; i < AmountOfShards; i++)
        {
            Pool.Add(new Shard());
        }
    }
    public void Destroy()
    {
        Target.Hide();
        CreateShards(AmountOfShards);
    }

    private void CreateShards(int amount = 200)
    {
        if (Target == null)
            return;

        foreach (int _ in Enumerable.Range(0, amount))
        {
            CallThreadSafe(MethodName.CreateShard);
        }

        EmitSignal(SignalName.Destroyed, amount);
    }

    private void CreateShard()
    {
        Shard body = Pool.First();
        Pool.RemoveAt(0);

        // The mesh scale is important to be modified before adding the mesh to the scene tree
        MeshInstance3D _mesh = new() { Scale = GenerateRandomShardScale() };

        if (_mesh.Scale.X < 0.25f && _mesh.Scale.Y < 0.25f && _mesh.Scale.Z < 0.25f)
            body.ContinuousCd = true;

        body.GravityScale = ShardsGravityScale;
        body.AddChild(_mesh);

        CreateMeshShape(_mesh);

        if (TargetMaterial != null)
            _mesh.Mesh.SurfaceSetMaterial(0, TargetMaterial);

        if (ShardsContainer == null)
            GetTree().Root.AddChild(body);
        else
            ShardsContainer.AddChild(body);

        CreateMeshCollision(body, _mesh);

        Vector3 spawnPosition = body.Position + GenerateRandomMeshSurfacePosition(_mesh);

        body.GlobalPosition = Target.GlobalPosition + spawnPosition;
        body.GlobalRotation = Target.GlobalRotation;
        body.AngularVelocity = Vector3.One.Generate3DRandomFixedDirection() * rng.RandfRange(.5f, 2.5f);
        body.ApplyImpulse(GenerateImpulse(ExplosionMode), spawnPosition.Flip().Normalized());
    }

    private Vector3 GenerateImpulse(ExplosionModes explosionMode)
    {
        float explosionPower = rng.RandfRange(MinExplosionPower, MaxExplosionPower);

        return explosionMode switch
        {
            ExplosionModes.ALL_DIRECTIONS => Vector3.One.Generate3DRandomDirection() * explosionPower,
            ExplosionModes.FORWARD => Vector3.Forward.RotateHorizontalRandom() * explosionPower,
            ExplosionModes.OPPOSITE_TO_CAMERA => GetViewport().GetCamera3D().GlobalDirectionTo(Target).RotateHorizontalRandom() * explosionPower,
            ExplosionModes.TO_THE_CAMERA => GetViewport().GetCamera3D().GlobalDirectionTo(Target).Flip().RotateHorizontalRandom() * explosionPower,
            ExplosionModes.HORIZONTAL => new Array<Vector3>() { Vector3.Left.RotateHorizontalRandom(), Vector3.Right.RotateHorizontalRandom() }.PickRandom() * explosionPower,
            ExplosionModes.LEFT => Vector3.Left.RotateHorizontalRandom() * explosionPower,
            ExplosionModes.ASCENDANT => Vector3.Up.RotateVerticalRandom() * explosionPower,
            ExplosionModes.LANDSLIDE => Vector3.Down.RotateVerticalRandom() * explosionPower,
            _ => Vector3.One.Generate3DRandomDirection() * explosionPower,
        };
    }

    private Vector3 GenerateRandomMeshSurfacePosition(MeshInstance3D _mesh)
    {
        if (rng.Randf() < 0.1)
            return Vector3.Zero;

        Vector3[] faces = _mesh.Mesh.GetFaces();
        Vector3 randomFace = faces[rng.Randi() % faces.Length] * 2;
        randomFace = randomFace with { X = Mathf.Abs(randomFace.X), Y = Mathf.Abs(randomFace.Y), Z = Mathf.Abs(randomFace.Z) };

        return new Vector3(rng.RandfRange(-randomFace.X, randomFace.X), rng.RandfRange(-randomFace.Y, randomFace.Y), rng.RandfRange(-randomFace.Z, randomFace.Z));
    }

    private Vector3 GenerateRandomShardScale()
    {
        Vector3 scale = Vector3.One * rng.RandfRange(MinShardSize, MaxShardSize);

        if (ShardType.Equals(ShardTypes.BRICK))
            // I reduce the scale of two axis randomly to create brick shards style
            scale = scale with { X = scale.X / rng.RandfRange(1.1f, 10f), Y = scale.Y / rng.RandfRange(2f, 10f) };

        return scale;
    }

    private void CreateMeshCollision(RigidBody3D body, MeshInstance3D mesh)
    {
        CollisionShape3D collision = new();

        /*  switch (Target.Mesh.GetType())
         {
             case Type tubeType when tubeType == typeof(TubeTrailMesh):

                 TubeTrailMesh tubeTrailMesh = Target.Mesh as TubeTrailMesh;
                 SphereShape3D tubeTrailShape = new()
                 {
                     Radius = tubeTrailMesh.Radius * (mesh.Scale.X / 2)
                 };

                 collision.Shape = tubeTrailShape;
                 break;

             case Type capsuleType when capsuleType == typeof(CylinderMesh):
                 CylinderMesh cylinderMesh = Target.Mesh as CylinderMesh;
                 CylinderShape3D cylinderShape = new();
                 cylinderMesh.TopRadius *= mesh.Scale.X / 2;
                 cylinderMesh.BottomRadius *= mesh.Scale.X / 2;
                 cylinderMesh.Height *= mesh.Scale.Y / 2;

                 collision.Shape = cylinderShape;
                 break;
             default:
                 BoxShape3D defaultShape = new() { Size = mesh.Scale };
                 collision.Shape = defaultShape;
                 break;
         } */

        BoxShape3D shape = new() { Size = mesh.Scale * 1.15f };

        collision.Shape = shape;
        body.AddChild(collision);
    }

    private void CreateMeshShape(MeshInstance3D mesh)
    {
        cachedMesh ??= new BoxMesh();
        mesh.Mesh = cachedMesh;
    }

    private void OnDestroyed(int amount)
    {
        Target?.QueueFree();
    }
}