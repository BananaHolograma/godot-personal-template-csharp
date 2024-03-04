using System;
using System.Linq;
using Godot;
using Godot.Collections;

namespace GameRoot;

[Tool]
[GlobalClass]
public partial class FootstepManager : Node3D
{
    [Export] public RayCast3D FloorDetectorRaycast;
    [Export] public float DefaultIntervalTime = .6f;
    public Timer IntervalTimer;
    public bool SfxPlaying = false;

    public override string[] _GetConfigurationWarnings()
    {
        if (GetChildCount() == 0)
            return new string[] { "No children found. Expected RayCast3D child." };

        if (GetChild(0) is not RayCast3D)
            return new string[] { "Expected child to be a RayCast3D" };

        return base._GetConfigurationWarnings();
    }

    // Sound queues must be added manually and referenced on variables as child of this node
    //Example: public SoundQueue3D DirtSounds = GetNode<SoundQueue3D>("DirtSoundQueue")
    public override void _Ready()
    {
        CreateIntervalTimer();
    }

    public void Footstep(State state, float interval = 0)
    {
        if (interval == 0)
            interval = DefaultIntervalTime;

        if (IsInstanceValid(IntervalTimer) && IntervalTimer.IsStopped() && FloorDetectorRaycast.IsColliding() && InAllowedStates(state))
        {
            GodotObject collider = FloorDetectorRaycast.GetCollider();

            if (collider is StaticBody3D floor)
            {
                Array<StringName> groups = floor.GetGroups();

                if (groups.Count > 0)
                {
                    switch (groups[0])
                    {
                        case "dirt":
                            // placeholder [Access the SoundQueue related to Dirt sounds and play the sound]
                            // Example: DirtSoundQueue.PlaySound()
                            IntervalTimer.Start(interval);
                            SfxPlaying = true;
                            break;
                        default:
                            // Default sound if material does not have a group defined
                            IntervalTimer.Start(interval);
                            SfxPlaying = true;
                            break;
                    }
                }
            }
        }
    }

    private bool InAllowedStates(State state)
    {
        return new string[] { "walk", "run" }.Contains(state.Name.ToString().Trim().ToLower());
    }

    private void CreateIntervalTimer()
    {
        if (IntervalTimer == null)
        {
            IntervalTimer = new Timer
            {
                Name = "IntervalTimer",
                WaitTime = DefaultIntervalTime,
                ProcessCallback = Timer.TimerProcessCallback.Physics,
                Autostart = false,
                OneShot = true
            };

            AddChild(IntervalTimer);
            IntervalTimer.Timeout += OnIntervalTimerTimeout;
        }
    }

    private void OnIntervalTimerTimeout()
    {
        SfxPlaying = false;
    }
}