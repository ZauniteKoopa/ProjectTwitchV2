using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
    private int totalStat;
    public const int MAX_STAT = 5;
    private const int TOTAL_STAT_LIMIT = 10;
    private const int MAX_AMMO = 60;

    //Ammo system
    private int ammo;
    private const int BEGIN_AMMO = 30;

    //Constants for damage/potency
    private const float BASE_DAMAGE = 2f;
    private const float DMG_GROWTH = 0.65f;

    //constants for poison
    private const float BASE_POISON = 0f;
    private const float POISON_GROWTH = 0.15f;

    //constants for reactivity
    private const float BASE_CONTAMINATE_DMG = 3f;
    private const float BASE_CON_GROWTH = 0.5f;
    private const float BASE_STACK_DMG = 1f;
    private const float STACK_DMG_GROWTH = 0.25f;

    //constants for stickiness
    private const float BASE_SLOWNESS = 0.85f;
    private const float SLOWNESS_GROWTH = -0.025f;

    //Side effects variables
    //  Enum: 0 = none, 1-3 = potency, 4-6 = poison, 7-9 = reactivity, 10-12 = stickiness
    public enum SideEffect
    {
        [Description("???")]
        NONE,
        [Description("Rat-ta-tat-tat")]
        PIERCING_SHOT,
        [Description("Acid Spill")]
        ACID_SPILL,
        [Description("Spray and Pray")]
        SPRAY_PRAY,
        [Description("Open Infection")]
        OPEN_INFECTION,
        [Description("Poison Fog")]
        POISON_FOG,
        [Description("Greater Decay")]
        GREATER_DECAY,
        [Description("Noxious Explosion")]
        NOXIOUS_EXPLOSION,
        [Description("Combustion Blast")]
        COMBUSTION_BLAST,
        [Description("Death Mark")]
        DEATH_MARK,
        [Description("Slime Bomb")]
        SLIME_BOMB,
        [Description("Slime Leak")]
        SLIME_LEAK,
        [Description("Induced Paralysis")]
        INDUCED_PARALYSIS
    }

    private SideEffect sideEffect;
    private const int SIDE_EFFECT_REQ = 3;
    private const int EFFECTS_PER_TYPE = 3;
    private bool upgradedToSideEffect = false;

    //Updates log kept for information purposes
    private int[] updateLog;


    //Pure Constructor with no side effects
    public PoisonVial(int pot, int poi, int r, int s, Color c, int initialAmmo)
    {
        potency = pot;
        poison = poi;
        reactivity = r;
        stickiness = s;
        totalStat = pot + poi + r + s;

        poisonColor = c;
        ammo = initialAmmo;
        sideEffect = SideEffect.NONE;

        updateLog = new int[4];
    }

    //Pure constructor with side effects
    public PoisonVial(int pot, int poi, int r, int s, Color c, int initialAmmo, SideEffect effect)
    {
        potency = pot;
        poison = poi;
        reactivity = r;
        stickiness = s;
        totalStat = pot + poi + r + s;

        poisonColor = c;
        ammo = initialAmmo;
        sideEffect = effect;

        updateLog = new int[4];
    }

    //Constructor to make PoisonVial from scratch from a list of ingredients
    public PoisonVial(List<Ingredient> ingredients, int randomBonus)
    {
        sideEffect = SideEffect.NONE;
        ammo = BEGIN_AMMO;
        potency = 0;
        poison = 0;
        reactivity = 0;
        stickiness = 0;
        totalStat = 0;

        poisonColor = new Color(Random.Range(0.2f, 1f), Random.Range(0.2f, 1f), Random.Range(0.2f, 1f), 1f);

        updateLog = new int[4];
        UpgradeVial(ingredients, randomBonus);
    }

    //Method to upgrade a single poison from a list of ingredients
    public void UpgradeVial(List<Ingredient> ingredients, int randomBonus)
    {
        upgradedToSideEffect = false;

        //Clear UpgradeLog
        for(int i = 0; i < updateLog.Length; i++)
            updateLog[i] = 0;

        //Make a list for side effect generation if no side effect found
        List<Ingredient.StatType> potentialEffects = null;
        if (sideEffect == SideEffect.NONE)
            potentialEffects = new List<Ingredient.StatType>();

        //Upgrade vial 1 ingredient at a time. If side effect upgrade detected, add to potential effects
        for (int i = 0; i < ingredients.Count; i++)
        {
            List<Ingredient.StatType> upgrades = ingredients[i].GetStatUpgrades(GetStats());
            for (int upgrade = 0; upgrade < upgrades.Count; upgrade++)
            {
                bool sideEffect = UpgradeStat(upgrades[upgrade], randomBonus);

                if (potentialEffects != null && sideEffect)
                    potentialEffects.Add(upgrades[upgrade]);
            }

            ammo += ingredients[i].GetAmmoOffered();
        }

        ammo = Mathf.Min(ammo, MAX_AMMO);

        //If side effected detected, give a random side effect from the list
        if (potentialEffects != null && potentialEffects.Count > 0)
        {
            Ingredient.StatType sideEffectType = potentialEffects[Random.Range(0, potentialEffects.Count)];
            sideEffect = GiveSideEffect(sideEffectType);
            upgradedToSideEffect = true;
        }
    }

    //Helper method that upgrades a stat based on input
    //  Returns true if side effect requirement is met with this stat ON THIS TURN
    //  An upgrade for a specific stat type should never return twice
    private bool UpgradeStat(Ingredient.StatType s, int randomBonus)
    {
        int bonus = 1;
        bonus += Random.Range(0, randomBonus + 1);
        int beforeStat = 0;
        int upgradedStat = 0;

        //Actually update stat
        if (potency < MAX_STAT && s == Ingredient.StatType.Potency)
        {
            beforeStat = potency;
            potency = Mathf.Min(potency + bonus, MAX_STAT);
            upgradedStat = potency;
        }
        else if (poison < MAX_STAT && s == Ingredient.StatType.Poison)
        {
            beforeStat = poison;
            poison = Mathf.Min(poison + bonus, MAX_STAT);
            upgradedStat = poison;
        }
        else if (reactivity < MAX_STAT && s == Ingredient.StatType.Reactivity)
        {
            beforeStat = reactivity;
            reactivity = Mathf.Min(reactivity + bonus, MAX_STAT);
            upgradedStat = reactivity;
        }
        else if (stickiness < MAX_STAT && s == Ingredient.StatType.Stickiness)
        {
            beforeStat = stickiness;
            stickiness = Mathf.Min(stickiness + bonus, MAX_STAT);
            upgradedStat = stickiness;
        }

        //record in log and get total stats
        updateLog[(int)s] += bonus;
        totalStat += (upgradedStat - beforeStat);
        return upgradedStat >= SIDE_EFFECT_REQ && beforeStat < SIDE_EFFECT_REQ;
    }


    //Private helper method to give this vial a side effect if need be
    //  Pre: This vial should not have a side effect before hand
    private SideEffect GiveSideEffect(Ingredient.StatType type)
    {
        int typeIndex = (int)type;

        int beginRand = (EFFECTS_PER_TYPE * typeIndex) + 1;
        int endRand = EFFECTS_PER_TYPE * (typeIndex + 1);
        int selected = Random.Range(beginRand, endRand + 1);

        return (SideEffect)selected;
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

        if (upgradedToSideEffect)
            logInfo.Add(sideEffect.GetDescription());

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

    // --------
    // Side effect methods
    // --------

    //Access side effect name (TO BE CHANGED)
    public string GetSideEffectName()
    {
        return sideEffect.GetDescription();
    }

    //Access side effect enum
    public SideEffect GetSideEffect()
    {
        return sideEffect;
    }

    //Accessor method to side effect level
    //  If no side effect, return 0
    public int GetSideEffectLevel()
    {
        if (sideEffect == SideEffect.NONE)
            return 0;
        
        int effectType = ((int)sideEffect - 1) / EFFECTS_PER_TYPE;
        int lvl = 2;
        
        if (effectType == 0)
            lvl = potency;
        else if (effectType == 1)
            lvl = poison;
        else if (effectType == 2)
            lvl = reactivity;
        else if (effectType == 3)
            lvl = stickiness;

        return lvl - 2;
    }

    //Get vial side effect specialization
    public Ingredient.StatType GetSpecialization()
    {
        if (sideEffect == SideEffect.NONE)
            return Ingredient.StatType.None;

        int effectType = (int)sideEffect - 1;
        effectType /= EFFECTS_PER_TYPE;

        return (Ingredient.StatType)effectType;
    }


    //Method to check if this vial is upgradable or not
    public bool IsUpgradable()
    {
        return totalStat < TOTAL_STAT_LIMIT;
    }

    //Accessor method to get stat total
    public float GetStatTotal()
    {
        return totalStat;
    }
}
