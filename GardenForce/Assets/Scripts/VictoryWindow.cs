using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VictoryWindow : MonoBehaviour
{
    readonly Color32[] textColors = {
        new Color32(224, 214, 184, 255),
        new Color32(217, 140, 62, 255),
        new Color32(62, 204, 217, 255),
    };
    readonly string[] victoryTexts = {
        "TIE!",
        "PLAYER RED WON!",
        "PLAYER BLUE WON!"
    };
    public void SetWinner(int winner)
    {
        var text = GetComponentInChildren<Text>();
        text.color = textColors[winner];
        text.text = victoryTexts[winner];
        gameObject.SetActive(true);
    }
}
