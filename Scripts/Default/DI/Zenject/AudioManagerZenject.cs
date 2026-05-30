#nullable enable
namespace UniT.Audio.Default.DI
{
    using Zenject;

    public static class AudioManagerZenject
    {
        public static void BindAudioManager(this DiContainer container)
        {
            container.BindInterfacesTo<AudioManager>().AsSingle();
        }
    }
}