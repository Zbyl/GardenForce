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
    public Sprite[] spriteSeedImages;

    public GameObject normalCursor;
    public GameObject forbiddenCursor;    ///< Cursor used when player cannot build on given field.
    public const int maxSeeds = 5;
    int seeds = 0;
    int lastTimeSeedReceive = 0;

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

        if (getButton("PlantA"))
        {
            plantFlower(map.attackFlowerPrefab);
        }

        if (getButton("PlantB"))
        {
            plantFlower(map.defenseFlowerPrefab);
        }

        if (getButton("PlantC"))
        {
            plantFlower(map.colonizeFlowerPrefab);
        }

        if (getButton("PlantD"))
        {
            plantFlower(map.growFlowerPrefab);
        }

        this.transform.position = map.mapPositionToWorldPosition(this.mapPosition, map.cursorZ);

        if (map.isMineFieldNearby(mapPosition, playerNumber))
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
            if (currentTime - lastTimeSeedReceive > 20)
            {
                setSeedsNumber(seeds + 1);
                lastTimeSeedReceive = currentTime;
            }
        }
        else
        {
            lastTimeSeedReceive = currentTime;
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
            Map.playRandomSound(flower.createSounds, new Vector3(0,0,-10));
            setSeedsNumber(seeds - 1);
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
