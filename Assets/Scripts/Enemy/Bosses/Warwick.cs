﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warwick : AbstractBoss
{
    //Variables to help with scouting
    [Header("Scouting")]
    private bool isTgtBleeding = false;
    private float sniffTimer = 0f;
    [SerializeField]
    private float sniffedTimeEnd = 0.5f;

    [Header("Lunge")]
    [SerializeField]
    private float lungeSpeedFactor = 2.25f;
    [SerializeField]
    private float lungeTime = 0.75f;
    [SerializeField]
    private float lungeDmgDist = 2f;
    [SerializeField]
    private float lungeDmg = 0f;

    [Header("Hungering Strike")]
    [SerializeField]
    private float spinAttackDist = 3f;
    [SerializeField]
    private float spinAttackRun = 2.5f;
    [SerializeField]
    private float spinAttackAnticipation = 0.5f;
    [SerializeField]
    private float spinAttackDmg = 0f;
    [SerializeField]
    private SpinAttack spinBox = null;
    private bool lunging;
    private bool hitWall; 

    //Audio
    [Header("Audio")]
    [SerializeField]
    private AudioClip[] sniffingClips = null;
    [SerializeField]
    private AudioClip[] discoveryClips = null;
    private AudioSource audioFX = null;

    //Box UI
    [Header("Color Testing")]
    [SerializeField]
    private Color lungeColor = Color.white;
    [SerializeField]
    private Color spinColor = Color.white;
    [SerializeField]
    private Color sniffColor = Color.white;
    [SerializeField]
    private Color normalColor = Color.white;
    [SerializeField]
    private Color discoveryColor = Color.white;

    //Method on awake
    void Awake()
    {
        audioFX = GetComponent<AudioSource>();
    }

    //Method used to choose attack
    protected override IEnumerator Attack(int phase)
    {
        //When player stealth is over, reset sniffTime
        if (sniffTimer >= sniffedTimeEnd && !IsTgtStealthing())
            sniffTimer = 0f;

        //Randomly choose 1 of 2 attacks
        int select = Random.Range(0, 2);
        if (select == 0)
            yield return StartCoroutine(Lunge());
        else
            yield return StartCoroutine(HungeringStrike());
        
    }


    //Method used to scout
    protected override IEnumerator Scout(int phase)
    {
        // audioFX.clip = sniffingClips[Random.Range(0, sniffingClips.Length)];
        // audioFX.Play(0);
        GetComponent<SpriteRenderer>().color = sniffColor;

        while (!TgtVisible())
        {
            yield return new WaitForFixedUpdate();
            sniffTimer += Time.fixedDeltaTime;
        }

        sniffTimer = sniffedTimeEnd;
    }

    //Method used upon discovery
    protected override IEnumerator Discovery()
    {
        audioFX.clip = discoveryClips[Random.Range(0, discoveryClips.Length)];
        audioFX.Play(0);
        GetComponent<SpriteRenderer>().color = discoveryColor;

        yield return new WaitForSeconds(1.5f);

        GetComponent<SpriteRenderer>().color = normalColor;
    }


    //Method used to indicate when enemy has discovered you discovered enemy
    protected override bool FoundTgt()
    {
        return isTgtBleeding || sniffTimer >= sniffedTimeEnd;
    }

    //Method used to attack do dash attack
    private IEnumerator Lunge()
    {
        //Set up lunge
        Vector3 lungeDir = GetTgtPos() - transform.position;
        lungeDir.Normalize();
        float lungeSpeed = GetMoveSpeed() * lungeSpeedFactor;
        GetComponent<SpriteRenderer>().color = lungeColor;

        yield return new WaitForSeconds(0.25f);
        
        //Timer for lunge
        float timer = 0f;
        hitWall = false;
        bool dmgTgt = false;


        while (timer < lungeTime && !hitWall)
        {
            yield return new WaitForFixedUpdate();
            lungeSpeed = GetMoveSpeed() * lungeSpeedFactor;
            transform.Translate(lungeDir * lungeSpeed * Time.fixedDeltaTime);

            //Damage tgt if close enough
            if (!dmgTgt && Vector3.Distance(GetTgtPos(), transform.position) < lungeDmgDist)
            {
                dmgTgt = true;
                DamageTgt(lungeDmg);
            }

            timer += Time.fixedDeltaTime;
        }

        //Reset. If hitWall, recoil back a little
        if (hitWall)
        {
            transform.Translate(-2 * lungeDir * lungeSpeed * Time.fixedDeltaTime);
        }

        GetComponent<SpriteRenderer>().color = normalColor;

    }

    
    //Method to do spin attack
    private IEnumerator HungeringStrike()
    {
        //Run towards player
        float timer = 0f;
        
        while (Vector3.Distance(GetTgtPos(), transform.position) > spinAttackDist && timer < spinAttackRun && TgtVisible())
        {
            yield return new WaitForFixedUpdate();

            //Move towards
            Vector3 dir = GetTgtPos() - transform.position;
            dir.Normalize();
            transform.Translate(dir * GetMoveSpeed() * Time.fixedDeltaTime);

            //Update timer
            timer += Time.fixedDeltaTime;
        }

        //actually do spin attack if visible
        if (TgtVisible())
        {
            GetComponent<SpriteRenderer>().color = spinColor;
            spinBox.Anticipation();
            yield return new WaitForSeconds(spinAttackAnticipation);
            spinBox.Attack(spinAttackDmg);
        }

        GetComponent<SpriteRenderer>().color = normalColor;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Wall")
        {
            hitWall = true;
        }
    }
}