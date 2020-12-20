using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ingredient
{
    //Array with booleans to be used, in enum order
    public enum StatType {Potency, Poison, Reactivity, Stickiness};
    public enum IngredientType {Puffcap, MutatedHeart, ShimmerOil, WhumpFeces}

    public IngredientType type;
    private List<StatType> availableTypes;
    private const int STAT_SIZE = 4;

    //Method that shows how many stats this ingredient offers
    private int statsOffered;
    private const int AMMO_OFFERED = 5;

    //Variable used for display
    private Color ingColor;

    //Gives a randomized ingredient with a given offer rate
    public Ingredient(IngredientType t, int offer, Color c)
    {
        type = t;
        statsOffered = offer;
        availableTypes = new List<StatType>();

        for(StatType i = StatType.Potency; i <= StatType.Stickiness; i++)
        {
            if (Random.Range(0, 2) == 0)
                availableTypes.Add(i);
        }
    }

    //Randomized ingredient with specified stat buff chances in enum order
    public Ingredient(IngredientType t, bool[] statAvailability, int offer, Color c)
    {
        type = t;
        statsOffered = offer;
        availableTypes = new List<StatType>();
        ingColor = c;

        if (statAvailability.Length == 0)
            Debug.Log("INVALID INGREDIENT STAT ARRAY. PLEASE FIX IT");

        for(int i = 0; i < statAvailability.Length; i++)
        {
            if (statAvailability[i])
                availableTypes.Add((StatType)i);
        }
    }

    //Method that returns a list of stats for poison to use when upgrading
    public List<StatType> GetStatUpgrades()
    {
        List<StatType> upgrades = new List<StatType>();
        
        for(int i = 0; i < statsOffered; i++)
        {
            int selectedIndex = Random.Range(0, availableTypes.Count);
            upgrades.Add(availableTypes[selectedIndex]);
        }

        return upgrades;
    }

    //Method to get how much ammo offered
    public int GetAmmoOffered()
    {
        return AMMO_OFFERED;
    }

    //Method to get hashcode according to type, not to pointer location
    public override int GetHashCode()
    {
        return (int)type;
    }

    //Method to check equality
    public override bool Equals(object other)
    {
        Ingredient ing = other as Ingredient;

        if (ing == null)
            return false;
        
        return type == ing.type;
    }

    //Method to get display for icons
    public Color GetColor()
    {
        return ingColor;
    }
}
