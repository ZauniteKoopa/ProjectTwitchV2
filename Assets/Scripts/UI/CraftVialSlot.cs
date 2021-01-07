using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class VialSelectDelegate : UnityEvent<PoisonVial> {}

public class CraftVialSlot : MonoBehaviour, IDropHandler, IPointerDownHandler
{
    //Reference variable to update
    private PoisonVial vial = null;

    [SerializeField]
    private Image vialSlot = null;

    //Event method for selection
    public VialSelectDelegate OnCraftVialSelect;

    //On awake, set up event
    void Awake()
    {
        OnCraftVialSelect = new VialSelectDelegate();
    }

    //Public method to set Craft Vial slot to this craft vial
    public void SetUpCraftVial(PoisonVial pv, VialIcon ui)
    {
        vial = pv;
        vialSlot.sprite = ui.GetSprite();
        vialSlot.color = pv.GetColor();
    }

    //Public method to reset this craft slot
    public void Reset()
    {
        vial = null;
        vialSlot.color = Color.black;
        vialSlot.sprite = null;
    }

    //Accessor method to craft vial
    public PoisonVial GetVial()
    {
        return vial;
    }

    //Event handler method for dropping
    //Method to drop Ingredient Icon in this slot
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            VialInventoryIcon vialIcon = eventData.pointerDrag.GetComponent<VialInventoryIcon>();
            if (vialIcon != null && vialIcon.GetVial() != null)
            {
                SetUpCraftVial(vialIcon.GetVial(), vialIcon);
                OnCraftVialSelect.Invoke(vial);
            }
        }
    }

    //Event handler method when clicking on this image
    public void OnPointerDown(PointerEventData eventData)
    {
        if (vial != null)
            OnCraftVialSelect.Invoke(vial);
    }
}
