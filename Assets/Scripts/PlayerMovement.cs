using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public float horizontalVelocity = 50f;
    public float landHeight = 0.5f;
    public int numberOfLanes = 2;
    public int currentLane = 2;

    private Rigidbody2D rb;
    public SwipeController swipeController;
    private SpriteRenderer sr;
    private PlayerMusic playerMusic;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        playerMusic = GetComponent<PlayerMusic>();
        playerMusic.PlayLevelMusic();
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

        if (collision.CompareTag("LevelEndZone"))
        {
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

            if (GameManager.Instance.HitByHazard(collision.gameObject.GetComponent<Hazard>().Cost)){
                StartFlickering();
                Destroy(collision.gameObject);
            }
            else
                sr.enabled = false;

            //TODO: play relevant sound
            //TODO: music stop
            //TODO: play relevant animation
        }
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
        sr.enabled = false;
    }

    private void SROn(){
        sr.enabled = true;
    }

    private void MakeMovable()
    {
        GameManager.Instance.ActorsMovable = true;
    }


}
