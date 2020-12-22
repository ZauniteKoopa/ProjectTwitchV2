using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractEnemy : MonoBehaviour
{
    //variable referencing enemy's own status
    private EntityStatus status;

    //Variable referencing player's controller object and how they react to it
    private TwitchController tgt;
    private bool isTgtVisible;

    //Method to determine behavior
    [SerializeField]
    private float attackTimerDelay = 1f;
    private float attackTimer;
    private bool canAttack;

    //Dead flag
    private bool dead;

    //On awake initialize variables
    private void Awake()
    {
        attackTimer = 0f;
        canAttack = false;
        tgt = null;
        isTgtVisible = false;

        status = GetComponent<EntityStatus>();
        dead = false;
    }


    //On every fixed update frame, run the AI decision tree
    private void FixedUpdate()
    {
        //Collect data
        if (tgt != null)
            isTgtVisible = tgt.IsVisible(GetComponent<Collider2D>());

        //Active decision tree
        if (!dead && status.CanMove())
        {
            float moveSpeed = status.GetCurSpeed() * Time.fixedDeltaTime;

            //Target is seen and is visible, do offensive movement. Else, do passive movement
            if (tgt != null && isTgtVisible)
            {
                HostileMovement(moveSpeed, tgt.transform);
                attackTimer += Time.fixedDeltaTime;

                if (canAttack && attackTimer > attackTimerDelay)
                {
                    Attack(tgt.transform);
                }
            }
            else
            {
                attackTimer = 0f;
                PassiveMovement(moveSpeed);
            }
        }
    }


    //Method to set target to player
    public void SetTgt(TwitchController player)
    {
        tgt = player;
    }


    //--------------------------------------------------
    //  Abstract Methods for classes to override
    //--------------------------------------------------

    //Movement when player is not noticed by enemy
    protected abstract void PassiveMovement(float moveDelta);

    //Movement when player is noticed by enemy
    protected abstract void HostileMovement(float moveDelta, Transform trans);

    //Method called for choosing how to attack
    protected abstract void Attack(Transform trans);

}
