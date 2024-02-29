namespace GameRoot;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

[GlobalClass]
public partial class DungeonGridMovement : Node
{
    #region Exports
    [Export]
    public int CellTravelSize = 2;
    [Export]
    public float MovementAnimationTime = .3f;
    #endregion
    public Node3D Target;
    public Tween TweenMovement;
    public Dictionary<Vector2, Vector3> DirectionMap = new()
    {
        [Vector2.Up] = Vector3.Forward,
        [Vector2.Down] = Vector3.Back,
        [Vector2.Right] = Vector3.Right,
        [Vector2.Left] = Vector3.Left,
    };

    public void Move(Vector2 direction)
    {
        if (TweenIsRunning() || Target == null) return;

        Vector3 localVector = DirectionMap[direction];

        TweenMovement = CreateTween();
        TweenMovement.TweenProperty(Target, "transform", Target.Transform.TranslatedLocal(localVector * CellTravelSize), MovementAnimationTime)
            .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
    }

    public void Rotate(Vector2 direction)
    {
        if (TweenIsRunning() || Target == null) return;

        bool isLeftOrRight = new[] { Vector2.Left, Vector2.Right }.Any(dir => dir == direction);

        if (isLeftOrRight)
        {
            TweenMovement = CreateTween();
            TweenMovement.TweenProperty(Target, "transform:basis", Target.Transform.Basis.Rotated(Vector3.Up, -Mathf.Sign(direction.X) * (float)Math.PI / 2), MovementAnimationTime)
                .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        }

    }

    private bool TweenIsRunning()
    {
        return TweenMovement is not null && TweenMovement.IsRunning();
    }
}

