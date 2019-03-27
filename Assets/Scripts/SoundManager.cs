using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;
    public static SoundManager Instance { get { return _instance; } }
    private static readonly object padlock = new object();

    [Space(10)]
    private AudioSource currentSoundEffectsAudioSource;
    private AudioSource currentDialogueAudioSource;

    private readonly String batutaString = "Batuta";

    // Start is called before the first frame update
    void Awake()
    {
        Debug.Log("AWAKE");
        lock (padlock)
        {
            if (_instance != null && _instance != this)
            {
                Debug.Log("DESTROY");
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
                LoadSoundEffects();
                LoadDialogues();
            }
        }

        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayLevelMusic(){
        if (currentSoundEffectsAudioSource != null){
            Debug.Log(String.Format("GettingLevelMusic for {0}", SceneManager.GetActiveScene().buildIndex));
            switch (SceneManager.GetActiveScene().buildIndex)
            {
                case 0:
                   currentSoundEffectsAudioSource.clip = Resources.Load<AudioClip>("Music/Yalda");
                    StartBackgroundMusic();
                    return;
                    //TODO: return this when finding some better soundtrack
                case 1:
                   currentSoundEffectsAudioSource.clip = Resources.Load<AudioClip>("Music/Beitar");
                    StartBackgroundMusic();
                    return;
                default:
                   currentSoundEffectsAudioSource.clip = null;
                    return;
            }
        }
        Debug.Log("Current Audio Source is not found!");
    }

    public void RegisterSoundEffectsAudioSource(AudioSource audioSource)
    {
       currentSoundEffectsAudioSource = audioSource;
    }

    public void RegisterDialogueAudioSource(AudioSource audioSource)
    {
        currentDialogueAudioSource = audioSource;
    }

    public void StartBackgroundMusic()
    {
       currentSoundEffectsAudioSource.Play();
    }

    public void StopBackgroundMusic()
    {
       currentSoundEffectsAudioSource.Pause();
    }

    public void LowerMusicVolume()
    {
       currentSoundEffectsAudioSource.volume = 0.05f;
    }

    public void DrivingMusicVolume()
    {
       currentSoundEffectsAudioSource.volume = 0.1f;
    }

    #region SoundEffects

    public enum SoundEffect
    {
        carStart,
        cash,
        crashHole,
        crashPolice,
        crashLamed,
        flash,
        siren,
        stop
    }

    private Dictionary<SoundEffect, AudioClip> soundEffects;
    private void LoadSoundEffects()
    {
        soundEffects = new Dictionary<SoundEffect, AudioClip>();
        foreach (SoundEffect soundEffect in (SoundEffect[])Enum.GetValues(typeof(SoundEffect))){
            soundEffects.Add(soundEffect, Resources.Load<AudioClip>(String.Format("Effects/{0}", soundEffect)));
        }
    }

    public void PlaySoundEffect(SoundEffect soundEffect)
    {
       currentSoundEffectsAudioSource.PlayOneShot(soundEffects[soundEffect]);
    }

    #endregion

    #region Dialogue

    public enum DialogueCategories
    {
        BatutaBye,
        BatutaGameOverMoney,
        BatutaGameOverPolice,
        BatutaHazardHole,
        BatutaHazardLamed,
        BatutaHazrdPaparazzi,
        BatutaHazardPolice,
        BatutaHot,
        BatutaLevelStart,
        BatutaRegular,
        BatutaRejection,
        BatutaYoung
    }

    //TODO: try to refactor this to be more performable
    private Dictionary<DialogueCategories, List<AudioClip>> dialogues;
    private void LoadDialogues()
    {
        dialogues = new Dictionary<DialogueCategories, List<AudioClip>>();
        foreach (DialogueCategories dialogueCategory in (DialogueCategories[])Enum.GetValues(typeof(DialogueCategories)))
        {
            dialogues.Add(dialogueCategory, new List<AudioClip>());
            String dialogueCategoryString = dialogueCategory.ToString();
            Debug.Log(String.Format("Loading Dialogue for {0}", dialogueCategoryString));
            if (dialogueCategoryString.Contains(batutaString))
            {
                for (int i = 1; i < 50; i++)
                {
                    String assetPath = String.Format("Dialogue/Batuta/{0}{1}", dialogueCategoryString.Replace(batutaString, ""), i);
                    AudioClip clip = Resources.Load<AudioClip>(assetPath);
                    if (clip != null){
                        dialogues[dialogueCategory].Add(clip);
                        Debug.Log(String.Format("Loaded {0}", assetPath));
                    }
                    else{
                        break;
                    }
                }
            }
        }
    }

    public void PlayRandomDialogue(DialogueCategories dialogueCategory){
        currentDialogueAudioSource.PlayOneShot(dialogues[dialogueCategory][Random.Range(0, dialogues[dialogueCategory].Count-1)]);
    }

    #endregion

}
