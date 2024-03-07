
using Godot;

namespace GodotExtensions;

public static class Camera3DExtension
{
    /// <summary>
    /// Gets the world-space origin of the ray projected from the center of the camera's viewport.
    /// This method is useful for determining the center of the camera's viewable area.
    /// </summary>
    /// <param name="camera">The Camera3D object to use.</param>
    /// <returns>The origin of the projected ray in world space.</returns>
    public static Vector3 CenterByRayOrigin(this Camera3D camera) => camera.ProjectRayOrigin(Vector2.Zero);

    /// <summary>
    /// Gets the origin (position) of the camera's global transformation in world space.
    /// This method provides the camera's absolute position in the world.
    /// </summary>
    /// <param name="camera">The Camera3D object to use.</param>
    /// <returns>The origin of the camera's global transformation in world space.</returns>
    public static Vector3 CenterByOrigin(this Camera3D camera) => camera.GlobalTransform.Origin;

    public static Vector3 ForwardDirection(this Camera3D camera) => Vector3.Forward.Z * camera.GlobalTransform.Basis.Z.Normalized();

}