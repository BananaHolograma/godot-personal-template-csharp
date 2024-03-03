namespace GameRoot;

using Godot;
using Godot.Collections;

public partial class GameSettings : Resource
{
    [Export] public float MouseSensitivity = 3f;
    [Export] public DisplayServer.WindowMode DisplayMode = DisplayServer.WindowMode.Fullscreen;
    [Export] public DisplayServer.VSyncMode Vsync = DisplayServer.VSyncMode.Disabled;
    [Export] public Viewport.Msaa AntiAliasing = Viewport.Msaa.Disabled;

    [Export]
    public Dictionary<string, float> AudioVolumes = new()
    {
        ["music"] = 0.9f,
        ["sfx"] = 0.9f,
        ["ui"] = 0.9f,
        ["ambient"] = 0.9f
    };
    [Export]
    public Vector2[] Resolutions = new[]
    {
        new Vector2(640, 360),
        new Vector2(960, 540),
        new Vector2(1280, 720),
        new Vector2(1440, 810),
        new Vector2(1920, 1080)
    };

    [Export] string language = "en";

    public void SetVolume(string bus, float volume)
    {
        bus = bus.ToLower();

        if (AudioVolumes.TryGetValue(bus, out float _))
            AudioVolumes[bus] = volume;
    }
}
