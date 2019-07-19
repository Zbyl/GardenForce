using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flower : MonoBehaviour
{
    public enum FlowerType
    {
        not_set,
        grow,
        colonize,
        protect,
        attack,
    };

    public FlowerType type;

    public int owner; // 1 lub 2
    public Vector2Int position;
    public int creationTIme;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Flower!");
    }

    public void init(Flower previousFlower)
    {
    }

    public virtual void logicUpdate(int currentTime)
    {
    }
}
