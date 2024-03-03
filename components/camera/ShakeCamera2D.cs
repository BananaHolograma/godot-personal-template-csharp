using Godot;

namespace GameRoot;

[GlobalClass]
public partial class ShakeCamera2D : Camera2D
{
    [Export] float DefaultShakeStrength = 15f;
    [Export] float DefaultFade = 5f;

    public float ShakeStrength = 0;
    public float ShakeFade = 5f;

    private RandomNumberGenerator rng = new();


    public override async void _Ready()
    {
        SetProcess(false);
    }

    public override void _Process(double delta)
    {
        ShakeCamera(delta);
    }
    public void ShakeCamera(double delta)
    {
        if (ShakeStrength > 0)
        {
            ShakeStrength = (float)Mathf.Lerp(ShakeStrength, 0f, ShakeFade * delta);
            Offset = new Vector2(rng.RandfRange(-ShakeStrength, ShakeStrength), rng.RandfRange(-ShakeStrength, ShakeStrength));
        }
        else
        {
            SetProcess(false);
        }
    }

    public void Shake(float strength = 0, float fade = 0)
    {
        ShakeStrength = strength == 0 ? DefaultShakeStrength : strength;
        ShakeFade = fade == 0 ? DefaultFade : fade;
        SetProcess(true);
    }

}