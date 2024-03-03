using System;
using Godot;

namespace GameRoot;

[GlobalClass]
public partial class ShakeCamera3D : Area3D, IShakeable
{
    [Export] public Camera3D camera;
    [Export] public double TraumaReductionRate = 1f;
    [Export] public float MaxRotationX = 10f;
    [Export] public float MaxRotationY = 10f;
    [Export] public float MaxRotationZ = 5f;

    [Export]
    public FastNoiseLite noise = new()
    {
        NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex
    };
    [Export] public double NoiseSpeed = 50f;

    public double Trauma = 0;
    public double Time = 0f;

    public Timer TraumaTimer;
    public Vector3 InitialRotation;

    public override void _Ready()
    {
        SetProcess(false);
        CreateTraumaTimer();
        InitialRotation = camera.RotationDegrees;

        Monitoring = false;
        Monitorable = true;
        CollisionLayer = 64; // Shakeable layer
        CollisionMask = 0;
    }
    public override void _Process(double delta)
    {
        Time += delta;
        Trauma = Mathf.Max(Trauma - delta * TraumaReductionRate, 0f);

        camera.RotationDegrees = camera.RotationDegrees with
        {
            X = (float)(InitialRotation.X + MaxRotationX * ShakeIntensity() * GetNoiseFromSeed(0)),
            Y = (float)(InitialRotation.Y + MaxRotationY * ShakeIntensity() * GetNoiseFromSeed(1)),
            Z = (float)(InitialRotation.Z + MaxRotationZ * ShakeIntensity() * GetNoiseFromSeed(2)),
        };
    }

    public void AddTrauma(float amount, float time = 1f)
    {
        GD.Print("Camera trauma");
        Trauma = Mathf.Clamp(Trauma + amount, 0, 1f);

        if (IsInstanceValid(TraumaTimer))
        {
            TraumaTimer.Stop();
            TraumaTimer.WaitTime = Mathf.Max(.05f, time);
            TraumaTimer.Start(time);
        }

        SetProcess(true);
    }

    public void FinishTrauma()
    {
        Time = 0;
        SetProcess(false);
    }

    private double ShakeIntensity()
    {
        return Trauma * Trauma;
    }

    private double GetNoiseFromSeed(int seed)
    {
        noise.Seed = seed;

        return noise.GetNoise1D((float)(Time * NoiseSpeed));
    }

    private void CreateTraumaTimer()
    {
        if (TraumaTimer == null)
        {
            TraumaTimer = new Timer
            {
                Name = "TraumaTimer",
                ProcessCallback = Timer.TimerProcessCallback.Idle,
                Autostart = false,
                OneShot = true
            };

            AddChild(TraumaTimer);
            TraumaTimer.Timeout += OnTraumaTimerTimeout;
        }
    }

    private void OnTraumaTimerTimeout()
    {
        GD.Print("Camera FINISH trauma");
        FinishTrauma();
    }
}

