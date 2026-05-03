#nullable enable
namespace UniT.Audio
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using UniT.Extensions;
    using UniT.Logging;
    using UniT.ResourceManagement;
    using UnityEngine;
    using UnityEngine.Scripting;
    using ILogger = UniT.Logging.ILogger;
    #if UNIT_UNITASK
    using System.Threading;
    using Cysharp.Threading.Tasks;
    #else
    using System.Collections;
    #endif

    public sealed class AudioManager : IAudioManagerSettings, IAudioManager
    {
        #region Constructor

        private readonly AudioSettings masterSettings = new();
        private readonly AudioPool     soundPool;
        private readonly AudioPool     musicPool;

        [Preserve]
        public AudioManager(IAssetsManager assetsManager, ILoggerManager loggerManager)
        {
            var sourceContainer = new GameObject(nameof(AudioManager)).DontDestroyOnLoad();
            var sourcePool      = new Queue<AudioSource>();
            var logger          = loggerManager.GetLogger(this);

            this.soundPool = new(this.masterSettings, assetsManager, sourceContainer, sourcePool, logger);
            this.musicPool = new(this.masterSettings, assetsManager, sourceContainer, sourcePool, logger);

            logger.Debug("Constructed");
        }

        #endregion

        #region Settings

        event Action IAudioManagerSettings.SoundVolumeChanged { add => this.soundPool.Settings.VolumeChanged += value; remove => this.soundPool.Settings.VolumeChanged -= value; }

        event Action IAudioManagerSettings.MuteSoundChanged { add => this.soundPool.Settings.MuteChanged += value; remove => this.soundPool.Settings.MuteChanged -= value; }

        event Action IAudioManagerSettings.MusicVolumeChanged { add => this.musicPool.Settings.VolumeChanged += value; remove => this.musicPool.Settings.VolumeChanged -= value; }

        event Action IAudioManagerSettings.MuteMusicChanged { add => this.musicPool.Settings.MuteChanged += value; remove => this.musicPool.Settings.MuteChanged -= value; }

        event Action IAudioManagerSettings.MasterVolumeChanged { add => this.masterSettings.VolumeChanged += value; remove => this.masterSettings.VolumeChanged -= value; }

        event Action IAudioManagerSettings.MuteMasterChanged { add => this.masterSettings.MuteChanged += value; remove => this.masterSettings.MuteChanged -= value; }

        float IAudioManagerSettings.SoundVolume { get => this.soundPool.Settings.Volume; set => this.soundPool.Settings.Volume = value; }

        bool IAudioManagerSettings.MuteSound { get => this.soundPool.Settings.Mute; set => this.soundPool.Settings.Mute = value; }

        float IAudioManagerSettings.MusicVolume { get => this.musicPool.Settings.Volume; set => this.musicPool.Settings.Volume = value; }

        bool IAudioManagerSettings.MuteMusic { get => this.musicPool.Settings.Mute; set => this.musicPool.Settings.Mute = value; }

        float IAudioManagerSettings.MasterVolume { get => this.masterSettings.Volume; set => this.masterSettings.Volume = value; }

        bool IAudioManagerSettings.MuteMaster { get => this.masterSettings.Mute; set => this.masterSettings.Mute = value; }

        #endregion

        #region Sound

        event Action IAudioManager.EffectiveSoundVolumeChanged { add => this.soundPool.EffectiveVolumeChanged += value; remove => this.soundPool.EffectiveVolumeChanged -= value; }

        event Action IAudioManager.EffectiveMuteSoundChanged { add => this.soundPool.EffectiveMuteChanged += value; remove => this.soundPool.EffectiveMuteChanged -= value; }

        float IAudioManager.EffectiveSoundVolume => this.soundPool.EffectiveVolume;

        bool IAudioManager.EffectiveMuteSound => this.soundPool.EffectiveMute;

        void IAudioManager.RegisterSoundSource(AudioSource source) => this.soundPool.Register(source);

        void IAudioManager.UnregisterSoundSource(AudioSource soundSource) => this.soundPool.Unregister(soundSource);

        void IAudioManager.LoadSound(AudioClip clip) => this.soundPool.Load(clip);

        #if !UNITY_WEBGL
        void IAudioManager.LoadSound(object key) => this.soundPool.Load(key);
        #endif

        #if UNIT_UNITASK
        UniTask IAudioManager.LoadSoundAsync(object key, IProgress<float>? progress, CancellationToken cancellationToken) => this.soundPool.LoadAsync(key, progress, cancellationToken);
        #else
        IEnumerator IAudioManager.LoadSoundAsync(object key, Action? callback, IProgress<float>? progress) => this.soundPool.LoadAsync(key, callback, progress);
        #endif

        void IAudioManager.PlaySoundOneShot(AudioClip clip) => this.soundPool.PlayOneShot(clip);

        void IAudioManager.PlaySoundOneShot(object key) => this.soundPool.PlayOneShot(key);

        void IAudioManager.PlaySound(AudioClip clip, bool loop, bool force) => this.soundPool.Play(clip, loop, force);

        void IAudioManager.PlaySound(object key, bool loop, bool force) => this.soundPool.Play(key, loop, force);

        bool IAudioManager.IsSoundPlaying(AudioClip clip) => this.soundPool.IsPlaying(clip);

        bool IAudioManager.IsSoundPlaying(object key) => this.soundPool.IsPlaying(key);

        float IAudioManager.GetSoundTime(AudioClip clip) => this.soundPool.GetTime(clip);

        float IAudioManager.GetSoundTime(object key) => this.soundPool.GetTime(key);

        void IAudioManager.SetSoundTime(AudioClip clip, float time) => this.soundPool.SetTime(clip, time);

        void IAudioManager.SetSoundTime(object key, float time) => this.soundPool.SetTime(key, time);

        void IAudioManager.PauseSound(AudioClip clip) => this.soundPool.Pause(clip);

        void IAudioManager.PauseSound(object key) => this.soundPool.Pause(key);

        void IAudioManager.PauseAllSounds() => this.soundPool.PauseAll();

        void IAudioManager.ResumeSound(AudioClip clip) => this.soundPool.Resume(clip);

        void IAudioManager.ResumeSound(object key) => this.soundPool.Resume(key);

        void IAudioManager.ResumeAllSounds() => this.soundPool.ResumeAll();

        void IAudioManager.StopSound(AudioClip clip) => this.soundPool.Stop(clip);

        void IAudioManager.StopSound(object key) => this.soundPool.Stop(key);

        void IAudioManager.StopAllSounds() => this.soundPool.StopAll();

        void IAudioManager.UnloadSound(AudioClip clip) => this.soundPool.Unload(clip);

        void IAudioManager.UnloadSound(object key) => this.soundPool.Unload(key);

        void IAudioManager.UnloadAllSounds() => this.soundPool.UnloadAll();

        #endregion

        #region Music

        private object? playingMusic;

        event Action IAudioManager.EffectiveMusicVolumeChanged { add => this.musicPool.EffectiveVolumeChanged += value; remove => this.musicPool.EffectiveVolumeChanged -= value; }

        event Action IAudioManager.EffectiveMuteMusicChanged { add => this.musicPool.EffectiveMuteChanged += value; remove => this.musicPool.EffectiveMuteChanged -= value; }

        float IAudioManager.EffectiveMusicVolume => this.musicPool.EffectiveVolume;

        bool IAudioManager.EffectiveMuteMusic => this.musicPool.EffectiveMute;

        void IAudioManager.LoadMusic(AudioClip clip) => this.musicPool.Load(clip);

        #if !UNITY_WEBGL
        void IAudioManager.LoadMusic(object key) => this.musicPool.Load(key);
        #endif

        #if UNIT_UNITASK
        UniTask IAudioManager.LoadMusicAsync(object key, IProgress<float>? progress, CancellationToken cancellationToken) => this.musicPool.LoadAsync(key, progress, cancellationToken);
        #else
        IEnumerator IAudioManager.LoadMusicAsync(object key, Action? callback, IProgress<float>? progress) => this.musicPool.LoadAsync(key, callback, progress);
        #endif

        void IAudioManager.PlayMusic(AudioClip clip, bool loop, bool force)
        {
            if (this.playingMusic is { } && !ReferenceEquals(this.playingMusic, clip))
            {
                this.musicPool.Stop(clip);
                this.playingMusic = null;
            }
            this.musicPool.Play(clip, loop, force);
            this.playingMusic = clip;
        }

        void IAudioManager.PlayMusic(object key, bool loop, bool force)
        {
            if (this.playingMusic is { } && this.playingMusic != key)
            {
                this.musicPool.Stop(key);
                this.playingMusic = null;
            }
            this.musicPool.Play(key, loop, force);
            this.playingMusic = key;
        }

        bool IAudioManager.IsMusicPlaying()
        {
            switch (this.playingMusic)
            {
                case AudioClip clip: return this.musicPool.IsPlaying(clip);
                case { } key:        return this.musicPool.IsPlaying(key);
                default:             return false;
            }
        }

        float IAudioManager.GetMusicTime()
        {
            switch (this.playingMusic)
            {
                case AudioClip clip: return this.musicPool.GetTime(clip);
                case { } key:        return this.musicPool.GetTime(key);
                default:             return 0;
            }
        }

        void IAudioManager.SetMusicTime(float time)
        {
            switch (this.playingMusic)
            {
                case AudioClip clip: this.musicPool.SetTime(clip, time); break;
                case { } key:        this.musicPool.SetTime(key, time); break;
            }
        }

        void IAudioManager.PauseMusic()
        {
            switch (this.playingMusic)
            {
                case AudioClip clip: this.musicPool.Pause(clip); break;
                case { } key:        this.musicPool.Pause(key); break;
            }
        }

        void IAudioManager.ResumeMusic()
        {
            switch (this.playingMusic)
            {
                case AudioClip clip: this.musicPool.Resume(clip); break;
                case { } key:        this.musicPool.Resume(key); break;
            }
        }

        void IAudioManager.StopMusic()
        {
            switch (this.playingMusic)
            {
                case AudioClip clip: this.musicPool.Stop(clip); break;
                case { } key:        this.musicPool.Stop(key); break;
            }
            this.playingMusic = null;
        }

        void IAudioManager.UnloadMusic(AudioClip clip)
        {
            this.musicPool.Unload(clip);
            if (ReferenceEquals(this.playingMusic, clip))
            {
                this.playingMusic = null;
            }
        }

        void IAudioManager.UnloadMusic(object key)
        {
            this.musicPool.Unload(key);
            if (this.playingMusic == key)
            {
                this.playingMusic = null;
            }
        }

        void IAudioManager.UnloadAllMusics()
        {
            this.musicPool.UnloadAll();
            this.playingMusic = null;
        }

        #endregion

        private sealed class AudioPool
        {
            private readonly AudioSettings      masterSettings;
            private readonly IAssetsManager     assetsManager;
            private readonly GameObject         sourcesContainer;
            private readonly Queue<AudioSource> sourcePool;
            private readonly ILogger            logger;

            private readonly HashSet<AudioSource>               registeredSources = new();
            private readonly Dictionary<object, AudioClip>      keyToClip         = new();
            private readonly Dictionary<AudioClip, AudioSource> clipToSource      = new();

            public AudioPool(AudioSettings masterSettings, IAssetsManager assetsManager, GameObject sourcesContainer, Queue<AudioSource> sourcePool, ILogger logger)
            {
                this.masterSettings   = masterSettings;
                this.assetsManager    = assetsManager;
                this.sourcesContainer = sourcesContainer;
                this.sourcePool       = sourcePool;
                this.logger           = logger;

                masterSettings.VolumeChanged += this.OnEffectiveVolumeChanged;
                masterSettings.MuteChanged   += this.OnEffectiveMuteChanged;

                this.Settings.VolumeChanged += this.OnEffectiveVolumeChanged;
                this.Settings.MuteChanged   += this.OnEffectiveMuteChanged;
            }

            public AudioSettings Settings { get; } = new();

            #region Public

            public event Action? EffectiveVolumeChanged;

            public event Action? EffectiveMuteChanged;

            public float EffectiveVolume => this.Settings.Volume * this.masterSettings.Volume;

            public bool EffectiveMute => this.Settings.Mute || this.masterSettings.Mute;

            public void Register(AudioSource source)
            {
                this.Configure(source);
                this.registeredSources.Add(source);
            }

            public void Unregister(AudioSource source)
            {
                this.registeredSources.Remove(source);
            }

            public AudioSource Load(AudioClip clip)
            {
                return this.clipToSource.GetOrAdd(clip, static state =>
                {
                    var source = state.@this.sourcePool.DequeueOrDefault(static sourcesContainer => sourcesContainer.AddComponent<AudioSource>(), state.@this.sourcesContainer);
                    state.@this.Configure(source);
                    source.clip = state.clip;
                    state.@this.logger.Debug($"Loaded {state.clip.name}");
                    return source;
                }, (@this: this, clip));
            }

            #if !UNITY_WEBGL
            public void Load(object key)
            {
                var clip = this.keyToClip.GetOrAdd(key, static state => state.assetsManager.Load<AudioClip>(state.key), (this.assetsManager, key));
                this.Load(clip);
            }
            #endif

            #if UNIT_UNITASK
            public async UniTask LoadAsync(object key, IProgress<float>? progress, CancellationToken cancellationToken)
            {
                var clip = await this.keyToClip.GetOrAddAsync(key, static state => state.assetsManager.LoadAsync<AudioClip>(state.key, state.progress, state.cancellationToken), (this.assetsManager, key, progress, cancellationToken));
                this.Load(clip);
            }
            #else
            public IEnumerator LoadAsync(object key, Action? callback, IProgress<float>? progress)
            {
                var clip = default(AudioClip)!;
                yield return this.keyToClip.GetOrAddAsync(
                    key,
                    callback => this.assetsManager.LoadAsync(key, callback, progress),
                    result => clip = result
                );
                this.Load(clip);
                callback?.Invoke();
            }
            #endif

            public void PlayOneShot(AudioClip clip)
            {
                var source = this.GetOrLoadSource(clip);
                source.PlayOneShot(source.clip);
                this.logger.Debug($"Playing one shot {clip.name}");
            }

            public void PlayOneShot(object key)
            {
                var clip = this.GetOrLoadClip(key);
                this.PlayOneShot(clip);
            }

            public void Play(AudioClip clip, bool loop, bool force)
            {
                var source = this.GetOrLoadSource(clip);
                source.loop = loop;
                if (!force && source.isPlaying) return;
                source.Play();
                this.logger.Debug($"Playing {clip.name}, loop: {loop}");
            }

            public void Play(object key, bool loop, bool force)
            {
                var clip = this.GetOrLoadClip(key);
                this.Play(clip, loop, force);
            }

            public bool IsPlaying(AudioClip clip)
            {
                if (!this.TryGetSource(clip, out var source)) return false;
                return source.isPlaying;
            }

            public bool IsPlaying(object key)
            {
                if (!this.TryGetClip(key, out var clip)) return false;
                return this.IsPlaying(clip);
            }

            public float GetTime(AudioClip clip)
            {
                if (!this.TryGetSource(clip, out var source)) return 0;
                return source.time;
            }

            public float GetTime(object key)
            {
                if (!this.TryGetClip(key, out var clip)) return 0;
                return this.GetTime(clip);
            }

            public void SetTime(AudioClip clip, float time)
            {
                if (!this.TryGetSource(clip, out var source)) return;
                source.time = time;
                this.logger.Debug($"Set {clip.name} time to {time}");
            }

            public void SetTime(object key, float time)
            {
                if (!this.TryGetClip(key, out var clip)) return;
                this.SetTime(clip, time);
            }

            public void Pause(AudioClip clip)
            {
                if (!this.TryGetSource(clip, out var source)) return;
                source.Pause();
                this.logger.Debug($"Paused {clip.name}");
            }

            public void Pause(object key)
            {
                if (!this.TryGetClip(key, out var clip)) return;
                this.Pause(clip);
            }

            public void PauseAll()
            {
                this.clipToSource.Keys.ForEach(this.Pause);
            }

            public void Resume(AudioClip clip)
            {
                if (!this.TryGetSource(clip, out var source)) return;
                source.UnPause();
                this.logger.Debug($"Resumed {clip.name}");
            }

            public void Resume(object key)
            {
                if (!this.TryGetClip(key, out var clip)) return;
                this.Resume(clip);
            }

            public void ResumeAll()
            {
                this.clipToSource.Keys.ForEach(this.Resume);
            }

            public void Stop(AudioClip clip)
            {
                if (!this.TryGetSource(clip, out var source)) return;
                source.Stop();
                this.logger.Debug($"Stopped {clip.name}");
            }

            public void Stop(object key)
            {
                if (!this.TryGetClip(key, out var clip)) return;
                this.Stop(clip);
            }

            public void StopAll()
            {
                this.clipToSource.Keys.ForEach(this.Stop);
            }

            public void Unload(AudioClip clip)
            {
                if (!this.TryGetSource(clip, out var source)) return;
                source.Stop();
                source.clip = null;
                this.clipToSource.Remove(clip);
                this.sourcePool.Enqueue(source);
                this.logger.Debug($"Unloaded {clip.name}");
            }

            public void Unload(object key)
            {
                if (!this.TryGetClip(key, out var clip)) return;
                this.Unload(clip);
                this.assetsManager.Unload(key);
                this.keyToClip.Remove(key);
            }

            public void UnloadAll()
            {
                this.keyToClip.Keys.SafeForEach(this.Unload);
                this.clipToSource.Keys.SafeForEach(this.Unload);
            }

            #endregion

            #region Private

            private void OnEffectiveVolumeChanged()
            {
                this.clipToSource.ForEach(this.ConfigureVolume);
                this.registeredSources.ForEach(this.ConfigureVolume);
                this.EffectiveVolumeChanged?.Invoke();
            }

            private void OnEffectiveMuteChanged()
            {
                this.clipToSource.ForEach(this.ConfigureMute);
                this.registeredSources.ForEach(this.ConfigureMute);
                this.EffectiveMuteChanged?.Invoke();
            }

            private void Configure(AudioSource source)
            {
                this.ConfigureVolume(source);
                this.ConfigureMute(source);
            }

            private void ConfigureVolume(AudioSource source)
            {
                source.volume = this.EffectiveVolume;
            }

            private void ConfigureMute(AudioSource source)
            {
                source.mute = this.EffectiveMute;
            }

            private AudioSource GetOrLoadSource(AudioClip clip)
            {
                if (this.clipToSource.TryGetValue(clip, out var source)) return source;
                source = this.Load(clip);
                this.logger.Warning($"Auto loaded {clip.name}. Consider preload it with `Load` or `LoadAsync` for better performance.");
                return source;
            }

            private AudioClip GetOrLoadClip(object key)
            {
                return this.keyToClip.GetOrAdd(key, static state =>
                {
                    #if !UNITY_WEBGL
                    return state.assetsManager.Load<AudioClip>(state.key);
                    #else
                    throw new NotSupportedException("Cannot directly Play with key on WebGL. Please preload it with `LoadAsync`.");
                    #endif
                }, (this.assetsManager, key));
            }

            private bool TryGetSource(AudioClip clip, [MaybeNullWhen(false)] out AudioSource source)
            {
                if (this.clipToSource.TryGetValue(clip, out source)) return true;
                this.logger.Warning($"{clip.name} not loaded");
                return false;
            }

            private bool TryGetClip(object key, [MaybeNullWhen(false)] out AudioClip clip)
            {
                if (this.keyToClip.TryGetValue(key, out clip)) return true;
                this.logger.Warning($"{key} not loaded");
                return false;
            }

            #endregion
        }

        private sealed class AudioSettings
        {
            public event Action? VolumeChanged;

            public event Action? MuteChanged;

            public float Volume
            {
                get => this.volume;
                set
                {
                    if (value is < 0 or > 1)
                    {
                        throw new ArgumentOutOfRangeException(nameof(value), "Volume must be between 0 and 1");
                    }
                    this.volume = value;
                    this.VolumeChanged?.Invoke();
                }
            }

            public bool Mute
            {
                get => this.mute;
                set
                {
                    this.mute = value;
                    this.MuteChanged?.Invoke();
                }
            }

            private float volume = 1;
            private bool  mute   = false;
        }
    }
}