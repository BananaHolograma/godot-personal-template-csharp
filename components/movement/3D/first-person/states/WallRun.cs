using Godot;
using System.Collections.Generic;

namespace GameRoot;

public partial class WallRun : Motion
{
	[Export]
	public float CameraRotationAngle = .15f;
	[Export]
	public float WallGravity = 1.5f;
	[Export]
	public float Speed = 3f;
	[Export]
	public float InitialBoostSpeed = 2f;

	internal enum WallSide
	{
		RIGHT,
		LEFT,
		FRONT
	}
	public Dictionary<string, Vector3> WallNormals = new() { };
	public Vector3 WallNormal = Vector3.Zero;
	private WallSide CurrentWallSide = WallSide.LEFT;

	public override void Enter()
	{
		Actor.Velocity += InitialBoostSpeed * Vector3.Forward.Rotated(Vector3.Up, Actor.GlobalTransform.Basis.GetEuler().Y).Normalized();

		//WallNormal = GetWallNormal();

		RotateCameraBasedOnNormal(WallNormal);
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
		tween.TweenProperty(Actor.Eyes, "rotation:z", Rotation, .3f).SetTrans(Tween.TransitionType.Cubic);
	}
}
