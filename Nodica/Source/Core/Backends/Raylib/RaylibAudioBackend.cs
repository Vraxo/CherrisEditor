using Raylib_cs;

namespace Cherris.Backends;

public sealed class RaylibAudioBackend : IAudioBackend
{
    public override bool IsAudioPlaying(Audio audio)
    {
        return Raylib.IsMusicStreamPlaying(audio);
    }

    public override float GetAudioTimePlayed(Audio audio)
    {
        return Raylib.GetMusicTimePlayed(audio);
    }

    public override void SetAudioVolume(Audio audio, float volume)
    {
        Raylib.SetMusicVolume(audio, volume);
    }

    public override void SetAudioPitch(Audio audio, float pitch)
    {
        Raylib.SetMusicPitch(audio, pitch);
    }

    public override void SetAudioPan(Audio audio, float pan)
    {
        Raylib.SetMusicPitch(audio, pan);
    }

    public override void UpdateAudio(Audio audio)
    {
        Raylib.UpdateMusicStream(audio);
    }

    public override void PlayAudio(Audio audio)
    {
        Raylib.PlayMusicStream(audio);
    }

    public override void ResumeAudio(Audio audio)
    {
        Raylib.ResumeMusicStream(audio);
    }

    public override void PauseAudio(Audio audio)
    {
        Raylib.PauseMusicStream(audio);
    }

    public override void StopAudio(Audio audio)
    {
        Raylib.StopMusicStream(audio);
    }

    public override void SeekAudio(Audio audio, float timestamp)
    {
        Raylib.SeekMusicStream(audio, timestamp);
    }
}