using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public static Map instance { get; private set; }

    public Transform mapOrigin;

    public GameObject dirtPrefab;

    public GameObject attackFlowerPrefab;
    public GameObject defenseFlowerPrefab;
    public GameObject colonizeFlowerPrefab;
    public GameObject growFlowerPrefab;

    public Flower[,] flowers { get; private set; }
    public GameObject[,] ground { get; private set; }

    public float tileSize { get; private set; }

    readonly int verticalSize = 10;
    readonly int horizontalSize = 10;
    readonly float tileZ = 0;

    int currentTime = 0;
    float lastTime = 0;

    public Map()
    {
        instance = this;
    }

    public void GenerateGround()
    {
        ground = new GameObject[verticalSize, horizontalSize];

        tileSize = dirtPrefab.GetComponent<SpriteRenderer>().bounds.size.x;
        GameObject tileParent = new GameObject();
        tileParent.name = "TileParent";
        for (var i = 0; i < horizontalSize; i++)
        {
            for (var j = 0; j < verticalSize; j++)
            {
                Vector3 pos = mapPositionToWorldPosition(new Vector2Int(j, i));
                pos.z = tileZ;
                ground[j, i] = Instantiate(
                    dirtPrefab, pos, Quaternion.identity
                );
                ground[j, i].transform.SetParent(tileParent.transform);
            }
        }
    }

    void Start()
    {
        flowers = new Flower[verticalSize, horizontalSize];
        GenerateGround();
        StartCoroutine("Tick");
    }

    IEnumerator Tick()
    {
        for (; ; )
        {
            logicUpdate();
            yield return new WaitForSeconds(0.5f);
        }
    }

    void logicUpdate()
    {
        currentTime++;
        foreach (var flower in flowers)
        {
                flower?.GetComponent<Flower>().logicUpdate(currentTime);
        }
    }

    public bool isMineFieldNearby(Vector2Int position, int owner)
    {
        return true;
    }

    public Flower plantFlower(Vector2Int position, GameObject flowerPrefab, int owner)
    {
        if (!isMineFieldNearby(position, owner))
            return null;

        return instantiateFlower(position, flowerPrefab, owner);
    }

    public Flower instantiateFlower(Vector2Int position, GameObject flowerPrefab, int owner)
    {
        var flower = flowers[position.x, position.y];
        if (flower != null)
            return null;

        return instantiateFlowerRaw(position, flowerPrefab, owner);
    }

    Flower instantiateFlowerRaw(Vector2Int position, GameObject flowerPrefab, int owner)
    {
        var newFLowerObject = Instantiate(flowerPrefab, mapPositionToWorldPosition(position), Quaternion.identity);
        var previousFlower = flowers[position.x, position.y];

        var newFlower = newFLowerObject.GetComponent<Flower>();
        newFlower.owner = owner;
        newFlower.position = position;
        newFlower.creationTIme = currentTime;
        newFlower.init(previousFlower);
        flowers[position.x, position.y] = newFlower;

        if (previousFlower != null)
        {
            Destroy(previousFlower.gameObject);
        }

        return newFlower;
    }

    public Vector3 mapPositionToWorldPosition(Vector2 position)
    {
        return this.mapOrigin.position + Vector3.right * tileSize * position.x + Vector3.down * tileSize * position.y;
    }

    public Vector3 mapPositionToWorldPosition(Vector2Int position)
    {
        return mapPositionToWorldPosition(new Vector2(position.x, position.y));
    }

    public Vector2 worldPositionToMapPosition(Vector3 position)
    {
        var dir = position - this.mapOrigin.position;
        return new Vector2(dir.x / tileSize, dir.y / tileSize);
    }

    public Vector2Int worldPositionToIntMapPosition(Vector3 position)
    {
        var pos = worldPositionToMapPosition(position);
        return new Vector2Int((int)pos.x, (int)pos.y);
    }
}
