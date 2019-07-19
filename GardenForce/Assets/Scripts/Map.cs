using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public static Map instance { get; private set; }

    public Transform cameraPosition;

    public GameObject dirtPrefab;

    public GameObject attackFlowerPrefab;
    public GameObject defenseFlowerPrefab;
    public GameObject colonizeFlowerPrefab;
    public GameObject growFlowerPrefab;

    public Flower[,] flowers { get; private set; }
    public GameObject[,] ground { get; private set; }

    public float tileSize { get; private set; }

    public readonly int verticalSize = 10;
    public readonly int horizontalSize = 10;
    public readonly float tileZ = 0;
    public readonly float flowerZ = -1;
    public readonly float cursorZ = -2;

    int currentTime = 0;

    public Map()
    {
        instance = this;
    }

    public void GenerateGround()
    {
        ground = new GameObject[verticalSize, horizontalSize];

        GameObject tileParent = new GameObject();
        tileParent.name = "TileParent";
        for (var i = 0; i < horizontalSize; i++)
        {
            for (var j = 0; j < verticalSize; j++)
            {
                Vector3 pos = mapPositionToWorldPosition(new Vector2Int(j, i), tileZ);
                ground[j, i] = Instantiate(
                    dirtPrefab, pos, Quaternion.identity
                );
                ground[j, i].transform.SetParent(tileParent.transform);
            }
        }
    }

    void Start()
    {
        // We need this initialized for position computations to work properly. So iniitialize it first.
        tileSize = dirtPrefab.GetComponent<SpriteRenderer>().bounds.size.x;

        var topLeft = mapPositionToWorldPosition(new Vector2Int(0, 0), tileZ);
        var bottomRight = mapPositionToWorldPosition(new Vector2Int(horizontalSize, verticalSize), tileZ);
        var mapCenter = (topLeft + bottomRight) / 2;
        mapCenter.z = cameraPosition.position.z;
        cameraPosition.position = mapCenter;

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

    /// Returns flower in given map position, or null.
    /// Position can be outside map (null is returned then).
    public Flower getFlower(Vector2Int position)
    {
        if (!isPositionInsideMap(position))
            return null;

        return flowers[position.x, position.y];
    }

    public Vector2Int getStartPosition(int owner)
    {
        if (owner == 2)
            return new Vector2Int(horizontalSize - 1, verticalSize - 1);
        return Vector2Int.zero;
    }

    public bool isMineFieldNearby(Vector2Int position, int owner)
    {
        if (position == getStartPosition(owner)) return true;
        
        if (getFlower(position + Vector2Int.right)?.owner == owner) return true;
        if (getFlower(position + Vector2Int.left)?.owner == owner) return true;
        if (getFlower(position + Vector2Int.up)?.owner == owner) return true;
        if (getFlower(position + Vector2Int.down)?.owner == owner) return true;
        if (getFlower(position + Vector2Int.right + Vector2Int.up)?.owner == owner) return true;
        if (getFlower(position + Vector2Int.left + Vector2Int.up)?.owner == owner) return true;
        if (getFlower(position + Vector2Int.right + Vector2Int.down)?.owner == owner) return true;
        if (getFlower(position + Vector2Int.left + Vector2Int.down)?.owner == owner) return true;
        return false;
    }

    public Flower plantFlower(Vector2Int position, GameObject flowerPrefab, int owner)
    {
        if (!isMineFieldNearby(position, owner))
            return null;

        return instantiateFlower(position, flowerPrefab, owner);
    }

    public Flower instantiateFlower(Vector2Int position, GameObject flowerPrefab, int owner)
    {
        var flower = getFlower(position);
        if (flower != null)
            return null;

        return instantiateFlowerRaw(position, flowerPrefab, owner);
    }

    Flower instantiateFlowerRaw(Vector2Int position, GameObject flowerPrefab, int owner)
    {
        if (!isPositionInsideMap(position))
        {
            return null;
        }

        var worldPosition = mapPositionToWorldPosition(position, flowerZ);
        var newFlowerObject = Instantiate(flowerPrefab, worldPosition, Quaternion.identity);
        newFlowerObject.transform.SetParent(transform);

        var previousFlower = flowers[position.x, position.y];

        var newFlower = newFlowerObject.GetComponent<Flower>();
        newFlower.owner = owner;
        newFlower.position = position;
        newFlower.creationTime = currentTime;
        newFlower.creationTimeInSeconds = Time.time;
        newFlower.init(previousFlower);
        flowers[position.x, position.y] = newFlower;

        if (previousFlower != null)
        {
            Destroy(previousFlower.gameObject);
        }

        return newFlower;
    }

    public bool isPositionInsideMap(Vector2Int position)
    {
        if (position.x < 0) return false;
        if (position.y < 0) return false;
        if (position.x >= horizontalSize) return false;
        if (position.y >= verticalSize) return false;
        return true;
    }

    public Vector3 mapPositionToWorldPosition(Vector2 position, float z)
    {
        return this.transform.position + Vector3.right * tileSize * position.x + Vector3.down * tileSize * position.y + Vector3.forward * z;
    }

    public Vector3 mapPositionToWorldPosition(Vector2Int position, float z)
    {
        return mapPositionToWorldPosition(new Vector2(position.x, position.y), z);
    }

    public Vector2 worldPositionToMapPosition(Vector3 position)
    {
        var dir = position - this.transform.position;
        return new Vector2(dir.x / tileSize, dir.y / tileSize);
    }

    public Vector2Int worldPositionToIntMapPosition(Vector3 position)
    {
        var pos = worldPositionToMapPosition(position);
        return new Vector2Int((int)pos.x, (int)pos.y);
    }
}
