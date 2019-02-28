using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum BabeType
{
    Hot = 0,
    Ok = 1,
    Young = 2
}

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }
    private static readonly object padlock = new object();

    private int cops;
    public int Cops { get => cops; set => cops = value; }

    private int cash;
    public int Cash { get => cash; set => cash = value; }

    private int babesGathered;
    public int BabesGathered { get => babesGathered; set => babesGathered = value; }

    private int hotnessGathered;
    public int HotnessScore { get => hotnessGathered; set => hotnessGathered = value; }

    private Dictionary<BabeType, Option[]> babeOptions;

    private bool actorsMovable = true;
    public bool ActorsMovable { get => actorsMovable; set => actorsMovable = value; }

    private PlayerMusic playerMusic;

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

    private void Awake()
    {
        Debug.Log("AWAKE");
        InitializeBabeDictionary();
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

    private void Start()
    {
       if (playerMusic != null){
            playerMusic.PlayLevelMusic();
        }

    }

    private void InitializeBabeDictionary()
    {
        babeOptions = new Dictionary<BabeType, Option[]>();
        Option[] hotOptions = new Option[4];
        hotOptions[0] = new Option(300, OptionType.iPhone);
        hotOptions[1] = new Option(300, OptionType.Watch);
        hotOptions[2] = new Option(400, OptionType.Jewlery);
        hotOptions[3] = new Option(500, OptionType.Dog);
        babeOptions.Add(BabeType.Hot, hotOptions);

        Option[] okOptions = new Option[4];
        okOptions[0] = new Option(0, OptionType.Compliment);
        okOptions[1] = new Option(50, OptionType.Chocolate);
        okOptions[2] = new Option(100, OptionType.FakeBrand);
        okOptions[3] = new Option(200, OptionType.Tickets);
        babeOptions.Add(BabeType.Ok, okOptions);

        Option[] youngOptions = new Option[4];
        youngOptions[0] = new Option(100, OptionType.HelloKitty);
        youngOptions[1] = new Option(10, OptionType.CandyBracelet);
        youngOptions[2] = new Option(5, OptionType.Candy);
        youngOptions[3] = new Option(50, OptionType.Slime);
        babeOptions.Add(BabeType.Young, youngOptions);
    }

    public AudioClip GetLevelMusic()
    {
        Debug.Log(String.Format("GettingLevelMusic for {0}", SceneManager.GetActiveScene().buildIndex));
        switch (SceneManager.GetActiveScene().buildIndex)
        {
            case 0:
                return Resources.Load<AudioClip>("Music/Yalda");
            case 1:
                return Resources.Load<AudioClip>("Music/Beitar");
            default:
                return null;
        }
    }

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        player = GameObject.Find("Player");
        playerMusic = player.GetComponent<PlayerMusic>();

        // meaning after the title screen:
        if (scene.buildIndex > 0)
        {
            dialogueCanvas = GameObject.Find("DialogueCanvas");
            if (dialogueCanvas != null)
                dialogueCanvas.GetComponent<Canvas>().enabled = false;

            uiCanvas = GameObject.Find("UICanvas");

            endOfLevelCanvas = GameObject.Find("EndOfLevelCanvas");
            if (endOfLevelCanvas != null)
                endOfLevelCanvas.GetComponent<Canvas>().enabled = false;
            endOfLevelHotnessCounter = GameObject.Find("TotalHotnessCounter");

            gameOverCanvas = GameObject.Find("GameOverCanvas");
            if (gameOverCanvas != null)
                gameOverCanvas.GetComponent<Canvas>().enabled = false;

            dialogueButtons = new GameObject[4];
            for (int i = 0; i < 4; i++)
            {
                dialogueButtons[i] = GameObject.Find(String.Format("DialogueButton{0}", i + 1));
            }
            dialogueSprite = GameObject.Find("DialogueSprite");
            dialogueHotnessCounter = GameObject.Find("HotnessCounter");
            dialogueTimer = GameObject.Find("DialogueTimer").GetComponent<Timer>();

            moneyCounter = GameObject.Find("MoneyCounter");
            babesCounter = GameObject.Find("BabesCounter");
            copsCounters = new GameObject[3];
            for (int i = 0; i < 3; i++)
            {
                copsCounters[i] = GameObject.Find(String.Format("CopCounter{0}", i + 1));
            }

            //TODO: add this cash in each level, and what you saved on the previous level 
            cash = 3000;
            cops = 0;
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
        for (int i = 0; i < 4; i++){
            dialogueButtons[i].GetComponent<DialogueButton>().ChangeOption(babeOptions[currentBabe.babeType][i]);
        }
        dialogueSprite.GetComponent<Image>().sprite = babe.gameObject.GetComponent<SpriteRenderer>().sprite;
        dialogueHotnessCounter.GetComponent<TextMeshProUGUI>().text = babe.hotness.ToString();

        dialogueCanvas.GetComponent<Canvas>().enabled = true;

        player.GetComponent<PlayerMovement>().StartInteracting();
        player.GetComponent<PlayerMusic>().LowerMusicVolume();
    }

    public void ChooseOption(Option option)
    {
        dialogueTimer.Stop();

        if (currentBabe.preferredOption == option.Type)
        {
            Debug.Log("Chose Correctly");

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

        cash -= option.Cost;
        CloseDialogue();
    }

    public void CloseDialogue()
    {
        player.GetComponent<PlayerMovement>().StopInteracting();
        player.GetComponent<PlayerMusic>().DrivingMusicVolume();
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
        for (int i = 0; i < 3; i++)
        {
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
        if (cash < 0)
        {
            Debug.Log("GameOver!");
            uiCanvas.GetComponent<Canvas>().enabled = false;
            gameOverCanvas.GetComponent<Canvas>().enabled = true;
            return false;
        }
        return true;
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
            player.GetComponent<AudioSource>().Play();
            paused = false;
        }
        //can't pause while in dialogue
        else if (actorsMovable)
        {
            actorsMovable = false;
            player.GetComponent<AudioSource>().Pause();
            paused = true;
        }
    }
}
