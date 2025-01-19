namespace Cherris.Backends;

public abstract class IAudioBackend
{
    public abstract bool IsAudioPlaying(Audio audio);
    public abstract float GetAudioTimePlayed(Audio audio);
    public abstract void SetAudioVolume(Audio audio, float volume);
    public abstract void SetAudioPitch(Audio audio, float pitch);
    public abstract void SetAudioPan(Audio audio, float pan);
    public abstract void UpdateAudio(Audio audio);
    public abstract void PlayAudio(Audio audio);
    public abstract void ResumeAudio(Audio audio);
    public abstract void PauseAudio(Audio audio);
    public abstract void StopAudio(Audio audio);
    public abstract void SeekAudio(Audio audio, float timestamp);
}