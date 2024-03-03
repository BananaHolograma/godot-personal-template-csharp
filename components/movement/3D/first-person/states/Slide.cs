using Godot;
using System;

namespace GameRoot;

[GlobalClass]
public partial class Slide : Motion
{
	[Export] public float Speed = 4.5f;
	[Export] public double SlideTime = 1.2f;
	[Export] public float FrictionMomentum = .1f;
	[Export] public float SlideTilt = 5f;
	[Export] public bool ReduceSpeedGradually = true;
	public Timer SlideTimer;
	public Vector3 LastDirection = Vector3.Zero;
	public double DecreaseRate;
	public int SlideSide;

	public AnimationPlayer AnimationPlayer;
	public Node3D Head;
	public ShapeCast3D CeilShapeDetector;
	RandomNumberGenerator rng = new();
	public override void Ready()
	{
		CeilShapeDetector = Actor.GetNode<ShapeCast3D>("CeilShapeDetector");
		Head = Actor.GetNode<Node3D>("Head");
		AnimationPlayer = Actor.GetNode<AnimationPlayer>("AnimationPlayer");

		CreateSlideTimer();
	}

	public override void Enter()
	{
		if (IsInstanceValid(SlideTimer))
			SlideTimer.Start();

		TransformedInput.UpdateInputDirection(Actor);
		LastDirection = TransformedInput.WorldCoordinateSpaceDirection;
		DecreaseRate = SlideTime;
		AnimationPlayer.Play("crouch");
		SlideSide = Mathf.Sign(rng.RandiRange(-1, 1));
	}

	public override void Exit(State nextState)
	{
		if (IsInstanceValid(SlideTimer))
			SlideTimer.Stop();

		DecreaseRate = SlideTime;
		Head.Rotation = Head.Rotation with { Z = 0 };

		if (Head.Rotation.Z != 0)
		{
			Tween tween = CreateTween();
			tween.TweenProperty(Head, "rotation:z", 0, .5f).SetEase(Tween.EaseType.Out);
		}

		if (nextState is Jump)
			AnimationPlayer.PlayBackwards("crouch");
	}


	public override void PhysicsUpdate(double delta)
	{
		if (ReduceSpeedGradually)
			DecreaseRate -= delta;

		double momentum = DecreaseRate + FrictionMomentum;

		Actor.Velocity = Actor.Velocity with
		{
			X = (float)(LastDirection.X * momentum * Speed),
			Z = (float)(LastDirection.Z * momentum * Speed)
		};

		if (SlideTilt > 0)
			Head.Rotation = Head.Rotation with { Z = (float)Mathf.Lerp(Actor.Head.Rotation.Z, SlideSide * Mathf.DegToRad(SlideTilt), delta * 8d) };

		if (!CeilShapeDetector.IsColliding())
			DetectJump();

		Actor.MoveAndSlide();
	}


	private void CreateSlideTimer()
	{
		if (SlideTimer == null)
		{
			SlideTimer = new Timer
			{
				Name = "SlideTimer",
				WaitTime = SlideTime,
				ProcessCallback = Timer.TimerProcessCallback.Physics,
				Autostart = false,
				OneShot = true
			};

			AddChild(SlideTimer);
			SlideTimer.Timeout += OnSlideTimerTimeout;
		}
	}

	private void OnSlideTimerTimeout()
	{
		DetectCrouch();

		if (CeilShapeDetector.IsColliding())
		{
			FSM.ChangeStateTo("Crouch");
		}
		else
		{
			AnimationPlayer.PlayBackwards("crouch");
			FSM.ChangeStateTo("Walk");
		}
	}

}
