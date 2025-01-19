namespace Cherris;

public class AudioPlayer : Node
{
    private Audio? _audio;
    public Audio? Audio
    {
        get => _audio;

        set
        {
            _audio = value;
            Volume = _volume;
            Pitch = _pitch;
            Pan = _pan;
        }
    }

    private static Backends.Backend Backend => App.Instance.Backend;

    public bool AutoPlay { get; set; } = false;
    public bool Loop { get; set; } = false;
    public bool Playing = false;
    public float TimePlayed => Backend.Audio.GetAudioTimePlayed(Audio!);

    private float _volume = 1;
    public float Volume
    {
        get => _volume;

        set
        {
            _volume = value;

            if (Audio is null)
            {
                return;
            }

            Backend.Audio.SetAudioVolume(Audio, _volume);
        }
    }

    private float _pitch = 1;
    public float Pitch
    {
        get => _pitch;

        set
        {
            _pitch = value;

            if (Audio is null)
            {
                return;
            }

            Backend.Audio.SetAudioPitch(Audio, _pitch);
        }
    }

    private float _pan = 1f;
    public float Pan
    {
        get => _pan;

        set
        {
            if (Audio is null)
            {
                return;
            }

            _pan = value;
            Backend.Audio.SetAudioPan(Audio, _pan);
        }
    }

    public delegate void AudioPlayerEventHandler(AudioPlayer sender);
    public event AudioPlayerEventHandler? Paused;
    public event AudioPlayerEventHandler? Resumed;
    public event AudioPlayerEventHandler? Finished;

    public override void Ready()
    {
        if (Audio is null)
        {
            return;
        }

        if (AutoPlay)
        {
            Play();
        }
    }

    public override void Update()
    {
        if (Audio is null)
        {
            return;
        }

        Backend.Audio.UpdateAudio(Audio);

        if (TimePlayed >= Audio.Length - 0.1 && Playing)
        {
            if (Loop)
            {
                Play();
                Finished?.Invoke(this);
            }
            else
            {
                Stop();
            }
        }
    }

    public void Play(float timestamp = 0.1f)
    {
        if (Audio is null)
        {
            return;
        }

        Playing = true;

        timestamp = Math.Clamp(timestamp, 0.1f, Audio.Length);

        if (timestamp >= Audio.Length - 0.1f)
        {
            Stop();
        }
        else
        {
            Backend.Audio.SeekAudio(Audio, timestamp);
            Backend.Audio.PlayAudio(Audio);
        }
    }

    public void Resume()
    {
        if (Audio is null)
        {
            return;
        }

        Backend.Audio.ResumeAudio(Audio);
        Resumed?.Invoke(this);
    }

    public void Pause()
    {
        if (Audio is null)
        {
            return;
        }

        Backend.Audio.PauseAudio(Audio);
        Paused?.Invoke(this);
    }

    public void Stop()
    {
        if (Audio is null)
        {
            return;
        }

        Playing = false;

        Backend.Audio.StopAudio(Audio);
        Finished?.Invoke(this);
    }

    public void Seek(float timestamp)
    {
        if (Audio is null)
        {
            return;
        }

        timestamp = Math.Clamp(timestamp, 0.1f, Audio.Length);

        Backend.Audio.SeekAudio(Audio, timestamp);
    }
}