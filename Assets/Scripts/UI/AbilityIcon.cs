using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityIcon : MonoBehaviour
{
    [SerializeField]
    private Image disabled = null;

    //Method to activate move disabled
    public void ShowDisabled()
    {
        disabled.enabled = true;
    }

    //Method to show move is enabled
    public void ShowEnabled()
    {
        disabled.enabled = false;
    }
}
