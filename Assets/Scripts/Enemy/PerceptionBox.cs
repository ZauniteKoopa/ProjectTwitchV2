using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerceptionBox : MonoBehaviour
{
    //Enemy behavior to refer to
    [SerializeField]
    private AbstractEnemy enemy = null;
    private bool active;

    // Start is called before the first frame update
    void Awake()
    {
        active = true;
    }

    //If player enters trigger, set enemy tgt
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (active)
        {
            TwitchController player = collider.GetComponent<TwitchController>();

            if (player != null)
            {
                active = false;
                enemy.SetTgt(player);
            }
        }
    }
}
