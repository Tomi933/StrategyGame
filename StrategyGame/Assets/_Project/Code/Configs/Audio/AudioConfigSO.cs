using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._Project.Code.Configs.Audio
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "Configs/Audio/AudioConfig")]
    public class AudioConfigSO : ScriptableObject
    {
        public List<AudioData> audioDatas = new List<AudioData>();

        public AudioClipData GetAudioData(string id)
        {
            foreach (AudioData audioData in audioDatas)
            {
                if (audioData.Id == id)
                    return audioData.Data;
            }

            throw new Exception($"Незнайшов кліп id: {id}");
        }

    }

    [Serializable]
    public struct AudioData

    {
        public string Id;  
        public AudioClipData Data;
    }

    [Serializable]
    public struct AudioClipData
    {
        public AudioClip clip;
        public float volume;
    }
}
