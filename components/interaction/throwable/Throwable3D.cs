using Godot;
using GodotExtensions;

namespace GameRoot;

[GlobalClass]
public partial class Throwable3D : RigidBody3D
{
    [Export] public GRAB_MODE GrabMode = GRAB_MODE.DYNAMIC;
    [Export] public int SlotPoints = 1;
    [Export(PropertyHint.Range, "0, 255, 1")] public int TransparencyOnPull = 255;

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
    public uint OriginalCollisionMask = 1 | 4 | 8 | 256; // Interact with world, player, enemies and shards
    public Vector3 CurrentLinearVelocity;

    public int OriginalTransparency;
    public Node3D Grabber;
    private GRAB_MODE CurrentGrabMode;
    private STATE CurrentState = STATE.NEUTRAL;

    public override void _Ready()
    {
        CurrentGrabMode = GrabMode;

        OriginalParent = GetParent();
        CollisionLayer = OriginalCollisionLayer;
        CollisionMask = OriginalCollisionMask;

        StandardMaterial3D material = (StandardMaterial3D)this.FirstNodeOfType<MeshInstance3D>().GetActiveMaterial(0);

        if (material != null)
            OriginalTransparency = material.AlbedoColor.A8;
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

        ApplyTransparency();
        CurrentState = STATE.PULL;
    }


    public void Throw(Vector3 impulse)
    {
        if (CurrentGrabMode.Equals(GRAB_MODE.FREEZE))
            Freeze = false;

        CollisionLayer = OriginalCollisionLayer;

        Reparent(OriginalParent);
        ApplyImpulse(impulse);

        Grabber = null;
        CurrentState = STATE.THROW;
        RecoverTransparency();
    }

    public void Drop()
    {
        if (CurrentGrabMode.Equals(GRAB_MODE.FREEZE))
            Freeze = false;

        CollisionLayer = OriginalCollisionLayer;

        Reparent(OriginalParent);

        LinearVelocity = Vector3.Zero;
        ApplyImpulse(Vector3.Zero);

        Grabber = null;
        CurrentState = STATE.NEUTRAL;
        RecoverTransparency();
    }

    public bool GrabModeIsFreeze() => CurrentGrabMode.Equals(GRAB_MODE.FREEZE);
    public bool GrabModeIsDynamic() => CurrentGrabMode.Equals(GRAB_MODE.DYNAMIC);

    private void ApplyTransparency()
    {
        if (TransparencyOnPull == 255)
            return;

        StandardMaterial3D material = (StandardMaterial3D)this.FirstNodeOfType<MeshInstance3D>().GetActiveMaterial(0);

        if (material != null)
        {
            material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            material.AlbedoColor = material.AlbedoColor with { A8 = TransparencyOnPull };
        }
    }

    private void RecoverTransparency()
    {
        if (TransparencyOnPull == 255)
            return;

        StandardMaterial3D material = (StandardMaterial3D)this.FirstNodeOfType<MeshInstance3D>().GetActiveMaterial(0);

        if (material != null)
            material.AlbedoColor = material.AlbedoColor with { A8 = OriginalTransparency };
    }
}



/* 1. Force vs.Impulse:

Force: Represents a continuous application of force over time (measured in newtons). When applied to a rigid body, it produces acceleration in the direction of the force.
Impulse: Represents a sudden change in momentum delivered instantly(measured in newton-seconds). Applying an impulse directly changes the linear or angular velocity of the rigid body based on the impulse magnitude and direction.

2. Application Point:

Central vs.Non-Central:
Central: Applied at the center of mass of the rigid body. This results in pure translation (linear movement) without rotation.
Non-Central: Applied at a specific point relative to the body's center of mass. This can produce both translation and rotation (torque) depending on the force/impulse direction and its distance from the center of mass. 

Continuous vs. Instantaneous: Use force methods (apply_force or apply_central_force) for situations where you want to continuously influence the body's movement over time. Use impulse methods (apply_impulse or apply_central_impulse) when you want to cause an immediate change in velocity without applying a prolonged force.
Central vs. Non-Central: Use central methods (apply_central_force or apply_central_impulse) if you only want to translate the body without introducing rotation. Use non-central methods (apply_force or apply_impulse) if you want to introduce both translation and rotation based on the application point relative to the center of mass.
Torque: Use apply_torque when you want to directly rotate the body around a specific axis without applying a force.
*/