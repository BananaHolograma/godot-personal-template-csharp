using System;
using System.Linq;
using Godot;
using Godot.Collections;
using GodotExtensions;

namespace GameRoot;
public partial class PushWaveArea : Area3D
{
	[Signal]
	public delegate void ActivatedEventHandler();

	[Export] public int PushableBodies = 7;
	[Export] public float MinPushForce = 5f;
	[Export] public float MaxPushForce = 20f;
	[Export] public float WaveSpeed = 2f;
	[Export] public float WaveRadius = 5f;
	[Export] public float TimeAlive = 1f;
	public bool Active
	{
		get => _active;
		set { _active = value; Monitoring = value; SetPhysicsProcess(value); }
	}
	private bool _active = false;
	public Vector3 Direction = Vector3.Forward;
	private readonly Dictionary<string, RigidBody3D> BodiesPushed = new();
	private readonly RandomNumberGenerator rng = new();
	private Timer AliveTimer;

	public override void _Ready()
	{
		Monitorable = false;
		Monitoring = Active;
		CollisionLayer = 0;
		CollisionMask = 1 | 128 | 256;

		AddChild(new CollisionShape3D() { Shape = new SphereShape3D() { Radius = WaveRadius } });
		CreateAliveTimer();
		SetPhysicsProcess(Active);
	}

	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition += WaveSpeed * Direction;
		PushBodiesOnRange();
	}
	public void Activate()
	{
		if (IsInstanceValid(AliveTimer))
			AliveTimer.Start();

		if (!Active)
			EmitSignal(SignalName.Activated);

		Active = true;
	}
	public void PushBodiesOnRange()
	{
		foreach (RigidBody3D body in GetOverlappingBodies().Where(body => !BodiesPushed.ContainsKey(body.Name)).Cast<RigidBody3D>())
		{
			BodiesPushed.Add(body.Name, body);
			body.LinearDamp = .1f;
			body.AngularDamp = .1f;
			body.AngularVelocity = Vector3.One.Generate3DRandomFixedDirection() * rng.RandfRange(0, 3f);
			body.ApplyImpulse(Direction.RotateHorizontalRandom() * rng.RandfRange(MinPushForce, MaxPushForce), body.Position.Flip().Normalized());
		}

		Active = BodiesPushed.Count < PushableBodies;
	}

	private void CreateAliveTimer()
	{
		if (AliveTimer == null)
		{
			AliveTimer = new Timer
			{
				Name = "AliveTimer",
				WaitTime = MathF.Max(.05f, TimeAlive),
				ProcessCallback = Timer.TimerProcessCallback.Physics,
				Autostart = false,
				OneShot = true
			};

			AddChild(AliveTimer);
			AliveTimer.Timeout += OnAliveTimerTimeout;
		}
	}

	private void OnAliveTimerTimeout()
	{
		QueueFree();
	}

}
