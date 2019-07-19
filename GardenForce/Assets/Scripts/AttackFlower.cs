﻿using UnityEngine;
using UnityEditor;

public class AttackFlower : Flower
{
    public bool canSpawn = true;

    readonly int[,] spawn_map = new int[,] {
        { 0, 4, 3, 4, 0 },
        { 4, 2, 1, 2, 4 },
        { 3, 1, 0, 1, 3 },
        { 4, 2, 1, 2, 4 },
        { 0, 4, 3, 4, 0 },
    };

    const int life_ticks = 5;


    int last_stage = 0;
    public override void logicUpdate(int currentTime)
    {
        int stage = (currentTime - creationTime) / 2;

        if (canSpawn && stage != last_stage)
        {
            for (int i = 0; i < spawn_map.GetLength(0); i++)
            {
                for (int j = 0; j < spawn_map.GetLength(1); j++)
                {
                    if (spawn_map[i,j] == stage)
                        TryToSpawn(new Vector2Int(i - spawn_map.GetLength(0) / 2,
                            j - spawn_map.GetLength(1) / 2));
                }
            }
        }

        last_stage = stage;
        if (stage == life_ticks)
        {
            Map.instance.removeFlower(this.position);
        }
    }

    void TryToSpawn(Vector2Int offset)
    {
        var position = this.position + offset;
        var childFlower = Map.instance.instantiateFlower(position, Map.instance.attackFlowerPrefab, owner, true) as AttackFlower;
        if (childFlower != null)
        {
            childFlower.canSpawn = false;
            childFlower.setSourcePosition(this.position);
        }
    }
}
