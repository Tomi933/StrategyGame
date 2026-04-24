using Assets._Project.Code.Configs.Audio;
using UnityEngine;

namespace Assets._Project.Code.Infrustructure
{
    public class GameEntryPoint : MonoBehaviour
    {
        public AudioConfigSO AudioConfig;

        private void Awake()
        {
            InitAudio(AudioConfig);

            GlobalServices.Initialize();

            GlobalServices.SceneLoader.LoadScene("Menu");
        }

        private static void InitAudio(AudioConfigSO audioConfig)
        {
            AudioSource musicSource = new GameObject("MUSIC").AddComponent<AudioSource>();
            AudioSource sfxSource = new GameObject("SFX").AddComponent<AudioSource>();

            DontDestroyOnLoad(musicSource.gameObject);
            DontDestroyOnLoad(sfxSource.gameObject);

            GlobalServices.InitializeAudio(musicSource, sfxSource, audioConfig);
        }
    }
}