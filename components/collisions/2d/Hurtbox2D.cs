namespace GameRoot;

using Godot;

[GlobalClass]
public partial class Hurtbox2D : Area2D
{
    [Signal]
    public delegate void Hitbox2DDetectedEventHandler();

    public override void _Ready()
    {
        Monitoring = true;
        Monitorable = false;
        CollisionMask = 16;

        AreaEntered += OnAreaEntered;
    }

    public void OnAreaEntered(Area2D area)
    {
        if (area is Hitbox2D)
        {
            EmitSignal(SignalName.Hitbox2DDetected, area);
        }

    }
}
