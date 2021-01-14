using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngredientWorldObj : MonoBehaviour
{
    //Type indicator
    [Header("Ingredient Name")]
    [SerializeField]
    Ingredient.IngredientType type = Ingredient.IngredientType.Puffcap;
    
    //Variables used to create ingredient
    [Header("Stat Offered")]
    [SerializeField]
    bool potency = false;
    [SerializeField]
    bool poison = false;
    [SerializeField]
    bool reactivity = false;
    [SerializeField]
    bool stickiness = false;
    [SerializeField]
    int statsOffered = 1;
    [SerializeField]
    Color ingColor = Color.clear;

    //Variables used for crafting
    [Header("Quick Crafting variables")]
    [SerializeField]
    float maxCraftDuration = 3.0f;
    [SerializeField]
    Image craftTimerUI = null;
    [SerializeField]
    GameObject craftTimerBorder = null;
    [SerializeField]
    Transform popup = null;
    float craftTimer;

    //Ingredient
    Ingredient mainIngredient;
    TwitchController player;

    // On Awake, create ingredient and set player to null
    void Awake()
    {
        bool[] statChances = new bool[4];
        statChances[0] = potency;
        statChances[1] = poison;
        statChances[2] = reactivity;
        statChances[3] = stickiness;

        mainIngredient = new Ingredient(type, statChances, statsOffered, ingColor);

        player = null;
        craftTimer = 0.0f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (player != null && !player.IsCrafting())
        {
            //Quick craft during battle
            if (Input.GetButtonDown("BoltCraft"))
                StartCoroutine(CheckCraft("BoltCraft", player.provoked));
            else if (Input.GetButtonDown("CaskCraft"))
                StartCoroutine(CheckCraft("CaskCraft", player.provoked));

            //Collecting after battle
            if (Input.GetButtonDown("Contaminate") && !player.provoked)
            {
                player.AddToInventory(mainIngredient);
                Destroy(gameObject);
            }
        }
    }

    //IEnumerator that checks crafting before crafting
    IEnumerator CheckCraft(string buttonInput, bool playerProvoked)
    {
        if (IsVialUpgradable(buttonInput))
        {
            yield return StartCoroutine(Craft(buttonInput, playerProvoked));
        }
        else
        {
            Transform curPopup = Object.Instantiate(popup, player.transform);
            curPopup.GetComponent<TextPopup>().SetUpPopup("Vial not craftable");
        }
    }

    //IEnumerator used to update progress
    IEnumerator Craft(string buttonInput, bool playerProvoked)
    {
        //Disable player movement and set up charge
        char buttonInputAbbrv = buttonInput[0];

        if (playerProvoked)
        {
            craftTimer = 0.0f;
            player.EnableCraftMode();
            craftTimerBorder.SetActive(true);

            //Do crafting process
            while(craftTimer < maxCraftDuration && player.IsCrafting() && Input.GetButton(buttonInput))
            {
                yield return new WaitForFixedUpdate();
                craftTimer += Time.fixedDeltaTime;
                craftTimerUI.fillAmount = craftTimer / maxCraftDuration;
            }

            //Enable player movement
            player.DisableCraftMode();
            craftTimerBorder.SetActive(false);
        }

        //If channel was completed, craft potion and destroy this object
        if (!playerProvoked || craftTimer >= maxCraftDuration)
        {
            List<Ingredient> ingredients = new List<Ingredient>();
            ingredients.Add(mainIngredient);

            if (buttonInputAbbrv == 'B')
                player.UpgradePrimary(ingredients);
            else if (buttonInputAbbrv == 'C')
                player.UpgradeSec(ingredients);

            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<Collider2D>().enabled = false;
            GetComponent<AudioSource>().Play();
            yield return new WaitForSeconds(1.5f);
            Destroy(gameObject);
        }
    }


    //Private helper method to return whether or not it is upgradable or not
    //  Pre: player != null
    private bool IsVialUpgradable(string buttonInput)
    {
        char buttonInputAbbrv = buttonInput[0];
        int vialIndex = -1;

        if (buttonInputAbbrv == 'B')
            vialIndex = 0;
        else if (buttonInputAbbrv == 'C')
            vialIndex = 1;
        else if (buttonInputAbbrv == 'T')
            vialIndex = 2;

        return player.IsVialUpgradable(vialIndex);
    }

    //Method when player enters collision field
    void OnTriggerEnter2D(Collider2D collider)
    {
        TwitchController twitch = collider.GetComponent<TwitchController>();

        if (twitch != null)
        {
            player = twitch;
        }
    }

    //method when player exits collision field
    void OnTriggerExit2D(Collider2D collider)
    {
        TwitchController twitch = collider.GetComponent<TwitchController>();

        if (twitch != null)
        {
            player = null;
        }
    }
}
