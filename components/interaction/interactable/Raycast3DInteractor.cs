using Godot;

namespace GameRoot;

[GlobalClass]
public partial class Raycast3DInteractor : RayCast3D, IInteractor
{
    [Export]
    public CharacterBody3D actor;
    public Interactable3D CurrentInteractable;
    public bool Focused = false;
    public bool Interacting = false;


    public override void _EnterTree()
    {

        Enabled = true;
        ExcludeParent = true;
        CollisionMask = 2;
        CollideWithAreas = true;
        CollideWithBodies = false;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (InputMap.HasAction("interact") && Input.IsActionJustPressed("interact") && CurrentInteractable is not null && !Interacting)
        {
            Interact(CurrentInteractable);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Interactable3D detectedInteractable = IsColliding() ? (Interactable3D)GetCollider() : null;

        if (detectedInteractable != null && detectedInteractable != CurrentInteractable)
        {
            CurrentInteractable = detectedInteractable;

            if (!Focused)
            {
                Focus(CurrentInteractable);
            }
            else
            {
                if (!Interacting)
                {
                    UnFocus(CurrentInteractable);
                    CurrentInteractable = null;
                }
            }
        }
    }
    public void Interact(Interactable3D interactable)
    {
        if (interactable is not null)
        {
            if (interactable.IsScannable())
            {
                Interacting = true;
                Enabled = false;
            }

            if (interactable.HasSignal(Interactable3D.SignalName.Interacted))
            {
                interactable.EmitSignal(Interactable3D.SignalName.Interacted, this);
            }
        }

    }

    public void CancelInteract(Interactable3D interactable)
    {
        Interacting = false;
        interactable ??= CurrentInteractable;

        if (interactable is not null)
        {
            if (interactable.IsScannable())
            {
                Enabled = true;
            }

            if (interactable.HasSignal(Interactable3D.SignalName.CanceledInteraction))
            {
                interactable.EmitSignal(Interactable3D.SignalName.CanceledInteraction, this);
            }
        }

        CurrentInteractable = null;
    }

    public void Focus(Interactable3D interactable)
    {
        if (interactable.HasSignal(Interactable3D.SignalName.Focused))
        {
            interactable.EmitSignal(Interactable3D.SignalName.Focused, this);
        }

        Focused = true;

    }

    public void UnFocus(Interactable3D interactable)
    {
        interactable ??= CurrentInteractable;

        if (interactable is not null && interactable.HasSignal(Interactable3D.SignalName.UnFocused))
        {
            interactable.EmitSignal(Interactable3D.SignalName.UnFocused, this);
        }

        Focused = false;
    }
}