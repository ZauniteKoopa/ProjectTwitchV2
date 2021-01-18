using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ingredient
{
    //Array with booleans to be used, in enum order
    public enum StatType {Potency, Poison, Reactivity, Stickiness, None};
    public enum IngredientType {Puffcap, MutatedHeart, ShimmerOil, WhumpFeces, SewerFlower, RustedSteel, RottenBlood, ChemtechFuel}

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
        Debug.Assert(statAvailability.Length > 0);

        type = t;
        statsOffered = offer;
        availableTypes = new List<StatType>();
        ingColor = c;

        for(int i = 0; i < statAvailability.Length; i++)
        {
            if (statAvailability[i])
                availableTypes.Add((StatType)i);
        }
    }

    //Method that creates an ingredient from just ingredient type alone
    public Ingredient(IngredientType t, int offer)
    {
        type = t;
        statsOffered = offer;

        //Switch based on what type
        switch (t)
        {
            case IngredientType.Puffcap:
                ConstructorHelper(new bool[] {true, true, false, false}, new Color(0.6f, 0f, 1f));
                break;
            
            case IngredientType.MutatedHeart:
                ConstructorHelper(new bool[] {true, false, false, true}, new Color(0.46f, 0.0f, 0.0f));
                break;
            
            case IngredientType.ShimmerOil:
                ConstructorHelper(new bool[] {false, false, true, true}, new Color(0.9f, 0.8f, 0.1f));
                break;
            
            case IngredientType.WhumpFeces:
                ConstructorHelper(new bool[] {false, true, false, true}, new Color(0.5f, 0.35f, 0f));
                break;

            case IngredientType.SewerFlower:
                ConstructorHelper(new bool[] {false, true, true, false}, new Color(1f, 0f, 0.75f));
                break;

            case IngredientType.RustedSteel:
                ConstructorHelper(new bool[] {false, false, true, true}, new Color(0.7f, 0.25f, 0f));
                break;

            case IngredientType.RottenBlood:
                ConstructorHelper(new bool[] {true, true, false, false}, new Color(1f, 0f, 0f));
                break;

            case IngredientType.ChemtechFuel:
                ConstructorHelper(new bool[] {true, false, true, false}, new Color(0f, 1f, 0f));
                break;
            
            default:
                throw new System.Exception("Unidentified ingredient type");
        }
    }


    //Public static method for getting a random ingredient type
    public static IngredientType GetRandomType()
    {
        int select = Random.Range((int)IngredientType.Puffcap, (int)IngredientType.ChemtechFuel + 1);
        return (IngredientType)select;
    }


    //Private helper method for ingredient only constructor
    private void ConstructorHelper (bool[] statAvailability, Color c)
    {
        Debug.Assert(statAvailability.Length > 0);

        availableTypes = new List<StatType>();
        ingColor = c;

        for(int i = 0; i < statAvailability.Length; i++)
        {
            if (statAvailability[i])
                availableTypes.Add((StatType)i);
        }
    }

    //Method that returns a list of stats for poison to use when upgrading
    //  Pre: int[] is an array of the vials stats in StatType enum order
    public List<StatType> GetStatUpgrades(int[] vialStats)
    {
        List<StatType> upgrades = new List<StatType>();
        HashSet<StatType> disabledTypes = new HashSet<StatType>();
        
        for(int i = 0; i < statsOffered; i++)
        {
            if (disabledTypes.Count < availableTypes.Count)
            {
                //Randomly choose a selected index
                int selectedIndex = Random.Range(0, availableTypes.Count);
                StatType selectedType = availableTypes[selectedIndex];

                //If selectedIndex of vialStat is equal to max stat, go to next index
                while (disabledTypes.Count < availableTypes.Count && vialStats[(int)selectedType] == PoisonVial.MAX_STAT)
                {
                    disabledTypes.Add(selectedType);
                    selectedIndex = (selectedIndex + 1) % availableTypes.Count;
                    selectedType = availableTypes[selectedIndex];
                }

                //Add that to list of upgrades
                upgrades.Add(availableTypes[selectedIndex]);
            }
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

    //Method to get string name
    public string GetName()
    {
        return type.ToString();
    }

    //Method to get available types
    public List<string> GetUpgrades()
    {
        List<string> upgrades = new List<string>();

        for (int i = 0; i < availableTypes.Count; i++)
        {
            upgrades.Add(availableTypes[i].ToString());
        }

        return upgrades;
    }
}
