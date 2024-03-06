using Godot;

namespace GameRoot;

public partial class Shard : RigidBody3D
{
    public override void _EnterTree()
    {
        CollisionLayer = 256;
        CollisionMask = 1 | 4 | 8 | 128; // Interact with world, player, enemies and throwables;
        AddToGroup("shards");
    }
}