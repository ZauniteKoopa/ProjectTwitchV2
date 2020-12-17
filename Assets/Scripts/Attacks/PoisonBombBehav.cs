using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonBombBehav : MonoBehaviour
{
    [SerializeField]
    private float tickDuration = 0.75f;
    [SerializeField]
    private float initialTickDuration = 0.2f;
    [SerializeField]
    private int maxTick = 6;

    private float initialDmgFactor = 2.0f;
    private float slownessFactor = 0.5f;
    private int initialStacks = 2;
    private PoisonVial bombVial = null;
    private string tgtTag = "";

    private bool initialPhase;
    private int curTick;
    private HashSet<Collider2D> effected;

    //Awake is called to set local variables
    void Awake()
    {
        initialPhase = true;
        curTick = 0;
        effected = new HashSet<Collider2D>();
    }


    // Start is called before the first frame update
    void Start()
    {
        Invoke("FinishInitial", initialTickDuration);
    }

    //Method to set bomb up
    public void SetBomb(bool isPlayer, PoisonVial vial)
    {
        tgtTag = (isPlayer) ? "Enemy" : "Player";
        bombVial = vial;
        
        Color bombColor = vial.GetColor();
        bombColor.a = 0.25f;
        GetComponent<SpriteRenderer>().color = bombColor;
    }
    

    //Method to stop initial damage phase
    void FinishInitial()
    {
        initialPhase = false;
        Invoke("ApplyTickDamage", tickDuration);
    }

    //Method to apply tick damage to all those are effected
    void ApplyTickDamage()
    {
        //Apply tick damage to all affected
        foreach (Collider2D enemy in effected)
        {
            EntityStatus enemyStatus = enemy.GetComponent<EntityStatus>();
            if (enemyStatus != null)
                enemy.GetComponent<EntityStatus>().PoisonDamageEntity(0.0f, 1, bombVial);
        }

        //Increment curTick and check if object ready for destroy
        curTick++;

        if (curTick == maxTick)
        {
            Destroy(gameObject);
        }
        else
        {
            Invoke("ApplyTickDamage", tickDuration);
        }
    }

    //Method to check collisions and add objects effected HashSet
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == tgtTag)
        {
            if (initialPhase)
            {
                float initialDamage = initialDmgFactor * bombVial.GetDamage();
                collider.GetComponent<EntityStatus>().PoisonDamageEntity(initialDamage, initialStacks, bombVial);
            }

            effected.Add(collider);
            collider.GetComponent<EntityStatus>().ChangeSpeed(slownessFactor);
        }
    }

    //Method to check collisions and remove objects from effected hashset
    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.tag == tgtTag)
        {
            effected.Remove(collider);
            collider.GetComponent<EntityStatus>().ChangeSpeed(1.0f / slownessFactor);
        }
    }

}
