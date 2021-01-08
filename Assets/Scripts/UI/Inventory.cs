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

    //Index display
    private enum DisplayVialEnum {None, Primary, Secondary, Third};
    private DisplayVialEnum displayVialIndex;

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
    [SerializeField]
    private TMP_Text sideEffectName = null;
    [SerializeField]
    private Color specializedColor = Color.black;
    [SerializeField]
    private Color basicColor = Color.black;

    //Ingredient inventory to manage
    Dictionary<Ingredient, int> ingredientInv;
    [Header("Ingredient Inventory")]
    [SerializeField]
    private IngredientIcon[] ingredientIcons = null;
    private int numIngTypes = 0;

    //Ingredient display information
    [SerializeField]
    private TMP_Text ingredientName = null;
    [SerializeField]
    private TMP_Text ingredientDescription = null;

    //Crafting system
    [Header("Crafting system")]
    [SerializeField]
    private CraftIngredientSlot[] craftSlots = null;
    [SerializeField]
    private CraftVialSlot craftVialSlot = null;
    [SerializeField]
    private TMP_Text craftingWarning = null;

    // Awake is called to initialize some variables
    void Awake()
    {
        curHighlight = null;
        active = false;
        displayVialIndex = DisplayVialEnum.None;
    }

    //On start listen to all icons
    void Start()
    {
        for (int i = 0; i < ingredientIcons.Length; i++)
            ingredientIcons[i].OnIngredientSelect.AddListener(UpdateIngredientInfo);

        for (int i = 0; i < craftSlots.Length; i++)
            craftSlots[i].OnIngredientSelect.AddListener(UpdateIngredientInfo);

        craftVialSlot.OnCraftVialSelect.AddListener(OnCraftVialSelect);
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
        displayVialIndex = DisplayVialEnum.None;

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

            numIngTypes = ingredientInv.Count;
        }
        
        ClearIngredientInfo();

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
            yield return new WaitForSecondsRealtime(0.01f);

            //Keyboard controls for crafting
            if (Input.GetButtonDown("BoltCraft"))
            {
                if (craftVialSlot.GetVial() != boltVial)
                    craftVialSlot.SetUpCraftVial(boltVial, boltIcon);
                else
                    craftVialSlot.Reset();
                Debug.Log("button pushed");
            }
            else if (Input.GetButtonDown("CaskCraft"))
            {
                if (craftVialSlot.GetVial() != caskVial)
                    craftVialSlot.SetUpCraftVial(caskVial, caskIcon);
                else
                    craftVialSlot.Reset();
            }
            else if (Input.GetButtonDown("ThirdCraft"))
            {
                if (craftVialSlot.GetVial() != thirdVial)
                    craftVialSlot.SetUpCraftVial(thirdVial, thirdIcon);
                else
                    craftVialSlot.Reset();
            }
            else if (Input.GetButtonDown("Inventory"))
            {
                CallClose();
            }

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
            for(int i = 0; i < numIngTypes; i++)
            {
                ingredientIcons[i].ClearIcon();
            }
        }

        numIngTypes = 0;

        //Reset craft
        ResetCraft();

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
        displayVialIndex = DisplayVialEnum.Primary;
    }


    //Method to show cask vial
    public void ShowCaskVial()
    {
        if (curHighlight != null)
            curHighlight.enabled = false;
        
        caskHighlight.enabled = true;
        curHighlight = caskHighlight;
        DisplayVialInfo(caskVial);
        displayVialIndex = DisplayVialEnum.Secondary;
    }


    //method to show third vial
    public void ShowThirdVial()
    {
        if (curHighlight != null)
            curHighlight.enabled = false;
        
        thirdHighlight.enabled = true;
        curHighlight = thirdHighlight;
        DisplayVialInfo(thirdVial);
        displayVialIndex = DisplayVialEnum.Third;
    }


    //Signal handler method to handle ingredient selection to display info
    public void UpdateIngredientInfo(Ingredient ing)
    {
        if (ing != null)
        {
            ingredientName.text = "Ingredient - " + ing.GetName() + ":";
            List<string> upgrades = ing.GetUpgrades();
            ingredientDescription.text = "";

            for (int i = 0; i < upgrades.Count; i++)
            {
                ingredientDescription.text += upgrades[i];
                ingredientDescription.text += "\n";
            }
        }
    }

    //Method to clear out ingredient info
    void ClearIngredientInfo()
    {
        ingredientName.text = "Ingredient - ???:";
        ingredientDescription.text = "";
    }


    //Signal handler method for when CraftVial is selected
    public void OnCraftVialSelect(PoisonVial vial)
    {
        if (vial == boltVial)
            ShowBoltVial();
        else if (vial == caskVial)
            ShowCaskVial();
        else if (vial == thirdVial)
            ShowThirdVial();
    }


    //Helper method to display vial information on inventory screen
    private void DisplayVialInfo(PoisonVial vial)
    {
        if (vial != null)
        {
            int[] vialInfo = vial.GetStats();
            Ingredient.StatType s = vial.GetSpecialization();

            DisplayStat(potencyText, Ingredient.StatType.Potency, vialInfo[0], s);
            DisplayStat(poisonText, Ingredient.StatType.Poison, vialInfo[1], s);
            DisplayStat(reactivityText, Ingredient.StatType.Reactivity, vialInfo[2], s);
            DisplayStat(stickinessText, Ingredient.StatType.Stickiness, vialInfo[3], s);

            sideEffectName.text = "Side Effect - " + vial.GetSideEffectName() + ":";
        }
        else
        {
            ClearVialInfo();
        }
    }

    //Private helper method for DisplayVialInfo to set texts for stats
    private void DisplayStat(TMP_Text display, Ingredient.StatType type, int typeValue, Ingredient.StatType specialization)
    {
        display.text = type.ToString() + ": " + typeValue;
        display.color = (type == specialization) ? specializedColor : basicColor;
    }


    //helper method to clear out information
    private void ClearVialInfo()
    {
        displayVialIndex = DisplayVialEnum.None;

        potencyText.text = "Potency: 0";
        potencyText.color = basicColor;

        poisonText.text = "Poison: 0";
        poisonText.color = basicColor;

        reactivityText.text = "Reactivity: 0";
        reactivityText.color = basicColor;

        stickinessText.text = "Stickiness: 0";
        stickinessText.color = basicColor;

        sideEffectName.text = "Side Effect - ???:";
    }

    // method to reset all crafting
    public void ResetCraft()
    {
        for(int i = 0; i < craftSlots.Length; i++)
        {
            craftSlots[i].Reset();
        }

        craftVialSlot.Reset();
        craftingWarning.gameObject.SetActive(false);
    }

    //Method to actually craft using the materials in the crafting section
    public void CraftPoison()
    {
        //Find what icon needs to be updated: if craftVial is null --> no icon was selected beforehand. Choose the next free one
        //If there are no free icons, you can't craft
        VialIcon updatedIcon = null;
        if (craftVialSlot.GetVial() != null)                  //Updating a poison
        {
            updatedIcon = (craftVialSlot.GetVial() == boltVial) ? boltIcon : updatedIcon;
            updatedIcon = (craftVialSlot.GetVial() == caskVial) ? caskIcon : updatedIcon;
            updatedIcon = (craftVialSlot.GetVial() == thirdVial) ? thirdIcon : updatedIcon;
        }
        else                                                  //Replacing a poison
        {
            updatedIcon = (displayVialIndex == DisplayVialEnum.Primary) ? boltIcon : updatedIcon;
            updatedIcon = (displayVialIndex == DisplayVialEnum.Secondary) ? caskIcon : updatedIcon;
            updatedIcon = (displayVialIndex == DisplayVialEnum.Third) ? thirdIcon : updatedIcon;
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
                PoisonVial craftVial = craftVialSlot.GetVial();

                //Get poison vial to update icon with
                if (craftVial == null)
                    craftVial = new PoisonVial(ingredientList, 1);
                else
                    craftVial.UpgradeVial(ingredientList, 1);

                //Update display information and craft
                UpdateVialDisplayInfo(updatedIcon, craftVial);
                ResetCraft();
            }
            else
            {
                DisplayCraftWarning("NO INGREDIENTS DETECTED");
            }
        }
        else
        {
            DisplayCraftWarning("NO VIAL OR SLOT SELECTED TO UPGRADE OR FILL");
        }
    }


    //Helper method to display craft warning 
    public void DisplayCraftWarning(string warning)
    {
        craftingWarning.gameObject.SetActive(true);
        craftingWarning.text = warning;
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
