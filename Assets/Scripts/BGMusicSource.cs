using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMusicSource : MonoBehaviour
{
    public enum BGMusicSourceType
    {
        Dialogue,
        SoundEffects
    }

    public BGMusicSourceType type;

    private void Awake(){
        switch (type)
        {
            case BGMusicSourceType.Dialogue:
                SoundManager.Instance.RegisterDialogueAudioSource(GetComponent<AudioSource>());
                break;
            case BGMusicSourceType.SoundEffects:
                SoundManager.Instance.RegisterSoundEffectsAudioSource(GetComponent<AudioSource>());
                break;
            default:
                break;
        }
    }

    // Start is called before the first frame update
    void Start(){
        SoundManager.Instance.PlayLevelMusic();
    }
}
