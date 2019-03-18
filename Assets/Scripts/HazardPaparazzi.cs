using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardPaparazzi : MonoBehaviour
{
    public bool bottomLane;
    public BoxCollider2D jumpZone;
    public BoxCollider2D photoZone;
    private Animator animator;

    // Start is called before the first frame update
    void Start(){
        animator = GetComponent<Animator>();
        photoZone.enabled = false;
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && jumpZone.enabled && InRelevantLane()) //1st collision
        {
            jumpZone.enabled = false;
            photoZone.enabled = true;
            Jump();
        }
        else if (collision.gameObject.CompareTag("Player") && photoZone.enabled && InRelevantLane()) //2nd collision
        {
            photoZone.enabled = false;
            Shoot();
        }
    }

    private bool InRelevantLane()
    {
        return bottomLane ? (GameManager.Instance.Player.currentLane == 1) : (GameManager.Instance.Player.currentLane == 3);
    }

    private void Jump(){
        if (bottomLane)
            animator.SetTrigger("JumpUp");
        else
            animator.SetTrigger("JumpDown");
    }

    private void Shoot()
    {
        GameManager.Instance.Flash();
    }
}
