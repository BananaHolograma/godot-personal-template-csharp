using Godot;

namespace GodotExtensions;
public static class Node2DExtension
{
	/// <summary>
	/// Calculates the global distance between two Node2D objects.
	/// </summary>
	/// <param name="node">The first Node2D object.</param>
	/// <param name="target">The second Node2D object.</param>
	/// <returns>The distance between the two nodes in global coordinates.</returns>
	/// <remarks>
	/// Global distance considers the nodes' positions within the entire scene's coordinate system,
	/// including any transformations applied to their parent nodes or ancestors.
	/// </remarks>
	public static float GlobalDistanceTo(this Node2D node, Node2D target)
	{
		return node.GlobalPosition.DistanceTo(target.GlobalPosition);
	}

	/// <summary>
	/// Calculates the local distance between two Node2D objects.
	/// </summary>
	/// <param name="node">The first Node2D object.</param>
	/// <param name="target">The second Node2D object.</param>
	/// <returns>The distance between the two nodes in local coordinates.</returns>
	/// <remarks>
	/// Local distance only considers the nodes' positions relative to their common parent or root,
	/// ignoring any transformations inherited from parent nodes.
	/// </remarks>
	public static float LocalDistanceTo(this Node2D node, Node2D target)
	{
		return node.Position.DistanceTo(target.Position);
	}

	/// <summary>
	/// Calculates the global direction vector pointing from one Node2D object to another.
	/// </summary>
	/// <param name="node">The origin Node2D object.</param>
	/// <param name="target">The destination Node2D object.</param>
	/// <returns>A Vector2 representing the direction from the origin node to the target node in global coordinates.</returns>
	/// <remarks>
	/// Global direction considers the nodes' positions within the entire scene's coordinate system,
	/// accounting for any transformations applied to their parent nodes or ancestors.
	/// </remarks>
	public static Vector2 GlobalDirectionTo(this Node2D node, Node2D target)
	{
		return node.GlobalPosition.DirectionTo(target.GlobalPosition);
	}

	/// <summary>
	/// Calculates the local direction vector pointing from one Node2D object to another.
	/// </summary>
	/// <param name="node">The origin Node2D object.</param>
	/// <param name="target">The destination Node2D object.</param>
	/// <returns>A Vector2 representing the direction from the origin node to the target node in local coordinates.</returns>
	/// <remarks>
	/// Local direction only considers the nodes' positions relative to their common parent or root,
	/// ignoring any transformations inherited from parent nodes.
	/// </remarks>
	public static Vector2 LocalDirectionTo(this Node2D node, Node2D target)
	{
		return node.Position.DirectionTo(target.Position);
	}

}