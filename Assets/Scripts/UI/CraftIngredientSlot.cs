using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CraftIngredientSlot : MonoBehaviour, IDropHandler
{
    //UI elements
    [SerializeField]
    private Image icon = null;
    [SerializeField]
    private Color emptyColor = Color.clear;
    private IngredientIcon ingredientIcon;

    // Start is called before the first frame update
    void Awake()
    {
        ingredientIcon = null;
    }

    //Method to drop Ingredient Icon in this slot
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            IngredientIcon ingIcon = eventData.pointerDrag.GetComponent<IngredientIcon>();
            if (ingIcon != null && ingIcon.GetIngredient() != null)
            {
                //if there's an ingredient icon already in here, give an ingredient back to that icon
                if (ingredientIcon != null)
                {
                    ingredientIcon.ReturnIngredient();
                }

                ingredientIcon = ingIcon;
                ingIcon.SetIngredientForCrafting();
                icon.color = ingIcon.GetIngredient().GetColor();
            }
        }
    }

    //Accessor method to ingredient
    public Ingredient GetIngredient()
    {
        if (ingredientIcon == null)
            return null;
        
        return ingredientIcon.GetIngredient();
    }

    //Method to reset all this slot
    public void Reset()
    {
        if (ingredientIcon != null)
            ingredientIcon.ReturnIngredient();
        
        ingredientIcon = null;
        icon.color = emptyColor;
    }

    //Method to craft ingredient in a visual level
    public void CraftIngredient()
    {
        ingredientIcon = null;
        Reset();
    }
}
