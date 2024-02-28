namespace GameRoot;

using System.Linq;
using Godot;

[GlobalClass]
public partial class AudioManager : Node
{
    public string[] availableBuses;


    public override void _Ready()
    {
        availableBuses = EnumerateAvailableBuses();
    }

    public void ChangeVolume(string busName, float volume_value)
    {
        int busIndex = AudioServer.GetBusIndex(busName);

        if (busIndex == -1)
        {
            GD.PushError($"The bus with the name {busName} does not exists in this project");
            return;
        }

        AudioServer.SetBusVolumeDb(busIndex, Mathf.LinearToDb(volume_value));
    }


    public float GetActualVolumeDBFromBusName(string busName)
    {
        int busIndex = AudioServer.GetBusIndex(busName);

        if (busIndex == -1)
        {
            GD.PushError($"The bus with the name {busName} does not exists in this project");
            return 0.0f;
        }

        return GetActualVolumeDbFromBusIndex(busIndex);
    }

    public float GetActualVolumeDbFromBusIndex(int busIndex)
    {
        return Mathf.DbToLinear(AudioServer.GetBusVolumeDb(busIndex));
    }


    public string[] EnumerateAvailableBuses()
    {
        return Enumerable.Range(0, AudioServer.BusCount)
                     .Select(bus_index => AudioServer.GetBusName(bus_index))
                     .ToArray();
    }

    public bool IsStreamLooped(AudioStream stream)
    {
        if (stream is AudioStreamMP3 mp3Stream)
        {
            return mp3Stream.Loop;
        }

        if (stream is AudioStreamOggVorbis oggStream)
        {
            return oggStream.Loop;
        }

        if (stream is AudioStreamWav wavStream)
        {
            return wavStream.LoopMode.Equals(AudioStreamWav.LoopModeEnum.Disabled);
        }


        return false;
    }

}
