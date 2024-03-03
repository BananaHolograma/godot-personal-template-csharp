using System.Linq;
using Godot;

namespace GameRoot;

[GlobalClass]
public partial class TraumaCauser : Area3D
{
    [Export(PropertyHint.Range, "0, 1, 0.01")] public float TraumaAmount = .5f;
    [Export] public float TraumaTime = 1f;

    public override void _Ready()
    {
        Monitoring = true;
        Monitorable = false;
        CollisionLayer = 0;
        CollisionMask = 64; // Shakeable layer
    }
    public void CauseTrauma()
    {
        foreach (IShakeable shakeable in GetOverlappingAreas().Where(area => area is IShakeable).Cast<IShakeable>())
        {
            shakeable.AddTrauma(TraumaAmount, TraumaTime);
        }
    }
}