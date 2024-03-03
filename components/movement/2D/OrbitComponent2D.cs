namespace GameRoot;

using System;
using Godot;

[GlobalClass]
public partial class OrbitComponent2D : Node2D
{
    #region Signals
    [Signal]
    public delegate void StartedEventHandler();
    [Signal]
    public delegate void StoppedEventHandler();
    #endregion

    #region Exports
    [Export] public Node2D RotationReference;
    [Export] public float Radius = 40f;
    [Export] public double AngleInRadians = Math.PI / 4;
    [Export] public double AngularVelocity = Math.PI / 2;
    #endregion

    public bool Active
    {
        get => active;
        set
        {
            if (value != active)
                EmitSignal(value ? SignalName.Started : SignalName.Stopped);

            active = value;
        }
    }

    private bool active;

    public override void _Process(double delta)
    {
        if (active)
            Orbit(delta);
    }

    public void Orbit(double delta)
    {
        if (RotationReference is not null)
        {
            Active = true;

            AngleInRadians += delta * AngularVelocity;
            AngleInRadians %= Math.PI * 2;

            Vector2 offset = new Vector2((float)Mathf.Cos(AngleInRadians), (float)Mathf.Sin(AngleInRadians)) * Radius;
            Position = RotationReference.Position + offset;
        }

    }

    public void Stop()
    {
        Active = false;
    }

}