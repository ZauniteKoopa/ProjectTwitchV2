using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutatedWhump : AbstractEnemy
{
    [SerializeField]
    float damage = 4f;

    //State variables for animation to reference
    bool stunned = false;

    //Constants
    private const float STUNNED_DURATION = 2f;
    private const float ATTACKING_DISTANCE = 1.4f;
    private const float PASSIVE_REACHED_DISTANCE = 0.25f;

    //Accessor method to direction of enemy based on target. (Either towards - 0 or away - 1
    protected override bool IsMovingAway()
    {
        return false;
    }

    //Movement when player is not noticed by enemy
    //  Returns true if reached destination
    protected override bool PassiveMovement(float moveDelta, Vector3 tgtPos)
    {
        if (!stunned)
        {
            //Move enemy to point
            Vector3 moveVector = tgtPos - transform.position;
            moveVector.Normalize();
            dir = moveVector;
            animState = EnemyAnimState.MOVE;

            transform.Translate(moveVector * moveDelta);

            //Check if patrol point has been reached
            float distance = Vector3.Distance(transform.position, tgtPos);
            return distance <= PASSIVE_REACHED_DISTANCE;
        }

        return false;
    }

    //Movement when player is noticed by enemy
    //  Returns true if reached destination
    protected override bool HostileMovement(float moveDelta, Vector3 tgtPos, bool onPath)
    {
        if (!stunned)
        {
            //Move enemy to or away target according to hostile distance constants
            Vector3 moveVector = tgtPos - transform.position;
            float distance = Vector3.Distance(transform.position, tgtPos);
            moveVector.Normalize();
            dir = moveVector;
            animState = EnemyAnimState.MOVE;

            transform.Translate(moveVector * moveDelta);

            distance = Vector3.Distance(transform.position, tgtPos);
            return distance <= PASSIVE_REACHED_DISTANCE;
        }

        return false;
    }

    //Method called for choosing how to attack
    protected override IEnumerator Attack(Transform tgt)
    {
        float dist = Vector3.Distance(tgt.position, transform.position);

        if (dist <= ATTACKING_DISTANCE)
        {
            stunned = true;
            animState = EnemyAnimState.IDLE;
            tgt.GetComponent<EntityStatus>().DamageEntity(damage);
            yield return new WaitForSeconds(STUNNED_DURATION);
            stunned = false;
        }
    }


    //Accessor methods for animation manager
    public bool IsStunned()
    {
        return stunned;
    }
}
