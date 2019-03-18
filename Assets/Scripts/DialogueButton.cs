using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueButton : MonoBehaviour
{
    public bool isAvailable;
    public OptionType currentOption;
    public TextMeshProUGUI costText;

    public Color availableColor = Color.green;
    public Color unavailableColor = Color.red;

    public void ChangeOption(OptionType optionType)
    {
        currentOption = optionType;
        isAvailable = (GameManager.Instance.OptionCosts[currentOption] <= Math.Max(GameManager.Instance.Cash, 0));

        costText.text = Extentions.Reverse(GameManager.Instance.OptionCosts[currentOption].ToString());
        costText.color = isAvailable ? availableColor : unavailableColor;

        GetComponent<Image>().sprite = Resources.Load<Sprite>(String.Format("UI/{0}", currentOption.ToString()));
    }

    public void OptionClicked()
    {
        Debug.Log("Option Clicked");
        if (isAvailable){
            GameManager.Instance.ChooseOption(currentOption);
        }
    }
}
