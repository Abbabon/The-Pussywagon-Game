using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{

    public float horizontalVelocity = 50f;
    public float landHeight = 0.5f;
    public int numberOfLanes = 2;
    public int currentLane = 2;

    private Rigidbody2D rb;
    public Animator animator;
    public SwipeController swipeController;
    public SpriteRenderer[] spriteRenderers;

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
        SoundManager.Instance.PlaySoundEffect(SoundManager.SoundEffect.carStart);

        switch (SceneManager.GetActiveScene().buildIndex)
        {
            case 2: // level 1
                SoundManager.Instance.PlaySpecificDialogue(SoundManager.DialogueCategories.BatutaLevelStart, 0);
                break; 
            case 3: // level 2
                SoundManager.Instance.PlaySpecificDialogue(SoundManager.DialogueCategories.BatutaLevelStart, 1);
                break;
            case 4: // level 3
                SoundManager.Instance.PlaySpecificDialogue(SoundManager.DialogueCategories.BatutaLevelStart, 2);
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //TODO: remove this when starting level
        if (GameManager.Instance.ActorsMovable)
        {
            rb.velocity = new Vector2((horizontalVelocity * GameManager.Instance.SpeedFactor * Time.deltaTime), rb.velocity.y);

            if ((swipeController.SwipeUp || Input.GetKeyDown(KeyCode.UpArrow)) && (currentLane < numberOfLanes))
            {
                transform.position = new Vector2(transform.position.x, transform.position.y + landHeight);
                currentLane++;
            }
            else if ((swipeController.SwipeDown || Input.GetKeyDown(KeyCode.DownArrow)) && (currentLane > 1))
            {
                transform.position = new Vector2(transform.position.x, transform.position.y - landHeight);
                currentLane--;
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

        if (collision.CompareTag("LevelEndZone")){
            GameManager.Instance.LevelEnded();

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Babe babe = collision.gameObject.GetComponent<Babe>();
        if (babe != null)
            babe.DisableInteration();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Babe babe = collision.gameObject.GetComponent<Babe>();
        if (babe != null && babe.Lane == currentLane)
            babe.EnableInteration();
        else if (babe != null)
            babe.DisableInteration();

        Hazard hazard = collision.gameObject.GetComponent<Hazard>();
        if (hazard != null)
        {
            if (hazard.Lane != currentLane)
                return;

            rb.velocity = Vector2.zero;
            GameManager.Instance.ActorsMovable = false;

            if (GameManager.Instance.HitByHazard(hazard)){
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
                SROff();
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
