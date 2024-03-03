
using System.Collections.Generic;
using Godot;

namespace GodotExtensions;

public static class CharacterBody2DExtension
{
    /// <summary>
    /// Retrieves the opposite direction vector based on the character's 2D up direction.
    /// </summary>
    /// <param name="character">The CharacterBody2D object to check.</param>
    /// <returns>The opposite direction vector corresponding to the character's up direction, or Vector2.Zero if the up direction is not found.</returns>
    public static Vector2 UpDirectionOppositeVector(this CharacterBody2D character)
    {
        Dictionary<Vector2, Vector2> oppositeDirections = new()
        {
            { Vector2.Up, Vector2.Down },
            { Vector2.Down, Vector2.Up },
            { Vector2.Right, Vector2.Left },
            { Vector2.Left, Vector2.Right }
        };

        if (oppositeDirections.TryGetValue(character.UpDirection, out Vector2 opposite))
            return opposite;

        return Vector2.Zero;
    }

}