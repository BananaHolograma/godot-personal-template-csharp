namespace GameRoot;

using System.Collections.Generic;
using System.Threading.Tasks;
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

    private int CrossFadingTime = 3;

    public override void _Ready()
    {
        AudioManager = GetTree().Root.GetNode<AudioManager>("AudioManager");
        Player = new AudioStreamPlayer
        {
            Bus = "Music",
            Autoplay = false
        };

        AddChild(Player);
    }
    public async Task PlayMusic(string name, bool crossfade = true)
    {
        if (MusicBank.ContainsKey(name))
        {
            AudioStream stream = MusicBank[name];

            if (Player.Playing)
            {
                if (stream == Player.Stream) return;

                if (crossfade)
                {
                    float volume = Mathf.LinearToDb(AudioManager.GetActualVolumeDBFromBusName(Player.Bus));

                    Tween crossFadeTween = CreateTween();
                    crossFadeTween.SetParallel(true);
                    crossFadeTween.TweenProperty(Player, "volume_db", -80, CrossFadingTime);
                    crossFadeTween.Chain().TweenProperty(Player, "volume_db", volume, CrossFadingTime);

                    await ToSignal(crossFadeTween, Tween.SignalName.Finished);
                }

                Player.Stop();
                Player.Stream = stream;
                Player.Play();
            }

        }
    }
}