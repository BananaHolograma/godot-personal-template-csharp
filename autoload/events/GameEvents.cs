namespace GameRoot;

using Godot;

public partial class GameEvents : Node
{
	#region Player
	[Signal]
	public delegate void LockPlayerEventHandler();
	[Signal]
	public delegate void UnlockPlayerEventHandler();

	#endregion

	#region UI
	[Signal]
	public delegate void ShowPauseMenuEventHandler();

	#endregion

	#region Interaction
	[Signal]
	public delegate void InteractedEventHandler(GodotObject interactor);
	[Signal]
	public delegate void FocusedEventHandler(GodotObject interactor);
	[Signal]
	public delegate void UnFocusedEventHandler(GodotObject interactor);

	#endregion

	#region Cameras
	[Signal]
	public delegate void GlobalTransitionCamera3DRequestedEventHandler(Camera3D from, Camera3D to);

	#endregion
}
