﻿using System.Collections;
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

    [Header("UI")]
    [SerializeField]
    private Image healthBar = null;
    [SerializeField]
    private TMP_Text stacksUI = null;
    [SerializeField]
    private Color normalColor = Color.black;

    //Management variables
    private float speedModifier;
    private float curHealth;
    private const float REGEN_TIME = 1.0f;

    //Poison variables
    private int curPoisonStacks;
    private const int MAX_POISON_STACKS = 5;
    private int curTick;
    private const int MAX_TICKS = 6;
    private const float TICK_TIME = 1.0f;
    private PoisonVial poison = null;
    private const float GREATER_DECAY_MODIFIER = 1.25f;

    //Events
    public UnityEvent onDeathEvent;

    // Awake is called to initialize variables
    void Awake()
    {
        curHealth = baseHealth;
        speedModifier = 1.0f;
        canMove = true;

        curPoisonStacks = 0;
        curTick = 0;
        onDeathEvent = new UnityEvent();
    }

    // Method to call to damage this entity
    public void DamageEntity(float dmg)
    {
        //Check if undamaged and decrement health
        bool undamaged = (curHealth >= baseHealth);
        curHealth -= dmg;
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

        //Poison enemy first
        poison = vial;
        GetComponent<SpriteRenderer>().color = vial.GetColor();

        curPoisonStacks += initStacks;
        if (curPoisonStacks > MAX_POISON_STACKS)
            curPoisonStacks = MAX_POISON_STACKS;
            
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
        float dmg = poison.GetPoisonDmg();
        dmg *= (poison.GetSideEffect() == PoisonVial.SideEffect.GREATER_DECAY) ? GREATER_DECAY_MODIFIER : 1.0f;
        curHealth -= (poison.GetPoisonDmg() * curPoisonStacks);

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
                poison = null;
                GetComponent<SpriteRenderer>().color = normalColor;

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
        if (curPoisonStacks > 0)
        {
            DamageEntity(poison.GetContaminateDmg(curPoisonStacks));
        }
    }

    //Method to kill object when health is low
    IEnumerator Death()
    {
        CancelInvoke();
        GetComponent<Collider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
        SendMessage("OnEntityDeath", null, SendMessageOptions.DontRequireReceiver);
        onDeathEvent.Invoke();

        yield return new WaitForSeconds(1.0f);

        if (tag == "Enemy")
        {
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Player has died");
        }
    }


    //Public accessor method for speed
    public float GetCurSpeed()
    {
        return speedModifier * baseSpeed;
    }

    //Public method to affect speed
    public void ChangeSpeed(float speedFactor)
    {
        speedModifier *= speedFactor;
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
