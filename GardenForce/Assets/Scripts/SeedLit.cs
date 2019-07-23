using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class SeedLit : MonoBehaviour
{
    public Sprite spriteLit;
    public Sprite spriteDim;
    public Color colorLit;
    public Color colorDim;

    public int minSeedForLit;
    public CursorHandler player;

    // Update is called once per frame
    void Update()
    {
        if (player.seeds >= minSeedForLit)
        {
            var image = this.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = spriteLit;
                image.color = colorLit;
            }
        }
        else
        {
            var image = this.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = spriteDim;
                image.color = colorDim;
            }
        }
    }
}
