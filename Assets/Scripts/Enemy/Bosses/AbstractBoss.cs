using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class AbstractBoss : MonoBehaviour
{
    //Own entity status
    private EntityStatus status = null;
    
    //Variables concerning target
    [SerializeField]
    private TwitchController tgt = null;

    //integer that represent what phase the enemy's in: 1 - 4
    private int phase;
    private bool isAttacking;
    protected bool isActive;

    //Delay between attacks
    [SerializeField]
    private float attackDelay = 0.75f;

    //Array of Ingredients / Loot that is dropped each phase
    [SerializeField]
    private int numLootDropped = 2;
    [SerializeField]
    private Transform[] loot = null;
    [SerializeField]
    private RoomCollision arena = null;
    [SerializeField]
    private float spawnRadius = 4f;

    //Testing UI
    [SerializeField]
    private Color invincibleColor = Color.white;


    // Start is called before the first frame update
    void Start()
    {
        phase = 1;
        isAttacking = true;
        isActive = true;
        status = GetComponent<EntityStatus>();
        status.invulnerable = true;
    }

    //Method used to activate boss
    public void Activate(TwitchController player)
    {
        if (player != null)
        {
            tgt = player;
            player.provoked = true;
            StartCoroutine(StartAI());
        }
    }

    //Private IEnumerator to start the boss up
    private IEnumerator StartAI()
    {
        //Method to do dialogue stuff
        yield return new WaitForSeconds(1f);

        //Do decision tree
        status.invulnerable = false;
        StartCoroutine(DecisionTree());
    }



    // Decision Tree that's played in every interval between attacks
    private IEnumerator DecisionTree()
    {
        //Each boss has two modes: attacking and scouting. Depends on whether target is visible (according to enemy's standards)
        isAttacking = TgtVisible();

        if (isAttacking)                    //Attacking phase
        {
            yield return StartCoroutine(Attack(phase));
            yield return new WaitForSeconds(attackDelay);
        }
        else                                //Scouting phase
        {
            yield return StartCoroutine(Scout(phase));
            yield return StartCoroutine(Discovery());
        }

        //Begin the loop again by starting decision tree ONLY IF boss is alive
        if (isActive && tgt.GetComponent<EntityStatus>().IsAlive())
            StartCoroutine(DecisionTree());
        
    }

    //Callback functions to deal with enemy dying
    public void OnEntityDeath()
    {
        tgt.provoked = false;
        StopAllCoroutines();
        CancelInvoke();
    }

    //Callback function to deal with enemy changing to next phase
    public void OnPhaseChange()
    {
        //Invulnerability
        StartCoroutine(InvulnerablePhaseChange());

        //Drop loot
        for (int i = 0; i < numLootDropped; i++)
        {
            //Get loot type
            Transform lootType = loot[Random.Range(0, loot.Length)];
            Vector3 lootSize = lootType.localScale;

            //Find a position within the room and test it
            Vector3 spawnPoint = GetSpawnPos(lootSize);

            //Spawns loot at that point
            Object.Instantiate(lootType, spawnPoint, Quaternion.identity);
        }

        //Increment phase change
        phase++;
    }

    //Helper method to get spawn position
    protected Vector3 GetSpawnPos(Vector3 size)
    {
        //Find a position within the room and test it
        Vector3 curPoint;

        do 
        {
            float posY = Random.Range(-spawnRadius, spawnRadius);
            float posX = Random.Range(-spawnRadius, spawnRadius);

            curPoint = new Vector3(posX, posY, -1);
            curPoint = transform.TransformPoint(curPoint);
        }
        while (!arena.ValidPoint(curPoint, size));

        return curPoint;
    }

    //Helper method for invulnerability
    private IEnumerator InvulnerablePhaseChange()
    {
        isActive = false;
        GetComponent<SpriteRenderer>().color = invincibleColor;
        status.invulnerable = true;

        yield return new WaitForSeconds(3.5f);

        isActive = true;
        status.invulnerable = false;
        GetComponent<SpriteRenderer>().color = Color.white;
        StartCoroutine(DecisionTree());
    }


    //Method to set target to player
    public void SetTgt(TwitchController player)
    {
        tgt = player;
    }

    //Method to check if target is visible
    protected bool TgtVisible()
    {
        return tgt.IsVisible(GetComponent<Collider2D>()) || FoundTgt();
    }

    //Accessor method to status movement speed
    protected float GetMoveSpeed()
    {
        return status.GetCurSpeed();
    }

    //Accessor method to get tgt position
    protected Vector3 GetTgtPos()
    {
        return tgt.transform.position;
    }

    //Accessor method to check if tgt is stealthing or not
    protected bool IsTgtStealthing()
    {
        return tgt.IsStealthing();
    }

    //Method to do damage to tgt
    protected void DamageTgt(float dmg)
    {
        tgt.GetComponent<EntityStatus>().DamageEntity(dmg);
    }


    // -------------
    // Abstract methods to override
    // -------------

    //Additional conditions to check if enemy has found player used for scouting
    protected abstract bool FoundTgt();

    //Method to use when attacking, doing 1 of many attacks
    protected abstract IEnumerator Attack(int p);

    //Method to use when trying to scout
    protected abstract IEnumerator Scout(int p);

    //Method to use upon discovering player
    protected abstract IEnumerator Discovery();

}
