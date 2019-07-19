using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public Transform mapOrigin;
    public float tileSize;

    public int playerNumber;
    public Vector2Int mapPosition;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(Input.mousePosition);
        Debug.Log(Input.GetAxisRaw("DPadX1"));

        if (Input.GetButton("Left" + playerNumber) || getDPadButton("Left", playerNumber))
        {
            this.mapPosition.x -= 1;
        }

        if (Input.GetButton("Right" + playerNumber) || getDPadButton("Right", playerNumber))
        {
            this.mapPosition.x += 1;
        }

        if (Input.GetButton("Up" + playerNumber) || getDPadButton("Up", playerNumber))
        {
            this.mapPosition.y -= 1;
        }

        if (Input.GetButton("Down" + playerNumber) || getDPadButton("Down", playerNumber))
        {
            this.mapPosition.y += 1;
        }

        this.transform.position = mapPositionToWorldPosition(this.mapPosition);
    }

    static bool getDPadButton(string button, int playerNumber)
    {
        if (button == "Right") return Input.GetAxisRaw("DPadX" + playerNumber) == 1;
        if (button == "Down") return Input.GetAxisRaw("DPadY" + playerNumber) == -1;
        if (button == "Left") return Input.GetAxisRaw("DPadX" + playerNumber) == -1;
        if (button == "Up") return Input.GetAxisRaw("DPadY" + playerNumber) == 1;
        return false;
    }

    Vector3 mapPositionToWorldPosition(Vector2 position)
    {
        return this.mapOrigin.position + Vector3.right * tileSize * position.x + Vector3.down * tileSize * position.y;
    }

    Vector3 mapPositionToWorldPosition(Vector2Int position)
    {
        return mapPositionToWorldPosition(new Vector2(position.x, position.y));
    }

    Vector2 worldPositionToMapPosition(Vector3 position)
    {
        var dir = position - this.mapOrigin.position;
        return new Vector2(dir.x / tileSize, dir.y / tileSize);
    }

    Vector2Int worldPositionToIntMapPosition(Vector3 position)
    {
        var pos = worldPositionToMapPosition(position);
        return new Vector2Int((int)pos.x, (int)pos.y);
    }
}
