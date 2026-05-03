#nullable enable
namespace UniT.Audio
{
    using System;

    public interface IAudioManagerSettings
    {
        public event Action SoundVolumeChanged;

        public event Action MuteSoundChanged;

        public event Action MusicVolumeChanged;

        public event Action MuteMusicChanged;

        public event Action MasterVolumeChanged;

        public event Action MuteMasterChanged;

        public float SoundVolume { get; set; }

        public bool MuteSound { get; set; }

        public float MusicVolume { get; set; }

        public bool MuteMusic { get; set; }

        public float MasterVolume { get; set; }

        public bool MuteMaster { get; set; }
    }
}