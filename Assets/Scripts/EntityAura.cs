using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityAura : MonoBehaviour
{
    //Variable that shows which side effect is active
    PoisonVial poison = null;
    private PoisonVial.SideEffect sideEffect = PoisonVial.SideEffect.NONE;
    private int effectLevel = 0;
    private bool activated = false;

    //Hashset to keep track of colliders
    private HashSet<Collider2D> effected;

    //Poison Fog constants
    private const float POISON_FOG_DAMAGE = 0.2f;

    //Noxious Explosion constants
    private const float NOXIOUS_DAMAGE = 3.0f;

    //Slime Leak variable
    private const float BASE_SLIME_SLOW = 0.6f;


    //On awake set up variables
    void Awake()
    {
        effected = new HashSet<Collider2D>();
    }

    //Method that enables aura
    public void EnableAura(PoisonVial vial)
    {
        //Get variables from poison vial
        PoisonVial.SideEffect effect = vial.GetSideEffect();
        int level = vial.GetSideEffectLevel();

        //If effect has something to do with slime leak, modify everyone that's already in the field
        if (sideEffect != effect)
        {
            if (effect == PoisonVial.SideEffect.SLIME_LEAK || sideEffect == PoisonVial.SideEffect.SLIME_LEAK)
            {
                float speedFactor = (effect == PoisonVial.SideEffect.SLIME_LEAK) ? BASE_SLIME_SLOW : 1f / BASE_SLIME_SLOW;

                foreach (Collider2D enemy in effected)
                {
                    if (enemy != null)
                    {
                        EntityStatus status = enemy.GetComponent<EntityStatus>();
                        status.ChangeSpeed(speedFactor);
                    }
                } 
            }
        }

        //Set variables
        activated = (effect == PoisonVial.SideEffect.POISON_FOG ||
                   effect == PoisonVial.SideEffect.NOXIOUS_EXPLOSION ||
                   effect == PoisonVial.SideEffect.SLIME_LEAK);

        if (activated)
        {   
            //Set variables
            poison = vial;
            sideEffect = effect;
            effectLevel = level;

            Color auraColor = vial.GetColor();
            auraColor.a = 0.2f;
            GetComponent<SpriteRenderer>().color = auraColor;
            GetComponent<SpriteRenderer>().enabled = true;
        }
        else
        {
            DisableAura();
        }
        
    }

    //Method that disables
    public void DisableAura()
    {
        //Disable slime leak for those inside
        if (sideEffect == PoisonVial.SideEffect.SLIME_LEAK)
        {

            foreach (Collider2D enemy in effected)
            {
                if (enemy != null)
                {
                    EntityStatus status = enemy.GetComponent<EntityStatus>();
                    status.ChangeSpeed(1f / BASE_SLIME_SLOW);
                }
            } 
        }

        activated = false;
        effectLevel = 0;
        sideEffect = PoisonVial.SideEffect.NONE;
        GetComponent<SpriteRenderer>().enabled = false;
    }

    //Methods adding enemies to effected
    void OnTriggerEnter2D(Collider2D collider)
    {
        AbstractEnemy enemy = collider.GetComponent<AbstractEnemy>();

        if (enemy != null)
        {
            effected.Add(collider);

            if (sideEffect == PoisonVial.SideEffect.SLIME_LEAK)
            {
                EntityStatus status = enemy.GetComponent<EntityStatus>();
                status.ChangeSpeed(BASE_SLIME_SLOW);
            }
        }
    }

    //Methods removing enemies
    void OnTriggerExit2D(Collider2D collider)
    {
        AbstractEnemy enemy = collider.GetComponent<AbstractEnemy>();

        if (enemy != null && effected.Contains(collider))
        {
            effected.Remove(collider);

            if (sideEffect == PoisonVial.SideEffect.SLIME_LEAK)
            {
                EntityStatus status = enemy.GetComponent<EntityStatus>();
                status.ChangeSpeed(1.0f / BASE_SLIME_SLOW);
            }
        }
    }

    //Method that deals a poison damage tick to everyone involved
    public void AuraPoisonTick()
    {
        if (sideEffect == PoisonVial.SideEffect.POISON_FOG)
        {
            List<EntityStatus> damaged = new List<EntityStatus>();

            //Collect who's getting damaged in iteration
            foreach (Collider2D enemy in effected)
            {
                if (enemy != null)
                {
                    EntityStatus status = enemy.GetComponent<EntityStatus>();
                    damaged.Add(status);
                }
            }

            //Actually damage enemies
            for (int i = 0; i < damaged.Count; i++)
            {
                damaged[i].WeakPoisonDamageEntity(effectLevel * POISON_FOG_DAMAGE, 1, poison);
            }
        }
    }

    //Method that makes enemies explode when contaminated
    public void ContaminateExplode(int numStacks)
    {
        if (sideEffect == PoisonVial.SideEffect.NOXIOUS_EXPLOSION)
        {
            List<EntityStatus> damaged = new List<EntityStatus>();

            foreach (Collider2D enemy in effected)
            {
                if (enemy != null)
                {
                    EntityStatus status = enemy.GetComponent<EntityStatus>();
                    damaged.Add(status);
                }
            }

            //Actually damage enemies
            for (int i = 0; i < damaged.Count; i++)
            {
                float dmg = effectLevel * NOXIOUS_DAMAGE;
                Debug.Log(dmg);
                damaged[i].WeakPoisonDamageEntity(effectLevel * NOXIOUS_DAMAGE, numStacks / 2, poison);
            }
        }
    }


}
