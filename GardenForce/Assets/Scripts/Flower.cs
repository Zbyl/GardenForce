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

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Dupa");
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
