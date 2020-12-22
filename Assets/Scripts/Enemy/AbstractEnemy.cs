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

    //Method flags
    private bool dead;
    private bool lostPlayer;
    private bool inAction;

    //On awake initialize variables
    private void Awake()
    {
        attackTimer = Random.Range(0f, attackTimerDelay);
        canAttack = true;
        tgt = null;
        isTgtVisible = false;

        status = GetComponent<EntityStatus>();
        dead = false;
        lostPlayer = false;
        inAction = false;
    }


    //On every fixed update frame, run the AI decision tree
    private void FixedUpdate()
    {
        //Only activate this if enemy is not in the middle of an action
        if (!inAction)
        {
            //Collect data
            bool prevTgtVisible = isTgtVisible;

            if (tgt != null)
                isTgtVisible = tgt.IsVisible(GetComponent<Collider2D>());

            //If target was previously seen but no longer visible, enter confused state
            lostPlayer = (prevTgtVisible == true && isTgtVisible == false);


            //Active decision tree
            if (lostPlayer)
            {
                StartCoroutine(Confusion());
            }
            else if (!dead && status.canMove)
            {
                float moveSpeed = status.GetCurSpeed() * Time.fixedDeltaTime;

                //Target is seen and is visible, do offensive movement. Else, do passive movement
                if (tgt != null && isTgtVisible)
                {
                    HostileMovement(moveSpeed, tgt.transform);
                    attackTimer += Time.fixedDeltaTime;

                    //Attack every attack timer interval
                    if (canAttack && attackTimer > attackTimerDelay)
                    {
                        StartCoroutine(ExecuteAttack());
                        attackTimer = 0f;
                    }
                }
                else
                {
                    attackTimer = 0f;
                    PassiveMovement(moveSpeed);
                }
            }
        }
        
    }

    //IEnumerator to indicate confusion from enemy: (Stand still for a second and go back)
    private IEnumerator Confusion()
    {
        inAction = true;

        float timer = 0f;
        while (timer < 0.75f && !isTgtVisible)
        {
            yield return new WaitForFixedUpdate();
            timer += Time.fixedDeltaTime;
            isTgtVisible = tgt.IsVisible(GetComponent<Collider2D>());
        }

        inAction = false;
    }

    //IEnumerator to set up attacking
    private IEnumerator ExecuteAttack()
    {
        inAction = true;
        yield return StartCoroutine(Attack(tgt.transform));
        inAction = false;
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

    //Method called for choosing how to attack: ALL METHODS THIS WAY MUST END IN ENDACTION (will be edited)
    protected abstract IEnumerator Attack(Transform trans);

}
