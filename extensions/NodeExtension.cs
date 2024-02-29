
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace GodotExtensions;

public static class NodeExtension
{

    /// <summary>
    /// Retrieves an autoloaded node by its name.
    /// </summary>
    /// <typeparam name="T">The type of the autoloaded node to retrieve. Must be a reference type (class).</typeparam>
    /// <param name="name">The name of the autoloaded node in the scene tree.</param>
    /// <returns>The autoloaded node of type T, or null if no node is found.</returns>
    /// <example>
    /// This example retrieves an autoloaded AudioManager node and casts it to the appropriate type:
    /// <code>csharp
    /// AudioManager audioManager = GetAutoloadNode<AudioManager>("AudioManager");
    /// </code>
    /// </example>
    public static T GetAutoloadNode<T>(this Node node, string name) where T : class
    {
        return node.GetTree().Root.GetNode<T>(name);
    }


    /// <summary>
    /// Recursively finds all nodes of a specific type within a sub-tree of a given root node and adds them to a list.
    /// </summary>
    /// <typeparam name="T">The type of node to search for. Must inherit from the Godot.Node class.</typeparam>
    /// <param name="node">The root node from which to start the search.</param>
    /// <param name="result">The list to which the found nodes of type T will be added.</param>
    /// <remarks>
    /// This function traverses the entire sub-tree starting from the root node, searching for nodes of type T. 
    /// It adds any found nodes to the provided list. You must ensure that the list is of the appropriate type").
    /// </remarks>
    public static void FindNodesRecursively<T>(this Node node, List<T> result) where T : Node
    {
        if (node.GetChildCount() == 0) return;

        foreach (Node child in node.GetChildren(true))
        {
            if (child is T nodeFound && child.GetType() == typeof(T))
            {
                result.Add(nodeFound);
            }
            FindNodesRecursively(child, result);
        }
    }

    /// <summary>
    /// Retrieves the last child node from the specified node.
    /// </summary>
    /// <param name="node">The node from which to retrieve the last child.</param>
    /// <returns>The last child node of the target node, or null if the target node has no children.</returns>
    public static Node GetLastChild(this Node node)
    {
        int count = node.GetChildCount();
        if (count == 0) return null;
        return node.GetChild(count - 1);
    }


    /// <summary>
    /// Retrieves the first node of the specified type from a given group in the scene tree.
    /// </summary>
    /// <typeparam name="T">The type of node to search for.</typeparam>
    /// <param name="group">The name of the group to search within.</param>
    /// <returns>
    /// The first node of type T found in the specified group, or null if no node of type T is found in the group.
    /// </returns>
    /// <remarks>
    /// This function uses the Godot `GetFirstNodeInGroup` method and attempts to cast the returned value to the specified type T.
    /// If the cast fails, the function returns null.
    /// </remarks>
    public static T GetFirstNodeInGroup<T>(this Node node, string group) where T : Node
    {
        return node.GetTree().GetFirstNodeInGroup(group) as T;
    }

    /// <summary>
    /// Retrieves all nodes of the specified type from a given group in the scene tree.
    /// </summary>
    /// <typeparam name="T">The type of node to search for.</typeparam>
    /// <param name="group">The name of the group to search within.</param>
    /// <returns>
    /// An `IEnumerable<T>` containing all nodes of type T found in the specified group.
    /// If no nodes of type T are found in the group, the returned collection will be empty.
    /// </returns>
    /// <remarks>
    /// This function uses the Godot `GetNodesInGroup` method and then casts each element in the returned array to the specified type T.
    /// Any element that cannot be cast is omitted from the resulting collection.
    /// </remarks>
    public static IEnumerable<T> GetNodesInGroup<T>(this Node node, string group) where T : Node
    {
        return node.GetTree().GetNodesInGroup(group).Cast<T>();
    }


    /// <summary>
    /// Removes all child nodes from the specified target node and queues them for freeing in Godot.
    /// </summary>
    /// <param name="node">The target node from which to remove and free children.</param>
    /// <remarks>
    /// This function iterates through the children of the target node in reverse order, removing each child and queuing it for freeing.
    /// Using reverse iteration ensures that removing a child doesn't affect the index of remaining children.
    /// </remarks>
    public static void RemoveAndQueueFreeChildren(this Node node)
    {
        for (int i = node.GetChildCount() - 1; i >= 0; i--)
        {
            Node child = node.GetChild(i);
            node.RemoveChild(child);
            child.QueueFree();
        }
    }

    /// <summary>
    /// Queues all child nodes of the specified target node for freeing in Godot.
    /// </summary>
    /// <param name="node">The target node whose children will be queued for freeing.</param>
    /// <remarks>
    /// This function iterates through the children of the target node and queues each child for freeing.
    /// No type check is necessary as `GetChildren()` returns a collection of nodes.
    /// </remarks>
    public static void QueueFreeChildren(this Node node)
    {
        foreach (Node child in node.GetChildren())
            child.QueueFree();
    }
}