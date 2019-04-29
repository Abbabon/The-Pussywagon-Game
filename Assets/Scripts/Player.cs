using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class Player : MonoBehaviour
{

    internal float horizontalVelocity = 1.8f;
    internal float defaultHorizontalVelocity = 1.8f;
    internal float slowedHorizontalVelocity = 0.95f;
    internal float laneHeight = 0.75f;
    internal int numberOfLanes = 3;
    internal int currentLane = 2;

    private Rigidbody2D rb;
    public Animator animator;
    public SwipeController swipeController;
    public SpriteRenderer[] spriteRenderers;
    public GameObject phoneHandGameObject;

    public ParticleSystem moneyParticles;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        SoundManager.Instance.RegisterDialogueAudioSource(GetComponent<AudioSource>());

        StartLevelRoutine();
    }

    private void StartLevelRoutine()
    {
        SoundManager.Instance.PlayLevelMusic();

        switch (SceneManager.GetActiveScene().buildIndex)
        {
            case 2: // tutorial
                SoundManager.Instance.PlaySoundEffect(SoundManager.SoundEffect.carStart);
                SoundManager.Instance.PlaySpecificDialogue(SoundManager.DialogueCategories.BatutaRegular, 0);
                break;
            case 3: // level 1
                SoundManager.Instance.PlaySoundEffect(SoundManager.SoundEffect.carStart);
                SoundManager.Instance.PlaySpecificDialogue(SoundManager.DialogueCategories.BatutaLevelStart, 0);
                break; 
            case 4: // level 2
                SoundManager.Instance.PlaySoundEffect(SoundManager.SoundEffect.carStart);
                SoundManager.Instance.PlaySpecificDialogue(SoundManager.DialogueCategories.BatutaLevelStart, 1);
                break;
            case 5: // level 3
                SoundManager.Instance.PlaySoundEffect(SoundManager.SoundEffect.carStart);
                SoundManager.Instance.PlaySpecificDialogue(SoundManager.DialogueCategories.BatutaLevelStart, 2);
                break;
            default:
                break;
        }

        horizontalVelocity = defaultHorizontalVelocity;
    }

    Tween currentLaneMovementTween;
    // Update is called once per frame
    void Update()
    {
        //TODO: remove this when starting level
        if (GameManager.Instance.ActorsMovable)
        {

            // handle horizontal movement
            rb.velocity = new Vector2((horizontalVelocity * GameManager.Instance.SpeedFactor), rb.velocity.y);

            // handle vertical movement
            if ((swipeController.SwipeUp || Input.GetKeyDown(KeyCode.UpArrow)) && (currentLane < numberOfLanes))
            {
                if (currentLaneMovementTween == null || !currentLaneMovementTween.active)
                {
                    currentLaneMovementTween = transform.DOMoveY(transform.position.y + laneHeight, 0.15f);
                    currentLane++;
                }
            }
            else if ((swipeController.SwipeDown || Input.GetKeyDown(KeyCode.DownArrow)) && (currentLane > 1))
            {
                if (currentLaneMovementTween == null || !currentLaneMovementTween.active)
                {
                    currentLaneMovementTween = transform.DOMoveY(transform.position.y - laneHeight, 0.15f);
                    currentLane--;
                }
            }
            else if (currentBabe != null && Input.GetKeyDown(KeyCode.F)){
                currentBabe.Interact();
            }
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    public void StartInteracting()
    {
        GameManager.Instance.ActorsMovable = false;
    }

    public void StopInteracting()
    {
        GameManager.Instance.ActorsMovable = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HazardDriver hazardDriver = collision.gameObject.GetComponent<HazardDriver>();
        if (hazardDriver != null)
            hazardDriver.StartDriving();

        Money money = collision.gameObject.GetComponent<Money>();
        if (money != null && (transform.position.y > money.transform.position.y))
            GameManager.Instance.CollectedMoney(money);

        if (collision.CompareTag("LevelEndZone")){
            GameManager.Instance.LevelEnded();
        }

        if (collision.CompareTag("SlowdownZone")){
            Destroy(collision.gameObject);
            horizontalVelocity = slowedHorizontalVelocity;
            GameManager.Instance.SpeedFactor = 1;
            GameManager.Instance.SlowdownSequenceInitiated();
        }
    }

    public void EnablePhoneHand(){
        phoneHandGameObject.SetActive(true);
    }

    Babe currentBabe;
    private void OnTriggerExit2D(Collider2D collision)
    {
        Babe babe = collision.gameObject.GetComponent<Babe>();
        if (babe != null)
        {
            babe.DisableInteration();
            currentBabe = null;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (GameManager.Instance.ActorsMovable)
        {
            Babe babe = collision.gameObject.GetComponent<Babe>();
            if (babe != null && babe.Lane == currentLane)
            {
                babe.EnableInteration();
                currentBabe = babe;
            }
            else if (babe != null)
            {
                babe.DisableInteration();
                currentBabe = null;
            }

            Hazard hazard = collision.gameObject.GetComponent<Hazard>();
            if (hazard != null)
            {
                if (hazard.Lane != currentLane)
                    return;

                rb.velocity = Vector2.zero;
                GameManager.Instance.ActorsMovable = false;

                if (GameManager.Instance.HitByHazard(hazard))
                {
                    StartFlickering();
                    Destroy(collision.gameObject);
                    switch (hazard.HazardType)
                    {
                        case HazardType.CopsBarricade:
                            SoundManager.Instance.PlayRandomDialogue(SoundManager.DialogueCategories.BatutaHazardPolice);
                            SoundManager.Instance.PlaySoundEffect(SoundManager.SoundEffect.crashPolice);
                            break;
                        case HazardType.Driver:
                            SoundManager.Instance.PlayRandomDialogue(SoundManager.DialogueCategories.BatutaHazardLamed);
                            SoundManager.Instance.PlaySoundEffect(SoundManager.SoundEffect.crashLamed);
                            break;
                        case HazardType.Hole:
                            SoundManager.Instance.PlayRandomDialogue(SoundManager.DialogueCategories.BatutaHazardHole);
                            SoundManager.Instance.PlaySoundEffect(SoundManager.SoundEffect.crashHole);
                            break;
                        default:
                            break;
                    }
                    animator.SetTrigger("HitByHazard");
                }
                else
                {
                    SoundManager.Instance.PlayRandomDialogue(SoundManager.DialogueCategories.BatutaGameOverMoney);
                    GameManager.Instance.ActorsMovable = false;
                    SROff();
                }
            }
        }
    }

    public void StartMoneyParticles()
    {
        moneyParticles.Play();
    }

    private readonly float interval = 0.25f;
    private void StartFlickering()
    {
        for (int i = 0; i < 3; i++){
            Invoke("SROff", interval*(i*2));
            Invoke("SROn", (interval*(i*2 + 1)));
        }
        Invoke("MakeMovable", interval*6);

    }

    private void SROff()
    {
        foreach (SpriteRenderer spriteRenderer in spriteRenderers){
            spriteRenderer.enabled = false;
        }

    }

    private void SROn(){
        foreach (SpriteRenderer spriteRenderer in spriteRenderers){
            spriteRenderer.enabled = true;
        }
    }

    private void MakeMovable(){
        GameManager.Instance.ActorsMovable = true;
    }
}
