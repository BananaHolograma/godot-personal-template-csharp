using Godot;

namespace GameRoot;

[GlobalClass]
public partial class Throwable3D : RigidBody3D
{
    [Export] public GRAB_MODE GrabMode = GRAB_MODE.DYNAMIC;

    public enum GRAB_MODE
    {
        FREEZE,
        DYNAMIC
    }
    internal enum STATE
    {
        NEUTRAL,
        PULL,
        THROW
    }

    public Node OriginalParent;
    public uint OriginalCollisionLayer = 128; // Throwable layer
    public uint OriginalCollisionMask = 1 | 4 | 8; // Interact with world, player and enemies
    public Vector3 CurrentLinearVelocity;
    public Node3D Grabber;
    private GRAB_MODE CurrentGrabMode;
    private STATE CurrentState = STATE.NEUTRAL;

    public override void _Ready()
    {
        CurrentGrabMode = GrabMode;

        OriginalParent = GetParent();
        CollisionLayer = OriginalCollisionLayer;
        CollisionMask = OriginalCollisionMask;
    }


    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        if (CurrentState.Equals(STATE.PULL))
        {
            state.LinearVelocity = CurrentLinearVelocity;
        }
    }

    public void UpdateLinearVelocity(Vector3 linearVelocity)
    {
        CurrentLinearVelocity = linearVelocity;
    }

    public void Pull(Node3D grabber)
    {
        if (Grabber == grabber)
            return;

        Grabber = grabber;
        CollisionLayer = 0;

        if (CurrentGrabMode.Equals(GRAB_MODE.FREEZE))
        {
            FreezeMode = FreezeModeEnum.Kinematic;
            Freeze = true;
        }

        Reparent(grabber);

        CurrentState = STATE.PULL;
    }

    public void Throw(Vector3 impulse)
    {
        if (CurrentGrabMode.Equals(GRAB_MODE.FREEZE))
            Freeze = false;

        Reparent(OriginalParent);

        CollisionLayer = OriginalCollisionLayer;
        ApplyImpulse(impulse);
        Grabber = null;
        CurrentState = STATE.THROW;
    }

    public bool GrabModeIsFreeze() => CurrentGrabMode.Equals(GRAB_MODE.FREEZE);
    public bool GrabModeIsDynamic() => CurrentGrabMode.Equals(GRAB_MODE.DYNAMIC);
}