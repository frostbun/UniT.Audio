#nullable enable
namespace UniT.Audio
{
    using System;

    public interface IAudioSettings
    {
        public event Action VolumeChanged;

        public event Action MuteChanged;

        public float Volume { get; set; }

        public bool Mute { get; set; }
    }
}