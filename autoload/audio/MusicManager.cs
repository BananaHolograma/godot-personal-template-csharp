namespace GameRoot;

using System.Collections.Generic;
using Godot;

/// <summary>
/// Manage the background music in the game, support cross fading and still plays when scenes are being changed or loading.
/// This manager class also contains all the global sounds that needs to be reusable across the game.
/// </summary>
public partial class MusicManager : Node
{
    [Signal]
    public delegate void ChangedStreamEventHandler(AudioStream from, AudioStream to);
    public AudioManager AudioManager;
    public Dictionary<string, AudioStream> MusicBank = new();

    public AudioStreamPlayer Player;
    public AudioStreamPlayer PlayerTwo;

    public const float VOLUME_DB_INAUDIBLE = -80f;

    private AudioStreamPlayer CurrentPlayer;
    private float CrossFadingTime = 3f;

    public override void _Ready()
    {
        AudioManager = GetTree().Root.GetNode<AudioManager>("AudioManager");
        Player = new AudioStreamPlayer
        {
            Name = "AudioStreamPlayer",
            Bus = "Music",
            Autoplay = false
        };

        PlayerTwo = new AudioStreamPlayer
        {
            Name = "AudioStreamPlayerTwo",
            Bus = "Music",
            Autoplay = false
        };

        AddChild(Player);
        AddChild(PlayerTwo);

        CurrentPlayer = Player;
    }
    public void PlayMusic(string name, bool crossfade = true)
    {
        if (MusicBank.ContainsKey(name))
        {
            AudioStream stream = MusicBank[name];

            if (CurrentPlayer.Playing)
            {
                if (stream == CurrentPlayer.Stream) return;

                if (crossfade)
                {
                    AudioStreamPlayer nextPlayer = CurrentPlayer.Name == "AudioStreamPlayer" ? PlayerTwo : Player;
                    nextPlayer.VolumeDb = VOLUME_DB_INAUDIBLE;
                    PlayStream(nextPlayer, stream);

                    EmitSignal(SignalName.ChangedStream, CurrentPlayer.Stream, stream);

                    float volume = Mathf.LinearToDb(AudioManager.GetActualVolumeDBFromBusName(Player.Bus));

                    Tween crossFadeTween = CreateTween();
                    crossFadeTween.SetParallel(true);
                    crossFadeTween.TweenProperty(CurrentPlayer, "volume_db", VOLUME_DB_INAUDIBLE, CrossFadingTime).SetEase(Tween.EaseType.Out);
                    crossFadeTween.TweenProperty(nextPlayer, "volume_db", volume, CrossFadingTime).SetEase(Tween.EaseType.In);
                    crossFadeTween.Chain().TweenCallback(Callable.From(() => { CurrentPlayer = nextPlayer; }));

                    return;
                }
            }

            EmitSignal(SignalName.ChangedStream, CurrentPlayer.Stream, stream);
            PlayStream(CurrentPlayer, stream);
            return;
        }

        GD.PushWarning($"Expected music name {name} to exists in the MusicBank but no stream was found.");
    }

    public void PlayStream(AudioStreamPlayer Player, AudioStream stream)
    {
        Player.Stop();
        Player.Stream = stream;
        Player.Play();
    }

    public void AddStreamToMusicBank(string name, AudioStream stream)
    {
        MusicBank.Add(name, stream);
    }
}