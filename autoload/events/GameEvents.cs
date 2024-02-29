namespace GameRoot;

using Godot;

public partial class GameEvents : Node
{
	[Signal]
	public delegate void LockPlayerEventHandler();
	[Signal]
	public delegate void UnlockPlayerEventHandler();

	[Signal]
	public delegate void ShowPauseMenuEventHandler();

	[Signal]
	public delegate void InteractedEventHandler(GodotObject interactor);
	[Signal]
	public delegate void FocusedEventHandler(GodotObject interactor);

	[Signal]
	public delegate void UnFocusedEventHandler(GodotObject interactor);
}
