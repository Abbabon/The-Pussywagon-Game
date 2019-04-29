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
    private readonly String girlsString = "Girls";
    private readonly String etcString = "Etc";

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

    internal void ToggleSilence()
    {
        throw new NotImplementedException();
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
                case 0: //title screen
                    currentSoundEffectsAudioSource.clip = Resources.Load<AudioClip>("Music/Yalda");
                    StartBackgroundMusic();
                    return;
                case 1: //intro screen
                    currentSoundEffectsAudioSource.clip = Resources.Load<AudioClip>("Music/Tamar");
                    StartBackgroundMusic();
                    return;
                case 2: //tutorial
                    currentSoundEffectsAudioSource.clip = Resources.Load<AudioClip>("Music/Turkish2");
                    StartBackgroundMusic();
                    return;
                case 3: //level 1
                    currentSoundEffectsAudioSource.clip = Resources.Load<AudioClip>("Music/Turkish");
                    StartBackgroundMusic();
                    return;
                case 4: //level 2
                    currentSoundEffectsAudioSource.clip = Resources.Load<AudioClip>("Music/Middle");
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

    public void StopLevelMusic()
    {
        currentSoundEffectsAudioSource.clip = null;
    }

    public void StopBackgroundMusic()
    {
       currentSoundEffectsAudioSource.Pause();
    }

    public void LowerMusicVolume()
    {
       currentSoundEffectsAudioSource.volume = 0.1f;
    }

    public void DrivingMusicVolume()
    {
       currentSoundEffectsAudioSource.volume = 0.5f;
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
        stop,
        endOfLevel
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
        BatutaHazardPaparazzi,
        BatutaHazardPolice,
        BatutaHot,
        BatutaLevelStart,
        BatutaRegular,
        BatutaRejection,
        BatutaYoung,

        GirlsAllYes,
        GirlsAllNo,
        GirlsYoungYes,
        GirlsBagNo,
        GirlsDogYes,
        GirlsTicketsYes,
        GirlsTicketsNo,
        GirlsJerusalemComplimentNo,
        GirlsJerusalemNo,
        GirlsCop,

        EtcRingtone,
        EtcAnswer,
        EtcFriendHigh,
        EtcFriendLow,
        EtcFriendMid,
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
            for (int i = 1; i < 50; i++)
            {
                String assetPath;
                if (dialogueCategoryString.Contains(batutaString)){
                    assetPath = String.Format("Dialogue/{0}/{1}{2}", batutaString, dialogueCategoryString.Replace(batutaString, ""), i);
                }
                else if (dialogueCategoryString.Contains(girlsString)){
                    assetPath = String.Format("Dialogue/{0}/{1}{2}", girlsString, dialogueCategoryString.Replace(girlsString, ""), i);
                }
                else{
                    assetPath = String.Format("Dialogue/{0}/{1}{2}", etcString, dialogueCategoryString.Replace(etcString, ""), i);
                }

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

    private AudioClip GetRandomDialogueClip(DialogueCategories dialogueCategory)
    {
        return dialogues[dialogueCategory][Random.Range(0, dialogues[dialogueCategory].Count - 1)];
    }

    public void PlayRandomDialogue(DialogueCategories dialogueCategory)
    {
        currentDialogueAudioSource.Stop();
        currentDialogueAudioSource.PlayOneShot(GetRandomDialogueClip(dialogueCategory));
    }

    public void PlaySpecificDialogue(DialogueCategories dialogueCategory, int dialogueIndex)
    {
        currentDialogueAudioSource.Stop();
        currentDialogueAudioSource.PlayOneShot(dialogues[dialogueCategory][dialogueIndex]);
    }

    public void PlayTwoRandomDialogues(DialogueCategories dialogueCategory1, DialogueCategories dialogueCategory2)
    {
        currentDialogueAudioSource.Stop();
        StartCoroutine(PlayTwoClips(GetRandomDialogueClip(dialogueCategory1), GetRandomDialogueClip(dialogueCategory2)));
    }

    #endregion

    IEnumerator PlayTwoClips(AudioClip one, AudioClip two)
    {
        currentDialogueAudioSource.PlayOneShot(one);
        yield return new WaitForSeconds(one.length);
        currentDialogueAudioSource.PlayOneShot(two);
    }

}
