using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VialIcon : AbilityIcon
{
    //UI reference variables
    [SerializeField]
    private Image vialDisplay = null;
    [SerializeField]
    private TMP_Text ammoDisplay = null;
    private PoisonVial vial = null;

    //Method to set up poison vial
    public void SetUpVial(PoisonVial pv)
    {
        vial = pv;

        if (pv == null)
        {
            ShowDisabled();
            ammoDisplay.text = "0";
        }
        else
        {
            ShowEnabled();
            vialDisplay.color = pv.GetColor();
            ammoDisplay.text = "" + pv.GetAmmo();
        }
    }

    //Method to update vial's ammo count.
    public void UpdateVial()
    {
        if (vial != null)
        {
            int vialAmmo = vial.GetAmmo();

            if (vialAmmo > 0)
            {
                ammoDisplay.text = "" + vial.GetAmmo();
            }
            else
            {
                vial = null;
                ammoDisplay.text = "0";
                ShowDisabled();
            }
        }
    }

    //Method to access the UI sprite for this icon
    public Sprite GetSprite()
    {
        return vialDisplay.sprite;
    }

    //Method to access poison vial
    public PoisonVial GetVial()
    {
        return vial;
    }
}
