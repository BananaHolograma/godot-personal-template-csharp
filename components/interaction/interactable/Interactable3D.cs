using System;
using Godot;

namespace GameRoot;

[GlobalClass]
public partial class Interactable3D : Area3D
{
    #region Signals
    [Signal]
    public delegate void FocusedEventHandler(GodotObject interactor);
    [Signal]
    public delegate void UnFocusedEventHandler(GodotObject interactor);
    [Signal]
    public delegate void InteractedEventHandler(GodotObject interactor);
    [Signal]
    public delegate void CanceledInteractionEventHandler(GodotObject interactor);

    #endregion

    [ExportGroup("InteractionParameters")]
    [Export(PropertyHint.ResourceType, nameof(InteractableParameters))] public InteractableParameters Parameters;
    [ExportGroup("Pointers")]
    [Export] public CompressedTexture2D FocusPointer;
    [Export] public CompressedTexture2D InteractPointer;
    [Export] public Node Target;

    public IInteractor actor;

    public override void _Ready()
    {
        Priority = 3;
        CollisionLayer = 2;
        CollisionMask = 0;
        Monitoring = false;
        Monitorable = true;

        Interacted += OnInteracted;
        CanceledInteraction += OnCancelInteraction;
        Focused += OnFocused;
        UnFocused += OnUnFocused;
    }

    public void Activate()
    {
        CollisionLayer = 2;
        Monitorable = true;
    }

    public void Deactivate()
    {
        CollisionLayer = 0;
        Monitorable = false;
    }


    public bool IsScannable()
    {
        return Parameters.Scannable;
    }
    public bool IsPickable()
    {
        return Parameters.Pickable;
    }
    public bool IsUsable()
    {
        return Parameters.Usable;
    }

    public bool CanBeSavedOnInventory()
    {
        return Parameters.CanBeSaved;
    }
    private void OnInteracted(GodotObject interactorParameter)
    {
        if (interactorParameter is IInteractor interact)
        {
            actor = interact;

            if (Parameters.LockPlayer)
                EmitSignal(GameEvents.SignalName.LockPlayer, this);

            EmitSignal(GameEvents.SignalName.Interacted, this);
        }
    }
    private void OnCancelInteraction(GodotObject _)
    {
        actor = null;

        if (Parameters.LockPlayer)
            EmitSignal(GameEvents.SignalName.UnlockPlayer, this);

        EmitSignal(GameEvents.SignalName.Interacted, this);
    }
    private void OnUnFocused(GodotObject interactor)
    {
        EmitSignal(GameEvents.SignalName.UnFocused, this);
    }

    private void OnFocused(GodotObject interactor)
    {
        EmitSignal(GameEvents.SignalName.Focused, this);
    }

}