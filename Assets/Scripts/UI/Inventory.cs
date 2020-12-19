using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Inventory : MonoBehaviour
{
    //Reference variables for PoisonVials
    [Header("Vial Displays")]
    [SerializeField]
    private VialIcon boltIcon = null;
    [SerializeField]
    private VialIcon caskIcon = null;
    [SerializeField]
    private VialIcon thirdIcon = null;
    [SerializeField]
    private Image boltHighlight = null;
    [SerializeField]
    private Image caskHighlight = null;
    [SerializeField]
    private Image thirdHighlight = null;
    private Image curHighlight;

    //variables to player's poison vial. MUST BE LOADED UPON OPEN
    private PoisonVial boltVial = null;
    private PoisonVial caskVial = null;
    private PoisonVial thirdVial = null;

    //Variables to display information about poison vials
    [Header("Vial Information")]
    [SerializeField]
    private TMP_Text potencyText = null;
    [SerializeField]
    private TMP_Text poisonText = null;
    [SerializeField]
    private TMP_Text reactivityText = null;
    [SerializeField]
    private TMP_Text stickinessText = null;

    //Ingredient inventory to manage
    Dictionary<Ingredient, int> ingredientInv;
    [Header("Ingredient Inventory")]
    [SerializeField]
    private Color emptyColor = Color.clear;
    [SerializeField]
    private Image[] ingredientIcons = null;
    [SerializeField]
    private TMP_Text[] ingredientCounts = null;
    [SerializeField]
    private Color[] inventorySprites = null;


    // Awake is called to initialize some variables
    void Awake()
    {
        curHighlight = null;
    }


    //Method to add to dictionary
    public void AddIngredient(Ingredient ing)
    {
        if (ingredientInv == null)
            ingredientInv = new Dictionary<Ingredient, int>();

        if (ingredientInv.ContainsKey(ing))
        {
            ingredientInv[ing]++;
        }
        else
        {
            ingredientInv.Add(ing, 1);
        }
    }


    //Method to open inventory with specific vials
    public void Open(PoisonVial bv, PoisonVial cv, PoisonVial tv)
    {
        //Set up poison vials
        boltVial = bv;
        caskVial = cv;
        thirdVial = tv;

        //Set up UI icons
        boltIcon.SetUpVial(bv);
        caskIcon.SetUpVial(cv);
        thirdIcon.SetUpVial(tv);

        //Update inventories
        int i = 0;
        foreach(KeyValuePair<Ingredient, int> entry in ingredientInv)
        {
            ingredientIcons[i].color = inventorySprites[entry.Key.GetHashCode()];
            ingredientCounts[i].text = "" + entry.Value;
            i++;
        }

        //Actually open up menu and pause game
        gameObject.SetActive(true);
        Time.timeScale = 0.0f;
    }


    //Method to close inventory
    public void Close()
    {
        //Set all vials to null
        boltVial = null;
        caskVial = null;
        thirdVial = null;

        //Set highlights to null if any
        if (curHighlight != null)
        {
            curHighlight.enabled = false;
            curHighlight = null;
        }

        //clear out vial information
        ClearVialInfo();

        //Clear out ingredient information
        for(int i = 0; i < ingredientInv.Count; i++)
        {
            ingredientIcons[i].color = emptyColor;
            ingredientCounts[i].text = "0";
        }

        //Close the menu
        gameObject.SetActive(false);
        Time.timeScale = 1.0f;
    }


    //Method to show bolt Vial
    public void ShowBoltVial()
    {
        if (curHighlight != null)
            curHighlight.enabled = false;
        
        boltHighlight.enabled = true;
        curHighlight = boltHighlight;
        DisplayVialInfo(boltVial);
    }


    //Method to show cask vial
    public void ShowCaskVial()
    {
        if (curHighlight != null)
            curHighlight.enabled = false;
        
        caskHighlight.enabled = true;
        curHighlight = caskHighlight;
        DisplayVialInfo(caskVial);
    }


    //method to show third vial
    public void ShowThirdVial()
    {
        if (curHighlight != null)
            curHighlight.enabled = false;
        
        thirdHighlight.enabled = true;
        curHighlight = thirdHighlight;
        DisplayVialInfo(thirdVial);
    }


    //Helper method to display vial information on inventory screen
    private void DisplayVialInfo(PoisonVial vial)
    {
        if (vial != null)
        {
            int[] vialInfo = vial.GetStats();

            potencyText.text = "Potency: " + vialInfo[0];
            poisonText.text = "Poison: " + vialInfo[1];
            reactivityText.text = "Reactivity: " + vialInfo[2];
            stickinessText.text = "Stickiness: "+ vialInfo[3];
        }
        else
        {
            ClearVialInfo();
        }
    }


    //helper method to clear out information
    private void ClearVialInfo()
    {
        potencyText.text = "Potency: 0";
        poisonText.text = "Poison: 0";
        reactivityText.text = "Reactivity: 0";
        stickinessText.text = "Stickiness: 0";
    }

}
