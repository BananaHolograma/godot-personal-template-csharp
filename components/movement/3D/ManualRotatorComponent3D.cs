namespace GameRoot;

using Godot;

[GlobalClass]
public partial class ManualRotatorComponent3D : Node
{
    [Export]
    public float MouseSensitivity = 3f;

    [Export]
    public Node3D Target;


    public override void _Input(InputEvent @event)
    {
        if (Target is not null && InputMap.HasAction("manual_rotate") && @event.IsActionPressed("manual_rotate") && @event is InputEventMouseMotion motion)
        {
            float mouseSensitivity = 1000 * MouseSensitivity;

            Target.RotateX(motion.Relative.Y / mouseSensitivity);
            Target.RotateY(motion.Relative.X / mouseSensitivity);
        }
    }

}