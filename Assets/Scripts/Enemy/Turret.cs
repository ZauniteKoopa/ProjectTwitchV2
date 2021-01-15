using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : AbstractEnemy
{
    //Projectile transform
    [SerializeField]
    Transform projectile = null;

    [SerializeField]
    float fireRate = 0.4f;
    [SerializeField]
    int numBullets = 5;
    [SerializeField]
    float damage = 3.5f;
    
    //Accessor method to direction of enemy based on target. (Either towards - 0 or away - 1
    protected override bool IsMovingAway()
    {
        return false;
    }

    //Movement when player is not noticed by enemy
    //  Returns true if reached destination
    protected override bool PassiveMovement(float moveDelta, Vector3 tgtPos)
    {
        return false;
    }

    //Movement when player is noticed by enemy
    //  Returns true if reached destination
    protected override bool HostileMovement(float moveDelta, Vector3 tgtPos, bool onPath)
    {
        Vector3 stareVector = tgtPos - transform.position;
        stareVector.Normalize();
        dir = stareVector;

        return false;
    }

    //Method called for choosing how to attack
    protected override IEnumerator Attack(Transform tgt)
    {
        animState = EnemyAnimState.ATTACK;

        for (int i = 0; i < numBullets; i++)
        {
            if (GetComponent<EntityStatus>().IsAlive() && TgtVisible())
            {
                yield return new WaitForSeconds(fireRate);
                dir = tgt.position - transform.position;
                Vector2 dirVect = new Vector2(tgt.position.x - transform.position.x, tgt.position.y - transform.position.y);
                Transform curProj = Object.Instantiate(projectile, transform);
                curProj.GetComponent<ProjectileBehav>().SetProj(dirVect, damage, false);
            }
        }

        animState = EnemyAnimState.IDLE;
    }
}
