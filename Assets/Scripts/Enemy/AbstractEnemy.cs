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

    //Variables for navigation
    [SerializeField]
    private int minPatrolPoints = 1;
    [SerializeField]
    private int maxPatrolPoints = 1;
    [SerializeField]
    private Vector3[] patrolPoints = null;
    private bool runAway;
    private int curPoint;
    private List<Waypoint> path;
    private int navIndex;

    //Method to determine attack behavior
    [SerializeField]
    private float attackTimerDelay = 1f;
    [SerializeField]
    private bool isHostile = true;
    private float attackTimer;
    private bool canAttack;

    //Loot drops
    [SerializeField]
    private float lootDropChance = 0.5f;
    [SerializeField]
    private float cogDropChance = 0.3f;
    [SerializeField]
    private Transform[] loot = null;
    [SerializeField]
    private Transform cog = null;

    //List of things to disable
    [SerializeField]
    private Canvas canvas = null;

    //Method flags
    private bool lostPlayer;
    private bool inAction;

    //Animation state variables to help management
    public enum EnemyAnimState {IDLE, MOVE, ATTACK};
    protected Vector3 dir = Vector3.down;
    protected EnemyAnimState animState = EnemyAnimState.IDLE;

    //On awake initialize variables
    private void Awake()
    {
        attackTimer = Random.Range(0f, attackTimerDelay);
        canAttack = true;
        tgt = null;
        isTgtVisible = false;

        status = GetComponent<EntityStatus>();
        lostPlayer = false;
        inAction = false;
        navIndex = -1;
        path = null;
        curPoint = 0;
    }


    //On every fixed update frame, run the AI decision tree
    private void FixedUpdate()
    {
        //Only activate this if enemy is not in the middle of an action
        if (!inAction && status.IsAlive())
        {
            //Collect data
            bool prevTgtVisible = isTgtVisible;

            if (tgt != null)
                isTgtVisible = tgt.IsVisible(GetComponent<Collider2D>());

            //If target was previously seen but no longer visible, enter confused state
            lostPlayer = (prevTgtVisible == true && isTgtVisible == false);
            bool prevDir = IsMovingAway();

            //Active decision tree
            DecisionTree(lostPlayer);

            //If enemy changed directions in this frame, set path to null
            if (IsMovingAway() != prevDir)
            {
                path = null;
            }
        }
        
    }


    //Private helper method that consists of the main decision tree of general AI
    private void DecisionTree(bool lostPlayer)
    {
        if (lostPlayer)
        {
            StartCoroutine(Confusion());
        }
        else if (status.IsAlive() && status.canMove)
        {
            float moveSpeed = status.GetCurSpeed() * Time.fixedDeltaTime;

            //Target is seen and is visible, do offensive movement. Else, do passive movement and reset timer
            if (isHostile && tgt != null && isTgtVisible)
            {
                bool reachedPoint = HostileMovement(moveSpeed, GetCurrentDest(), path != null);
                if (path != null && reachedPoint)
                    AdvancePath();

                //Attack every attack timer interval
                attackTimer += Time.fixedDeltaTime;

                if (canAttack && attackTimer > attackTimerDelay)
                {
                    StartCoroutine(ExecuteAttack());
                    attackTimer = 0f;
                }
            }
            else if (patrolPoints != null && patrolPoints.Length > 1)
            {
                attackTimer = 0f;

                //Move enemy. If reached a point, go to next point on path or delete path
                bool reachedPoint = PassiveMovement(moveSpeed, GetCurrentDest());
                if (reachedPoint)
                    AdvancePath();
            }
        }
    }

    //Private helper method to get next vector position for enemy to head to
    private Vector3 GetCurrentDest()
    {
        //If path exists, advance in path. If path doesn't exit and is still hostile, just go to target. Else, go to patrol point
        if (path != null)
        {
            return path[navIndex].GetPos();
        }
        else if (isHostile && tgt != null && isTgtVisible)      
        {
            return tgt.transform.position;
        }
        else
        {
            return patrolPoints[curPoint];
        }
    }

    //Private helper method used to advance enemy to the next point in path or patrolPoints array
    private void AdvancePath()
    {
        //If on a path, go to the next
        if (path != null)
        {
            navIndex--;

            if (navIndex < 0)
                path = null;
        }
        else
        {
            curPoint = (curPoint + 1) % patrolPoints.Length;
        }
    }

    //Accessor method for children classes to access tgt's position
    protected Vector3 GetTgtPosition()
    {
        return tgt.transform.position;
    }

    //When colliding with an obstacle, find a path around that obstacle
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Collider2D collider = collision.collider;
        Obstacle obstacle = collider.GetComponent<Obstacle>();

        if (obstacle != null)
        {
            Vector3 start = transform.position;
            Vector3 end = GetCurrentDest();

            path = (tgt != null && isTgtVisible && IsMovingAway()) ? obstacle.GetPathAway(start, end) : obstacle.GetPathTo(start, end);
            navIndex = path.Count - 1;
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
        attackTimer = Random.Range(0.0f, attackTimerDelay * 0.75f);
        tgt = player;
    }

    //Method to deal with enemy when on death
    public void OnEntityDeath()
    {
        //Disable enemy canvas
        if (canvas != null)
        {
            canvas.enabled = false;
        }

        //Check if the enemy actually drops loot
        float lootDrop = Random.Range(0f, 1f);
        float cogChanceEnd = lootDropChance + cogDropChance;
        
        if (lootDrop < lootDropChance)
        {
            int select = Random.Range(0, loot.Length);
            Transform chosenLoot = Object.Instantiate(loot[select], transform);
            chosenLoot.parent = null;
        }
        else if (lootDrop >= lootDropChance && lootDrop < cogChanceEnd)
        {
            Transform curCog = Object.Instantiate(cog, transform);
            curCog.parent = null;
        }
    }


    //Method to check if target is visible
    protected bool TgtVisible()
    {
        return tgt.IsVisible(GetComponent<Collider2D>());
    }

    
    //Accessor method to maximum patrol points for this enemy
    public int GetNumPatrolPoints()
    {
        return Random.Range(minPatrolPoints, maxPatrolPoints + 1);
    }

    //Det Patrol points
    public void SetPatrolPoints(Vector3[] points)
    {
        Debug.Assert(points != null && points.Length > 0);
        patrolPoints = points;
    }


    //Accessor method for animation manager
    public Vector3 GetForwardVector()
    {
        return dir;
    }

    public int GetAnimState()
    {
        return (int)animState;
    }



    //--------------------------------------------------
    //  Abstract Methods for classes to override
    //--------------------------------------------------

    //Accessor method to direction of enemy based on target. (Either towards - 0 or away - 1
    protected abstract bool IsMovingAway();

    //Movement when player is not noticed by enemy
    //  Returns true if reached destination
    protected abstract bool PassiveMovement(float moveDelta, Vector3 tgtPos);

    //Movement when player is noticed by enemy
    //  Returns true if reached destination
    protected abstract bool HostileMovement(float moveDelta, Vector3 tgtPos, bool onPath);

    //Method called for choosing how to attack
    protected abstract IEnumerator Attack(Transform trans);

}
