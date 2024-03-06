using System.Collections.Generic;
using System.Linq;
using GameRoot;
using Godot;
using Godot.Collections;
using GodotExtensions;

public partial class Telekinesis : Node3D
{
    #region Signals
    [Signal]
    public delegate void PulledThrowableEventHandler(Throwable3D body);
    [Signal]
    public delegate void ThrowedThrowableEventHandler(Throwable3D body);

    #endregion Signals

    #region Exports
    [Export] public CharacterBody3D Actor;
    [ExportGroup("Interactor ray")]
    [Export] public RayCast3D Interactor;
    [Export] public float InteractorDistance = 5f;
    [ExportGroup("Slots")]
    [Export] public Marker3D RightSlot;
    [Export] public Marker3D LeftSlot;
    [Export(PropertyHint.Range, "1, 2, 1")] public int UsableSlots = 1;
    [ExportGroup("Force parameters")]
    [Export] public float PullPower = 20f;
    [Export] public float ThrowPower = 15f;
    [Export] public float MassLiftForce = 1.5f;

    #endregion
    public Area3D ObjectDetector;
    public Array<Marker3D> AvailableSlots = new();
    public Array<Throwable3D> ActiveBodies = new();

    public int TotalSlotPoints
    {
        get => _totalslotPoints;
        set { _totalslotPoints = Mathf.Max(0, value); }
    }
    private int _totalslotPoints = 0;

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Input.IsActionJustPressed("pull") && ThereAreFreeSlots())
        {
            if (Interactor.IsColliding())
            {
                Throwable3D body = Interactor.GetCollider() as Throwable3D;

                if (CanBeLifted(body))
                    PullBody(body);
            }
            else
            {
                foreach (Throwable3D body in RetrieveNearThrowables())
                {
                    if (CanBeLifted(body))
                        PullBody(body);
                }
            }
        }

        if (Input.IsActionJustPressed("throw") && ActiveBodies.Count > 0)
        {
            ThrowBody(ActiveBodies.First());
        }
    }
    public override void _Ready()
    {
        PrepareInteractor();
        PrepareObjectDetector();
        AvailableSlots = new() { RightSlot, LeftSlot };
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (Throwable3D body in ActiveBodies)
        {
            PullForceBasedOnThrowableMode(body, delta);
        }
    }

    public bool CanBeLifted(Throwable3D body)
    {
        return body.Mass <= MassLiftForce;
    }

    public Marker3D GetNearFreeSlot()
    {
        return AvailableSlots.Where(slot => slot.GetChildCount() == 0).First();
    }

    public IEnumerable<Throwable3D> RetrieveNearThrowables()
    {
        return ObjectDetector.GetOverlappingBodies()
            .Where(body => body is Throwable3D)
            .OrderBy(body => body.GlobalDistanceTo(Actor))
            .Cast<Throwable3D>();
    }

    private void PullForceBasedOnThrowableMode(Throwable3D body, double delta)
    {
        if (body.GrabModeIsDynamic())
            body.UpdateLinearVelocity((body.Grabber.GlobalPosition - body.GlobalPosition) * PullPower);
        else if (body.GrabModeIsFreeze())
            body.GlobalPosition = body.GlobalPosition.MoveToward(body.Grabber.GlobalPosition, (float)(PullPower * delta));
    }

    private void PullBody(Throwable3D body)
    {
        if (!ThereAreFreeSlots() || TotalSlotPoints + body.SlotPoints > UsableSlots)
        {
            ObjectDetector.Monitoring = false;
            return;
        }

        Marker3D freeSlot = GetNearFreeSlot();

        if (freeSlot == null)
            return;

        body.Pull(freeSlot);
        ActiveBodies.Add(body);
        TotalSlotPoints += body.SlotPoints;

        EmitSignal(SignalName.PulledThrowable, body);
    }
    private void ThrowBody(Throwable3D body)
    {
        Vector3 impulse = Vector3.Forward.Z * GetViewport().GetCamera3D().GlobalTransform.Basis.Z.Normalized() * ThrowPower;

        body.Throw(impulse);
        ActiveBodies.Remove(body);

        ObjectDetector.Monitoring = true;
        TotalSlotPoints -= body.SlotPoints;

        EmitSignal(SignalName.ThrowedThrowable, body);
    }

    private bool ThereAreFreeSlots()
    {
        return TotalSlotPoints <= UsableSlots && UsableSlots > 1 ? AvailableSlots.Where(slot => slot.GetChildCount() == 0).Any() : ActiveBodies.Count == 0;
    }

    private void PrepareInteractor()
    {
        if (Interactor != null)
        {
            Interactor.CollisionMask = 128; // Throwable layer
            Interactor.TargetPosition = new Vector3(0, 0, InteractorDistance);
        }
    }
    private void PrepareObjectDetector()
    {
        ObjectDetector = GetNode<Area3D>("%ObjectDetector");
        ObjectDetector.Monitorable = false;
        ObjectDetector.Monitoring = true;
        ObjectDetector.Priority = 2;
        ObjectDetector.CollisionLayer = 0;
        ObjectDetector.CollisionMask = 128; // Throwable layer
    }
}
