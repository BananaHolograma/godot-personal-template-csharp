using Godot;
using GodotExtensions;

namespace GameRoot;

public class AnyToVaultTransition : Transition
{
    public override bool ShouldTransition()
    {
        if (ToState is Vault vault)
        {
            CsgBox3D obstacle = vault.FrontWallDetector.GetCollider() as CsgBox3D;
            Vector3 normal = vault.FrontWallDetector.GetCollisionNormal();

            float obstacleSize = SizeBasedOnCollisionNormal(obstacle, normal);

            if (obstacleSize > 0 && obstacleSize <= vault.VaultMaxObstacleSize && obstacle.Size.Y <= vault.VaultMaxObstacleHeight)
            {
                CapsuleShape3D characterCollisionShape = (CapsuleShape3D)vault.Actor.GetNode<CollisionShape3D>("StandCollisionShape").Shape;

                vault.MovementAmount = normal.Flip() * (obstacleSize + characterCollisionShape.Radius + .5f);
            }

            return vault.MovementAmount.IsNotZeroApprox();
        }

        return false;
    }
    public override void OnTransition() { }


    private float SizeBasedOnCollisionNormal(CsgBox3D obstacle, Vector3 normal)
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