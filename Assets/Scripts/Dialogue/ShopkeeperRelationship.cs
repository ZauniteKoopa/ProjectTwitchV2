using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopkeeperRelationship : RelationshipManager
{
    //Dictionary that consist of shop keeper stuff
    private Dictionary<Ingredient.IngredientType, DialogueEvent> ingredientShopEvents = null;
    private DialogueEvent healthPackEvent = null;
    private DialogueEvent boughtOutEvent = null;
    private DialogueEvent bestShopEvent = null;
    int stuffBought = 0;

    private const int NUM_OBJECTS_IN_STORE = 4;


    //Overriden method to initialize data structures for this element
    protected override void InitReqDicts()
    {
        base.InitReqDicts();
        ingredientShopEvents = new Dictionary<Ingredient.IngredientType, DialogueEvent>();
    }

    //Override method to parse segments
    protected override void ParseEventSegment(DialogueEvent curEvent, string reqType, string reqInfo)
    {
        base.ParseEventSegment(curEvent, reqType, reqInfo);

        if (reqType == "b")             //Req type == buy ingredient
        {
            Ingredient.IngredientType ingType = Ingredient.ParseIngredientType(reqInfo);
            ingredientShopEvents.Add(ingType, curEvent);
        }
        else if (reqType == "h")        //Req type == health pack
        {
            healthPackEvent = curEvent;
        }
        else if (reqType == "bo")       //Req type == all bought out
        {
            boughtOutEvent = curEvent;
        }
    }


    //Event method when someone buys an ingredient from the store
    public void OnBuyIngredient(Ingredient.IngredientType ing)
    {
        stuffBought++;

        if (ingredientShopEvents.ContainsKey(ing))
        {
            DialogueEvent curEvent = ingredientShopEvents[ing];

            if (bestShopEvent == null || curEvent.IsHigherPriority(bestShopEvent))
                bestShopEvent = curEvent;
        }

        CheckBoughtOut(); 
    }

    //Event method when player buys a health pack from the store
    public void OnBuyHealthPack()
    {
        stuffBought++;

        if (healthPackEvent != null)
        {
            if (bestShopEvent == null || healthPackEvent.IsHigherPriority(bestShopEvent))
                bestShopEvent = healthPackEvent;
        }

        CheckBoughtOut();
    }


    //Private helper method to check if everything is bought out
    private void CheckBoughtOut()
    {
        if (boughtOutEvent != null)
        {
            if (stuffBought >= NUM_OBJECTS_IN_STORE && (bestShopEvent == null || boughtOutEvent.IsHigherPriority(bestShopEvent)))
            {
                bestShopEvent = boughtOutEvent;
            }
        }
    }

    //Override method of Getting best dialogue event
    protected override DialogueEvent GetBestEvent()
    {
        DialogueEvent bestEvent = base.GetBestEvent();

        //Check if there's a best shop event to contest this
        if (bestShopEvent != null)
        {
            if (bestEvent == null || bestShopEvent.IsHigherPriority(bestEvent))
                bestEvent = bestShopEvent;
        }

        return bestEvent;
    }

    //Method meant to reset everything
    public void ResetShop()
    {
        bestShopEvent = null;
        stuffBought = 0;
    }

    
}
