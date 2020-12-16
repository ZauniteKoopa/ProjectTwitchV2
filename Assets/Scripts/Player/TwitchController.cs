using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwitchController : MonoBehaviour
{
    //Attack hitbox properties
    [Header("Prefab attacks")]
    [SerializeField]
    private Transform arrowBolt = null;
    [SerializeField]
    private Transform poisonCask = null;

    //Player mobility properties
    [Header("Player stats")]
    private bool canMove;
    [SerializeField]
    private EntityStatus status = null;
    [SerializeField]
    private float attackMoveReduction = 0.6f;

    //Primary attack management
    [Header("Primary attack management")]
    [SerializeField]
    private int boltCost = 1;
    private PoisonVial boltVial = null;
    [SerializeField]
    private float fireRate = 0.35f;
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
    private int caskCost = 5;
    private PoisonVial caskVial = null;
    private bool canThrow;

    //Contaminate management
    [Header("Contamination management")]
    [SerializeField]
    private float conCD = 7.0f;
    [SerializeField]
    private ContaminateManager conManager = null;
    private bool canCon;

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

    //Audio Source Management
    [Header("Audio")]
    [SerializeField]
    private AudioSource audioFX = null;
    [SerializeField]
    private AudioClip caskThrowFX = null;
    [SerializeField]
    private AudioClip stealthingFX = null;

    //Initialize variables
    void Awake()
    {
        //Initialize flag variables
        fireTimerRunning = false;
        canMove = true;
        canThrow = true;
        canCon = true;
        invisible = false;
        attackBuffed = false;
        canStealth = true;

        //Initialize poisonVial variables
        caskVial = new PoisonVial(0, 2, 1, 2, Color.magenta, 20);
        boltVial = new PoisonVial(2, 0, 3, 0, Color.yellow, 20);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (canMove)
        {
            movement();

            if (!fireTimerRunning && Input.GetButtonDown("Fire1"))
                primaryAttack();

            if (canThrow && Input.GetButtonDown("Fire2"))
                StartCoroutine(throwCask());

            if (canStealth && Input.GetButtonDown("Mobility"))
                StartCoroutine(initiateStealth());

            if (canCon && Input.GetButtonDown("Contaminate"))
            {
                conManager.ContaminateAll();
                canCon = false;
                Invoke("refreshContaminate", conCD);
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

        transform.Translate(dir * status.GetCurSpeed() * Time.fixedDeltaTime * speedModifier);
    }

    //Method to create primary attack loop
    void primaryAttack()
    {
        if (Input.GetButton("Fire1"))
        {
            //set up and create projectile first
            Vector3 mousePos = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0);
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector2 dirVect = new Vector2 (mousePos.x - transform.position.x, mousePos.y - transform.position.y);

            Transform curBolt = Object.Instantiate (arrowBolt, transform);
            curBolt.GetComponent<PoisonProjBehav>().SetPoisonProj(dirVect, boltVial, true);
            curBolt.parent = null;

            //Buff player if player was previously invisible
            if (invisible)
                StartStealthAttackBuff();

            //Create timer for loop
            fireTimerRunning = true;
            float curFireRate = fireRate;
            curFireRate *= (attackBuffed) ? stealthFireRateBuff : 1.0f;

            Invoke("primaryAttack", curFireRate);
        }
        else
        {
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
        canMove = false;
        audioFX.clip = caskThrowFX;
        audioFX.Play();

        yield return new WaitForSeconds (throwTime);

        Transform curCask = Object.Instantiate (poisonCask, dirVector, Quaternion.identity, transform);
        curCask.GetComponent<PoisonBombBehav>().SetBomb (true, caskVial);
        curCask.parent = null;

        canThrow = false;
        canMove = true;
        Invoke("refreshCaskCD", throwCD);
    }
    
    //Method to call cause character to stealth
    IEnumerator initiateStealth()
    {
        //Get necessary variables
        SpriteRenderer render = GetComponent<SpriteRenderer>();
        float timer = 0.0f;

        //Initiate stealth delay
        audioFX.clip = stealthingFX;
        audioFX.Play();
        render.color = stealthDelayColor;
        yield return new WaitForSeconds(stealthDelay);

        //Actually make character invisible and do stealth
        invisible = true;
        render.color = stealthColor;
        while (invisible && timer < stealthDuration)
        {
            yield return new WaitForFixedUpdate();
            timer += Time.fixedDeltaTime;
        }

        //When get out of stealth, check if attack buff was activated or not (attacking in stealth will activate attack buff)
        if (invisible)
        {
            invisible = false;
            render.color = normalColor;
        }

        canStealth = false;
        Invoke("refreshStealth", stealthCD);
    }

    //Method to activate stealth attack buff when attacking, if invisible
    void StartStealthAttackBuff()
    {
        attackBuffed = true;
        invisible = false;
        GetComponent<SpriteRenderer>().color = stealthBuffColor;
        Invoke("endAttkStealthBuff", stealthBuffDuration);
    }

    //Methods to refresh cooldowns and effects. Called on invoke after attack sequences
    void refreshCaskCD()
    {
        canThrow = true;
    }

    void refreshStealth()
    {
        canStealth = true;
    }

    void refreshContaminate()
    {
        canCon = true;
    }

    void endAttkStealthBuff()
    {
        attackBuffed = false;
        GetComponent<SpriteRenderer>().color = normalColor;
    }
}
