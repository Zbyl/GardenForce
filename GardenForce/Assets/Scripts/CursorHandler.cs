﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorHandler : MonoBehaviour
{
    public int playerNumber;
    public Vector2Int mapPosition;

    public float inputDelay;                ///< Delay between presses of one button.
    public ParticleSystem particleBurst;    ///< Played when plant is being planted.

    private Dictionary<string, float> lastPressed = new Dictionary<string, float>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(Input.mousePosition);
        //Debug.Log("X=" + Input.GetAxisRaw("AxisX" + playerNumber) + " Y=" + Input.GetAxisRaw("AxisY" + playerNumber));

        if (getButton("Left"))
        {
            this.mapPosition.x -= 1;
        }

        if (getButton("Right"))
        {
            this.mapPosition.x += 1;
        }

        if (getButton("Up"))
        {
            this.mapPosition.y -= 1;
        }

        if (getButton("Down"))
        {
            this.mapPosition.y += 1;
        }

        if (getButton("PlantA"))
        {
            Debug.Log("Planting A" + playerNumber);
            particleBurst.Play();

            Map.instance.plantFlower(mapPosition, Map.instance.attackFlowerPrefab, playerNumber);
        }

        if (getButton("PlantB"))
        {
            Debug.Log("Planting B" + playerNumber);
            particleBurst.Play();
        }

        if (getButton("PlantC"))
        {
            Debug.Log("Planting C" + playerNumber);
            particleBurst.Play();
        }

        if (getButton("PlantD"))
        {
            Debug.Log("Planting D" + playerNumber);
            particleBurst.Play();
        }

        this.transform.position = Map.instance.mapPositionToWorldPosition(this.mapPosition, Map.instance.cursorZ);
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