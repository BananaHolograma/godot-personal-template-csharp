using Godot;
using GodotExtensions;
using System.Collections.Generic;

namespace GameRoot;

[GlobalClass]
public partial class WallRun : Motion
{
	#region Exports
	[Export] public float CameraRotationAngle = .15f;
	[Export] public float WallGravity = 1.5f;
	[Export] public float Speed = 3f;
	[Export] public float HorizontalBoostSpeed = 2.5f;
	[Export] public float VerticalBoostSpeed = 1.5f;

	#endregion
	internal enum WallSide
	{
		RIGHT,
		LEFT,
		FRONT
	}

	//This back walls detectors are activated in order to not fall the player when rotate around the wall to select the jump direction
	public RayCast3D BackWallDetectorLeft;
	public RayCast3D BackWallDetectorRight;

	public Node3D Eyes;

	public Dictionary<string, Vector3> WallNormals = new() { };
	public Vector3 CurrentWallNormal = Vector3.Zero;
	private WallSide CurrentWallSide = WallSide.LEFT;
	public Vector3 Direction = Vector3.Zero;
	public float CurrentGravity;

	private readonly Dictionary<string, WallSide> WallSideLookup = new();

	public override void Ready()
	{
		BackWallDetectorLeft = Actor.GetNode<RayCast3D>("%BackWallDetectorLeft");
		BackWallDetectorRight = Actor.GetNode<RayCast3D>("%BackWallDetectorRight");

		Eyes = Actor.GetNode<Node3D>("%Eyes");

		WallSideLookup[RightWallDetector.Name] = WallSide.RIGHT;
		WallSideLookup[LeftWallDetector.Name] = WallSide.LEFT;
		WallSideLookup[FrontWallDetector.Name] = WallSide.FRONT;
	}

	public override void Enter()
	{
		BackWallDetectorLeft.Enabled = true;
		BackWallDetectorRight.Enabled = true;

		CurrentGravity = WallGravity;
		CurrentWallNormal = GetWallNormal();

		Actor.Velocity += CurrentWallSide.Equals(WallSide.FRONT) ?
			HorizontalBoostSpeed * Vector3.Up.Rotated(Vector3.Right, Actor.GlobalTransform.Basis.GetEuler().X).Normalized() :
			VerticalBoostSpeed * Vector3.Forward.Rotated(Vector3.Up, Actor.GlobalTransform.Basis.GetEuler().Y).Normalized();

		RotateCameraBasedOnNormal(CurrentWallNormal);
	}

	public override void Exit(State _nextState)
	{
		BackWallDetectorLeft.Enabled = false;
		BackWallDetectorRight.Enabled = false;

		Tween tween = CreateTween();
		tween.TweenProperty(Eyes, "rotation:z", 0, .3f).SetTrans(Tween.TransitionType.Cubic);

		CurrentGravity = WallGravity;
		CurrentWallNormal = Vector3.Zero;

		WallNormals = new() { };
	}

	public override void PhysicsUpdate(double delta)
	{
		TransformedInput.UpdateInputDirection(Actor);

		if (Actor.IsOnFloor())
		{
			if (TransformedInput.WorldCoordinateSpaceDirection.IsZeroApprox())
			{
				FSM.ChangeStateTo("Idle");
			}
			else
			{
				FSM.ChangeStateTo("Walk");
			}

			return;
		}

		if (WallGravity > 0)
			ApplyGravity(CurrentGravity, delta);


		if (!WallDetected())
		{
			if (CurrentWallSide.Equals(WallSide.FRONT))
			{
				FSM.ChangeStateTo("Jump");
			}
			else
			{
				FSM.ChangeStateTo("Fall");
			}
		}

		Actor.MoveAndSlide();

		if (Actor.WallJump)
			DetectJump();
	}

	private Vector3 GetWallNormal()
	{
		Vector3 wallNormal = Vector3.Zero;

		foreach (string wallDetector in WallNormals.Keys)
		{
			if (WallNormals[wallDetector].IsNotZeroApprox())
			{
				wallNormal = WallNormals[wallDetector];
				CurrentWallSide = WallSideLookup[wallDetector];
				break;
			}
		}

		return wallNormal;
	}

	private void RotateCameraBasedOnNormal(Vector3 wallNormal)
	{
		if (wallNormal.IsZeroApprox())
			return;

		float Rotation = CameraRotationAngle;

		switch (CurrentWallSide)
		{
			case WallSide.RIGHT:
				Rotation *= 1;
				break;
			case WallSide.LEFT:
				Rotation *= -1;
				break;
			case WallSide.FRONT:
				Rotation = 0;
				break;
			default:
				Rotation = 0;
				break;
		}

		Tween tween = CreateTween();
		tween.TweenProperty(Eyes, "rotation:z", Rotation, .3f).SetTrans(Tween.TransitionType.Cubic);
	}

	private bool WallDetected()
	{
		return !Actor.IsOnFloor() &&
				(RightWallDetector.IsColliding() ||
				LeftWallDetector.IsColliding() ||
				FrontWallDetector.IsColliding() ||
				BackWallDetectorLeft.IsColliding() ||
				BackWallDetectorRight.IsColliding());
	}
}
