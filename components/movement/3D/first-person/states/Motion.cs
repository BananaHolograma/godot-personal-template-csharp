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
	[ExportGroup("StairStepping")]
	[Export]
	public bool StairSteppingEnabled = true;
	[Export]
	public float MaxStepUp = .5f;
	[Export]
	public float MaxStepDown = .5f;
	[Export]
	public float StepDistanceToCheck = .1f;
	[Export]
	public int FloorMaxAngleCheckInDegrees = 20;
	[Export]
	public Vector3 Vertical = new(0, 1, 0);
	[Export]
	public Vector3 Horizontal = new(1, 0, 1);
	#endregion
	private bool gravityActive = true;

	public TransformedInput TransformedInput = new();

	public bool IsGrounded = true;
	public bool WasGrounded = true;
	public bool StairStepping = false;

	public override void PhysicsUpdate(double delta)
	{
		WasGrounded = IsGrounded;
		IsGrounded = Actor.IsOnFloor();

		TransformedInput.UpdateInputDirection(Actor);

		ApplyGravity(delta);

		if (IsFalling() && !StairStepping)
			FSM.ChangeStateTo("Fall");
	}
	public void ApplyGravity(double delta)
	{
		if (GravityActive && !Actor.IsOnFloor() && !FSM.CurrentStateIs("Jump"))
		{
			Actor.Velocity += Vector3.Down * (float)(Gravity * delta);
		}
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

	public void StairStepUp()
	{
		if (!StairSteppingEnabled)
			return;

		StairStepping = false;

		if (TransformedInput.WorldCoordinateSpaceDirection.IsZeroApprox())
			return;

		PhysicsTestMotionParameters3D BodyTestParams = new();
		PhysicsTestMotionResult3D BodyTestResult = new();
		Transform3D TestTransform = Actor.GlobalTransform; // Storing current global_transform for testing
		Vector3 distance = TransformedInput.WorldCoordinateSpaceDirection * StepDistanceToCheck; // Distance forward we want to check

		BodyTestParams.From = Actor.GlobalTransform; // Actor (Character) as origin point
		BodyTestParams.Motion = distance; // Go forward by current distance

		// Pre-check: Are we colliding?
		if (!PhysicsServer3D.BodyTestMotion(Actor.GetRid(), BodyTestParams, BodyTestResult))
			return;

		// 1- Move tes transform to collision location
		Vector3 remainder = BodyTestResult.GetRemainder();
		TestTransform = TestTransform.Translated(BodyTestResult.GetTravel()); //  Move test_transform by distance traveled before collision

		// 2 - Move TestTransform up to ceiling (if any)
		Vector3 StepUp = MaxStepUp * Vertical;

		BodyTestParams.From = TestTransform;
		BodyTestParams.Motion = StepUp;

		PhysicsServer3D.BodyTestMotion(Actor.GetRid(), BodyTestParams, BodyTestResult);
		TestTransform = TestTransform.Translated(BodyTestResult.GetTravel());

		// 3 - Move test_transform forward by remaining distance
		BodyTestParams.From = TestTransform;
		BodyTestParams.Motion = remainder;
		PhysicsServer3D.BodyTestMotion(Actor.GetRid(), BodyTestParams, BodyTestResult);
		TestTransform = TestTransform.Translated(BodyTestResult.GetTravel());

		// 4 - Project remaining along wall normal (if any). So you can walk into a wall and up a step
		if (BodyTestResult.GetCollisionCount() != 0)
		{
			float remainderLength = BodyTestResult.GetRemainder().Length();

			Vector3 wallNormal = BodyTestResult.GetCollisionNormal();
			float DotDivMagnitude = TransformedInput.WorldCoordinateSpaceDirection.Dot(wallNormal) / (wallNormal * wallNormal).Length();
			Vector3 projectedVector = (TransformedInput.WorldCoordinateSpaceDirection - DotDivMagnitude * wallNormal).Normalized();

			BodyTestParams.From = TestTransform;
			BodyTestParams.Motion = remainderLength * projectedVector;
			PhysicsServer3D.BodyTestMotion(Actor.GetRid(), BodyTestParams, BodyTestResult);
			TestTransform = TestTransform.Translated(BodyTestResult.GetTravel());
		}

		// 5 -  Move test_transform down onto step
		BodyTestParams.From = TestTransform;
		BodyTestParams.Motion = MaxStepUp * -Vertical;

		// Return if no collision
		if (!PhysicsServer3D.BodyTestMotion(Actor.GetRid(), BodyTestParams, BodyTestResult))
			return;

		TestTransform = TestTransform.Translated(BodyTestResult.GetTravel());

		// 5.5 Check floor normal for un-walkable slope
		Vector3 surfaceNormal = BodyTestResult.GetCollisionNormal();
		float tempFloorMaxAngle = Actor.FloorMaxAngle + Mathf.DegToRad(FloorMaxAngleCheckInDegrees);

		if (Mathf.Snapped(surfaceNormal.AngleTo(Vertical), 0.001f) > tempFloorMaxAngle)
			return;

		StairStepping = true;

		// 6 - Move player up
		//float StepUpDistance = TestTransform.Origin.Y - Actor.GlobalPosition.Y;
		Actor.GlobalPosition = Actor.GlobalPosition with { Y = TestTransform.Origin.Y };
	}

	public void StairStepDown()
	{
		if (!StairSteppingEnabled)
			return;

		StairStepping = false;

		if (Actor.Velocity.Y <= 0 && WasGrounded)
		{
			PhysicsTestMotionResult3D BodyTestResult = new();
			PhysicsTestMotionParameters3D BodyTestParams = new()
			{
				From = Actor.GlobalTransform, // We get the characters's current global_transform
				Motion = new Vector3(0, MaxStepDown, 0) // We project the player downward
			};

			if (PhysicsServer3D.BodyTestMotion(Actor.GetRid(), BodyTestParams, BodyTestResult))
			{
				StairStepping = true;
				//Enters if a collision is detected by BodyTestMotion
				//Get distance to step and move character downward by that much
				Actor.Position = Actor.Position with { Y = Actor.Position.Y + BodyTestResult.GetTravel().Y };
				Actor.ApplyFloorSnap();
				IsGrounded = true;
			}
		}
	}

	public void DetectJump()
	{
		if (Input.IsActionJustPressed("jump") && Actor.Jump && (Actor.IsOnFloor() || FSM.CurrentStateIs("WallRun")))
			FSM.ChangeStateTo("Jump");
	}

	public void DetectCrouch()
	{
		if (Input.IsActionPressed("crouch") && Actor.IsOnFloor() && Actor.Crouch)
			FSM.ChangeStateTo("Crouch");
	}

	public void DetectCrawl()
	{
		if (Input.IsActionPressed("crawl") && Actor.IsOnFloor() && Actor.Crawl)
			FSM.ChangeStateTo("Crawl");
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