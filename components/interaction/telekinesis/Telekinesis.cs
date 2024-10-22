using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using GodotExtensions;

namespace GameRoot;

[GlobalClass]
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
    [Export] public RayCast3D ThrowableInteractor;
    [ExportGroup("Detector area")]
    [Export] public float ThrowableInteractorDistance = 5f;
    [Export] public Area3D ThrowableDetector;
    [ExportGroup("Slots")]
    [Export] public Marker3D RightSlot;
    [Export] public Marker3D LeftSlot;
    [Export(PropertyHint.Range, "1, 2, 1")] public int UsableSlots = 1;
    [ExportGroup("Force parameters")]
    [Export] public float PullPower = 20f;
    [Export] public float ThrowPower = 15f;
    [Export] public float MassLiftForce = 1.5f;

    #endregion
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
        // TODO - SEE WHAT CONTROLS CREATE THE PUSH WAVE
        if (Input.IsActionJustPressed("throw"))
            PushWave();

        if (Input.IsActionJustPressed("pull") && ThereAreFreeSlots())
        {
            // TODO - REVISIT THIS BEHAVIOUR AS CAN BE USED IN A MORE OPTIMAL WAY COMBINING RAYCAST & AREA OR NOT
            if (ThrowableInteractor.IsColliding())
            {
                Throwable3D body = ThrowableInteractor.GetCollider() as Throwable3D;

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
        PrepareThrowableDetector();

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
        return ThrowableDetector.GetOverlappingBodies()
            .Where(body => body is Throwable3D)
            .OrderBy(body => body.GlobalDistanceTo(Actor))
            .Cast<Throwable3D>();
    }

    public void PullForceBasedOnThrowableMode(Throwable3D body, double delta)
    {
        if (body.GrabModeIsDynamic())
            body.UpdateLinearVelocity((body.Grabber.GlobalPosition - body.GlobalPosition) * PullPower);
        else if (body.GrabModeIsFreeze())
            body.GlobalPosition = body.GlobalPosition.MoveToward(body.Grabber.GlobalPosition, (float)(PullPower * delta));
    }

    public void PullBody(Throwable3D body)
    {
        if (!ThereAreFreeSlots() || TotalSlotPoints + body.SlotPoints > UsableSlots)
        {
            ThrowableDetector.Monitoring = false;
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
    public void ThrowBody(Throwable3D body)
    {
        Vector3 impulse = GetViewport().GetCamera3D().ForwardDirection() * ThrowPower;

        body.Throw(impulse);
        ActiveBodies.Remove(body);

        ThrowableDetector.Monitoring = true;
        TotalSlotPoints -= body.SlotPoints;

        EmitSignal(SignalName.ThrowedThrowable, body);
    }


    public void PushWave()
    {
        PushWaveArea wave = new() { Direction = GetViewport().GetCamera3D().ForwardDirection(), Actor = Actor };
        AddChild(wave);

        wave.Activate();
    }

    public bool ThereAreFreeSlots()
    {
        return TotalSlotPoints <= UsableSlots && UsableSlots > 1 ? AvailableSlots.Where(slot => slot.GetChildCount() == 0).Any() : ActiveBodies.Count == 0;
    }

    private void PrepareInteractor()
    {
        if (ThrowableInteractor != null)
        {
            ThrowableInteractor.CollisionMask = 128; // Throwable layer
            ThrowableInteractor.TargetPosition = new Vector3(0, 0, -ThrowableInteractorDistance);
        }
    }
    private void PrepareThrowableDetector()
    {
        if (ThrowableDetector != null)
        {
            ThrowableDetector.Monitorable = false;
            ThrowableDetector.Monitoring = true;
            ThrowableDetector.Priority = 2;
            ThrowableDetector.CollisionLayer = 0;
            ThrowableDetector.CollisionMask = 128; // Throwable layer
        }

    }
}
