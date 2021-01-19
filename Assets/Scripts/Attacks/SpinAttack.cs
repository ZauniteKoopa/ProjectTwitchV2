using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinAttack : MonoBehaviour
{
    //Method to keep track of player
    private EntityStatus tgt;


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
    public void Attack(float dmg)
    {
        if (tgt != null)
            tgt.DamageEntity(dmg);

        GetComponent<SpriteRenderer>().enabled = false;
    }
}
