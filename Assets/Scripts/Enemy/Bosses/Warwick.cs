﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warwick : AbstractBoss
{
    //Variables to help with scouting
    [SerializeField]
    private float phaseThreeMoveSpeedBuff = 1.2f;

    [Header("Scouting")]
    private bool isTgtBleeding = false;
    private float sniffTimer = 0f;
    [SerializeField]
    private float sniffedTimeEnd = 0.5f;
    private bool scoutedPlayer = false;

    [Header("Lunge")]
    [SerializeField]
    private float lungeSpeedFactor = 2.25f;
    [SerializeField]
    private float lungeTime = 0.75f;
    [SerializeField]
    private float lungeDmgDist = 2f;
    [SerializeField]
    private float lungeDmg = 0f;
    [SerializeField]
    private float minLungeSpeed = 8f;

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


    [Header("Howling")]
    [SerializeField]
    private float slowFactor = 0.75f;
    [SerializeField]
    private SlowAoE howlBox = null;
    [SerializeField]
    private float howlAnticipation = 0.75f;

    [Header("Leaking")]
    [SerializeField]
    private Transform leakPuddle = null;
    [SerializeField]
    private float leakInterval = 3f;
    [SerializeField]
    private float leakSlow = 0.5f;
    [SerializeField]
    private float puddleDuration = 6f;
    [SerializeField]
    private int numPuddles = 3;
    private bool leaking = false;

    int prevAttack = 5;

    //Audio
    [Header("Audio")]
    [SerializeField]
    private AudioClip[] discoveryClips = null;
    [SerializeField]
    private AudioClip lungeClip = null;
    [SerializeField]
    private AudioClip strikeClip = null;
    [SerializeField]
    private AudioClip howlClip = null;
    [SerializeField]
    private AudioSource voiceSource = null;
    [SerializeField]
    private AudioSource audioFX = null;
    [SerializeField]
    private AudioSource leakingSound = null;

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
    [SerializeField]
    private Color howlColor = Color.white;

    //Method on awake
    void Awake()
    {

    }

    //Method used to choose attack
    protected override IEnumerator Attack(int phase)
    {
        //When player stealth is over, reset sniffTime
        if (sniffTimer >= sniffedTimeEnd && !IsTgtStealthing())
            sniffTimer = 0f;

        //Check leaking
        if (phase >= 4 && !leaking)
        {
            leaking = true;
            SpawnLeak();
        }

        //Get movement
        float moveSpeedFactor = (phase >= 3) ? phaseThreeMoveSpeedBuff : 1.0f;

        //Randomly choose 1 of 4 attacks. Warwick cannot howl if he howled previously or scouted out player
        int selectMax = Mathf.Min(1 + phase, 4);
        int select = Random.Range(0, selectMax);
        if (prevAttack == 2 && (select == prevAttack || scoutedPlayer))
            select = Random.Range(0, 1) == 0 ? 0 : 3;
        prevAttack = select;

        if (select == 0)
            yield return StartCoroutine(Lunge(moveSpeedFactor));
        else if (select == 1)
            yield return StartCoroutine(HungeringStrike(moveSpeedFactor));
        else if (select == 2)
            yield return StartCoroutine(Howl());
        else if (select == 3)
        {
            yield return StartCoroutine(Lunge(moveSpeedFactor));
            yield return StartCoroutine(HungeringStrike(moveSpeedFactor));
        }

        //Reset scouted player flag
        scoutedPlayer = false;
        
    }


    //Method used to scout
    protected override IEnumerator Scout(int phase)
    {
        //Check leaking
        if (phase >= 4 && !leaking)
        {
            leaking = true;
            SpawnLeak();
        }

        GetComponent<SpriteRenderer>().color = sniffColor;

        while (!TgtVisible() && sniffTimer < sniffedTimeEnd)
        {
            yield return new WaitForFixedUpdate();
            sniffTimer += Time.fixedDeltaTime;
        }

        sniffTimer = sniffedTimeEnd;
    }

    //Method used upon discovery
    protected override IEnumerator Discovery()
    {
        scoutedPlayer = true;
        voiceSource.clip = discoveryClips[Random.Range(0, discoveryClips.Length)];
        voiceSource.Play(0);
        GetComponent<SpriteRenderer>().color = discoveryColor;

        yield return new WaitForSeconds(1.8f);

        GetComponent<SpriteRenderer>().color = normalColor;
    }


    //Method used to indicate when enemy has discovered you discovered enemy
    protected override bool FoundTgt()
    {
        return isTgtBleeding || sniffTimer >= sniffedTimeEnd;
    }

    //Method used to attack do dash attack
    private IEnumerator Lunge(float moveSpeedFactor)
    {
        //Set up lunge
        GetComponent<SpriteRenderer>().color = lungeColor;

        yield return new WaitForSeconds(0.4f);
        
        //Set up variables for lunge
        float lungeSpeed = GetMoveSpeed() * lungeSpeedFactor * moveSpeedFactor;
        lungeSpeed = Mathf.Max(lungeSpeed, minLungeSpeed);
        Vector3 lungeDir = GetTgtPos() - transform.position;
        lungeDir.Normalize();

        //Timer
        float timer = 0f;
        hitWall = false;
        bool dmgTgt = false;
        audioFX.clip = lungeClip;
        audioFX.Play(0);

        while (timer < lungeTime && !hitWall)
        {
            yield return new WaitForFixedUpdate();
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

        if (isActive)
        {
            GetComponent<SpriteRenderer>().color = normalColor;
        }

    }

    
    //Method to do spin attack
    private IEnumerator HungeringStrike(float moveSpeedFactor)
    {
        //Run towards player
        float timer = 0f;
        
        while (Vector3.Distance(GetTgtPos(), transform.position) > spinAttackDist && timer < spinAttackRun && TgtVisible() && isActive)
        {
            yield return new WaitForFixedUpdate();

            //Move towards
            Vector3 dir = GetTgtPos() - transform.position;
            dir.Normalize();
            transform.Translate(dir * GetMoveSpeed() * moveSpeedFactor * Time.fixedDeltaTime);

            //Update timer
            timer += Time.fixedDeltaTime;
        }

        //actually do spin attack if visible
        if (TgtVisible() && isActive)
        {
            GetComponent<SpriteRenderer>().color = spinColor;
            spinBox.Anticipation();
            yield return new WaitForSeconds(spinAttackAnticipation);
            audioFX.clip = strikeClip;
            audioFX.Play(0);
            spinBox.Attack(spinAttackDmg);
        }

        if (isActive)
        {
            GetComponent<SpriteRenderer>().color = normalColor;
        }
    }

    //Method to howl
    private IEnumerator Howl()
    {
        GetComponent<SpriteRenderer>().color = howlColor;
        howlBox.Anticipation();
        yield return new WaitForSeconds(howlAnticipation);
        audioFX.clip = howlClip;
        audioFX.Play(0);
        howlBox.Attack(slowFactor);

        if (isActive)
        {
            GetComponent<SpriteRenderer>().color = normalColor;
        }
    }

    //Invoke loop method to induce leaks
    private void SpawnLeak()
    {
        for (int i = 0; i < numPuddles; i++)
        {
            Vector3 spawnPoint = GetSpawnPos(leakPuddle.localScale);
            Transform curPuddle = Object.Instantiate(leakPuddle, spawnPoint, Quaternion.identity);
            curPuddle.GetComponent<SlowZone>().SetUp(leakSlow, puddleDuration);
        }

        Invoke("SpawnLeak", leakInterval);
        leakingSound.Play(0);
    }

    //Method for wall collisions
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Wall")
        {
            hitWall = true;
        }
    }
}
