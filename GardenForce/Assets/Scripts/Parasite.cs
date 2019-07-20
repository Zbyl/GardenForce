using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parasite : MonoBehaviour
{
    public float travelSpeed;               /// How fast does the parasite travel (in map units per second).
    public float lifeDuration;              /// How long does the parasite live (in ticks).
    public int changeDirectionInterval;     /// How often does the parasite change direction (in ticks).
    public float minAngleChange;            /// Minimal direction change (in degrees).
    public float maxAngleChange;            /// Minimal direction change (in degrees).

    internal Vector2Int startPosition;
    internal int owner;
    internal int creationTime;                       /// Creation time in game ticks.
    internal float creationTimeInSeconds;       /// Creation time in seconds - used for animations and such. Not for logic.

    private float currentAngle;             /// Angle between right vector and target.
    private int lastDirectionChangeTime;

    protected Map map { get { return Map.instance; } }

    // Start is called before the first frame update
    void Start()
    {
        var target = map.getStartPosition(3 - owner);
        if (target == startPosition) {
            target = map.getStartPosition(owner);
        }

        var target3d = map.mapPositionToWorldPosition(target, map.parasiteZ);
        var position3d = map.mapPositionToWorldPosition(startPosition, map.parasiteZ);
        var direction = (target3d - position3d).normalized;

        currentAngle = Vector3.SignedAngle(Vector3.right, direction, Vector3.forward);
        lastDirectionChangeTime = creationTime;

        var position3dCenter = map.mapPositionToWorldPosition(new Vector2(startPosition.x + 0.5f, startPosition.y + 0.5f), map.parasiteZ);
        transform.position = position3dCenter;
        transform.rotation = Quaternion.AngleAxis(currentAngle, Vector3.forward);

        // Remove what's under parasite.
        map.removeFlower(startPosition);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void logicUpdate(int currentTime)
    {
        if (currentTime - lastDirectionChangeTime > changeDirectionInterval)
        {
            lastDirectionChangeTime = currentTime;
            fluctuateAngle();
        }

        // Move parasite.
        var oldPosition = map.worldPositionToIntMapPosition(transform.position);
        transform.position += transform.right * map.mapUnitsToWorldUnits(travelSpeed);
        var mapPosition = map.worldPositionToIntMapPosition(transform.position);

        var dieNow = false;
        if (map.isPositionInsideMap(mapPosition))
        {
            // Remove what's under parasite.
            map.removeFlower(mapPosition);
        }
        else
        {
            // When leaving map die immediately.
            mapPosition = oldPosition;
            dieNow = true;
        }

        // Check if it's time to die.
        if (!dieNow && (currentTime - creationTime < lifeDuration))
            return;

        Destroy(gameObject);
        map.parasites.Remove(this);

        // Instantiate AttackFlower.
        var flower = map.instantiateFlower(mapPosition, map.attackFlowerPrefab, owner) as AttackFlower;
        if (flower == null)
            return;
        flower.spawnAfterDeath = map.idleFlowerPrefab;
    }

    private void fluctuateAngle()
    {
        var change = Random.Range(minAngleChange, maxAngleChange);
        if (Random.Range(0, 2) == 0)
            change *= -1;

        currentAngle += change;
        transform.rotation = Quaternion.AngleAxis(currentAngle, Vector3.forward);
    }
}
