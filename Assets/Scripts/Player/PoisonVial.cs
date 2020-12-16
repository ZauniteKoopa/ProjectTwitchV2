using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonVial
{
    //Display for UI
    private Color poisonColor;

    //Set viewable stats for Poison
    private int potency;
    private int poison;
    private int reactivity;
    private int stickiness;

    //Ammo system
    private int ammo;

    //Constants for damage/potency
    private const float BASE_DAMAGE = 1.5f;
    private const float DMG_GROWTH = 0.5f;

    //constants for poison
    private const float BASE_POISON = 0f;
    private const float POISON_GROWTH = 0.1f;

    //constants for reactivity
    private const float BASE_CONTAMINATE_DMG = 2f;
    private const float BASE_CON_GROWTH = 2f;
    private const float BASE_STACK_DMG = 1f;
    private const float STACK_DMG_GROWTH = 0.75f;

    //constants for stickiness
    private const float BASE_SLOWNESS = 0.85f;
    private const float SLOWNESS_GROWTH = -0.025f;


    //Pure Constructor
    public PoisonVial(int pot, int poi, int r, int s, Color c, int initialAmmo)
    {
        potency = pot;
        poison = poi;
        reactivity = r;
        stickiness = s;
        poisonColor = c;
        ammo = initialAmmo;
    }

    //Ammo checker: Checks if can use poison given the cost
    public bool CanUsePoison(int cost)
    {
        return cost <= ammo;
    }

    //Method to use poison
    public void UsePoison(int cost)
    {
        ammo -= cost;

        if (ammo <= 0)
        {
            poisonColor = Color.black;
        }
    }

    //Accessor variable to damage
    public float GetDamage()
    {
        return BASE_DAMAGE + (DMG_GROWTH * potency);
    }

    //Accessor variable to Poison damage
    public float GetPoisonDmg()
    {
        return BASE_POISON + (POISON_GROWTH * poison);
    }

    //Method to calculate contaminate damage, given that you have this many stacks
    public float GetContaminateDmg(int stacks)
    {
        float curBaseDmg = BASE_CONTAMINATE_DMG + (BASE_CON_GROWTH * reactivity);
        float stackDmg = BASE_STACK_DMG + (STACK_DMG_GROWTH * reactivity);

        return curBaseDmg + (stackDmg * stacks);
    }

    //Method to get this poison's CC stats
    public float GetSlowFactor()
    {
        return (stickiness > 0) ? BASE_SLOWNESS + (SLOWNESS_GROWTH * (stickiness - 1)) : 1.0f;
    }

    //Accessor method to get color
    public Color GetColor()
    {
        return poisonColor;
    }

}
