using Assets._Project.Code.CommonServices;
using Assets._Project.Code.Configs.Audio;
using UnityEngine;

namespace Assets._Project.Code.Infrustructure
{
    public static class GlobalServices
    {
        public static SceneLoader SceneLoader;
        public static AudioService AudioService;

        public static void Initialize()
        {
            SceneLoader = new SceneLoader();
        }

        public static void InitializeAudio(AudioSource sfxAudio, AudioSource musicAudio, AudioConfigSO audioConfig)
        {
            AudioService = new AudioService(sfxAudio, musicAudio, audioConfig);
        }
    }
}