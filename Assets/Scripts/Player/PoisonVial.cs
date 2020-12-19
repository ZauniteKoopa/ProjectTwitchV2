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
    private const int MAX_STAT = 5;

    //Ammo system
    private int ammo;
    private const int BEGIN_AMMO = 15;

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

    //Updates log kept for information purposes
    private int[] updateLog;


    //Pure Constructor
    public PoisonVial(int pot, int poi, int r, int s, Color c, int initialAmmo)
    {
        potency = pot;
        poison = poi;
        reactivity = r;
        stickiness = s;
        poisonColor = c;
        ammo = initialAmmo;

        updateLog = new int[4];
    }

    //Constructor to make PoisonVial from scratch from a list of ingredients
    public PoisonVial(List<Ingredient> ingredients)
    {
        ammo = BEGIN_AMMO;
        potency = 0;
        poison = 0;
        reactivity = 0;
        stickiness = 0;
        poisonColor = new Color(Random.Range(0.2f, 1f), Random.Range(0.2f, 1f), Random.Range(0.2f, 1f), 1f);

        updateLog = new int[4];
        UpgradeVial(ingredients);
    }

    //Method to upgrade a single poison from a list of ingredients
    public void UpgradeVial(List<Ingredient> ingredients)
    {
        //Clear UpgradeLog
        for(int i = 0; i < updateLog.Length; i++)
            updateLog[i] = 0;

        //Upgrade vial 1 ingredient at a time
        for (int i = 0; i < ingredients.Count; i++)
        {
            List<Ingredient.StatType> upgrades = ingredients[i].GetStatUpgrades();
            for (int upgrade = 0; upgrade < upgrades.Count; upgrade++)
            {
                UpgradeStat(upgrades[upgrade]);
            }

            ammo += ingredients[i].GetAmmoOffered();
        }
    }

    //Helper method that upgrades a stat based on input
    private void UpgradeStat(Ingredient.StatType s)
    {
        //Actually update stat
        if (potency < MAX_STAT && s == Ingredient.StatType.Potency)
            potency++;
        else if (poison < MAX_STAT && s == Ingredient.StatType.Poison)
            poison++;
        else if (reactivity < MAX_STAT && s == Ingredient.StatType.Reactivity)
            reactivity++;
        else if (stickiness < MAX_STAT && s == Ingredient.StatType.Stickiness)
            stickiness++;

        //record in log
        updateLog[(int)s]++;
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

    //Accessor method to ammo
    public int GetAmmo()
    {
        return ammo;
    }

    //ToString method for poison vial for debugging purposes
    public override string ToString()
    {
        return "A vial with potency " + potency + ", poison " + poison + ", reactivity " + reactivity + ", and stickiness " + stickiness;
    }

    //Get Update log information from the latest update. To be used for text popups
    public List<string> GetLatestUpdateInfo()
    {
        List<string> logInfo = new List<string>();

        for (int i = 0; i < updateLog.Length; i++)
        {
            if (updateLog[i] > 0)
            {
                string curStatInfo = (Ingredient.StatType)i + ": +" + updateLog[i];
                logInfo.Add(curStatInfo);
            }
        }

        return logInfo;
    }

    //Access stats information in the form of an int array in enum order
    public int[] GetStats()
    {
        int[] stats = new int[4];
        stats[0] = potency;
        stats[1] = poison;
        stats[2] = reactivity;
        stats[3] = stickiness;

        return stats;
    }

}
