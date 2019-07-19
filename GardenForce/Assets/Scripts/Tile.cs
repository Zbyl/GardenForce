using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Owners
{
    None,
    Player1,
    Player2
}

public class Tile : MonoBehaviour
{
    public Owners owner { get; private set; }
    public void SetOwner(Owners owner)
    {
        this.owner = owner;

        //PlaceHolder
        switch (owner)
        {
            case Owners.None:
                GetComponent<SpriteRenderer>().color =
                    new Color(1, 1, 1, 1);
                break;
            case Owners.Player1:
                GetComponent<SpriteRenderer>().color =
                    new Color(1, 0.5f, 0.5f, 1);
                break;
            case Owners.Player2:
                GetComponent<SpriteRenderer>().color =
                    new Color(0.5f, 0.5f, 1f, 1);
                break;
        }
    }
}
