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
                Debug.Log("DO SOMETHING!");
            }
        }
    }

    //Accessor method to ingredient
    public Ingredient GetIngredient()
    {
        return ingredientIcon.GetIngredient();
    }
}
