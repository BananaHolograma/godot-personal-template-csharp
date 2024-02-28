namespace GameRoot;

using Godot;

[GlobalClass]
public partial class Hitbox2D : Area2D
{

    public override void _Ready()
    {
        Monitorable = true;
        Monitoring = false;
        CollisionMask = 0;
        CollisionLayer = 16;
    }

}
