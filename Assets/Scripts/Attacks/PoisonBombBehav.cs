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
    private const float BASE_DAMAGE_PER_TICK = 0.2f;
    private PoisonVial bombVial = null;
    private string tgtTag = "";

    private bool initialPhase;
    private int curTick;
    private HashSet<Collider2D> effected;

    //Audio
    [SerializeField]
    private AudioClip blastSound = null;


    //---------------
    // Side Effects Constants
    //---------------

    //Acid Spill constant
    private const float BASE_ACID_DAMAGE = 1.25f;
    private const float ACID_DAMAGE_GROWTH = 1.25f;

    //Combustion Blast constants
    private const float BASE_BLAST_DAMAGE = 2.5f;


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
        //If greater decay, increase rate of poison
        if (bombVial.GetSideEffect() == PoisonVial.SideEffect.GREATER_DECAY)
        {
            tickDuration /= 2.0f;
            maxTick *= 2;
        }

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
        List<EntityStatus> damaged = new List<EntityStatus>();
        //Calculate damage if need be
        float initDmg = BASE_DAMAGE_PER_TICK;
        if (bombVial.GetSideEffect() == PoisonVial.SideEffect.ACID_SPILL)
            initDmg += ((bombVial.GetSideEffectLevel() * ACID_DAMAGE_GROWTH) + BASE_ACID_DAMAGE);

        //Put enemies in separate list to avoid iteration error
        foreach (Collider2D enemy in effected)
        {
            if (enemy != null)
            {
                EntityStatus enemyStatus = enemy.GetComponent<EntityStatus>();
                if (enemyStatus != null)
                {
                    damaged.Add(enemyStatus);
                }
            }
        }

        //Iterate through list to actually damage enemy
        for (int i = 0; i < damaged.Count; i++)
            damaged[i].WeakPoisonDamageEntity(initDmg, 1, bombVial);

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


    //Explosion used when contamination is used: a signal handler method
    public void CombustionBlast()
    {
        //Put all enemies in a list to avoid iteration error
        List<Collider2D> damaged = new List<Collider2D>();

        foreach(Collider2D enemy in effected)
        {
            if (enemy != null)
                damaged.Add(enemy);
        }

        for (int i = 0; i < damaged.Count; i++)
        {
            int numStacks = bombVial.GetSideEffectLevel();
            float dmg = BASE_BLAST_DAMAGE * numStacks;

            damaged[i].GetComponent<EntityStatus>().PoisonDamageEntity(dmg, numStacks, bombVial);
        }

        StartCoroutine(ExplodeFog());
    }

    //IEnumerator sequence to destroy fog by explosion
    private IEnumerator ExplodeFog()
    {
        //Disable enemy
        CancelInvoke();
        GetComponent<SpriteRenderer>().enabled = false;

        //Play audio
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = blastSound;
        audio.Play();

        yield return new WaitForSeconds(0.65f);

        Destroy(gameObject);
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
