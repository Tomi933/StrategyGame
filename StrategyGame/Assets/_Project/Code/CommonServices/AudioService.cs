using Assets._Project.Code.Configs.Audio;
using UnityEngine;

namespace Assets._Project.Code.CommonServices
{
    public class AudioService
    {
        private readonly AudioSource _sfxAudio;
        private readonly AudioSource _musicAudio;
        private readonly AudioConfigSO _audioConfig;

        public AudioService(AudioSource sfxAudio, AudioSource musicAudio, AudioConfigSO audioConfig)
        {
            _sfxAudio = sfxAudio;
            _musicAudio = musicAudio;
            _audioConfig = audioConfig;

            _sfxAudio.playOnAwake = false;
            _musicAudio.playOnAwake = false;
        }

        public void PlayClip(string id)
        {
            AudioClipData clipData = _audioConfig.GetAudioData(id);
            PlayClip(clipData.clip, clipData.volume);
        }

        public void PlayMusic(string id)
        {
            AudioClipData musicData = _audioConfig.GetAudioData(id);
            PlayMusic(musicData.clip, musicData.volume);
        }

        public void PlayMusic(AudioClip music, float volume = 1f)
        {
            _musicAudio.Stop();
            _musicAudio.clip = music;
            _musicAudio.Play();
            _musicAudio.volume = volume;
            _musicAudio.loop = true;
        }

        public void PlayClip(AudioClip clip, float volume = 1f)
        {
            _sfxAudio.PlayOneShot(clip, volume);
        }

        public void SetMusicEnabled(bool enabled)
        {
            if (enabled)
            {
                _musicAudio.Play();
            }
            else
            {
                _musicAudio.Stop();
            }
        }
    }
}