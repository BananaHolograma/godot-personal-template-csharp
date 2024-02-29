using Godot;

namespace GameRoot;

public partial class Door : Node3D
{
    [Signal]
    public delegate void OpenedEventHandler();
    [Signal]
    public delegate void ClosedEventHandler();
    [Signal]
    public delegate void LockedEventHandler();
    [Signal]
    public delegate void UnlockedEventHandler();

    [ExportGroup("Interaction parameters")]
    public bool IsOpen
    {
        get => isOpen;
        set
        {
            if (value != isOpen)
            {
                EmitSignal(value ? SignalName.Opened : SignalName.Closed);
            }

            isOpen = value;
        }
    }

    public bool Locked
    {
        get => locked;
        set
        {
            if (value != locked)
            {
                EmitSignal(value ? SignalName.Locked : SignalName.Unlocked);
            }

            locked = value;
        }
    }

    [Export]
    public PackedScene Key;

    [Export]
    public float DelayBeforeClose = 0f;


    private bool isOpen;
    private bool locked;
    public virtual void Open()
    {

    }

    public virtual void Close()
    {

    }

    public virtual void Unlock()
    {

    }

    public virtual void IsLocked()
    {

    }
}