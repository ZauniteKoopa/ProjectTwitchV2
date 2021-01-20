using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowZone : MonoBehaviour
{
    //Variables to keep track of
    float slowFactor = 1f;
    float duration = 2f;
    float anticipation = 0.5f;
    bool active = false;

    //Variable to keep track of tgt
    EntityStatus tgt = null;

    //Colors to keep track of
    [SerializeField]
    Color anticipationColor = Color.white;
    [SerializeField]
    Color activeColor = Color.white;


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Activate());
    }

    //Activate sequence
    IEnumerator Activate()
    {
        //Start anticipation
        GetComponent<SpriteRenderer>().color = anticipationColor;
        active = false;

        yield return new WaitForSeconds(anticipation);

        GetComponent<SpriteRenderer>().color = activeColor;
        if (tgt != null)
            tgt.ChangeSpeed(slowFactor);
        
        active = true;
        Invoke("Destroy", duration);
    }

    //On trigger methods to manage Twitch variable
    private void OnTriggerEnter2D(Collider2D collider)
    {
        TwitchController twitch = collider.GetComponent<TwitchController>();

        if (twitch != null)
        {
            tgt = twitch.GetComponent<EntityStatus>();

            if (active)
                tgt.ChangeSpeed(slowFactor);
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        TwitchController twitch = collider.GetComponent<TwitchController>();

        if (twitch != null)
        {
            if (active)
                tgt.ChangeSpeed(1f / slowFactor);
            
            tgt = null;
        }
    }

    //Method to set up zone
    public void SetUp(float sf, float dur)
    {
        slowFactor = sf;
        duration = dur;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Destroy()
    {
        Destroy(gameObject);
    }
}
