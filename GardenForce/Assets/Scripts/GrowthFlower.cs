using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GrowthFlower : Flower
{
    public bool canSpawn = true;
    GrowthFlower parentFlower;  /// Flower that spawned us.

    public int growthStart;         /// First sprout is generated in this tick.
    public int growthDelay;         /// Delay between generating growing stages.
    public int growthStages;        /// How many growth stages to do.
    public float growthRadius;      /// Maximal growth radius.
    public float growthRadiusPerStage;  /// Growth radius per stage.


    public override void logicUpdate(int currentTime)
    {
        if (!canSpawn)
        {
            /// Logic for a sprout.
            return;
        }

        int stage = (currentTime - creationTime) - growthStart;
        if (stage % growthDelay != 0)
            return;
        stage /= growthDelay;

        if (stage < 0)
            return;
        if (stage >= growthStages)
            return;

        var stageDist = (stage + 1) * growthRadiusPerStage;
        if (stageDist > growthRadius)
            return;
        spawnGrowthFlowers(stageDist);
    }

    private List<Tuple<Vector2Int, Vector2Int, Vector3>> sprouts = new List<Tuple<Vector2Int, Vector2Int, Vector3>>();  /// Sprouts generated in this logic update.
    private bool[,] reachableMap;   /// Map of growth flowers that are reachable from the source.

    /// Gets previous cells towards source.
    private static Tuple<Vector2Int, Vector2Int, Vector3> getPreviousCells(Vector2Int cell, Vector2Int source)
    {
        var diff = cell - source;
        var absDX = Mathf.Abs(diff.x);
        var absDY = Mathf.Abs(diff.y);
        var epsilon = 0.01f;
        if (absDX > absDY)
        {
            // "horizontal" line
            var dY = (float)diff.y / absDX * (absDX - 1);
            var dX = (diff.x >= 0) ? -1 : 1;
            return Tuple.Create(
                new Vector2Int(cell.x + dX, source.y + Mathf.FloorToInt(dY + epsilon)),
                new Vector2Int(cell.x + dX, source.y + Mathf.CeilToInt(dY - epsilon)),
                Map.instance.mapPositionToWorldPosition(new Vector2(cell.x + dX, source.y + dY), Map.instance.flowerZ)
                );
        }
        else
        {
            // "vertical" line
            var dX = (float)diff.x / absDY * (absDY - 1);
            var dY = (diff.y >= 0) ? -1 : 1;
            return Tuple.Create(
                new Vector2Int(source.x + Mathf.FloorToInt(dX + epsilon), cell.y + dY),
                new Vector2Int(source.x + Mathf.CeilToInt(dX - epsilon), cell.y + dY),
                Map.instance.mapPositionToWorldPosition(new Vector2(source.x + dX, cell.y + dY), Map.instance.flowerZ)
                );
        }
    }

    /// Returns true if both cellPaths are reachable.
    /// Note: Both cells must be inside the map.
    private bool canReach(Vector2Int cell0, Vector2Int cell1)
    {
        if (!reachableMap[cell0.x, cell0.y]) return false;
        if (!reachableMap[cell1.x, cell1.y]) return false;

        return true;
    }

    /// Marks growth flowers that are reachable from the source.
    /// Only those can be later source for sprouts.
    /// Returns true if given cell was marked as reachable.
    /// Note: cell must be inside the map.
    private bool markReachable(Vector2Int cell)
    {
        var previousCells = getPreviousCells(cell, position);

        // Mark cell reachable only if previous cell is reachable and this cell contains a growth flower.
        if (!canReach(previousCells.Item1, previousCells.Item2))
        {
            return false;
        }

        var cellFlower = map.getFlowerNoBoundsCheck(cell);
        if (cellFlower == null)
        {
            return false;
        }

        if ((cellFlower.type == FlowerType.grow) && (cellFlower.owner == owner))
        {
            var cellFlowerIsDescendant = (cellFlower == this) || ((cellFlower as GrowthFlower).parentFlower == this);
            if (!cellFlowerIsDescendant)
                return false;
            reachableMap[cell.x, cell.y] = true;
            return true;
        }

        return false;
    }

    /// Tries to spawn growth flower in given cell.
    /// Note: cell must be inside the map.
    private void spawnInCell(Vector2Int cell)
    {
        var cellFlower = map.getFlowerNoBoundsCheck(cell);
        if (cellFlower != null)
        {
            return;
        }

        var previousCells = getPreviousCells(cell, position);

        // Can spawn only if previous cell is reachable.
        if (canReach(previousCells.Item1, previousCells.Item2))
        {
            // Spawn new growth flower here.
            sprouts.Add(Tuple.Create(cell, previousCells.Item1, previousCells.Item3));
        }
    }

    /// Tries to spawn a flower here.
    /// Also tries to mark cell as reachable.
    /// Returns true if cell was marked reachable.
    /// Note: cell doesn't have to be inside the map.
    private bool visitCell(Vector2Int cell, float stageDist)
    {
        if (!map.isPositionInsideMap(cell))
            return false;

        // If cell is too far we return.
        if ((cell - position).magnitude > stageDist)
        {
            return false;
        }

        spawnInCell(cell);
        return markReachable(cell);
    }

    private void spawnGrowthFlowers(float stageDist)
    {
        sprouts.Clear();
        if (reachableMap == null)
            reachableMap = new bool[map.width, map.height];
        for (int x = 0; x < map.width; ++x)
        {
            for (int y = 0; y < map.height; ++y)
            {
                reachableMap[x, y] = false;
            }
        }

        reachableMap[position.x, position.y] = true;

        for (int dist = 1; dist < Mathf.Max(map.width, map.height); ++dist)
        {
            if (dist > stageDist)
                break;

            var foundReachable = false;    // True if any cell was marked as reachable. Only those can be later growing.
            // We need to go inside out to make sure to compute reachability dependencies first in reachability computations.
            for (int d = 0; d <= dist; ++d)
            {
                if (d < dist)
                {
                    // .. __
                    // |    :
                    // :__..|
                    //
                    foundReachable |= visitCell(position + new Vector2Int(d, -dist), stageDist);
                    foundReachable |= visitCell(position + new Vector2Int(dist, d), stageDist);
                    foundReachable |= visitCell(position + new Vector2Int(-d, dist), stageDist);
                    foundReachable |= visitCell(position + new Vector2Int(-dist, -d), stageDist);
                }

                if (d > 0)
                {
                    //  __..
                    // :    |
                    // |..__:
                    //
                    foundReachable |= visitCell(position + new Vector2Int(-d, -dist), stageDist);
                    foundReachable |= visitCell(position + new Vector2Int(-dist, d), stageDist);
                    foundReachable |= visitCell(position + new Vector2Int(d, dist), stageDist);
                    foundReachable |= visitCell(position + new Vector2Int(dist, -d), stageDist);
                }
            }
            if (!foundReachable)
                break;  /// There is no point in checking further.
        }

        // Now actually spawn all the sprouts.
        foreach (var kv in sprouts)
        {
            TryToSpawn(kv.Item1, kv.Item2, kv.Item3);
        }
    }

    bool TryToSpawn(Vector2Int position, Vector2Int sourcePosition, Vector3 sourcePosition3d)
    {
        var childFlower = Map.instance.instantiateFlower(position, Map.instance.growFlowerPrefab, owner) as GrowthFlower;
        if (childFlower == null)
            return false;

        childFlower.canSpawn = false;
        childFlower.setSourcePosition(sourcePosition, sourcePosition3d);
        childFlower.parentFlower = this;
        return true;
    }
}
