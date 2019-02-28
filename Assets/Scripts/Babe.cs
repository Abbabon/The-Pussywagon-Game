using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Babe : MonoBehaviour
{
    private bool isInteractable = false;
    private bool interacted = false;
    public int Lane = 1;
    public BabeType babeType;
    public OptionType preferredOption;
    public int hotness = 5;

    [SerializeField]
    private SpriteRenderer outglowSR;

    public void EnableInteration()
    {
        if (!interacted)
            isInteractable = outglowSR.enabled = true;
    }

    public void DisableInteration()
    {
        isInteractable = outglowSR.enabled = false;
    }

    public void MarkInteracted()
    {
        this.interacted = true;
        outglowSR.enabled = false;
    }

    private void OnMouseDown()
    {
        if (isInteractable && !interacted)
            GameManager.Instance.StartDialogue(this);
    }
}
