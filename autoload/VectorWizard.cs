using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace GameRoot;

public static class VectorWizard
{
    /// <summary>
    /// Generates a sequence of random directions with angles in degrees, confined to a horizontal plane.
    /// 
    /// This function uses Vector2.Rotated with Vector3.Up as the rotation axis to create 
    /// random directions within a 2D plane around the specified origin.
    /// 
    /// - The generated directions will always be perpendicular to the Y-axis.
    /// - Consider potential adjustments if your use case requires full 3D space interpretation 
    ///   of these directions.
    /// </summary>
    /// <param name="origin">The origin point from which the directions are generated.</param>
    /// <param name="numDirections">The number of random directions to generate (default: 10).</param>
    /// <param name="minAngleRange">The range of angles in degrees (default: 0 to 360).</param>
    /// <param name="maxAngleRange">The range of angles in degrees (default: 0 to 360).</param>
    /// <returns>An enumerable sequence of Vector2 representing the random directions.</returns>
    public static IEnumerable<Vector2> Generate2DRandomDirectionsOnDegreesAngleRange(Vector2 origin, int numDirections = 10, float minAngleRange = 0, float maxAngleRange = 360f)
    {
        return Enumerable.Range(0, numDirections).Select(_ => origin.Rotated(GenerateRandomAngleInDegrees(minAngleRange, maxAngleRange)));
    }


    /// <summary>
    /// Generates a sequence of random directions with angles in radians, confined to a horizontal plane.
    /// 
    /// This function uses Vector2.Rotated with Vector3.Up as the rotation axis to create 
    /// random directions within a 2D plane around the specified origin. Similar to the degrees version.
    /// 
    /// - The generated directions will always be perpendicular to the Y-axis.
    /// - Consider potential adjustments if your use case requires full 3D space interpretation 
    ///   of these directions.
    /// </summary>
    /// <param name="origin">The origin point from which the directions are generated.</param>
    /// <param name="numDirections">The number of random directions to generate (default: 10).</param>
    /// <param name="minAngleRange">The range of angles in radians (default: 0 to 2π).</param>
    /// <param name="maxAngleRange">The range of angles in radians (default: 0 to 2π).</param>
    /// <returns>An enumerable sequence of Vector2 representing the random directions.</returns>
    public static IEnumerable<Vector2> Generate2DRandomDirectionsOnRadiansAngleRange(Vector2 origin, int numDirections = 10, float minAngleRange = 0, float maxAngleRange = 6.2831853072f)
    {
        return Enumerable.Range(0, numDirections).Select(_ => origin.Rotated(GenerateRandomAngleInRadians(minAngleRange, maxAngleRange)));
    }

    /// <summary>
    /// Generates a sequence of random directions with angles in degrees, rotating around the Y-axis.
    /// 
    /// This function uses Vector3.Rotated with Vector3.Up as the rotation axis to create 
    /// random directions in 3D space, effectively rotating around the Y-axis.
    /// 
    /// - The generated directions will be distributed within a full circle around the Y-axis.
    /// </summary>
    /// <param name="origin">The origin point from which the directions are generated.</param>
    /// <param name="numDirections">The number of random directions to generate (default: 10).</param>
    /// <param name="minAngleRange">The range of angles in degrees (default: 0 to 360).</param>
    /// <param name="maxAngleRange">The range of angles in degrees (default: 0 to 360).</param>
    /// <returns>An enumerable sequence of Vector3 representing the random directions.</returns>
    public static IEnumerable<Vector3> Generate3DRandomDirectionsOnDegreesAngleRange(Vector3 origin, int numDirections = 10, float minAngleRange = 0, float maxAngleRange = 360f)
    {
        return Enumerable.Range(0, numDirections).Select(_ => origin.Rotated(Vector3.Up, GenerateRandomAngleInDegrees(minAngleRange, maxAngleRange)));
    }

    /// <summary>
    /// Generates a sequence of random directions with angles in radians, rotating around the Y-axis.
    /// 
    /// This function uses Vector3.Rotated with Vector3.Up as the rotation axis to create 
    /// random directions in 3D
    public static IEnumerable<Vector3> Generate3DRandomDirectionsOnRadiansAngleRange(Vector3 origin, int numDirections = 10, float minAngleRange = 0, float maxAngleRange = 6.2831853072f)
    {
        return Enumerable.Range(0, numDirections).Select(_ => origin.Rotated(Vector3.Up, GenerateRandomAngleInRadians(minAngleRange, maxAngleRange)));
    }

    public static float GenerateRandomAngleInRadians(float minAngle = 0, float maxAngle = 6.2831853072f)
    {
        return minAngle + GD.Randf() * (maxAngle - minAngle);
    }

    public static float GenerateRandomAngleInDegrees(float minAngle = 0, float maxAngle = 360f)
    {
        return minAngle + GD.Randf() * (maxAngle - minAngle);
    }


    /// <summary>
    /// Generates a random and **normalized** 2D direction vector with values 
    /// ranging from -1 to 1 (inclusive) for both x and y components.
    /// 
    /// This function ensures even distribution across all directions and guarantees 
    /// a magnitude of 1 for the resulting vector.
    /// </summary>
    /// <returns>A random normalized 2D direction vector.</returns>
    public static Vector2 Generate2DRandomDirection()
    {
        RandomNumberGenerator rng = new();

        return new Vector2(rng.RandfRange(-1f, 1f), rng.RandfRange(-1f, 1f));
    }

    /// <summary>
    /// Generates a random **normalized** 2D direction vector with integer components 
    /// ranging from -1 to 1. 
    /// 
    /// **Note:** This approach also has limitations and does not cover 
    /// the entire spectrum of directions due to the use of integer components.
    /// </summary>
    /// <returns>A random normalized 2D vector with integer components between -1 and 1.</returns>
    public static Vector2 Generate2DRandomFixedDirection()
    {
        RandomNumberGenerator rng = new();

        return new Vector2(rng.RandiRange(-1, 1), rng.RandiRange(-1, 1)).Normalized();
    }

    /// <summary>
    /// Generates a random and **normalized** 2D direction vector with values 
    /// ranging from -1 to 1 (inclusive) for both x and y components.
    /// 
    /// This function ensures even distribution across all directions and guarantees 
    /// a magnitude of 1 for the resulting vector.
    /// </summary>
    /// <returns>A random normalized 3D direction vector.</returns>
    public static Vector3 Generate3DRandomDirection()
    {
        RandomNumberGenerator rng = new();

        return new Vector3(rng.RandfRange(-1f, 1f), rng.RandfRange(-1f, 1f), rng.RandfRange(-1f, 1f));
    }

    /// <summary>
    /// Generates a random **normalized** 3D direction vector with integer components 
    /// ranging from -1 to 1. 
    /// 
    /// **Note:** This approach also has limitations and does not cover 
    /// the entire spectrum of directions due to the use of integer components.
    /// </summary>
    /// <returns>A random normalized 3D vector with integer components between -1 and 1.</returns>
    public static Vector3 Generate3DRandomFixedDirection()
    {
        RandomNumberGenerator rng = new();

        return new Vector3(rng.RandiRange(-1, 1), rng.RandiRange(-1, 1), rng.RandiRange(-1, 1)).Normalized();
    }


    /// <summary>
    /// Translates an X-axis value (-1.0f or 1.0f) to a corresponding 2D direction vector.
    /// 
    /// This function accurately handles near-equal comparisons using `Mathf.Epsilon`
    /// to account for potential floating-point imprecision.
    /// 
    /// - If the input value is close to -1.0f (within `Mathf.Epsilon`), the function returns Vector2.Left.
    /// - If the input value is close to 1.0f (within `Mathf.Epsilon`), the function returns Vector2.Right.
    /// </summary>
    /// <param name="axis">The X-axis value (-1.0f or 1.0f) to translate.</param>
    /// <returns>The corresponding 2D direction vector (Vector2.Left or Vector2.Right), 
    /// or a default value (optional) if the input is invalid.</returns>
    public static Vector2 TranslateXAxisToVector(float axis)
    {
        if (Mathf.Abs(axis - 1.0f) < Mathf.Epsilon)
        {
            return Vector2.Right;
        }
        else if (Mathf.Abs(axis + 1.0f) < Mathf.Epsilon)
        {
            return Vector2.Left;
        }
        else
        {
            return Vector2.Zero;
        }
    }

    public static Vector2 NormalizeVector2(Vector2 vector)
    {
        Vector2 result = NormalizeDiagonalVector2(vector);

        if (result.IsEqualApprox(vector))
        {
            return vector.IsNormalized() ? vector : result.Normalized();
        }

        return result;
    }

    public static Vector2 NormalizeDiagonalVector2(Vector2 vector)
    {
        if (IsDiagonalDirection2D(vector))
        {
            return vector * Mathf.Sqrt(2);
        }

        return vector;
    }

    private static bool IsDiagonalDirection2D(Vector2 direction)
    {
        return direction.X != 0 && direction.Y != 0;
    }

    public static Vector3 NormalizeVector3(Vector3 vector)
    {
        Vector3 result = NormalizeDiagonalVector3(vector);

        if (result.IsEqualApprox(vector))
        {
            return vector.IsNormalized() ? vector : result.Normalized();
        }

        return result;
    }

    public static Vector3 NormalizeDiagonalVector3(Vector3 vector)
    {
        if (IsDiagonalDirection3D(vector))
        {
            return vector * Mathf.Sqrt(3f);
        }

        return vector;
    }

    private static bool IsDiagonalDirection3D(Vector3 direction)
    {
        return direction.X != 0 && direction.Y != 0 && direction.Z != 0;
    }

    /// <summary>
    /// Calculates the Manhattan distance between two 2D points.
    /// 
    /// The Manhattan distance is the sum of the absolute differences between the corresponding 
    /// coordinates of the two points.
    /// </summary>
    /// <param name="a">The first 2D point.</param>
    /// <param name="b">The second 2D point.</param>
    /// <returns>The Manhattan distance between the two points.</returns>
    public static float DistanceManhattanV2(Vector2 a, Vector2 b)
    {
        return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
    }

    /// <summary>
    /// Calculates the Chebyshev distance between two 2D points.
    /// 
    /// The Chebyshev distance is the maximum absolute difference between the corresponding 
    /// coordinates of the two points.
    /// </summary>
    /// <param name="a">The first 2D point.</param>
    /// <param name="b">The second 2D point.</param>
    /// <returns>The Chebyshev distance between the two points.</returns>
    public static float DistanceChebysevV2(Vector2 a, Vector2 b)
    {
        return Mathf.Max(Mathf.Abs(a.X - b.X), Mathf.Abs(a.Y - b.Y));
    }

    /// <summary>
    /// Calculates the Manhattan length of a 2D vector.
    /// 
    /// The Manhattan length of a vector is the sum of the absolute values of its components.
    /// This function is equivalent to `DistanceManhattanV2(a, Vector2.Zero)`.
    /// </summary>
    /// <param name="a">The 2D vector.</param>
    /// <returns>The Manhattan length of the vector.</returns>
    public static float LengthManhattanV2(Vector2 a)
    {
        return Mathf.Abs(a.X) + Mathf.Abs(a.Y);
    }

    /// <summary>
    /// The Chebyshev length of a vector should be the maximum absolute value of its components.
    /// </summary>
    /// <param name="a">The 2D vector.</param>
    /// <returns>The Chebyshev length of the vector.</returns>
    public static float LengthChebysevV2(Vector2 a)
    {
        return Mathf.Max(Mathf.Abs(a.X), Mathf.Abs(a.Y));
    }


    /// <summary>
    /// Finds the closest point on a line segment (defined by points `a` and `b`) to another point `c`, 
    /// clamping the result to lie within the segment.
    /// 
    /// This function calculates the projection of `c` onto the line segment defined by `a` and `b`. 
    /// The `Mathf.Clamp` function ensures the returned point lies on the segment between `a` and `b`.
    /// </summary>
    /// <param name="a">The first point defining the line segment.</param>
    /// <param name="b">The second point defining the line segment.</param>
    /// <param name="c">The point to find the closest point to on the line segment.</param>
    /// <returns>The closest point on the line segment to point `c`, clamped to lie within the segment.</returns>
    public static Vector2 ClosestPointOnLineClampedV2(Vector2 a, Vector2 b, Vector2 c)
    {
        b = (b - a).Normalized();
        c -= a;

        return a + b * Mathf.Clamp(c.Dot(b), 0.0f, 1.0f);
    }

    /// <summary>
    /// Finds the closest point on a line segment (defined by points `a` and `b`) to another point `c`.
    /// 
    /// This function calculates the projection of `c` onto the line segment defined by `a` and `b`. 
    /// The returned point may lie outside the line segment itself.
    /// </summary>
    /// <param name="a">The first point defining the line segment.</param>
    /// <param name="b">The second point defining the line segment.</param>
    /// <param name="c">The point to find the closest point to on the line segment.</param>
    /// <returns>The closest point on the line segment to point `c`.</returns>
    public static Vector2 ClosestPointOnLineV2(Vector2 a, Vector2 b, Vector2 c)
    {
        b = (b - a).Normalized();
        c -= a;

        return a + b * c.Dot(b);
    }

    /// <summary>
    /// Calculates the Manhattan distance between two 3D points.
    /// 
    /// The Manhattan distance is the sum of the absolute differences between the corresponding 
    /// coordinates of the two points.
    /// </summary>
    /// <param name="a">The first 3D point.</param>
    /// <param name="b">The second 3D point.</param>
    /// <returns>The Manhattan distance between the two points.</returns>
    public static float DistanceManhattanV3(Vector3 a, Vector3 b)
    {
        return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y) + Mathf.Abs(a.Z - b.Z);
    }

    /// <summary>
    /// Calculates the Chebyshev distance between two 3D points.
    /// 
    /// The Chebyshev distance is the maximum absolute difference between the corresponding 
    /// coordinates of the two points.
    /// </summary>
    /// <param name="a">The first 3D point.</param>
    /// <param name="b">The second 3D point.</param>
    /// <returns>The Chebyshev distance between the two points.</returns>
    public static float DistanceChebysevV3(Vector3 a, Vector3 b)
    {
        return Mathf.Max(Mathf.Abs(a.X - b.X), Mathf.Max(Mathf.Abs(a.Y - b.Y), Mathf.Abs(a.Z - b.Z)));
    }

    /// <summary>
    /// Calculates the Manhattan length of a 3D vector.
    /// 
    /// The Manhattan length of a vector is the sum of the absolute values of its components.
    /// </summary>
    /// <param name="a">The 3D vector.</param>
    /// <returns>The Manhattan length of the vector.</returns>
    public static float LengthManhattanV3(Vector3 a)
    {
        return Mathf.Abs(a.X) + Mathf.Abs(a.Y) + Mathf.Abs(a.Z);
    }

    /// <summary>
    /// Calculates the Chebyshev length of a 3D vector.
    /// 
    /// The Chebyshev length of a vector is the maximum absolute value of its components.
    /// </summary>
    /// <param name="a">The 3D vector.</param>
    /// <returns>The Chebyshev length of the vector.</returns>
    public static float LengthChebysevV3(Vector3 a, Vector3 b)
    {
        return Mathf.Max(Mathf.Abs(a.X), Mathf.Max(Mathf.Abs(a.Y), Mathf.Abs(a.Z)));
    }

    /// <summary>
    /// Calculates the normalized projection of a point `c` onto the direction vector representing the line segment defined by points `a` and `b`.
    /// 
    /// This function does not consider the actual endpoints of the line segment and simply calculates the projection of `c` onto the normalized direction vector `b - a`.
    /// The result is a scalar value representing the distance along the normalized line direction from point `a` to the projection of `c`.
    /// </summary>
    /// <param name="a">The first point defining the line segment.</param>
    /// <param name="b">The second point defining the line segment.</param>
    /// <param name="c">The point to project onto the line direction.</param>
    /// <returns>The projection of point `c` onto the line direction vector, representing the distance along the line from point `a`.</returns>
    public static Vector3 ClosestPointOnLineClampedV3(Vector3 a, Vector3 b, Vector3 c)
    {
        b = (b - a).Normalized();
        c -= a;

        return a + b * Mathf.Clamp(c.Dot(b), 0.0f, 1.0f);
    }

    /// <summary>
    /// Calculates the normalized projection of a point `c` onto the direction vector representing the line segment defined by points `a` and `b`.
    /// 
    /// This function does not consider the actual endpoints of the line segment and simply calculates the projection of `c` onto the normalized direction vector `b - a`.
    /// The result is a scalar value representing the distance along the normalized line direction from point `a` to the projection of `c`.
    /// </summary>
    /// <param name="a">The first point defining the line segment.</param>
    /// <param name="b">The second point defining the line segment.</param>
    /// <param name="c">The point to project onto the line direction.</param>
    /// <returns>The projection of point `c` onto the line direction vector, representing the distance along the line from point `a`.</returns>
    public static Vector3 ClosestPointOnLineV3(Vector3 a, Vector3 b, Vector3 c)
    {
        b = (b - a).Normalized();
        c -= a;

        return a + b * c.Dot(b);
    }

    /// <summary>
    /// Calculates the normalized projection of a point `c` onto the direction vector representing the line segment defined by points `a` and `b`.
    /// 
    /// This function does not consider the actual endpoints of the line segment and simply calculates the projection of `c` onto the normalized direction vector `b - a`.
    /// The result is a scalar value representing the distance along the normalized line direction from point `a` to the projection of `c`.
    /// </summary>
    /// <param name="a">The first point defining the line segment.</param>
    /// <param name="b">The second point defining the line segment.</param>
    /// <param name="c">The point to project onto the line direction.</param>
    /// <returns>The normalized projection of point `c` onto the line direction vector, representing the distance along the line from point `a`.</returns>
    public static float ClosestPointOnLineNormalizedV3(Vector3 a, Vector3 b, Vector3 c)
    {
        b -= a;
        c -= a;

        return c.Dot(b.Normalized()) / b.Length();
    }
}