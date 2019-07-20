using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GrowthFlower : Flower
{
    public bool canSpawn = true;

    public int growthStart;         /// First sprout is generated in this tick.
    public int growthDelay;         /// Delay between generating growing stages.
    public int growthStages;        /// How many growth stages to do.
    public float growthRadius;      /// Maximal growth radius.

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

        spawnGrowthFlowers();
    }

    private Dictionary<Vector2Int, Vector2Int> sprouts = new Dictionary<Vector2Int, Vector2Int>();  /// Sprouts generated in this logic update.
    private bool[,] reachableMap;   /// Map of growth flowers that are reachable from the source.

    /// Gets previous cell towards source.
    private static Vector2Int getPreviousCell(Vector2Int cell, Vector2Int source)
    {
        Vector2Int previousCell;

        var diff = cell - source;
        var absDX = Mathf.Abs(diff.x);
        var absDY = Mathf.Abs(diff.y);
        if (absDX > absDY)
        {
            // "horizontal" line
            var dY = (int)((float)diff.y / absDX * (absDX - 1));
            var dX = (diff.x >= 0) ? -1 : 1;
            previousCell = new Vector2Int(cell.x + dX, source.y + dY);
        }
        else
        {
            // "vertical" line
            var dX = (int)((float)diff.x / absDY * (absDY - 1));
            var dY = (diff.y >= 0) ? -1 : 1;
            previousCell = new Vector2Int(source.x + dX, cell.y + dY);
        }

        /*if (!Map.instance.isPositionInsideMap(previousCell))
        {
            Debug.LogError("Source: " + source);
            Debug.LogError("Cell: " + cell);
        }*/

        return previousCell;
    }

    /// Returns true if cellEnd can be reached from cellStart by walking only on reachable cell, and not diagonally.
    /// cellEnd doesn't have to be reachable.
    /// cellStart and cellEnd should be neighbouring cells.
    /// Note: Both cells must be inside the map.
    private bool canReach(Vector2Int cellEnd, Vector2Int cellStart)
    {
        if (!reachableMap[cellStart.x, cellStart.y]) return false;

        var dX = (cellEnd.x > cellStart.x) ? 1 : ((cellEnd.x < cellStart.x) ? -1 : 0);
        var dY = (cellEnd.y > cellStart.y) ? 1 : ((cellEnd.y < cellStart.y) ? -1 : 0);

        if (reachableMap[cellStart.x + dX, cellStart.y]) return true;
        if (reachableMap[cellStart.x, cellStart.y + dY]) return true;

        return false;
    }

    /// Marks growth flowers that are reachable from the source.
    /// Only those can be later source for sprouts.
    /// Returns true if given cell was marked as reachable.
    /// Note: cell must be inside the map.
    private bool markReachable(Vector2Int cell)
    {
        var previousCell = getPreviousCell(cell, position);

        // Mark cell reachable only if previous cell is reachable and this cell contains a growth flower.
        if (!canReach(cell, previousCell))
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

        var previousCell = getPreviousCell(cell, position);

        // Can spawn only if previous cell is reachable.
        if (canReach(cell, previousCell))
        {
            // Spawn new growth flower here.
            sprouts.Add(cell, previousCell);
        }
    }

    /// Tries to spawn a flower here.
    /// Also tries to mark cell as reachable.
    /// Returns true if cell was marked reachable.
    /// Note: cell doesn't have to be inside the map.
    private bool visitCell(Vector2Int cell)
    {
        if (!map.isPositionInsideMap(cell))
            return false;

        // If cell is too far we return.
        if ((cell - position).magnitude > growthRadius)
        {
            return false;
        }

        spawnInCell(cell);
        return markReachable(cell);
    }

    private void spawnGrowthFlowers()
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
                    foundReachable |= visitCell(position + new Vector2Int(d, -dist));
                    foundReachable |= visitCell(position + new Vector2Int(dist, d));
                    foundReachable |= visitCell(position + new Vector2Int(-d, dist));
                    foundReachable |= visitCell(position + new Vector2Int(-dist, -d));
                }

                if (d > 0)
                {
                    //  __..
                    // :    |
                    // |..__:
                    //
                    foundReachable |= visitCell(position + new Vector2Int(-d, -dist));
                    foundReachable |= visitCell(position + new Vector2Int(-dist, d));
                    foundReachable |= visitCell(position + new Vector2Int(d, dist));
                    foundReachable |= visitCell(position + new Vector2Int(dist, -d));
                }
            }
            if (!foundReachable)
                break;  /// There is no point in checking further.
        }

        // Now actually spawn all the sprouts.
        foreach (var kv in sprouts)
        {
            TryToSpawn(kv.Key, kv.Value);
        }
    }

    bool TryToSpawn(Vector2Int position, Vector2Int sourcePosition)
    {
        var childFlower = Map.instance.instantiateFlower(position, Map.instance.growFlowerPrefab, owner) as GrowthFlower;
        if (childFlower == null)
            return false;

        childFlower.canSpawn = false;
        childFlower.setSourcePosition(sourcePosition);
        return true;
    }
}
