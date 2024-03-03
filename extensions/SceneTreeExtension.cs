
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace GodotExtensions;

public static class SceneTreeExtension
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
    public static T GetAutoloadNode<T>(this SceneTree tree, string name) where T : class
    {
        return tree.Root.GetNode<T>(name);
    }
}