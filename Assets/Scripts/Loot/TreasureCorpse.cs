using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureCorpse : AbstractInteractable
{
    //Popup object to use for display
    [SerializeField]
    private Transform treasurePopup = null;
    [SerializeField]
    private int numIngredients = 4;

    //Override method for interact
    protected override IEnumerator Interact(TwitchController player)
    {
        //Open corpse to indicate that it's activated
        GetComponent<SpriteRenderer>().color = Color.black;
        yield return new WaitForSeconds(0.6f);

        //Collect ingredients
        for (int i = 0; i < numIngredients; i++)
        {
            //Get type
            Ingredient.IngredientType type = Ingredient.GetRandomType();
            string popupText = type.ToString();

            //Add to inventory
            player.AddToInventory(new Ingredient(type, 1));

            //Display popup
            Transform curPopup = Object.Instantiate(treasurePopup, player.transform);
            curPopup.GetComponent<TextPopup>().SetUpPopup(popupText);
            yield return new WaitForSeconds(0.4f);
        }

    }
}
