namespace GameRoot;

public interface ISoundQueue
{
    void PlaySound();
    void PlayWithPitchRange(float minPitchScale = 0.9f, float maxPitchScale = 1.3f);
}