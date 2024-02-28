namespace GameRoot;

using Godot;

[GlobalClass]
public partial class Hitbox3D : Area3D
{

    public override void _Ready()
    {
        Monitorable = true;
        Monitoring = false;
        CollisionLayer = 16;
        CollisionMask = 0;
    }

}
