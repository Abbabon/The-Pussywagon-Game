using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum BabeType
{
    Hot = 0,
    Ok = 1,
    Young = 2
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

    #region GUI

    private GameObject uiCanvas;
    private GameObject dialogueCanvas;
    private GameObject endOfLevelCanvas;
    private GameObject endOfLevelHotnessCounter;
    private GameObject gameOverCanvas;
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
        InitializeBabeDictionaries();
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
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

            GameObject paparazziCanvas = GameObject.Find("PaparazziCanvas");
            if (paparazziCanvas != null)
                paparaziFlash = paparazziCanvas.GetComponent<CanvasGroup>();

            uiCanvas = GameObject.Find("UICanvas");

            endOfLevelCanvas = GameObject.Find("EndOfLevelCanvas");
            if (endOfLevelCanvas != null)
                endOfLevelCanvas.GetComponent<Canvas>().enabled = false;
            endOfLevelHotnessCounter = GameObject.Find("TotalHotnessCounter");

            gameOverCanvas = GameObject.Find("GameOverCanvas");
            if (gameOverCanvas != null)
                gameOverCanvas.GetComponent<Canvas>().enabled = false;

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



            //TODO: add this cash in each level, and add it to what you saved on the previous level 
            cash = 15000;
            cops = 0;
            SpeedFactor = 1;
            babesGathered = 0;
            hotnessGathered = 0;
            UpdateCashGUI();
            UpdateCopsGUI();
            UpdateBabesGUI();
        }
    }

    #region BabeInteraction

    public Babe currentBabe;

    public void StartDialogue(Babe babe)
    {
        //TODO: start dialogue sound:

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

        Player.StartInteracting();
        SoundManager.Instance.LowerMusicVolume();
    }

    public void ChooseOption(OptionType option)
    {
        dialogueTimer.Stop();

        //does the babe accept the option?
        if (babeOptions[currentBabe.babeType].Contains(option) || (option == OptionType.Compliment && ComplimentReceived()))
        {
            Debug.Log("Chose Correctly!");

            //TODO: update babes counter
            babesGathered += 1;
            hotnessGathered += currentBabe.hotness;

            //TODO: play getting on animation & sound, THEN disable the babe.
            currentBabe.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("DENIED");

            //TODO: play the queue sound
        }

        //TODO: act on drugs if chosen!
        if (currentBabe.babeType == BabeType.Young || (option == OptionType.Drugs && DrugsReported())){
            CallThePopo();
        }

        cash -= optionCosts[option];
        CloseDialogue();
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
        //TODO: play siren sound

        if (cops < 5){
            cops += 1;
            SpeedFactor += SpeedFactorIncrement;
        }
        else{
            Debug.Log("Max amount of cops!");
        }
        UpdateCopsGUI();
    }

    public void CloseDialogue()
    {
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
        CloseDialogue();
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

    public bool HitByHazard(int hazardCost)
    {
        cash -= hazardCost;
        UpdateCashGUI();

        //GAME OVER
        if (cash < 0){
            Debug.Log("GameOver!");
            uiCanvas.GetComponent<Canvas>().enabled = false;
            gameOverCanvas.GetComponent<Canvas>().enabled = true;
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
        uiCanvas.GetComponent<Canvas>().enabled = false;
        endOfLevelCanvas.GetComponent<Canvas>().enabled = true;

        endOfLevelHotnessCounter.GetComponent<TextMeshProUGUI>().text = Extentions.Reverse(hotnessGathered.ToString());
    }

    //called only from main screen... there must be a finer way:
    internal void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    internal void RestartLevel()
    {
        GameManager.Instance.ActorsMovable = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    bool paused = false;
    internal void TogglePause()
    {
        if (paused)
        {
            actorsMovable = true;
            SoundManager.Instance.StartBackgroundMusic();
            paused = false;
        }
        //can't pause while in dialogue
        else if (actorsMovable)
        {
            actorsMovable = false;
            SoundManager.Instance.StopBackgroundMusic();
            paused = true;
        }
    }
}
