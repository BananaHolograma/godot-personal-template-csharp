
using System.Collections.Generic;
using Godot;

namespace GodotExtensions;

public static class CharacterBody3DExtension
{
    /// <summary>
    /// Retrieves the opposite direction of a given vector, considering gravity as the "up" direction.
    /// This is useful for converting character-based directions to world space directions 
    /// where gravity defines the "up" axis.
    /// </summary>
    /// <param name="character">The character to use his up direction vector</param>
    /// <returns>The opposite direction based on the assumption that gravity defines "up".</returns>
    public static Vector3 UpDirectionOppositeVector(this CharacterBody3D character)
    {
        Dictionary<Vector3, Vector3> oppositeDirections = new(){
            { Vector3.Up, Vector3.Down },
            { Vector3.Down, Vector3.Up },
            { Vector3.Right, Vector3.Left },
            { Vector3.Left, Vector3.Right },
            { Vector3.Forward, Vector3.Back },
            { Vector3.Back, Vector3.Forward }
        };

        if (oppositeDirections.TryGetValue(character.UpDirection, out Vector3 opposite))
            return opposite;

        return Vector3.Zero;
    }

}