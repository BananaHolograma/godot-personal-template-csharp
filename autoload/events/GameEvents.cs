namespace GameRoot;

using Godot;

public partial class GameEvents : Node
{
	[Signal]
	public delegate void LockPlayerEventHandler();
	[Signal]
	public delegate void UnLockPlayerEventHandler();

	[Signal]
	public delegate void ShowPauseMenuEventHandler();
}
