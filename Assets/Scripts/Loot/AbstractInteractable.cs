using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractInteractable : MonoBehaviour
{
    //Player to keep track of
    private TwitchController player;
    bool canActivate;
        
    // Start is called before the first frame update
    void Awake()
    {
        player = null;
        canActivate = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (canActivate && player != null && Input.GetButtonDown("Contaminate"))
        {
            canActivate = false;
            StartCoroutine(Interact(player));
        }
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


    //Protected method to override canActivate
    protected void SetCanActivate(bool status)
    {
        canActivate = status;
    }

    //Abstract method to override
    protected abstract IEnumerator Interact(TwitchController twitch);
}
