﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

//Event classes
[System.Serializable]
public class ForwardVectorUpdateDelegate : UnityEvent<Vector3> {}
[System.Serializable]
public class AnimStateUpdateDelegate : UnityEvent<int> {}

public class TwitchController : MonoBehaviour
{
    //Attack hitbox properties
    [Header("Prefab attacks")]
    [SerializeField]
    private Transform arrowBolt = null;
    [SerializeField]
    private Transform weakBolt = null;
    [SerializeField]
    private Transform poisonCask = null;

    //Player mobility properties
    [Header("Player stats")]
    [SerializeField]
    private EntityStatus status = null;
    [SerializeField]
    private float attackMoveReduction = 0.6f;

    //Poison vials && swap management
    private PoisonVial mainVial = null;
    private PoisonVial secVial = null;
    private bool canSwap;
    private float swapDelay = 0.2f;
    private bool crafting;

    //Primary attack management
    [Header("Primary attack management")]
    [SerializeField]
    private int boltCost = 1;
    [SerializeField]
    private float fireRate = 0.35f;
    [SerializeField]
    private float weakBoltDmg = 1.25f;
    private bool fireTimerRunning;

    //Secondary attack management
    [Header("Secondary attack management")]
    [SerializeField]
    private float throwTime = 0.3f;
    [SerializeField]
    private float throwCD = 1.75f;
    [SerializeField]
    private float maxThrowDist = 5f;
    [SerializeField]
    private int caskCost = 3;
    private bool canThrow;

    //Contaminate management
    [Header("Contamination management")]
    [SerializeField]
    private float conCD = 9.5f;
    [SerializeField]
    private ContaminateManager conManager = null;
    private bool canCon;
    private List<PoisonBombBehav> reactiveBombs;

    //Stealth management
    [Header("Stealth management")]
    [SerializeField]
    private float stealthFireRateBuff = 0.65f;
    [SerializeField]
    private float stealthSpeedBuff = 1.2f;
    [SerializeField]
    private float stealthCD = 10.0f;
    [SerializeField]
    private float stealthDelay = 1.0f;
    [SerializeField]
    private float stealthDuration = 6.0f;
    [SerializeField]
    private float stealthBuffDuration = 5.0f;
    [SerializeField]
    private InvisibilityRange invisRange = null;
    [SerializeField]
    private GameObject invisGauge = null;
    [SerializeField]
    private Image invisGaugeBar = null;
    private bool invisible;
    private bool attackBuffed;
    private bool canStealth;
    
    //Visual indicators (will be moved)
    [Header("Visuals")]
    [SerializeField]
    private Color stealthDelayColor = Color.black;
    [SerializeField]
    private Color stealthColor = Color.black;
    [SerializeField]
    private Color stealthBuffColor = Color.black;
    [SerializeField]
    private Color normalColor = Color.black;

    //UI Management
    [Header("User Interface")]
    [SerializeField]
    private AbilityIcon stealthIcon = null;
    [SerializeField]
    private AbilityIcon contaminateIcon = null;
    [SerializeField]
    private VialIcon boltIcon = null;
    [SerializeField]
    private VialIcon caskIcon = null;
    [SerializeField]
    private VialIcon secIcon = null;
    [SerializeField]
    private Inventory inventory = null;
    [SerializeField]
    private TMP_Text cogsDisplay = null;
    [SerializeField]
    private Image vialEquippedDisplay = null;
    [SerializeField]
    private Transform upgradePopup = null;

    private int cogs = 0;

    //Slowdown when getting hit
    private bool processingDamaged = false;
    private const float DAMAGED_PERIOD = 0.5f;
    

    //Audio Source Management (Seperate class)
    [Header("Audio")]
    [SerializeField]
    private AudioSource audioFX = null;
    [SerializeField]
    private AudioSource voiceAudio = null;
    [SerializeField]
    private AudioClip caskThrowFX = null;
    [SerializeField]
    private AudioClip stealthingFX = null;
    [SerializeField]
    private AudioClip contaminateFX = null;
    [SerializeField]
    private AudioClip[] hurtClips = null;


    //Variable to help manage animation
    private enum TwitchAnimState {IDLE, MOVING, SHOOTING, CONTAMINATION, CASK};
    private TwitchAnimState animState = TwitchAnimState.IDLE;
    public ForwardVectorUpdateDelegate OnForwardDirUpdate;
    public AnimStateUpdateDelegate OnAnimStateUpdate;
    

    //Provoked flag
    public bool provoked;


    //Initialize variables
    void Awake()
    {
        //Initialize flag variables
        fireTimerRunning = false;
        canThrow = true;
        canCon = true;
        invisible = false;
        attackBuffed = false;
        canStealth = true;
        canSwap = true;
        crafting = false;
        provoked = false;

        //Initialize events
        OnForwardDirUpdate = new ForwardVectorUpdateDelegate();
        OnAnimStateUpdate = new AnimStateUpdateDelegate();

        //Initialize poisonVial variables
        secVial = new PoisonVial(0, 2, 1, 2, Color.magenta, 40);
        mainVial = new PoisonVial(2, 1, 2, 0, Color.yellow, 40);
        vialEquippedDisplay.color = mainVial.GetColor();

        reactiveBombs = new List<PoisonBombBehav>();
    }


    // Start is called before the first frame update: intializes variables connected to other objects (UI)
    void Start()
    {
        inventory.InitInventory();
        inventory.onClose.AddListener(OnInventoryClose);
        caskIcon.SetUpVial(mainVial);
        boltIcon.SetUpVial(mainVial);
        secIcon.SetUpVial(secVial);
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (status.canMove)
        {
            movement();

            //Primary attack
            if (!fireTimerRunning && Input.GetButton("Fire1"))
                primaryAttack();

            //Secondary attack
            if (canThrow && Input.GetButtonDown("Fire2") && checkVial(mainVial, caskCost))
                StartCoroutine(throwCask());

            //Invisibility
            if (canStealth && Input.GetButtonDown("Mobility"))
                StartCoroutine(initiateStealth());

            //Contamination
            if (canCon && Input.GetButtonDown("Contaminate") && conManager.CanContaminate())
            {
                StartCoroutine(Contaminate());
            }

            //Swapping
            if (canSwap && Input.GetButton("PrimarySwitch"))
                swapSec();

            //Opening inventory
            if (!provoked && Input.GetButtonDown("Inventory"))
            {
                status.canMove = false;
                inventory.Open(mainVial, secVial);
            }
        }
    }


    //Method to call upon movement
    void movement()
    {
        //Get axis values
        float hDir = Input.GetAxis("Horizontal");
        float vDir = Input.GetAxis("Vertical");

        //Get speed modifier
        float speedModifier = (invisible) ? stealthSpeedBuff : 1.0f;
        speedModifier *= (Input.GetButton("Fire1")) ? attackMoveReduction : 1.0f;

        Vector3 dir = new Vector3(hDir, vDir, 0);


        dir.Normalize();
        if (!Input.GetButton("Fire1") && dir.magnitude != 0f)
        {
            OnForwardDirUpdate.Invoke(dir);
            OnAnimStateUpdate.Invoke((int)TwitchAnimState.MOVING);
            animState = TwitchAnimState.MOVING;
        }
        else if (!Input.GetButton("Fire1") && dir.magnitude == 0f)
        {
            animState = TwitchAnimState.IDLE;
            OnAnimStateUpdate.Invoke((int)TwitchAnimState.IDLE);
        }

        transform.Translate(dir * status.GetCurSpeed() * Time.fixedDeltaTime * speedModifier);
    }

    //Helper method to check PoisonVial ammo in a safe manner
    bool checkVial(PoisonVial vial, int cost)
    {
        return vial != null && vial.CanUsePoison(cost);
    }


    //Method to create primary attack loop
    void primaryAttack()
    {
        if (Input.GetButton("Fire1"))
        {
            //set up projectile direction
            Vector3 mousePos = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0);
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector2 dirVect = new Vector2 (mousePos.x - transform.position.x, mousePos.y - transform.position.y);
            OnForwardDirUpdate.Invoke(mousePos - transform.position);
            OnAnimStateUpdate.Invoke((int)TwitchAnimState.SHOOTING);
            animState = TwitchAnimState.SHOOTING;

            //make projectile
            bool usesPoison = checkVial(mainVial, boltCost);
            Transform curTemplate = (usesPoison) ? arrowBolt : weakBolt;
            Transform curBolt = Object.Instantiate (curTemplate, transform);
            if (usesPoison)
                curBolt.GetComponent<PoisonProjBehav>().SetPoisonProj(dirVect, mainVial, true);
            else
                curBolt.GetComponent<ProjectileBehav>().SetProj(dirVect, weakBoltDmg, true);
            curBolt.parent = null;

            //Buff player if player was previously invisible
            if (invisible)
                StartStealthAttackBuff();

            //Update vial properties if poison was used
            if (usesPoison)
            {
                mainVial.UsePoison(boltCost);
                UpdateMainVial();
            }

            //Create timer for loop
            fireTimerRunning = true;
            float curFireRate = fireRate;
            curFireRate *= (attackBuffed) ? stealthFireRateBuff : 1.0f;

            Invoke("primaryAttack", curFireRate);
        }
        else
        {
            animState = TwitchAnimState.IDLE;
            OnAnimStateUpdate.Invoke((int)TwitchAnimState.IDLE);
            fireTimerRunning = false;
        }
    }


    /* Throws cask at mouse direction with distance of MAX_THROW_DIST units or less */
    IEnumerator throwCask ()
    {
        /* Initiate attack buff if invisible */
        if (invisible)
            StartStealthAttackBuff();

        /* Get position of mouse */
        Vector3 mousePos = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0);
        mousePos = Camera.main.ScreenToWorldPoint (mousePos);

        /* Get directional vector from player to mouse */
        Vector3 dirVector = new Vector3 (mousePos.x - transform.position.x,
                                         mousePos.y - transform.position.y, 0);
        
        /* If magnitude of dirVector > MAX_THROW_DIST, minimize vector magnitude */
        if (dirVector.magnitude > maxThrowDist)
        {
            dirVector.Normalize ();
            dirVector *= maxThrowDist;
        }

        dirVector += transform.position;

        /* Execute action: play sound, disable movement for some time and throw cask */
        status.canMove = false;
        OnForwardDirUpdate.Invoke(dirVector);
        OnAnimStateUpdate.Invoke((int)TwitchAnimState.CASK);
        animState = TwitchAnimState.CASK;
        audioFX.clip = caskThrowFX;
        audioFX.Play();

        yield return new WaitForSeconds (throwTime);

        //Instatiate cask instance
        Transform curCask = Object.Instantiate (poisonCask, dirVector, Quaternion.identity, transform);
        PoisonBombBehav caskBomb = curCask.GetComponent<PoisonBombBehav>();
        caskBomb.SetBomb (true, mainVial);
        if (mainVial.GetSideEffect() == PoisonVial.SideEffect.COMBUSTION_BLAST)
            reactiveBombs.Add(caskBomb);
            
        curCask.parent = null;

        //Update vial properties
        mainVial.UsePoison(caskCost);
        UpdateMainVial();
        caskIcon.ShowDisabled();

        canThrow = false;
        status.canMove = true;
        Invoke("refreshCaskCD", throwCD);
    }


    //Private helper method to update mainVial's icons
    void UpdateMainVial()
    {
        caskIcon.UpdateVial();
        boltIcon.UpdateVial();

        if (mainVial.GetAmmo() == 0)
        {
            caskIcon.ShowDisabled();
            boltIcon.ShowDisabled();
            mainVial = null;
            vialEquippedDisplay.color = Color.black;
        }
    }

    
    //Method to call cause character to stealth
    IEnumerator initiateStealth()
    {
        //Disable stealth
        canStealth = false;
        stealthIcon.ShowDisabled();

        //Get necessary variables
        SpriteRenderer render = GetComponent<SpriteRenderer>();
        float timer = 0.0f;
        invisGaugeBar.fillAmount = 0f;

        //Initiate stealth delay
        audioFX.clip = stealthingFX;
        audioFX.Play(0);
        render.color = stealthDelayColor;
        invisGauge.SetActive(true);

        while (timer < stealthDelay)
        {
            yield return new WaitForFixedUpdate();
            timer += Time.fixedDeltaTime;
            invisGaugeBar.fillAmount = timer / stealthDelay;
        }

        //Actually make character invisible and do stealth
        timer = 0.0f;
        invisGaugeBar.fillAmount = 1f;
        invisible = true;
        render.color = stealthColor;

        while (invisible && timer < stealthDuration)
        {
            yield return new WaitForFixedUpdate();
            timer += Time.fixedDeltaTime;
            invisGaugeBar.fillAmount = (stealthDuration - timer) / stealthDuration;
        }

        //When get out of stealth, check if attack buff was activated or not (attacking in stealth will activate attack buff)
        if (invisible)
        {
            invisible = false;
            render.color = normalColor;
        }

        invisGauge.SetActive(false);
        Invoke("refreshStealth", stealthCD);
    }


    //IEnumerator for contamination
    IEnumerator Contaminate()
    {
        //Contaminate animation
        canCon = false;
        contaminateIcon.ShowDisabled();
        animState = TwitchAnimState.CONTAMINATION;
        OnAnimStateUpdate.Invoke((int)animState);
        status.canMove = false;
        yield return new WaitForSeconds(0.2f);
        status.canMove = true;

        //Activate reactive bombs
        int i = reactiveBombs.Count - 1;
        while (i >= 0 && reactiveBombs[i] != null)
        {
            reactiveBombs[i].CombustionBlast();
            i--;
        }
        reactiveBombs.Clear();

        //Contaminate
        conManager.ContaminateAll();

        //play sound fx
        audioFX.clip = contaminateFX;
        audioFX.Play();
                
        Invoke("refreshContaminate", conCD);
    }


    //Checks if this player is visible to enemy perceiver
    public bool IsVisible(Collider2D perceiver)
    {
        return !invisible || invisRange.IsInRange(perceiver);
    }

    //Check if Twitch is stealthing
    public bool IsStealthing()
    {
        return invisible;
    }


    //Method to activate stealth attack buff when attacking, if invisible
    void StartStealthAttackBuff()
    {
        attackBuffed = true;
        invisible = false;
        GetComponent<SpriteRenderer>().color = stealthBuffColor;
        Invoke("endAttkStealthBuff", stealthBuffDuration);
    }


    //Helper method to swap between primary and secondary
    void swapSec()
    {
        //Swap
        PoisonVial temp = mainVial;
        mainVial = secVial;
        secVial = temp;

        //Update UI
        boltIcon.SetUpVial(mainVial);
        caskIcon.SetUpVial(mainVial, canThrow);
        secIcon.SetUpVial(secVial);
        swapEquippedDisplay();

        canSwap = false;
        Invoke("refreshSwap", swapDelay);
    }


    //Helper method to update vial display for equipped.
    void swapEquippedDisplay()
    {
        //Check if main vial is null
        if (mainVial == null)
        {
            vialEquippedDisplay.color = Color.black;
        }
        else
        {
            //Get color and display the side effect this vial has. If none, print "???"
            vialEquippedDisplay.color = mainVial.GetColor();
            PoisonVial.SideEffect mainEffect = mainVial.GetSideEffect();
        }

    }


    //Accessor method to crafting
    public bool IsCrafting()
    {
        return crafting;
    }

    //Mutator method for crafting
    public void EnableCraftMode()
    {
        crafting = true;
        status.canMove = false;
    }

    public void DisableCraftMode()
    {
        crafting = false;
        status.canMove = true;
    }


    //Accessor method to check if poison is valid
    //  Pre: 0 = main, 1 = sec. If anything else, just return false
    public bool IsVialUpgradable(int vialIndex)
    {
        if (vialIndex == 0)
            return mainVial == null || mainVial.IsUpgradable();
        else if (vialIndex == 1)
            return secVial == null || secVial.IsUpgradable();
        else
            return false;
    }
    

    //Method to upgrade poisons
    public void UpgradePrimary(List<Ingredient> ingredients)
    {
        int bonus = 1;

        if (mainVial == null)
        {
            mainVial = new PoisonVial(ingredients, bonus);
            boltIcon.SetUpVial(mainVial);
            caskIcon.SetUpVial(mainVial);
        }
        else
        {
            mainVial.UpgradeVial(ingredients, bonus);
            boltIcon.UpdateVial();
            caskIcon.UpdateVial();
        }

        StartCoroutine(DisplayVialUpdates(mainVial));
    }


    public void UpgradeSec(List<Ingredient> ingredients)
    {
        int bonus = 1;

        if (secVial == null)
        {
            secVial = new PoisonVial(ingredients, bonus);
            secIcon.SetUpVial(secVial);
        }
        else
        {
            secVial.UpgradeVial(ingredients, bonus);
            secIcon.UpdateVial();
        }

        StartCoroutine(DisplayVialUpdates(secVial));
    }


    //IEnumerator to display update popups after a cask update
    IEnumerator DisplayVialUpdates(PoisonVial vial)
    {
        List<string> vialUpdates = vial.GetLatestUpdateInfo();

        for(int i = 0; i < vialUpdates.Count; i++)
        {
            Transform curPopup = Object.Instantiate(upgradePopup, transform);
            curPopup.GetComponent<TextPopup>().SetUpPopup(vialUpdates[i]);
            yield return new WaitForSeconds(0.5f);
        }
    }


    //Method concerning inventory
    public void AddToInventory(Ingredient ingredient)
    {
        inventory.AddIngredient(ingredient);
    }


    //Method to align poison vials with those in the inventory
    public void OnInventoryClose(PoisonVial[] inventoryVials)
    {   
        status.canMove = true;

        mainVial = inventoryVials[0];
        secVial = inventoryVials[1];

        boltIcon.SetUpVial(mainVial);
        caskIcon.SetUpVial(mainVial);
        secIcon.SetUpVial(secVial);
    }


    //Cog mutator method: can be positive or negative
    public void AddCogs(int cogAmount)
    {
        cogs += cogAmount;
        if (cogs >= 1000)
            cogs = 999;
        
        cogsDisplay.text = "Cogs: " + cogs;
    }

    //Cog accessor method: tells you if you can afford price
    public bool CanAfford(int price)
    {
        return cogs >= price;
    }


    //Methods to refresh cooldowns and effects. Called on invoke after attack sequences
    void refreshCaskCD()
    {
        if (mainVial != null)
            caskIcon.ShowEnabled();
        canThrow = true;
    }

    void refreshStealth()
    {
        stealthIcon.ShowEnabled();
        canStealth = true;
    }

    void refreshContaminate()
    {
        contaminateIcon.ShowEnabled();
        canCon = true;
    }

    void refreshSwap()
    {
        canSwap = true;
    }

    void endAttkStealthBuff()
    {
        attackBuffed = false;
        GetComponent<SpriteRenderer>().color = normalColor;
    }


    //Event handler: if entity is damaged during crafting, interrupt crafting
    public void OnEntityDamage()
    {
        if (crafting)
            DisableCraftMode();

        if (!processingDamaged)
            StartCoroutine(ProcessDamage());
        
    }

    //Does slowdown upon getting damaged
    private IEnumerator ProcessDamage()
    {
        processingDamaged = true;
        Time.timeScale = 0.2f;

        voiceAudio.clip = hurtClips[Random.Range(0, hurtClips.Length)];
        voiceAudio.Play(0);

        yield return new WaitForSeconds(DAMAGED_PERIOD * 0.2f);

        Time.timeScale = 1f;
        processingDamaged = false;
    }

    //Method to force character into idle animation during dialogue scenes
    public void ForceToIdle()
    {
        animState = TwitchAnimState.IDLE;
        OnAnimStateUpdate.Invoke((int)TwitchAnimState.IDLE);
        status.canMove = false;

        if (invisible)
        {
            GetComponent<SpriteRenderer>().color = normalColor;
            invisible = false;
        }
    }
}
