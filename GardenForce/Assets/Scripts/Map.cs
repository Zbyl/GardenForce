using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Map : MonoBehaviour
{
    public static Map instance { get; private set; }

    public Camera camera;

    public GameObject playerOneGameObject;
    public GameObject playerTwoGameObject;
    CursorHandler playerOne;
    CursorHandler playerTwo;

    public GameObject[] dirtPrefabs;
    public GameObject[] rockPrefabs;
    public float rockProbability;   /// Probability rock will be chosen during map generation.

    public GameObject attackFlowerPrefab;
    public GameObject defenseFlowerPrefab;
    public GameObject colonizeFlowerPrefab;
    public GameObject growFlowerPrefab;
    public GameObject idleFlowerPrefab; /// Flower that doesn't do anything. It just sits on the map.
    public GameObject parasitePrefab;   /// Parasite. It is not a flower.

    public GameObject fadeOutPrefab;

    public GameObject victoryWindow;

    public Flower[,] flowers { get; private set; }
    public int[,] ground { get; private set; }  // 0 - dirt, 1 - rock
    internal List<Parasite> parasites = new List<Parasite>();
    public int ticksPassed { get; private set; }

    public readonly int width = 40;
    public readonly int height = 20;
    public float tickLimit;
    public float tileSize { get; private set; }
    public float tileZ;
    public float flowerZ;
    public float parasiteZ;
    public float cursorZ;

    readonly Color[] playerColor = {
        new Color(128, 0, 0),
        new Color(0, 0, 250)
    };

    public readonly int[] playerPoints = new int[2];


    void Awake()
    {
        instance = this;
    }

    static GameObject randomPrefab(GameObject[] gameObjects)
    {
        if (gameObjects.Length == 0)
            return null;

        var index = Random.Range(0, gameObjects.Length);
        return gameObjects[index];
    }

    void instantiateTile(Vector2Int position, GameObject prefab, GameObject tileParent)
    {
        Vector3 pos = mapPositionToWorldPosition(
            position, prefab.transform.position.z
        );
        var tile = Instantiate(prefab, pos, Quaternion.identity);
        tile.transform.SetParent(tileParent.transform);
    }

    public void GenerateGround()
    {
        ground = new int[width, height];

        GameObject tileParent = new GameObject();
        tileParent.name = "TileParent";
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                var placeRock = Random.value < rockProbability;
                ground[i, j] = placeRock ? 1 : 0;

                var mapPosition = new Vector2Int(i, j);
                instantiateTile(mapPosition, randomPrefab(dirtPrefabs), tileParent);
                if (placeRock)
                    instantiateTile(mapPosition, randomPrefab(rockPrefabs), tileParent);
            }
        }
    }
    void Start()
    {
        // We need this initialized for position computations to work properly. So iniitialize it first.
        tileSize = dirtPrefabs[0].GetComponent<SpriteRenderer>().bounds.size.x;

        var topLeft = mapPositionToWorldPosition(new Vector2Int(0, 0), tileZ);
        var bottomRight = mapPositionToWorldPosition(new Vector2Int(width, height), tileZ);
        var mapCenter = (topLeft + bottomRight) / 2;
        mapCenter.z = camera.transform.position.z;
        camera.transform.position = mapCenter;

        float aspectScreen = Screen.width / Screen.height;
        float aspectMap = width / height;

        camera.orthographicSize = width / 5.0f;

        flowers = new Flower[width, height];
        GenerateGround();

        playerOne = playerOneGameObject.GetComponent<CursorHandler>();
        playerTwo = playerTwoGameObject.GetComponent<CursorHandler>();

        StartCoroutine("Tick");
    }

    IEnumerator Tick()
    {
        for (; ; )
        {
            logicUpdate();
            yield return new WaitForSeconds(0.1f);
        }
    }

    void logicUpdate()
    {
        ticksPassed++;

        playerPoints[0] = 0;
        playerPoints[1] = 0;
        playerOne.logicUpdate(ticksPassed);
        playerTwo.logicUpdate(ticksPassed);

        foreach (var flower in flowers)
        {
            if (flower == null)
                continue;

            flower.GetComponent<Flower>().logicUpdate(ticksPassed);
            playerPoints[flower.owner - 1]++;
        }

        var parasitesCopy = new List<Parasite>(parasites);  // Parasites delete themselves, so protect against this.
        foreach (var parasite in parasitesCopy)
        {
            parasite.logicUpdate(ticksPassed);
        }
        if(ticksPassed >= tickLimit)
        {
            StopCoroutine("Tick");
            StartCoroutine(FinishGame());
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

    /// Returns flower in given map position, or null.
    /// Position must be inside the map.
    public Flower getFlowerNoBoundsCheck(Vector2Int position)
    {
        return flowers[position.x, position.y];
    }

    public Vector2Int getStartPosition(int owner)
    {
        if (owner == 2)
            return new Vector2Int(width - 1, height - 1);
        return Vector2Int.zero;
    }

    public bool isMineFieldHere(Vector2Int position, int owner)
    {
        if (position == getStartPosition(owner)) return true;
        if (getFlower(position)?.owner == owner) return true;
        return false;
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

    public void removeFlower(Vector2Int position)
    {
        var flower = getFlower(position);
        if (flower == null)
            return;

        flowers[position.x, position.y] = null;

        if (flower.destroyFadeOutSeconds > 0)
        {
            var fadeOut = Instantiate(fadeOutPrefab, flower.transform.position, flower.transform.rotation);
            var model = flower.transform.Find("Model");
            model.SetParent(fadeOut.transform);
            fadeOut.GetComponent<FadeOut>().fadeOutSeconds = flower.destroyFadeOutSeconds;
        }

        Destroy(flower.gameObject);
    }

    public Flower instantiateFlower(Vector2Int position, GameObject flowerPrefab, int owner, bool force = false)
    {
        var flower = getFlower(position);
        if ((!force && flower != null) || (flower?.type == Flower.FlowerType.protect))
            return null;

        return instantiateFlowerRaw(position, flowerPrefab, owner);
    }

    Flower instantiateFlowerRaw(Vector2Int position, GameObject flowerPrefab, int owner)
    {
        if (!isPositionInsideMap(position))
        {
            return null;
        }

        // We cannot instantiate any flower on a rock.
        if (ground[position.x, position.y] != 0)
        {
            return null;
        }

        var worldPosition = mapPositionToWorldPosition(position, flowerZ);
        var newFlowerObject = Instantiate(flowerPrefab, worldPosition, Quaternion.identity);
        newFlowerObject.transform.SetParent(transform);

        var previousFlower = getFlower(position);

        var newFlower = newFlowerObject.GetComponent<Flower>();
        newFlower.owner = owner;
        newFlower.position = position;
        newFlower.sourcePosition2d = position;
        newFlower.sourcePosition3d = worldPosition;
        newFlower.creationTime = ticksPassed;
        newFlower.creationTimeInSeconds = Time.time;
        newFlower.init(previousFlower);
        flowers[position.x, position.y] = newFlower;

        Transform newFlowerColor = newFlowerObject.transform.GetChild(0).GetChild(1);
        var colorComponent = newFlowerColor.GetComponent<SpriteRenderer>();

        colorComponent.color = playerColor[owner - 1];

        if (previousFlower != null)
        {
            Destroy(previousFlower.gameObject);
        }

        return newFlower;
    }

    public Parasite createParasite(Vector2Int position, int owner)
    {
        if (!isMineFieldHere(position, owner) && !isMineFieldNearby(position, owner))
            return null;

        if (!isPositionInsideMap(position))
            return null;

        var worldPosition = mapPositionToWorldPosition(position, flowerZ);
        var newParasiteObject = Instantiate(parasitePrefab, worldPosition, Quaternion.identity);
        newParasiteObject.transform.SetParent(transform);

        var parasite = newParasiteObject.GetComponent<Parasite>();
        parasite.owner = owner;
        parasite.startPosition = position;
        parasite.creationTime = ticksPassed;
        parasite.creationTimeInSeconds = Time.time;
        parasites.Add(parasite);

        return parasite;
    }

    public bool isPositionInsideMap(Vector2Int position)
    {
        if (position.x < 0) return false;
        if (position.y < 0) return false;
        if (position.x >= width) return false;
        if (position.y >= height) return false;
        return true;
    }

    public Vector3 mapPositionToWorldPosition(Vector2 position, float z, bool useCenter = false)
    {
        var delta = useCenter ? 0.5f : 0.0f;
        return this.transform.position + Vector3.right * tileSize * (position.x + delta) + Vector3.down * tileSize * (position.y + delta) + Vector3.forward * z;
    }

    public Vector3 mapPositionToWorldPosition(Vector2Int position, float z, bool useCenter = false)
    {
        return mapPositionToWorldPosition(new Vector2(position.x, position.y), z, useCenter);
    }

    public Vector2 worldPositionToMapPosition(Vector3 position, bool useCenter = false)
    {
        var dir = position - this.transform.position;
        var delta = useCenter ? 0.5f : 0.0f;
        return new Vector2(dir.x / tileSize - delta, -dir.y / tileSize - delta);
    }

    public Vector2Int worldPositionToIntMapPosition(Vector3 position, bool useCenter = false)
    {
        var pos = worldPositionToMapPosition(position, useCenter);
        return new Vector2Int((int)pos.x, (int)pos.y);
    }

    public float mapUnitsToWorldUnits(float mapUnits)
    {
        return mapUnits * tileSize;
    }

    public bool playRandomSound(AudioClip[] sounds)
    {
        if (sounds.Length == 0)
            return false;

        var soundsIdx = Random.Range(0, sounds.Length);
        var sound = sounds[soundsIdx];
        if (sound == null)
            return false;

        AudioSource.PlayClipAtPoint(sound, camera.transform.position);
        return true;
    }
    IEnumerator FinishGame()
    {
        Time.timeScale = 0;

        int winner = 0;
        if (playerPoints[0] > playerPoints[1])
            winner = 1;
        else if (playerPoints[0] < playerPoints[1])
            winner = 2;
        victoryWindow.GetComponent<VictoryWindow>().SetWinner(winner);
        yield return new WaitForSecondsRealtime(2);
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
