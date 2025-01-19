using SDL2;

namespace Cherris.Backends;

public sealed class SDL2AudioBackend : IAudioBackend
{
    public override bool IsAudioPlaying(Audio audio)
    {
        return false;
    }

    public override float GetAudioTimePlayed(Audio audio)
    {
        return 0;
    }

    public override void SetAudioVolume(Audio audio, float volume)
    {
        volume = (int)(volume * SDL_mixer.MIX_MAX_VOLUME);
        _ = SDL_mixer.Mix_VolumeMusic((int)volume);
    }

    public override void SetAudioPitch(Audio audio, float pitch)
    {

    }

    public override void SetAudioPan(Audio audio, float pan)
    {

    }

    public override void UpdateAudio(Audio audio)
    {

    }

    public override void PlayAudio(Audio audio)
    {
        _ = SDL_mixer.Mix_PlayMusic(audio, -1);
    }

    public override void ResumeAudio(Audio audio)
    {

    }

    public override void PauseAudio(Audio audio)
    {

    }

    public override void StopAudio(Audio audio)
    {

    }

    public override void SeekAudio(Audio audio, float timestamp)
    {

    }
}