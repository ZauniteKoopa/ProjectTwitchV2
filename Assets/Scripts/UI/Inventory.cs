using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Inventory : MonoBehaviour
{
    //Flag that say if it's active or not
    private bool active;

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
    private IngredientIcon[] ingredientIcons = null;

    //Crafting system
    [Header("Crafting system")]
    [SerializeField]
    private CraftIngredientSlot[] craftSlots = null;
    [SerializeField]
    private Image craftVialSlot = null;
    private PoisonVial craftVial;

    // Awake is called to initialize some variables
    void Awake()
    {
        curHighlight = null;
        craftVial = null;
        active = false;
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

        //Update inventories if necessary
        if (ingredientInv != null)
        {
            int i = 0;
            foreach(KeyValuePair<Ingredient, int> entry in ingredientInv)
            {
                ingredientIcons[i].SetUpIcon(entry.Key, entry.Value);
                i++;
            }
        }
        

        //Actually open up menu and pause game
        gameObject.SetActive(true);
        Time.timeScale = 0.0f;
        StartCoroutine(MenuLoop());
    }

    //Active method loop
    private IEnumerator MenuLoop()
    {
        active = true;

        while(active)
        {
            //Keyboard controls for crafting
            if (Input.GetButtonDown("BoltCraft"))
            {
                if (craftVial != boltVial)
                    SetUpCraftVial(boltVial, boltIcon);
                else
                    ResetCraftVial();
            }
            else if (Input.GetButtonDown("CaskCraft"))
            {
                if (craftVial != caskVial)
                    SetUpCraftVial(caskVial, caskIcon);
                else
                    ResetCraftVial();
            }
            else if (Input.GetButtonDown("ThirdCraft"))
            {
                if (craftVial != thirdVial)
                    SetUpCraftVial(thirdVial, thirdIcon);
                else
                    ResetCraftVial();
            }

            yield return new WaitForSecondsRealtime(0.01f);
        }

        Close();
    }

    //Method used to flag the menu for closing
    public void CallClose()
    {
        active = false;
    }

    //Method to close inventory
    private void Close()
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
        if (ingredientInv != null)
        {
            for(int i = 0; i < ingredientInv.Count; i++)
            {
                ingredientIcons[i].ClearIcon();
            }
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

    // method to reset all crafting
    public void ResetCraft()
    {
        for(int i = 0; i < craftSlots.Length; i++)
        {
            craftSlots[i].Reset();
        }

        ResetCraftVial();
    }

    //Method to actually craft using the materials in the crafting section
    public void CraftPoison()
    {
        //Find what icon needs to be updated: if craftVial is null --> no icon was selected beforehand. Choose the next free one
        //If there are no free icons, you can't craft
        VialIcon updatedIcon = null;
        if (craftVial != null)
        {
            updatedIcon = (craftVial == boltVial) ? boltIcon : updatedIcon;
            updatedIcon = (craftVial == caskVial) ? caskIcon : updatedIcon;
            updatedIcon = (craftVial == thirdVial) ? thirdIcon : updatedIcon;
        }
        else
        {
            updatedIcon = (boltVial == null) ? boltIcon : updatedIcon;
            updatedIcon = (caskVial == null) ? caskIcon : updatedIcon;
            updatedIcon = (thirdVial == null) ? thirdIcon : updatedIcon;
        }

        //If slot was found, continue with the crafting process
        if (updatedIcon != null)
        {
            //Get all ingredients from the slots
            List<Ingredient> ingredientList = new List<Ingredient>();

            for(int i = 0; i < craftSlots.Length; i++)
            {
                Ingredient ing = craftSlots[i].GetIngredient();

                if (ing != null)
                {
                    ingredientList.Add(ing);
                    ingredientInv[ing]--;

                    //If that was the last ingredient of that type, remove it from dictionary
                    if (ingredientInv[ing] == 0)
                    {
                        ingredientInv.Remove(ing);
                    }
                }

                craftSlots[i].CraftIngredient();
            }

            //Actually craft the poison if there was actually ingredients
            if (ingredientList.Count > 0)
            {
                //Get poison vial to update icon with
                if (craftVial == null)
                    craftVial = new PoisonVial(ingredientList);
                else
                    craftVial.UpgradeVial(ingredientList);

                //Update display information and craft
                UpdateVialDisplayInfo(updatedIcon, craftVial);
                ResetCraft();
            }
            else
            {
                Debug.Log("NO INGREDIENTS DETECTED");
            }
        }
        else
        {
            Debug.Log("NO OPEN SLOTS AVAILABLE");
        }
    }


    //Helper method to update vial information in the UI
    private void UpdateVialDisplayInfo(VialIcon icon, PoisonVial newVial)
    {
        if (icon == boltIcon)
        {
            boltVial = newVial;
            boltIcon.SetUpVial(newVial);
            ShowBoltVial();
        }
        else if (icon == caskIcon)
        {
            caskVial = newVial;
            caskIcon.SetUpVial(caskVial);
            ShowCaskVial();
        }
        else if(icon == thirdIcon)
        {
            thirdVial = newVial;
            thirdIcon.SetUpVial(thirdVial);
            ShowThirdVial();
        }
    }


    //Method for setting up Crafting Vial Slot
    private void SetUpCraftVial(PoisonVial vial, VialIcon vialIcon)
    {
        craftVial = vial;
        craftVialSlot.sprite = vialIcon.GetSprite();
        craftVialSlot.color = vial.GetColor();
    }

    //Method to reset Crafting vial slot
    private void ResetCraftVial()
    {
        craftVial = null;
        craftVialSlot.color = Color.black;
        craftVialSlot.sprite = null;
    }

    //Method that returns an array of PoisonVials used to help update info in player
    //  Post: returns an array in this order {Bolt, Cask, Third}
    public PoisonVial[] GetVials()
    {
        PoisonVial[] vials = new PoisonVial[3];
        vials[0] = boltVial;
        vials[1] = caskVial;
        vials[2] = thirdVial;

        return vials;
    }

}
