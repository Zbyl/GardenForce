﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorHandler : MonoBehaviour
{
    public int playerNumber;
    public Vector2Int mapPosition;

    public float inputDelay;                ///< Delay between presses of one button.
    public ParticleSystem particleBurst;    ///< Played when plant is being planted.
    public ParticleSystem particleGetSeed;
    public SpriteRenderer spriteSeedCounter;
    public GameObject parasiteIndicator;    /// Indicates that a parasite is ready.
    public Sprite[] spriteSeedImages;

    public GameObject normalCursor;
    public GameObject forbiddenCursor;    ///< Cursor used when player cannot build on given field.
    public const int maxSeeds = 5;
    public int seeds { get; private set; } = 2;
    public int ticksForSeed = 20;

    int lastTimeSeedReceive = 0;

    public int parasiteSpawnDelay;      /// Delay between successive parasite spawns (in ticks).
    public int kidsParasiteSpawnDelay;  /// Delay between successive parasite spawns for kids mode (in ticks).
    bool canSpawnParasite = false;
    int lastTimeParasiteReleased;  /// Last time a parasite was spawned.
    bool parasiteReleased = false;

    private Map map { get { return Map.instance; } }

    private Dictionary<string, float> lastPressed = new Dictionary<string, float>();

    // Start is called before the first frame update
    void Start()
    {
        mapPosition = map.getStartPosition(playerNumber);
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(Input.mousePosition);
        //Debug.Log("X=" + Input.GetAxisRaw("AxisX" + playerNumber) + " Y=" + Input.GetAxisRaw("AxisY" + playerNumber));

        if (Time.timeScale <= 0)
            return;

        if (getButton("Left"))
        {
            moveCursor(-1, 0);
        }

        if (getButton("Right"))
        {
            moveCursor(1, 0);
        }

        if (getButton("Up"))
        {
            moveCursor(0, -1);
        }

        if (getButton("Down"))
        {
            moveCursor(0, 1);
        }

        if (getButton("Parasite") && canSpawnParasite)
        {
            var parasite = map.createParasite(mapPosition, playerNumber);
            if (parasite != null)
            {
                map.playRandomSound(parasite.createSounds);
                canSpawnParasite = false;
                parasiteReleased = true;
            }
        }
        parasiteIndicator.SetActive(canSpawnParasite);


        if (getButton("PlantA"))
        {
            plantFlower(map.growFlowerPrefab);
        }

        if (getButton("PlantB"))
        {
            plantFlower(map.colonizeFlowerPrefab);
        }

        if (getButton("PlantC"))
        {
            plantFlower(map.attackFlowerPrefab);
        }

        if (getButton("PlantD"))
        {
            plantFlower(map.defenseFlowerPrefab);
        }

        this.transform.position = map.mapPositionToWorldPosition(this.mapPosition, map.cursorZ);

        if (Map.kidsMode || map.isMineFieldNearby(mapPosition, playerNumber))
        {
            normalCursor.SetActive(true);
            forbiddenCursor.SetActive(false);
        }
        else
        {
            normalCursor.SetActive(false);
            forbiddenCursor.SetActive(true);
        }
    }

    public void logicUpdate(int currentTime)
    {
        if (seeds < maxSeeds)
        {
            if (currentTime - lastTimeSeedReceive > ticksForSeed)
            {
                setSeedsNumber(seeds + 1);
                lastTimeSeedReceive = currentTime;
            }
        }
        else
        {
            lastTimeSeedReceive = currentTime;
        }

        if (currentTime - lastTimeParasiteReleased > (Map.kidsMode ? kidsParasiteSpawnDelay : parasiteSpawnDelay))
        {
            canSpawnParasite = true;
            lastTimeParasiteReleased = currentTime;
        }

        if (parasiteReleased)
        {
            canSpawnParasite = parasiteReleased = false;
            lastTimeParasiteReleased = currentTime;
        }
    }

    void plantFlower(GameObject flowerPrefab)
    {
        if (seeds <= 0)
            return;

        var flower = map.plantFlower(mapPosition, flowerPrefab, playerNumber);
        if (flower != null)
        {
            //Debug.Log("Planted " + flowerPrefab.name + " for player " + playerNumber);
            particleBurst.Play();
            map.playRandomSound(flower.createSounds);
            if (!Map.kidsMode)
            {
                setSeedsNumber(seeds - 1);
            }
        }
    }

    void setSeedsNumber(int set_seeds)
    {
        if (set_seeds > seeds)
        {
            particleGetSeed.Play();
        }

        if (set_seeds <= 0)
        {
            spriteSeedCounter.sprite = null;
        }
        else
        {
            spriteSeedCounter.sprite = spriteSeedImages[set_seeds - 1];
        }

        seeds = set_seeds;
    }

    bool moveCursor(int deltaX, int deltaY)
    {
        var newPosition = mapPosition + new Vector2Int(deltaX, deltaY);
        if (!map.isPositionInsideMap(newPosition))
            return false;
        mapPosition = newPosition;
        return true;
    }

    /// Returns if button was pressed. Handles input delay.
    bool getButton(string button)
    {
        var buttonName = button + playerNumber;
        if (!this.lastPressed.ContainsKey(buttonName))
        {
            this.lastPressed.Add(buttonName, 0);
        }

        var lastTime = this.lastPressed[buttonName];
        if (Time.time < lastTime + this.inputDelay)
            return false;

        if (!getButtonRaw(button, playerNumber))
            return false;

        this.lastPressed[buttonName] = Time.time;
        return true;
    }

    /// Returns if button was pressed.
    static bool getButtonRaw(string button, int playerNumber)
    {
        var buttonName = button + playerNumber;
        if (Input.GetButton(buttonName))
            return true;

        if ((button == "Left") && (Input.GetAxisRaw("DPadX" + playerNumber) == -1)) return true;
        if ((button == "Right") && (Input.GetAxisRaw("DPadX" + playerNumber) == 1)) return true;
        if ((button == "Up") && (Input.GetAxisRaw("DPadY" + playerNumber) == 1)) return true;
        if ((button == "Down") && (Input.GetAxisRaw("DPadY" + playerNumber) == -1)) return true;

        if ((button == "Left") && (Input.GetAxisRaw("AxisX" + playerNumber) < -0.9)) return true;
        if ((button == "Right") && (Input.GetAxisRaw("AxisX" + playerNumber) > 0.9)) return true;
        if ((button == "Up") && (Input.GetAxisRaw("AxisY" + playerNumber) < -0.9)) return true;
        if ((button == "Down") && (Input.GetAxisRaw("AxisY" + playerNumber) > 0.9)) return true;

        return false;
    }
}
