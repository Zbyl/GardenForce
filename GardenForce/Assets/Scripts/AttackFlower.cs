using UnityEngine;
using UnityEditor;

public class AttackFlower : Flower
{
    public bool canSpawn = true;

    public void init(Flower previousFlower)
    {
        var model = previousFlower.transform.Find("Model");
        model.name = "StolenModel";
        model.SetParent(transform);
    }

    public override void logicUpdate(int currentTime)
    {
        if (canSpawn && (currentTime - creationTIme == 10))
        {
            var position = new Vector2Int(this.position.x, this.position.y + 2);
            var childFlower = Map.instance.instantiateFlower(position, Map.instance.attackFlowerPrefab, owner) as AttackFlower;
            if (childFlower != null)
            {
                childFlower.canSpawn = false;
            }
        }
    }
}
