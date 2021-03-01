using CSCore.CoreAudioAPI;

namespace MixerController {
    public class SoundApplication {

        public SoundApplication(float volume, ref SimpleAudioVolume audioInt) {
            Volume = volume;
            AudioInt = audioInt;
        }

        public float Volume { get; }

        public SimpleAudioVolume AudioInt { get; }

    }
}
