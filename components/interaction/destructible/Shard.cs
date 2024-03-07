using Godot;

namespace GameRoot;

public partial class Shard : RigidBody3D
{
    private readonly RandomNumberGenerator rng = new();

    public override void _EnterTree()
    {
        PhysicsMaterialOverride = new PhysicsMaterial { Bounce = rng.RandfRange(.05f, .2f), Friction = rng.RandfRange(.95f, 1f) };
        CollisionLayer = 256;
        CollisionMask = 1 | 4 | 8 | 128; // Interact with world, player, enemies and throwables;
        AddToGroup("shards");
    }
}