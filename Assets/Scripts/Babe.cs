using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Babe : MonoBehaviour
{
    private bool isInteractable = false;
    private bool interacted = false;
    public bool Interacted { get => interacted; set => interacted = value; }
    public int Lane = 1;
    public BabeType babeType;
    public int hotness = 5;

    [SerializeField]
    private SpriteRenderer outglowSR;

    private void Start()
    {
        GameManager.Instance.TotalBabesInStage += 1;
    }

    public void EnableInteration()
    {
        if (!Interacted)
            isInteractable = outglowSR.enabled = true;
    }

    public void DisableInteration()
    {
        isInteractable = outglowSR.enabled = false;
    }

    public void MarkInteracted()
    {
        Interacted = true;
        outglowSR.enabled = false;
    }

    private void OnMouseDown(){
        Interact();
    }

    public void Interact()
    {
        if (isInteractable && !Interacted)
            GameManager.Instance.StartDialogue(this);
    }
}
