using Godot;
using GodotExtensions;

namespace GameRoot;

public class AnyToVaultTransition : Transition
{
    // This check it's limited to static bodies with collision box shapes, let's revisit in the future to handle more type of obstacle shapes
    public override bool ShouldTransition()
    {
        if (ToState is Vault vault)
        {
            Vector3 normal = vault.FrontWallDetector.GetCollisionNormal();

            if (vault.FrontWallDetector.GetCollider() is not StaticBody3D staticBody || normal.IsZeroApprox())
                return false;

            CollisionShape3D collision = staticBody.FirstNodeOfType<CollisionShape3D>();

            if (collision != null && collision.Shape is BoxShape3D obstacle)
            {
                float obstacleSize = SizeBasedOnCollisionNormal(obstacle, normal);

                if (obstacleSize > 0 && obstacleSize <= vault.VaultMaxObstacleSize && obstacle.Size.Y <= vault.VaultMaxObstacleHeight)
                {
                    CapsuleShape3D characterCollisionShape = (CapsuleShape3D)vault.Actor.GetNode<CollisionShape3D>("StandCollisionShape").Shape;

                    vault.MovementAmount = normal.Flip() * (obstacleSize + characterCollisionShape.Radius + .5f);
                }

                return vault.MovementAmount.IsNotZeroApprox();
            }
        }

        return false;
    }
    public override void OnTransition() { }

    private float SizeBasedOnCollisionNormal(BoxShape3D obstacle, Vector3 normal)
    {
        float size = 0;

        if (normal.IsEqualApprox(Vector3.Forward) || normal.IsEqualApprox(Vector3.Back))
        {
            size = Mathf.Abs(obstacle.Size.Z);
        }

        if (normal.IsEqualApprox(Vector3.Left) || normal.IsEqualApprox(Vector3.Right))
        {
            size = Mathf.Abs(obstacle.Size.X);
        }

        return size;
    }
}