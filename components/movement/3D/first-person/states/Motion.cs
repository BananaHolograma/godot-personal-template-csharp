using Godot;

namespace GameRoot;
public partial class Motion : State
{
	#region Signals
	[Signal]
	public delegate void GravityEnabledEventHandler();
	[Signal]
	public delegate void GravityDisabledEventHandler();
	#endregion

	#region Exports
	[Export]
	public FirstPersonController Actor;

	[ExportGroup("Gravity")]
	[Export]
	public double Gravity = 30f;

	[Export]
	public bool GravityActive
	{
		get => gravityActive;
		set
		{
			if (value != gravityActive)
			{
				EmitSignal(value ? SignalName.GravityEnabled : SignalName.GravityDisabled);
			}

			gravityActive = value;
		}
	}

	[Export]
	public float FallVelocityLimit = 300f;

	[ExportGroup("Motion")]
	[Export]
	public float Friction = 7f;
	[Export]
	public bool StairSteppingEnabled = true;
	[Export]
	public float MaxStepUp = .5f;
	[Export]
	public float MaxStepDown = .5f;
	[Export]
	public Vector3 Vertical = new(0, 1, 0);
	[Export]
	public Vector3 Horizontal = new(1, 0, 1);

	#endregion
	private bool gravityActive = true;

	public TransformedInput TransformedInput = new();

	public bool IsGrounded = true;
	public bool WasGrounded = true;
	public bool StairtStepping = false;

	public override void PhysicsUpdate(double delta)
	{
		WasGrounded = IsGrounded;
		IsGrounded = Actor.IsOnFloor();

		TransformedInput.UpdateInputDirection(Actor);

		if (GravityActive && !Actor.IsOnFloor() && !FSM.CurrentStateIs("Jump"))
		{
			Actor.Velocity += Vector3.Down * (float)(Gravity * delta);
		}

		/* if (IsFalling() && !StairtStepping)
		{
			FSM.ChangeStateTo("Fall");
		} */

	}

	public bool IsFalling()
	{
		return Actor.Velocity.Y < 0 && WasGrounded && !IsGrounded && !FSM.CurrentStateIs("Fall");
	}

	public void Move(float speed, double delta)
	{
		Vector3 worldCoordinateSpaceDirection = TransformedInput.WorldCoordinateSpaceDirection;

		if (worldCoordinateSpaceDirection.IsZeroApprox())
		{
			Actor.Velocity = Actor.Velocity.Lerp(
				new Vector3(worldCoordinateSpaceDirection.X * speed, Actor.Velocity.Y, worldCoordinateSpaceDirection.Z * speed)
				, (float)delta * Friction);
		}
		else
		{
			Actor.Velocity = Actor.Velocity with { X = worldCoordinateSpaceDirection.X * speed, Z = worldCoordinateSpaceDirection.Z * speed };
		}
	}

	public void EnableGravity()
	{
		GravityActive = true;
	}

	public void DisableGravity()
	{
		GravityActive = false;
	}
}

public class TransformedInput
{
	public Vector2 InputDirection { get; set; }
	public Vector3 WorldCoordinateSpaceDirection { get; set; }

	public void UpdateInputDirection(FirstPersonController actor)
	{
		InputDirection = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		WorldCoordinateSpaceDirection = (actor.Transform.Basis * new Vector3(InputDirection.X, 0, InputDirection.Y)).Normalized();
	}
}