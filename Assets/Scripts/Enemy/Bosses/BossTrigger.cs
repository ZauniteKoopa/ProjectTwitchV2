using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    //Boss that this trigger is connected to
    [SerializeField]
    private AbstractBoss boss = null;

    void OnTriggerEnter2D(Collider2D collider)
    {
        TwitchController twitch = collider.GetComponent<TwitchController>();

        if (twitch != null)
        {
            boss.Activate(twitch);
            Destroy(gameObject);
        }
    }
}
