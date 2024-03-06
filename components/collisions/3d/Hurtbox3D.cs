namespace GameRoot;

using Godot;

[GlobalClass]
public partial class Hurtbox3D : Area3D
{
    [Signal]
    public delegate void Hitbox3DDetectedEventHandler(Hitbox3D hitbox);

    public override void _Ready()
    {
        Monitoring = true;
        Monitorable = false;
        CollisionLayer = 0;
        CollisionMask = 16; // Hitboxes layer

        AreaEntered += OnAreaEntered;
    }

    public void OnAreaEntered(Area3D area)
    {
        if (area is Hitbox3D)
            EmitSignal(SignalName.Hitbox3DDetected, area);
    }
}
