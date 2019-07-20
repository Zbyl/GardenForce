using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextSetter : MonoBehaviour
{
    public int mode;
    Text text;
    int lastValue = int.MinValue;
    void Awake()
    {
        text = GetComponent<Text>();
    }

    void Update()
    {
        switch (mode)
        {
            case 0:
                if (lastValue != Map.instance.playerPoints[0])
                {
                    text.text = Map.instance.playerPoints[0].ToString();
                    lastValue = Map.instance.playerPoints[0];
                }
                break;
            case 1:
                if (lastValue != Map.instance.playerPoints[1])
                {
                    text.text = Map.instance.playerPoints[1].ToString();
                    lastValue = Map.instance.playerPoints[1];
                }
                break;
        }
    }
}
