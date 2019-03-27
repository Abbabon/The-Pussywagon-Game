using UnityEngine;

public class MusicAndEffectsSource : MonoBehaviour
{
    // Start is called before the first frame update
    void Start(){
        SoundManager.Instance.RegisterSoundEffectsAudioSource(GetComponent<AudioSource>());
    }
}
