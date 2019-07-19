using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flower : MonoBehaviour
{
    public enum FlowerType
    {
        not_set,
        grow,
        colonize,
        protect,
        attack,
    };

    public FlowerType type;

    public int owner; // 1 lub 2
    public Vector2Int position;
    public int creationTime;                /// Creation time in game ticks.
    public float creationTimeInSeconds;       /// Creation time in seconds - used for animations and such. Not for logic.
    public bool self_destruct;

    public Vector2Int sourcePosition;      /// On creation flower will smoothly travel from this position to it's destination.
    public float travelSpeed;       /// How fast does a sprout travel. In tiles per second.
    public float createFadeInSeconds;      /// Duration of fade in during creation.
    public float destroyFadeOutSeconds;    /// Duration of fade out during destruction.
    public float previousFadeOutSeconds;   /// Duration of fade out of previous flower.

    private Map map { get { return Map.instance; } }

    // Start is called before the first frame update
    void Start()
    {
    }

    void Update()
    {
        travelToPosition();
        fadeIn();
        fadeOutPrevious();
    }

    public void setSourcePosition(Vector2Int position)
    {
        sourcePosition = position;
        transform.position = map.mapPositionToWorldPosition(position, map.flowerZ);
    }

    /// Logic for smooth movement from sourcePosition to final position.
    void travelToPosition()
    {
        var travelFraction = 1.0f;
        if (position != sourcePosition)
        {
            travelFraction = (Time.time - creationTimeInSeconds) * travelSpeed / (position - sourcePosition).magnitude;
        }

        var sourcePos = map.mapPositionToWorldPosition(sourcePosition, map.flowerZ);
        var destPos = map.mapPositionToWorldPosition(position, map.flowerZ);
        if (travelFraction < 1.0f)
        {
            travelFraction = Mathf.Sqrt(travelFraction);
            transform.position = sourcePos * (1 - travelFraction) + destPos * travelFraction;
        }
        else
        {
            transform.position = destPos;
        }
    }

    /// Logic for fade in on create.
    void fadeIn()
    {
        if (createFadeInSeconds == 0)
            return;

        var fadeFraction = (Time.time - creationTimeInSeconds) / createFadeInSeconds;
        if (fadeFraction > 1.0f)
        {
            fadeFraction = 1.0f;
        }
        else
        {
            fadeFraction = Mathf.Sqrt(fadeFraction);
        }

        var model = this.transform.Find("Model");
        foreach (var child in model.GetComponentsInChildren<SpriteRenderer>())
        {
            var color = child.color;
            color.a = fadeFraction;
            child.color = color;
        }
    }

    /// Logic for fade out of previous flower.
    void fadeOutPrevious()
    {
        if (previousFadeOutSeconds <= 0)
            return;

        var model = this.transform.Find("StolenModel");
        if (model == null)
            return;

        var fadeFraction = (Time.time - creationTimeInSeconds) / previousFadeOutSeconds;
        if (fadeFraction > 1.0f)
        {
            fadeFraction = 1.0f;
        }
        else
        {
            fadeFraction = Mathf.Sqrt(fadeFraction);
        }

        foreach (var child in model.GetComponentsInChildren<SpriteRenderer>())
        {
            var color = child.color;
            color.a = Mathf.Min(color.a, fadeFraction);
            child.color = color;
        }

        if (fadeFraction >= 1.0f)
        {
            Destroy(model);
        }
    }

    public virtual void init(Flower previousFlower)
    {
        if ((previousFlower != null) && (previousFadeOutSeconds > 0))
        {
            var model = previousFlower.transform.Find("Model");
            model.name = "StolenModel";
            model.SetParent(transform);
        }
    }

    public virtual void logicUpdate(int currentTime)
    {
    }
}
