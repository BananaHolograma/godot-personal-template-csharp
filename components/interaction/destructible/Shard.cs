using Godot;

namespace GameRoot;

public partial class Shard : RigidBody3D
{
    private readonly RandomNumberGenerator rng = new();

    public override void _EnterTree()
    {
        PhysicsMaterialOverride = new PhysicsMaterial { Bounce = rng.Randf(), Friction = rng.Randf() };
        CollisionLayer = 256;
        CollisionMask = 1 | 4 | 8 | 128; // Interact with world, player, enemies and throwables;
        AddToGroup("shards");
    }
}