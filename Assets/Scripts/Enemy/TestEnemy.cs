using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemy : AbstractEnemy
{
    //List of patrol points
    [SerializeField]
    private Vector3[] patrolPoints = null;
    private int curPoint = 0;
    private const float PASSIVE_REACHED_DISTANCE = 0.25f;
    private const float HOSTILE_MAX_DISTANCE = 3.5f;
    private const float HOSTILE_MIN_DISTANCE = 2f;

    //Attack hitbox
    [SerializeField]
    private Transform projectile = null;
    [SerializeField]
    private float damage = 1.0f;

    //Passive movement
    protected override void PassiveMovement(float moveDelta)
    {
        //Move enemy to point
        Vector3 moveVector = patrolPoints[curPoint] - transform.position;
        moveVector.Normalize();
        transform.Translate(moveVector * moveDelta);

        //Check if patrol point has been reached
        float distance = Vector3.Distance(transform.position, patrolPoints[curPoint]);

        if (distance <= PASSIVE_REACHED_DISTANCE)
        {
            curPoint = (curPoint + 1) % patrolPoints.Length;
        }
    }

    //Hostile movement
    protected override void HostileMovement(float moveDelta, Transform tgt)
    {
        //Move enemy to or away target according to hostile distance constants
        Vector3 moveVector = tgt.position - transform.position;
        float distance = Vector3.Distance(transform.position, tgt.position);
        moveVector.Normalize();

        if (distance < HOSTILE_MIN_DISTANCE)
            moveVector *= -1;
        else if (distance < HOSTILE_MAX_DISTANCE)
            moveVector = Vector3.zero;

        transform.Translate(moveVector * moveDelta);
    }

    //Attacking method
    protected override IEnumerator Attack(Transform tgt)
    {
        Debug.Log("ENEMY ATTACK");

        //Wait for delay
        yield return new WaitForSeconds(0.5f);

        //Make projectile
        
        Vector2 dirVect = new Vector2(tgt.position.x - transform.position.x, tgt.position.y - transform.position.y);
        Transform curProj = Object.Instantiate(projectile, transform);
        curProj.GetComponent<ProjectileBehav>().SetProj(dirVect, damage, false);
    }
}
