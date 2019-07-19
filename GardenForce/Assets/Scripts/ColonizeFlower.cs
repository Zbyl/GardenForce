using UnityEngine;
using UnityEditor;

public class ColonizeFlower : Flower
{
    public bool canSpawn = true;

    public int colonizationStart;   /// First sprout is generated in this tick.
    public int colonizationDelay;   /// Delay between generating sprouts.
    public int colonizationMinRadius;  /// How far can sprouts fly, minimum.
    public int colonizationMaxRadius;  /// How far can sprouts fly, maximum.
    public int colonizationCount;   /// How many sprouts to generate.

    private int colonizationRetryCount = 15;   /// How many times to try to find a place for a sprout before giving up.

    public override void init(Flower previousFlower)
    {
        if (previousFlower != null)
        {
            var model = previousFlower.transform.Find("Model");
            model.name = "StolenModel";
            model.SetParent(transform);
        }
    }

    public override void logicUpdate(int currentTime)
    {
        if (!canSpawn)
        {
            /// Logic for a sprout.
            return;
        }

        int stage = (currentTime - creationTime) - colonizationStart;
        if (colonizationDelay > 0)
        {
            if (stage % colonizationDelay != 0)
                return;
            stage /= colonizationDelay;
        }

        if (stage < 0)
            return;
        if (stage >= colonizationCount)
            return;

        if (colonizationDelay > 0)
        {
            // Spawn just one sprout.
            spawnSprout();
            return;
        }

        // Spawn all sprout at once.
        for (var i = 0; i < colonizationCount; ++i)
            spawnSprout();
    }

    void spawnSprout()
    {
        for (int i = 0; i < colonizationRetryCount; ++i)
        {
            Vector2Int randomPosition;
            while (true)
            {
                var delta = new Vector2Int(Random.Range(-colonizationMaxRadius, colonizationMaxRadius + 1), Random.Range(-colonizationMaxRadius, colonizationMaxRadius + 1));
                if (delta.magnitude < colonizationMinRadius)
                    continue;
                if (delta.magnitude > colonizationMaxRadius)
                    continue;

                randomPosition = position + delta;
                break;
            }

            if (Map.instance.getFlower(randomPosition) != null)
                continue;

            if (TryToSpawn(randomPosition))
                break;
        }
    }

    bool TryToSpawn(Vector2Int position)
    {
        var childFlower = Map.instance.instantiateFlower(position, Map.instance.colonizeFlowerPrefab, owner) as ColonizeFlower;
        if (childFlower == null)
            return false;

        childFlower.canSpawn = false;
        childFlower.setSourcePosition(this.position);
        return true;
    }
}
