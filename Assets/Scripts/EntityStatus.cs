using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class EntityStatus : MonoBehaviour
{
    //Base stats
    [Header("Base Stats")]
    [SerializeField]
    private float baseHealth = 0.0f;
    [SerializeField]
    private float baseSpeed = 0.0f;
    [SerializeField]
    private float healthRegen = 0.0f;
    public bool canMove;

    //Aura variable
    [Header("Aura")]
    [SerializeField]
    private EntityAura aura = null;
    private const int AURA_REQ = 3;

    [Header("UI")]
    [SerializeField]
    private Image healthBar = null;
    [SerializeField]
    private TMP_Text stacksUI = null;
    [SerializeField]
    private Color normalColor = Color.black;

    [Header("Audio")]
    [SerializeField]
    private AudioClip deathSound = null;
    private AudioSource audioFX;

    //Management variables
    private float speedModifier;
    private float curHealth;
    private const float REGEN_TIME = 1.0f;

    //Poison variables
    private int curPoisonStacks;
    private const int MAX_POISON_STACKS = 5;
    private int curTick;
    private int totalTicks;
    private const int MAX_TICKS = 6;
    private const float TICK_TIME = 1.0f;
    private PoisonVial poison = null;
    private const float GREATER_DECAY_MODIFIER = 2.0f;

    //Events
    public UnityEvent onDeathEvent;

    //Variables for side effects concerning contamination
    private const float SIDE_EFFECT_DURATION = 3f;
    private const float SIDE_EFFECT_DMG_BUFF = 1.5f;
    private const float PARALYSIS_SPEED_REDUCTION = 0.1f;
    private const float DEATH_MARK_THRESHOLD = 0.4f;
    private PoisonVial.SideEffect contaminateEffect;


    // Awake is called to initialize variables
    void Awake()
    {
        curHealth = baseHealth;
        speedModifier = 1.0f;
        canMove = true;

        curPoisonStacks = 0;
        curTick = 0;
        totalTicks = 0;
        onDeathEvent = new UnityEvent();
        contaminateEffect = PoisonVial.SideEffect.NONE;

        audioFX = GetComponent<AudioSource>();
    }

    // Method to call to damage this entity
    public void DamageEntity(float dmg)
    {
        //Check if undamaged and decrement health
        float dmgModifier = (contaminateEffect != PoisonVial.SideEffect.NONE) ? SIDE_EFFECT_DMG_BUFF : 1.0f;
        bool undamaged = (curHealth >= baseHealth);

        curHealth -= (dmg * dmgModifier);

        if (healthBar != null)
            healthBar.fillAmount = curHealth / baseHealth;

        //If health is zero, kill either enemy or player. If alive and not poisoned, activate loop
        if (curHealth <= 0.0f)
        {
            StartCoroutine(Death());
        }
        else     //To keep only 2 invoke loops concerning health at max, only activate health regen loop if undamaged and not poisoned
        {
            SendMessage("OnEntityDamage", null, SendMessageOptions.DontRequireReceiver);

            if (undamaged && curPoisonStacks == 0)
            {
                Invoke("HealthRegenLoop", REGEN_TIME);
            }
        }
    }

    //Method to do poison damage to do this enemy
    //  If vial is null, we just stick with the same poison
    public void PoisonDamageEntity(float initDmg, int initStacks, PoisonVial vial)
    {
        //Check if enemy wasn't poisoned initially
        bool notPoisoned = curPoisonStacks == 0;
            
        curPoisonStacks += initStacks;
        if (curPoisonStacks > MAX_POISON_STACKS)
            curPoisonStacks = MAX_POISON_STACKS;

        //Poison enemy first if enemy were to be poisoned
        if (vial != null)
        {
            poison = vial;
            GetComponent<SpriteRenderer>().color = vial.GetColor();

            if (aura != null && curPoisonStacks >= AURA_REQ)
                aura.EnableAura(vial);
        }
            
        //Update UI
        if (stacksUI != null)
            stacksUI.text = "" + curPoisonStacks;
            
        //Reset timer
        curTick = 0;

        //Activate invoke if first time poisoned
        if (notPoisoned)
            Invoke("PoisonTickLoop", TICK_TIME);


        DamageEntity(initDmg);
    }


    //Public method to do weak poison damage (vial will NOT  be overriden)
    //  If enemy wasn't previously poisoned, just do weak poison
    public void WeakPoisonDamageEntity(float initDmg, int initStacks, PoisonVial weakVial)
    {
        if (curPoisonStacks == 0)
        {
            PoisonDamageEntity(initDmg, initStacks, weakVial);
        }
        else
        {
            PoisonDamageEntity(initDmg, initStacks, null);
            if (aura != null && curPoisonStacks >= AURA_REQ)
                aura.EnableAura(poison);
        }
    }


    //Invoke loop for health regen: only continues loop if not poisoned
    void HealthRegenLoop()
    {
        if (curHealth < baseHealth && curPoisonStacks == 0)
        {
            curHealth += healthRegen;
            
            if (curHealth > baseHealth)
                curHealth = baseHealth;

            if (healthBar != null)
                healthBar.fillAmount = curHealth / baseHealth;
            
            if (curHealth < baseHealth)
                Invoke("HealthRegenLoop", REGEN_TIME);
        }
    }

    //Invoke loop for poison tick: continues looping up until curTick == max tick
    void PoisonTickLoop()
    {
        //Activate poison damage. Add damage modifier if "Greater Decay"
        curTick++;
        totalTicks++;
        float dmg = poison.GetPoisonDmg();
        dmg *= (poison.GetSideEffect() == PoisonVial.SideEffect.GREATER_DECAY) ? GREATER_DECAY_MODIFIER : 1.0f;
        curHealth -= (poison.GetPoisonDmg() * curPoisonStacks);

        if (totalTicks % 2 == 0 && aura != null)
            aura.AuraPoisonTick();
        
        //Decide what to do next depending on health and tick timer
        if (curHealth <= 0)
        {
            StartCoroutine(Death());
        }
        else
        {
            if (healthBar != null)
                healthBar.fillAmount = curHealth / baseHealth;

            //If maxTicks has passed, disable poison and start healthregen loop, else continue PoisonTickLoop
            if (curTick == MAX_TICKS)
            {
                curPoisonStacks = 0;
                curTick = 0;
                totalTicks = 0;
                poison = null;
                GetComponent<SpriteRenderer>().color = normalColor;

                if (aura != null)
                    aura.DisableAura();

                if (stacksUI != null)
                    stacksUI.text = "0";

                Invoke("HealthRegenLoop", REGEN_TIME);
            }
            else
            {
                Invoke("PoisonTickLoop", TICK_TIME);
            }
        }
    }

    //Method to do contaminate damage to this entity
    public void Contaminate()
    {
        //Only contaminate if enemy has side effects
        if (curPoisonStacks > 0)
        {

            //do aura damage if applicable
            if (aura != null)
                aura.ContaminateExplode(curPoisonStacks);
            
            //Actually do contaminate damage. If DEATH_MARK, check if can execute (below threshhold)
            if (poison.GetSideEffect() == PoisonVial.SideEffect.DEATH_MARK && curHealth < DEATH_MARK_THRESHOLD * baseHealth)
            {
                DamageEntity(baseHealth);
            }
            else
            {
                DamageEntity(poison.GetContaminateDmg(curPoisonStacks));
            }

            //Do side effect damage and put up contaminate timer if applicable
            if (poison.GetSideEffect() == PoisonVial.SideEffect.INDUCED_PARALYSIS || poison.GetSideEffect() == PoisonVial.SideEffect.DEATH_MARK)
            {
                contaminateEffect = poison.GetSideEffect();
                if (contaminateEffect == PoisonVial.SideEffect.INDUCED_PARALYSIS)
                    ChangeSpeed(PARALYSIS_SPEED_REDUCTION);
                
                Invoke("ExpireContaminateEffects", SIDE_EFFECT_DURATION);
            }
        }
    }


    //Method to expire contaminate side effects
    void ExpireContaminateEffects()
    {
        //Reverse any specific changes
        if (contaminateEffect == PoisonVial.SideEffect.INDUCED_PARALYSIS)
            ChangeSpeed(1.0f / PARALYSIS_SPEED_REDUCTION);
        
        contaminateEffect = PoisonVial.SideEffect.NONE;
    }

    //Method to kill object when health is low
    IEnumerator Death()
    {
        if (tag == "Player")
            transform.position = new Vector3(27.5f, 0f, 0f);

        CancelInvoke();
        GetComponent<Collider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
        SendMessage("OnEntityDeath", null, SendMessageOptions.DontRequireReceiver);
        onDeathEvent.Invoke();

        if (aura != null)
            aura.DisableAura();

        //Play audio clips
        if (deathSound != null)
        {
            audioFX.clip = deathSound;
            audioFX.Play();
        }

        yield return new WaitForSeconds(1.0f);

        if (tag == "Enemy")
        {
            Destroy(gameObject);
        }
    }


    //Public method to heal for a fixed amount
    public void Heal(float healthGained)
    {
        //Update health value
        curHealth += healthGained;
        if (curHealth > baseHealth)
            curHealth = baseHealth;

        //Update health UI
        if (healthBar != null)
            healthBar.fillAmount = curHealth / baseHealth;
    }


    //Public accessor method for speed
    public float GetCurSpeed()
    {
        return speedModifier * baseSpeed;
    }

    //Public method to affect speed - NON-BUFF
    public void ChangeSpeed(float speedFactor)
    {
        speedModifier *= speedFactor;

        //duct tape solution to ensure that if speedModifier > 0, reset it to 1.0f
        if (speedModifier > 1.0f)
            speedModifier = 1.0f;
    }

    //Public accessor method to get current aamount of poison stacks
    public int GetPoisonStacks()
    {
        return curPoisonStacks;
    }

    //Acessor method on whether or not the entity is dead
    public bool IsAlive()
    {
        return curHealth > 0f;
    }

}
