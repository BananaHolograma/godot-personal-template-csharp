namespace GameRoot;

using System;
using Godot;

public partial class Utilities : Node
{
    [Signal]
    public delegate void FrameFreezeEventHandler();

    private readonly Random Random = new();

    public async void StartFrameFreeze(double timeScale, double duration)
    {
        EmitSignal(SignalName.FrameFreeze);

        double originalTimeScaleValue = Engine.TimeScale;
        Engine.TimeScale = timeScale;

        await ToSignal(GetTree().CreateTimer(duration * timeScale), Timer.SignalName.Timeout);

        Engine.TimeScale = originalTimeScaleValue;
    }

    public string GenerateRandomString(int length, string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
    {
        string result = string.Empty;

        if (!string.IsNullOrEmpty(characters) && length > 0)
        {
            foreach (int _ in GD.Range(length))
            {
                result += characters[Random.Next(characters.Length)];
            }
        }

        return result;
    }

    public static string FormatSeconds(float time, bool useMilliseconds)
    {
        int minutes = (int)Math.Floor(time / 60);
        int seconds = (int)(time % 60);

        if (!useMilliseconds)
        {
            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        int milliseconds = (int)(Math.Floor(time * 100) % 100);

        return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }

    public bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    public void OpenExternalLink(string url)
    {
        if (IsValidUrl(url))
        {
            if (OS.GetName() == "Web")
            {
                url = url.URIEncode();
            }

            OS.ShellOpen(url);
        }
    }

    public float Sigmoid(float x)
    {
        return 1 / (1 + Mathf.Exp(-x));
    }

    public static T RandomEnum<T>() where T : Enum
    {
        // Get all possible enum values
        T[] values = (T[])Enum.GetValues(typeof(T));

        // Generate a random index within the valid range
        int randomIndex = new Random().Next(values.Length);

        // Return the random enum value
        return values[randomIndex];
    }
}