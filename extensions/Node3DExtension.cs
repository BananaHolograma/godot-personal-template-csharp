using Godot;

namespace GodotExtensions;
public static class Node3DExtension
{

	/// <summary>
	/// Calculates the global distance between two Node3D objects.
	/// </summary>
	/// <param name="node">The first Node3D object.</param>
	/// <param name="target">The second Node3D object.</param>
	/// <returns>The distance between the two nodes in global coordinates.</returns>
	/// <remarks>
	/// Global distance considers the nodes' positions within the entire scene's coordinate system,
	/// including any transformations applied to their parent nodes or ancestors.
	/// </remarks>
	public static float GlobalDistanceTo(this Node3D node, Node3D target)
	{
		return node.GlobalPosition.DistanceTo(target.GlobalPosition);
	}

	/// <summary>
	/// Calculates the local distance between two Node3D objects.
	/// </summary>
	/// <param name="node">The first Node3D object.</param>
	/// <param name="target">The second Node3D object.</param>
	/// <returns>The distance between the two nodes in local coordinates.</returns>
	/// <remarks>
	/// Local distance only considers the nodes' positions relative to their common parent or root,
	/// ignoring any transformations inherited from parent nodes.
	/// </remarks>
	public static float LocalDistanceTo(this Node3D node, Node3D target)
	{
		return node.Position.DistanceTo(target.Position);
	}

	/// <summary>
	/// Calculates the global direction vector pointing from one Node3D object to another.
	/// </summary>
	/// <param name="node">The origin Node3D object.</param>
	/// <param name="target">The destination Node3D object.</param>
	/// <returns>A Vector3 representing the direction from the origin node to the target node in global coordinates.</returns>
	/// <remarks>
	/// Global direction considers the nodes' positions within the entire scene's coordinate system,
	/// accounting for any transformations applied to their parent nodes or ancestors.
	/// </remarks>
	public static Vector3 GlobalDirectionTo(this Node3D node, Node3D target)
	{
		return node.GlobalPosition.DirectionTo(target.GlobalPosition);
	}

	/// <summary>
	/// Calculates the local direction vector pointing from one Node3D object to another.
	/// </summary>
	/// <param name="node">The origin Node3D object.</param>
	/// <param name="target">The destination Node3D object.</param>
	/// <returns>A Vector3 representing the direction from the origin node to the target node in local coordinates.</returns>
	/// <remarks>
	/// Local direction only considers the nodes' positions relative to their common parent or root,
	/// ignoring any transformations inherited from parent nodes.
	/// </remarks>
	public static Vector3 LocalDirectionTo(this Node3D node, Node3D target)
	{
		return node.Position.DirectionTo(target.Position);
	}
}