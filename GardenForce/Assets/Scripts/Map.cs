using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public GameObject tilePref;
    public Tile[,] tiles { get; private set; }
    public static Map instance { get; private set; }
    public float tileSize { get; private set; }
    GameObject[,] tileObjects;
    readonly int verticalSize = 10;
    readonly int horizontalSize = 10;
    readonly float tileZ = 0;
    public Map()
    {
        instance = this;
    }
    public void Generate()
    {
        tileObjects = new GameObject[verticalSize, horizontalSize];
        tiles = new Tile[verticalSize, horizontalSize];
        tileSize = tilePref.GetComponent<SpriteRenderer>().bounds.size.x;
        Vector2 startPoint = new Vector2(
            -(tileSize * verticalSize) / 2,
            (tileSize * horizontalSize) / 2
        );
        GameObject tileParent = new GameObject();
        tileParent.name = "TileParent";
        for (var i = 0; i < horizontalSize; i++)
        {
            for (var j = 0; j < verticalSize; j++)
            {
                Vector3 pos = new Vector3(
                    startPoint.x + j * tileSize,
                    startPoint.y - i * tileSize,
                    tileZ
                );
                tileObjects[j, i] = Instantiate(
                    tilePref, pos, Quaternion.identity
                );
                tileObjects[j, i].transform.SetParent(tileParent.transform);
                tiles[j,i] = tileObjects[j, i].GetComponent<Tile>();
            }
        }
    }

    void Start()
    {
        Generate();
    }
}
