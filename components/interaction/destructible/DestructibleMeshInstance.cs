using Godot;
using GodotExtensions;

namespace GameRoot;

[GlobalClass]
public partial class DestructibleMeshInstance : MeshInstance3D
{
    public Hurtbox3D Hurtbox;
    public Destructible Destructible;

    public override void _Ready()
    {
        Hurtbox = this.FirstNodeOfClass<Hurtbox3D>();
        Destructible = this.FirstNodeOfClass<Destructible>();

        if (Hurtbox != null)
            Hurtbox.Hitbox3DDetected += OnHitboxDetected;
    }

    private void OnHitboxDetected(Hitbox3D hitbox)
    {
        if (hitbox.GetParent() is Throwable3D throwable)
        {
            Destructible.Destroy();
        }
    }

}
