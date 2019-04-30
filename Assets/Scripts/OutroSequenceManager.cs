using System.Collections;
using System.Collections.Generic;
using CodeMonkey.Utils;
using UnityEngine;
using UnityEngine.Playables;

public class OutroSequenceManager : MonoBehaviour
{
    PlayableDirector sequence;

    //TODO: Sloppy. Remove when not 4AM.
    public GameObject fader;
    public GameObject faderStatic;

    private void Awake()
    {
        sequence = GetComponent<PlayableDirector>();
    }

    // Start is called before the first frame update
    void Start()
    {
        FunctionTimer.Create(() => SoundManager.Instance.PlaySoundEffect(SoundManager.SoundEffect.siren), 0.0f);
        FunctionTimer.Create(() => SoundManager.Instance.PlaySpecificDialogue(SoundManager.DialogueCategories.EtcHandcuffs, 0), 0.5f);
        FunctionTimer.Create(() => SoundManager.Instance.PlaySpecificDialogue(SoundManager.DialogueCategories.EtcThreat, 0), 1f);
        FunctionTimer.Create(() => SoundManager.Instance.StartBackgroundMusic(), 6f);
        FunctionTimer.Create(() => fader.GetComponent<Animator>().SetTrigger("FadeIn"), 6f);
        FunctionTimer.Create(() => faderStatic.SetActive(false), 6.1f);
        FunctionTimer.Create(() => sequence.Play(), 6f);
        //end
        FunctionTimer.Create(() => fader.GetComponent<Animator>().SetTrigger("FadeOut"), 29f);
        FunctionTimer.Create(() => GameManager.Instance.ToMainMenu(), 30.5f);
    }
}
