using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum BabeType
{
    Hot = 0,
    Ok = 1,
    Young = 2,
    Cop = 3
}

public enum OptionType
{
    Compliment,
    FakeBrand,
    Tickets,
    Dog,
    Candy,
    Drugs
}

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }
    private static readonly object padlock = new object();

    private int cops;
    public int Cops { get => cops; set => cops = value; }

    public float SpeedFactor = 1.0f;
    public float SpeedFactorIncrement = 0.65f;

    private int cash;
    public int Cash { get => cash; set => cash = value; }

    private int babesGathered;
    public int BabesGathered { get => babesGathered; set => babesGathered = value; }

    private int hotnessGathered;
    public int HotnessScore { get => hotnessGathered; set => hotnessGathered = value; }

    private Dictionary<BabeType, OptionType[]> babeOptions;
    public Dictionary<BabeType, OptionType[]> BabeOptions { get => babeOptions; set => babeOptions = value; }

    private Dictionary<OptionType, int> optionCosts;
    public Dictionary<OptionType, int> OptionCosts { get => optionCosts; set => optionCosts = value; }

    private bool actorsMovable = true;
    public bool ActorsMovable { get => actorsMovable; set => actorsMovable = value; }

    private bool finishedTutorial = false;

    #region GUI

    private GameObject uiCanvas;
    private GameObject pauseButton;
    private GameObject dialogueCanvas;
    private PlayableDirector dialoguePanelDirector;
    private Animator babeImageAnimator;
    private GameObject endOfLevelCanvas;
    private GameObject endOfLevelHotnessCounter;
    private GameObject gameOverCanvas;
    private GameObject copsCanvas;
    private GameObject exitGameCanvas;
    private GameObject dialogueSprite;
    private GameObject dialogueHotnessCounter;
    private GameObject[] dialogueButtons;
    private GameObject moneyCounter;

    private GameObject[] copsCounters;
    private GameObject babesCounter;
    private Timer dialogueTimer;

    #endregion

    private GameObject player;
    public Player Player { get => player.GetComponent<Player>(); }

    private void Awake()
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
                InitializeBabeDictionaries();
                SceneManager.sceneLoaded += OnLevelFinishedLoading;
            }
        }
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        HandleScreenFlashing();
    }

    private void Start()
    {

    }

    private void InitializeBabeDictionaries()
    {
        babeOptions = new Dictionary<BabeType, OptionType[]>();

        OptionType[] hotOptions = { OptionType.Drugs, OptionType.Dog };
        BabeOptions.Add(BabeType.Hot, hotOptions);

        OptionType[] okOptions = { OptionType.Tickets, OptionType.FakeBrand, OptionType.Drugs, OptionType.Dog };
        BabeOptions.Add(BabeType.Ok, okOptions);

        OptionType[] youngOptions = { OptionType.Tickets, OptionType.FakeBrand, OptionType.Candy, OptionType.Dog };
        BabeOptions.Add(BabeType.Young, youngOptions);

        optionCosts = new Dictionary<OptionType, int>
        {
            { OptionType.Compliment, 0 },
            { OptionType.Candy, 10 },
            { OptionType.Tickets, 300 },
            { OptionType.FakeBrand, 400 },
            { OptionType.Drugs, 1000 },
            { OptionType.Dog, 4000 }
        };
    }

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        player = GameObject.Find("Player");

        // meaning after the title screen:
        if (scene.buildIndex > 0)
        {
            dialogueCanvas = GameObject.Find("DialogueCanvas");
            if (dialogueCanvas != null)
                dialogueCanvas.GetComponent<Canvas>().enabled = false;
        
            GameObject dialoguePanel = GameObject.Find("DialoguePanel");
            if (dialoguePanel != null)
                dialoguePanelDirector = dialoguePanel.GetComponent<PlayableDirector>();

            GameObject babeImage = GameObject.Find("BabeImage");
            if (babeImage != null)
                babeImageAnimator = babeImage.GetComponent<Animator>();

            GameObject paparazziCanvas = GameObject.Find("PaparazziCanvas");
            if (paparazziCanvas != null)
                paparaziFlash = paparazziCanvas.GetComponent<CanvasGroup>();

            uiCanvas = GameObject.Find("UICanvas");
            pauseButton = GameObject.Find("PauseButton");

            endOfLevelCanvas = GameObject.Find("EndOfLevelCanvas");
            if (endOfLevelCanvas != null)
                endOfLevelCanvas.GetComponent<Canvas>().enabled = false;
            else
            {
                endOfLevelCanvas = GameObject.Find("EndOfTutorialCanvas");
                if (endOfLevelCanvas != null){
                    endOfLevelCanvas.GetComponent<Canvas>().enabled = false;
                }
            }
            endOfLevelHotnessCounter = GameObject.Find("TotalHotnessCounter");

            gameOverCanvas = GameObject.Find("GameOverCanvas");
            if (gameOverCanvas != null)
                gameOverCanvas.GetComponent<Canvas>().enabled = false;

            copsCanvas = GameObject.Find("CopsCanvas");
            if (copsCanvas != null)
                copsCanvas.GetComponent<Canvas>().enabled = false;

            exitGameCanvas = GameObject.Find("ExitGameCanvas");
            if (exitGameCanvas != null)
                exitGameCanvas.GetComponent<Canvas>().enabled = false;

            dialogueButtons = new GameObject[6];
            for (int i = 0; i < 6; i++)
            {
                dialogueButtons[i] = GameObject.Find(String.Format("DialogueButton{0}", i + 1));
            }
            dialogueSprite = GameObject.Find("DialogueSprite");
            dialogueHotnessCounter = GameObject.Find("HotnessCounter");
            dialogueTimer = GameObject.Find("DialogueTimer").GetComponent<Timer>();

            moneyCounter = GameObject.Find("MoneyCounter");
            babesCounter = GameObject.Find("BabesCounter");
            copsCounters = new GameObject[6];
            for (int i = 0; i < 5; i++){
                copsCounters[i] = GameObject.Find(String.Format("CopCounter{0}", i + 1));
            }

            // add money and make small changes according to each level's specifications
            switch (SceneManager.GetActiveScene().buildIndex)
            {
                case 1: //Tutorial
                    if (pauseButton != null){
                        pauseButton.SetActive(false);
                    }
                    cash = 600;
                    break;
                case 2: //Level 1
                    cash = 15000;
                    break;
                case 3: //Level 2
                    cash = 15000;
                    break;
                case 4: //Level 3
                    cash = 15000;
                    break;
                default:
                    break;
            }
            ResetLevelParameters();
        }
    }

    public void FirstLevelParametersInitialization()
    {
        babesGathered = 0;
        hotnessGathered = 0;
    }

    private void ResetLevelParameters()
    {
        actorsMovable = true;
        cops = 0;
        SpeedFactor = 1;
        UpdateCashGUI();
        UpdateCopsGUI();
        UpdateBabesGUI();
    }

    #region BabeInteraction

    public Babe currentBabe;

    public void StartDialogue(Babe babe)
    {
        if (actorsMovable)
        {
            switch (babe.babeType)
            {
                case BabeType.Hot:
                    SoundManager.Instance.PlayRandomDialogue(SoundManager.DialogueCategories.BatutaHot);
                    break;
                case BabeType.Young:
                    SoundManager.Instance.PlayRandomDialogue(SoundManager.DialogueCategories.BatutaYoung);
                    break;
                case BabeType.Ok:
                    SoundManager.Instance.PlayRandomDialogue(SoundManager.DialogueCategories.BatutaRegular);
                    break;
                case BabeType.Cop:
                    SoundManager.Instance.PlayRandomDialogue(SoundManager.DialogueCategories.GirlsCop);
                    break;
                default:
                    break;
            }
            SoundManager.Instance.LowerMusicVolume();

            dialogueTimer.StartTimer();
            currentBabe = babe;
            List<OptionType> options = optionCosts.Keys.ToList();
            for (int i = 0; i < 6; i++)
            {
                dialogueButtons[i].GetComponent<DialogueButton>().ChangeOption(options[i]);
            }
            dialogueSprite.GetComponent<Image>().sprite = babe.gameObject.GetComponent<SpriteRenderer>().sprite;
            dialogueHotnessCounter.GetComponent<TextMeshProUGUI>().text = babe.hotness.ToString();

            dialogueCanvas.GetComponent<Canvas>().enabled = true;
            dialoguePanelDirector.Play();

            Player.StartInteracting();
        }

    }

    public void ChooseOption(OptionType option)
    {
        dialogueTimer.Stop();

        //handle hidden cops!
        if (currentBabe.babeType == BabeType.Cop){
            uiCanvas.GetComponent<Canvas>().enabled = false;
            dialogueCanvas.GetComponent<Canvas>().enabled = false;
            copsCanvas.GetComponent<Canvas>().enabled = true;

            SoundManager.Instance.PlayRandomDialogue(SoundManager.DialogueCategories.BatutaGameOverPolice);
        }
        else //ok it's just a babe
        {
            DialogueResult result;
            //does the babe accept the option?
            if (babeOptions[currentBabe.babeType].Contains(option) || (option == OptionType.Compliment && ComplimentReceived())){
                Debug.Log("Chose Correctly!");

                babesGathered += 1;
                hotnessGathered += currentBabe.hotness;

                if (option == OptionType.Dog){
                    SoundManager.Instance.PlayRandomDialogue(SoundManager.DialogueCategories.GirlsDogYes);
                }
                else if (option == OptionType.Tickets){
                    SoundManager.Instance.PlayRandomDialogue(SoundManager.DialogueCategories.GirlsTicketsYes);
                }
                else if (currentBabe.babeType == BabeType.Young){
                    SoundManager.Instance.PlayRandomDialogue(SoundManager.DialogueCategories.GirlsYoungYes);
                }
                else{
                    SoundManager.Instance.PlayRandomDialogue(SoundManager.DialogueCategories.GirlsAllYes);
                }

                currentBabe.gameObject.SetActive(false);
                result = DialogueResult.accept;
            }
            else{
                Debug.Log("DENIED");

                if (SceneManager.GetActiveScene().buildIndex == 3){ //jerusalem level
                    if (option == OptionType.Compliment){
                        SoundManager.Instance.PlayTwoRandomDialogues(SoundManager.DialogueCategories.GirlsJerusalemComplimentNo, SoundManager.DialogueCategories.BatutaRejection);
                    }
                    else{
                        SoundManager.Instance.PlayTwoRandomDialogues(SoundManager.DialogueCategories.GirlsJerusalemNo, SoundManager.DialogueCategories.BatutaRejection);
                    }
                }
                else if (option == OptionType.FakeBrand){
                    SoundManager.Instance.PlayTwoRandomDialogues(SoundManager.DialogueCategories.GirlsBagNo, SoundManager.DialogueCategories.BatutaRejection);
                }
                else if (option == OptionType.Tickets){
                    SoundManager.Instance.PlayTwoRandomDialogues(SoundManager.DialogueCategories.GirlsTicketsNo, SoundManager.DialogueCategories.BatutaRejection);
                }
                else{
                    SoundManager.Instance.PlayTwoRandomDialogues(SoundManager.DialogueCategories.GirlsAllNo, SoundManager.DialogueCategories.BatutaRejection);
                }
                result = DialogueResult.decline;
            }

            if (currentBabe.babeType == BabeType.Young || (option == OptionType.Drugs && DrugsReported())){
                CallThePopo();
            }

            cash -= optionCosts[option];
            Player.StartMoneyParticles();
            SoundManager.Instance.PlaySoundEffect(SoundManager.SoundEffect.cash);

            //TODO: play money animation!
            StartCoroutine(CloseDialogue(result));
        }
    }

    private bool ComplimentReceived()
    {
        return Random.Range(0, 100) < ((currentBabe.babeType == BabeType.Young) ? 80 : 40);
    }

    private bool DrugsReported()
    {
        return (Random.Range(0, 100) <= 33);
    }

    public void CallThePopo()
    {
        SoundManager.Instance.PlaySoundEffect(SoundManager.SoundEffect.siren);
        Camera.main.gameObject.GetComponent<MainCameraLogic>().StartParticles();

        if (cops < 5){
            cops += 1;
            SpeedFactor += SpeedFactorIncrement;
        }
        else{
            Debug.Log("Max amount of cops!");
        }
        UpdateCopsGUI();
    }



    public enum DialogueResult
    {
        accept,
        decline,
        timeout,
        escape
    }

    public IEnumerator CloseDialogue(DialogueResult result)
    {
        switch (result)
        {
            case DialogueResult.accept:
                babeImageAnimator.SetTrigger("BabeAccept");
                yield return new WaitForSeconds(2f);
                break;
            case DialogueResult.decline:
                babeImageAnimator.SetTrigger("BabeReject");
                yield return new WaitForSeconds(1.0f);
                break;
            case DialogueResult.timeout:
                babeImageAnimator.SetTrigger("BabeReject");
                yield return new WaitForSeconds(1.0f);
                break;
            case DialogueResult.escape:
                yield return new WaitForSeconds(0.0f);
                break;
            default:
                break;
        }

        dialoguePanelDirector.Stop();

        Player.StopInteracting();
        SoundManager.Instance.DrivingMusicVolume();
        currentBabe.MarkInteracted();
        UpdateBabesGUI();
        UpdateCashGUI();
        UpdateCopsGUI();
        dialogueCanvas.GetComponent<Canvas>().enabled = false;
    }

    public void TimerRanOut()
    {
        //TODO: play sound of babe walking away
        StartCoroutine(CloseDialogue(DialogueResult.timeout));
    }

    private void UpdateCashGUI()
    {
        moneyCounter.GetComponent<TextMeshProUGUI>().text = Extentions.Reverse(cash.ToString());
    }

    private void UpdateCopsGUI()
    {
        for (int i = 0; i < 5; i++){
            copsCounters[i].GetComponent<Image>().sprite = Resources.Load<Sprite>((i < cops) ? "UI/police-full" : "UI/police-empty");
        }
    }

    private void UpdateBabesGUI()
    {
        babesCounter.GetComponent<TextMeshProUGUI>().text = Extentions.Reverse(babesGathered.ToString());
    }

    #endregion

    #region Hazards

    public bool HitByHazard(Hazard hazrd)
    {
        cash -= hazrd.Cost();
        UpdateCashGUI();
        Player.StartMoneyParticles();

        //GAME OVER
        if (cash < 0){
            Debug.Log("GameOver!");
            uiCanvas.GetComponent<Canvas>().enabled = false;
            gameOverCanvas.GetComponent<Canvas>().enabled = true;
            SoundManager.Instance.LowerMusicVolume();
            return false;
        }
        return true;
    }

    public CanvasGroup paparaziFlash;
    private bool lowerFlash = false;
    public float flashDuration = 3.0f;
    internal void Flash()
    {
        paparaziFlash.alpha = 1;
        Invoke("EndFlash", flashDuration);
    }

    private void EndFlash(){
        lowerFlash = true;
    }

    private void HandleScreenFlashing()
    {
        if (lowerFlash && paparaziFlash != null)
        {
            paparaziFlash.alpha = paparaziFlash.alpha - Time.deltaTime;
            if (paparaziFlash.alpha <= 0){
                paparaziFlash.alpha = 0;
                lowerFlash = false;
            }
        }
    }

    #endregion

    public void LevelEnded()
    {
        actorsMovable = false;
        uiCanvas.GetComponent<Canvas>().enabled = false;
        endOfLevelCanvas.GetComponent<Canvas>().enabled = true;
        if (endOfLevelHotnessCounter != null) //tutorial has no such thing
            endOfLevelHotnessCounter.GetComponent<TextMeshProUGUI>().text = Extentions.Reverse(hotnessGathered.ToString());

        SoundManager.Instance.StopBackgroundMusic();
        SoundManager.Instance.StopLevelMusic();
        SoundManager.Instance.PlaySoundEffect(SoundManager.SoundEffect.endOfLevel);
    }

    //called only from main screen... there must be a finer way:
    internal void StartGame()
    {
        //TODO: fade out + sound
        FirstLevelParametersInitialization();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + (finishedTutorial ? 2 : 1));
    }

    internal void FinishedTutorial(){
        finishedTutorial = true;
        NextLevel();
    }

    internal void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    internal void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    internal void ToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    bool paused = false;
    internal void ToggleMovementPause()
    {
        if (paused)
        {
            actorsMovable = true;
            paused = false;
        }
        //can't pause while in dialogue
        else if (actorsMovable)
        {
            actorsMovable = false;
            paused = true;
        }
    }

    internal void ToggleExitGameCanvas()
    {
        if (paused) //in the menu currently, and pressed cancel
        {
            actorsMovable = true;
            exitGameCanvas.GetComponent<Canvas>().enabled = false;
            SoundManager.Instance.StartBackgroundMusic();
            paused = false;
        }
        else if (actorsMovable) //can't pause while in dialogue, don't enter the activation phase
        {
            actorsMovable = false;
            exitGameCanvas.GetComponent<Canvas>().enabled = true;
            SoundManager.Instance.StopBackgroundMusic();
            paused = true;
        }
    }
}
