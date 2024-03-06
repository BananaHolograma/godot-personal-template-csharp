
namespace GameRoot;

using System.Linq;
using Godot;

[GlobalClass]
public partial class RoomRotatorComponent3D : Node
{
	[Export] public Node3D Room;
	[Export] public float RotationTime = .45f;
	[Export] public RotationModes RotationMode = RotationModes.SMOOTHLY;
	public enum RotationModes
	{
		SMOOTHLY,
		COMPRESSION
	}

	public Tween TweenMovement;


	public override void _UnhandledInput(InputEvent @event)
	{
		if (Input.IsActionJustPressed("rotate_left"))
			RotateTarget(Vector2.Left);

		if (Input.IsActionJustPressed("rotate_right"))
			RotateTarget(Vector2.Right);
	}

	public void RotateTarget(Vector2 direction)
	{
		switch (RotationMode)
		{
			case RotationModes.SMOOTHLY:
				RotateSmoothly(direction);
				break;
			case RotationModes.COMPRESSION:
				RotateWithCompressionEffect(direction);
				break;
			default:
				RotateSmoothly(direction);
				break;
		}
	}

	public void RotateWithCompressionEffect(Vector2 direction)
	{
		if (CanRotate(direction))
		{
			TweenMovement = CreateTween();
			TweenMovement
				.TweenProperty(Room, "transform:basis", Room.Transform.Basis.Rotated(Vector3.Up, -Mathf.Sign(direction.X) * Mathf.Pi / 2), RotationTime)
				.SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.Out);
		}
	}

	public void RotateSmoothly(Vector2 direction)
	{
		if (CanRotate(direction))
		{
			TweenMovement = CreateTween();
			TweenMovement
				.TweenProperty(Room, "rotation_degrees:y", Room.RotationDegrees.Y + (-Mathf.Sign(direction.X) * Mathf.RadToDeg(Mathf.Pi / 2)), RotationTime)
				.SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.Out);
		}
	}

	private bool CanRotate(Vector2 direction)
	{
		bool isLeftOrRight = new[] { Vector2.Left, Vector2.Right }.Any(dir => dir == direction);

		return isLeftOrRight && TweenMovement != null && !TweenMovement.IsRunning();
	}

}
