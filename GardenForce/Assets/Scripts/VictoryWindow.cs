using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VictoryScreen : MonoBehaviour
{
    readonly Color32[] textColors = {
        new Color(62, 204, 217),
        new Color(217, 140, 62),
    };
    readonly string[] victoryTexts = {
        "PLAER ORANGE WON!",
        "PLAYER CYAN WON!"
    };
    public void SetWinner(int winner)
    {
        var text = GetComponentInChildren<Text>();
        text.color = textColors[winner];
        text.text = victoryTexts[winner];
    }
}
