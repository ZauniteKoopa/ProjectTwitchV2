using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MeleeAoE : MonoBehaviour
{
    //Method to keep track of player
    protected EntityStatus tgt;


    //On trigger methods to manage Twitch variable
    private void OnTriggerEnter2D(Collider2D collider)
    {
        TwitchController twitch = collider.GetComponent<TwitchController>();

        if (twitch != null)
        {
            tgt = twitch.GetComponent<EntityStatus>();
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        TwitchController twitch = collider.GetComponent<TwitchController>();

        if (twitch != null)
        {
            tgt = null;
        }
    }

    //Public method to activate anticipation
    public void Anticipation()
    {
        GetComponent<SpriteRenderer>().enabled = true;
    }

    //Public method to do damage
    public void Attack(float effectNumber)
    {
        if (tgt != null)
            AffectTarget(tgt, effectNumber);

        GetComponent<SpriteRenderer>().enabled = false;
    }


    //Abstract methods to override
    protected abstract void AffectTarget(EntityStatus tgt, float effectNumber);
}
