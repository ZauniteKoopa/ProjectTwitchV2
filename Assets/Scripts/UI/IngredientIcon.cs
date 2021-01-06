using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

[System.Serializable]
public class IngredientSelectDelegate : UnityEvent<Ingredient> {}

public class IngredientIcon : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField]
    private Image icon = null;
    [SerializeField]
    private TMP_Text countText = null;
    [SerializeField]
    private Color emptyColor = Color.clear;
    private Ingredient ingredient = null;
    private int count;

    //Variables for managing drag and drop
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 startPosition;
    [SerializeField]
    private Canvas canvas = null;
    public bool dropped;

    //Event when dragged on
    public IngredientSelectDelegate OnIngredientSelect;
    private const float ICON_SNAPBACK_TIME = 0.1f;

    //On awake, set start position
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        dropped = false;
        OnIngredientSelect = new IngredientSelectDelegate();
    }

    //Method to set up ingredient
    public void SetUpIcon(Ingredient ing, int n)
    {
        startPosition = GetComponent<RectTransform>().anchoredPosition;

        if (ing != null)
        {
            count = n;
            countText.text = "" + n;
            ingredient = ing;
            icon.color = ing.GetColor();
        }
        else
        {
            icon.color = emptyColor;
        }
    }

    //Method to clear icon
    public void ClearIcon()
    {
        icon.color = emptyColor;
        countText.text = "0";
        ingredient = null;
        rectTransform.anchoredPosition = startPosition;
        startPosition = rectTransform.anchoredPosition;
    }

    //Set this ingredient for crafting. THIS DOES NOT USE UP AN INGREDIENT AND DOESN'T CHANGE INVENTORY'S DICTIONARY
    public void SetIngredientForCrafting()
    {
        if (count > 0)
        {
            count--;
            countText.text = "" + count;
            
            if (count == 0)
            {
                icon.color = emptyColor;
                rectTransform.anchoredPosition = startPosition;
            }
        }
    }

    //Method for ingredient icon to get ingredient back from crafting
    public void ReturnIngredient()
    {
        if (count == 0)
        {
            icon.color = ingredient.GetColor();
        }

        count++;
        countText.text = "" + count;
    }

    //Accessor method to ingredient
    public Ingredient GetIngredient()
    {
        return ingredient;
    }

    //Event handler when clicking down on icon
    public void OnPointerDown(PointerEventData eventData)
    {
        if (ingredient != null)
            OnIngredientSelect.Invoke(ingredient);
    }

    //Event handler when beginning to drag
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ingredient != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.6f;
        }
    }

    //Event handler when dragging icon
    public void OnDrag(PointerEventData eventData)
    {
        if (ingredient != null)
        {
            rectTransform.anchoredPosition += (eventData.delta / canvas.scaleFactor);
        }
    }

    //Event handler when dropping icon
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dropped && ingredient != null)
        { 
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
            StartCoroutine(BackToStart());
        }

        dropped = false;
    }

    //Private IEnumerator to go back to start.
    private IEnumerator BackToStart()
    {
        Vector3 curPos = rectTransform.anchoredPosition;
        float timer = 0.0f;
        float delta = 0.02f;

        while(timer < ICON_SNAPBACK_TIME)
        {
            yield return new WaitForSecondsRealtime(delta);
            timer += delta;
            float percent = timer / ICON_SNAPBACK_TIME;
            rectTransform.anchoredPosition = Vector3.Lerp(curPos, startPosition, percent);
        }
    }
}
