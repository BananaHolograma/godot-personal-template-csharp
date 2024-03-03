using Godot;
using System;
using System.Linq;

namespace GodotExtensions;
public static class InputEventExtension
{
	/// <summary>
	/// An array containing the keycodes for numeric keys (0-9 and numpad 0-9).
	/// </summary>
	public static int[] NumericKeys = new[] { 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105 };

	/// <summary>
	/// Checks if the provided InputEvent is a left mouse button click.
	/// </summary>
	/// <param name="event">The InputEvent to check.</param>
	/// <returns>True if the event is a left mouse button click, false otherwise.</returns>
	public static bool MouseLeftClick(this InputEvent @event)
	{
		if (@event is InputEventMouseButton button)
			return button.ButtonIndex == MouseButton.Left && button.Pressed;

		return false;
	}

	/// <summary>
	/// Checks if the provided InputEvent is a right mouse button click.
	/// </summary>
	/// <param name="event">The InputEvent to check.</param>
	/// <returns>True if the event is a right mouse button click, false otherwise.</returns>
	public static bool MouseRightClick(this InputEvent @event)
	{
		if (@event is InputEventMouseButton button)
			return button.ButtonIndex == MouseButton.Right && button.Pressed;

		return false;
	}

	/// <summary>
	/// Checks if the provided InputEvent is a key press event for a numeric key.
	/// </summary>
	/// <param name="event">The InputEvent to check.</param>
	/// <returns>True if the event is a key press event for a numeric key, false otherwise.</returns>
	public static bool NumericKeyPressed(this InputEvent @event)
	{
		return @event is InputEventKey key && key.Pressed && NumericKeys.Contains((int)key.Keycode);
	}
}
