﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class IngredientIcon : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField]
    private Image icon = null;
    [SerializeField]
    private TMP_Text count = null;
    [SerializeField]
    private Color emptyColor = Color.clear;
    private Ingredient ingredient = null;

    //Variables for managing drag and drop
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 startPosition;
    [SerializeField]
    private Canvas canvas = null;
    public bool dropped;

    private const float ICON_SNAPBACK_TIME = 0.1f;

    //On awake, set start position
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        dropped = false;
    }

    //Method to set up ingredient
    public void SetUpIcon(Ingredient ing, int n)
    {
        startPosition = GetComponent<RectTransform>().anchoredPosition;

        if (ing != null)
        {
            count.text = "" + n;
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
        count.text = "0";
        ingredient = null;
        rectTransform.anchoredPosition = startPosition;
        startPosition = rectTransform.anchoredPosition;
    }

    //Accessor method to ingredient
    public Ingredient GetIngredient()
    {
        return ingredient;
    }

    //Event handler when clicking down on icon
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("On Pointer down");
    }

    //Event handler when beginning to drag
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("On begin drag");

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
        Debug.Log("On end drag");

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