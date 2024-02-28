using System.Collections.Generic;
using Godot;

namespace GameRoot;

/// <summary>
/// It allows you to play multiple sounds sequentially or concurrently without creating individual AudioStreamPlayer nodes for each sound. 
/// This can improve performance and memory usage, especially when dealing with numerous short sounds
/// </summary>
[Tool]
[GlobalClass]
public partial class SoundQueue2D : Node2D, ISoundQueue
{
    private int _next = 0;

    private readonly List<AudioStreamPlayer2D> _audioStreamPlayers = new();

    [Export]
    public int Count { get; set; } = 1;
    public override void _Ready()
    {
        if (GetChildCount() == 0)
        {
            GD.PushWarning("SoundQueue: No AudioStreamPlayer child found.");
            return;
        }

        var child = GetChild(0);

        if (child is AudioStreamPlayer2D audioStreamPlayer)
        {
            _audioStreamPlayers.Add(audioStreamPlayer);

            for (int i = 0; i < Count; i++)
            {
                AudioStreamPlayer2D duplicate = audioStreamPlayer.Duplicate() as AudioStreamPlayer2D;
                AddChild(duplicate);
                _audioStreamPlayers.Add(audioStreamPlayer);
            }
        }
    }

    public void PlaySound()
    {
        if (!_audioStreamPlayers[_next].Playing)
        {
            _audioStreamPlayers[_next++].Play();
            _next %= _audioStreamPlayers.Count;
        }
    }

    public void PlayWithPitchRange(float minPitchScale = 0.9f, float maxPitchScale = 1.3f)
    {
        RandomNumberGenerator rng = new();

        if (!_audioStreamPlayers[_next].Playing)
        {
            AudioStreamPlayer2D nextPlayer = _audioStreamPlayers[_next++];
            nextPlayer.PitchScale = rng.RandfRange(minPitchScale, maxPitchScale);
            nextPlayer.Play();
            _next %= _audioStreamPlayers.Count;
        }
    }
    public override string[] _GetConfigurationWarnings()
    {
        if (GetChildCount() == 0)
        {
            return new string[] { "No children found. Expected AudioStreamPlayer2D child." };
        }

        if (GetChild(0) is not AudioStreamPlayer2D)
        {
            return new string[] { "Expected child to be an AudioStreamPlayer2D" };
        }

        return base._GetConfigurationWarnings();
    }
}
