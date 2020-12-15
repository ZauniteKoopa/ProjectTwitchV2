using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonBombBehav : MonoBehaviour
{
    [SerializeField]
    private float tickDuration = 0.5f;
    [SerializeField]
    private float initialTickDuration = 0.2f;
    [SerializeField]
    private int maxTick = 6;

    private float initialDamage = 2.0f;
    private float tickDamage = 0.5f;
    private string tgtTag = "";

    private bool initialPhase;
    private int curTick;
    private HashSet<Collider2D> effected;

    //Awake is called to set local variables
    void Awake()
    {
        initialPhase = true;
        curTick = 0;
        effected = new HashSet<Collider2D>();
    }


    // Start is called before the first frame update
    void Start()
    {
        Invoke("FinishInitial", initialTickDuration);
    }

    //Method to set bomb up
    public void SetBomb(bool isPlayer)
    {
        tgtTag = (isPlayer) ? "Enemy" : "Player";
    }
    

    //Method to stop initial damage phase
    void FinishInitial()
    {
        initialPhase = false;
        Invoke("ApplyTickDamage", tickDuration);
    }

    //Method to apply tick damage to all those are effected
    void ApplyTickDamage()
    {
        //Apply tick damage to all affected
        foreach (Collider2D enemy in effected)
        {
            Debug.Log("Do " + tickDamage + " damage to enemy");
        }

        //Increment curTick and check if object ready for destroy
        curTick++;

        if (curTick == maxTick)
        {
            Destroy(gameObject);
        }
        else
        {
            Invoke("ApplyTickDamage", tickDuration);
        }
    }

    //Method to check collisions and add objects effected HashSet
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == tgtTag)
        {
            if (initialPhase)
            {
                Debug.Log("Do initial bomb damage to enemy: " + initialDamage);
            }

            effected.Add(collider);
        }
    }

    //Method to check collisions and remove objects from effected hashset
    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.tag == tgtTag)
        {
            effected.Remove(collider);
        }
    }

}
