using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwitchController : MonoBehaviour
{
    //Attack hitbox properties
    [SerializeField]
    private Transform arrowBolt = null;
    [SerializeField]
    private Transform poisonCask = null;

    //Player mobility properties
    private bool canMove;
    [SerializeField]
    private float moveSpeed = 0.0f;
    [SerializeField]
    private float attackMoveReduction = 0.6f;

    //Primary attack management
    [SerializeField]
    private float primaryDmg = 1.0f;
    [SerializeField]
    private float fireRate = 0.35f;
    private bool fireTimerRunning;

    //Secondary attack management
    [SerializeField]
    private float throwTime = 0.3f;
    [SerializeField]
    private float throwCD = 1.5f;
    [SerializeField]
    private float maxThrowDist = 5f;
    private bool canThrow;


    //Initialize variables
    void Awake()
    {
        fireTimerRunning = false;
        canMove = true;
        canThrow = true;
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
        }
    }

    //Method to call upon movement
    void movement()
    {
        float hDir = Input.GetAxis("Horizontal");
        float vDir = Input.GetAxis("Vertical");
        float reduction = (Input.GetButton("Fire1")) ? attackMoveReduction : 1.0f;

        Vector3 dir = new Vector3(hDir, vDir, 0);
        dir.Normalize();

        transform.Translate(dir * moveSpeed * Time.fixedDeltaTime * reduction);
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
            curBolt.GetComponent<ProjectileBehav>().SetProj(dirVect, primaryDmg, true);
            curBolt.parent = null;

            //Create timer for loop
            fireTimerRunning = true;
            Invoke("primaryAttack", fireRate);
        }
        else
        {
            fireTimerRunning = false;
        }
    }

    /* Throws cask at mouse direction with distance of MAX_THROW_DIST units or less */
    IEnumerator throwCask ()
    {
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

        /* Execute action: disable movement for some time and throw cask */
        canMove = false;

        yield return new WaitForSeconds (throwTime);

        Transform curCask = Object.Instantiate (poisonCask, dirVector, Quaternion.identity, transform);
        curCask.GetComponent<PoisonBombBehav>().SetBomb (true);
        curCask.parent = null;

        canThrow = false;
        canMove = true;
        Invoke("refreshCaskCD", throwCD);
    }

    void refreshCaskCD()
    {
        canThrow = true;
    }
    
}
