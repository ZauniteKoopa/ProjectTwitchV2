using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonProjBehav : ProjectileBehav
{
    //Vial 
    private PoisonVial vial;

    //Piercing bolts constant
    private float dmgModifier = 1.0f;
    private const float REDUCTION_PER_HIT = 0.1f;
    private const float MIN_DMG_MODIFIER = 0.6f;

    //Open infection constants
    private const float INFECTED_DMG_PER_LVL = 0.15f;

    //Slime Bomb constants
    [SerializeField]
    private Transform slimeBomb = null;
    private const float SLIME_BOMB_DMG_PERCENT = 0.4f;

    //Spray and pray constants
    private const float SPRAY_CRIT_CHANCE = 0.4f;
    private const float CRIT_DAMAGE_PERCENT = 3.0f;
    

    //Method to damage enemy
    protected override void DamageEntity(Collider2D collider)
    {
        EntityStatus tgtStatus = collider.GetComponent<EntityStatus>();

        //Get damage method (Open Infection)
        float dmg = vial.GetDamage() * dmgModifier;
        if (vial.GetSideEffect() == PoisonVial.SideEffect.OPEN_INFECTION)
            dmg += (INFECTED_DMG_PER_LVL * vial.GetSideEffectLevel() * tgtStatus.GetPoisonStacks());


        tgtStatus.PoisonDamageEntity(dmg, 1, vial);

        //Instantiate slime bomb if poison can
        if (vial.GetSideEffect() == PoisonVial.SideEffect.SLIME_BOMB)
        {
            Transform curBomb = Object.Instantiate(slimeBomb, transform);
            curBomb.parent = null;
            curBomb.GetComponent<SlimeBomb>().SetUpBomb(dmg * SLIME_BOMB_DMG_PERCENT, vial);
        }

        //Projectile destruction (Piercing Projectile)
        if (vial.GetSideEffect() == PoisonVial.SideEffect.PIERCING_SHOT)
        {
            if (dmgModifier - REDUCTION_PER_HIT < MIN_DMG_MODIFIER)
                dmgModifier = MIN_DMG_MODIFIER;
            else
                dmgModifier -= REDUCTION_PER_HIT;
            
            StartCoroutine(PlayOnHitSound(false));
        }
        else
        {
            StartCoroutine(PlayOnHitSound(true));
        }
    }

    public void SetPoisonProj(Vector2 dir, PoisonVial pv, bool isPlayer)
    {
        vial = pv;
        dmgModifier = 1.0f;

        if (pv.GetSideEffect() == PoisonVial.SideEffect.SPRAY_PRAY && Random.Range(0.0f, 1.0f) <= SPRAY_CRIT_CHANCE)
        {
            dmgModifier *= CRIT_DAMAGE_PERCENT;
        }

        GetComponent<SpriteRenderer>().color = pv.GetColor();
        SetProj(dir, 0f, isPlayer);
    }
}
