using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemy : AbstractEnemy
{
    //Movement variables
    [SerializeField]
    private const float PASSIVE_REACHED_DISTANCE = 0.25f;
    private const float HOSTILE_MAX_DISTANCE = 4.5f;
    private const float HOSTILE_MIN_DISTANCE = 3f;

    //Boolean flag that indicates direction: 0 is towards, 1 is away
    private bool isMovingAway = false;

    //Attack hitbox
    [SerializeField]
    private Transform projectile = null;
    [SerializeField]
    private float damage = 1.0f;

    //Accessor method on whether or not this enemy is moving away / towards tgt
    protected override bool IsMovingAway()
    {
        return isMovingAway;
    }

    //Passive movement
    protected override bool PassiveMovement(float moveDelta, Vector3 tgtPos)
    {
        //Move enemy to point
        Vector3 moveVector = tgtPos - transform.position;
        moveVector.Normalize();
        transform.Translate(moveVector * moveDelta);

        //Check if patrol point has been reached
        float distance = Vector3.Distance(transform.position, tgtPos);
        return distance <= PASSIVE_REACHED_DISTANCE;
    }

    //Hostile movement
    protected override bool HostileMovement(float moveDelta, Vector3 tgtPos, bool onPath)
    {
        //Move enemy to or away target according to hostile distance constants
        isMovingAway = Vector3.Distance(transform.position, GetTgtPosition()) < HOSTILE_MIN_DISTANCE;
        Vector3 moveVector = tgtPos - transform.position;
        float distance = Vector3.Distance(transform.position, tgtPos);

        moveVector.Normalize();

        if (distance < HOSTILE_MIN_DISTANCE && !onPath)
            moveVector *= -1;
        else if (distance < HOSTILE_MAX_DISTANCE && distance > HOSTILE_MAX_DISTANCE)
            moveVector = Vector3.zero;

        transform.Translate(moveVector * moveDelta);

        distance = Vector3.Distance(transform.position, tgtPos);
        return distance <= PASSIVE_REACHED_DISTANCE;
    }

    //Attacking method
    protected override IEnumerator Attack(Transform tgt)
    {
        //Wait for delay
        yield return new WaitForSeconds(0.5f);

        //Make projectile
        
        Vector2 dirVect = new Vector2(tgt.position.x - transform.position.x, tgt.position.y - transform.position.y);
        Transform curProj = Object.Instantiate(projectile, transform);
        curProj.GetComponent<ProjectileBehav>().SetProj(dirVect, damage, false);
    }
}
