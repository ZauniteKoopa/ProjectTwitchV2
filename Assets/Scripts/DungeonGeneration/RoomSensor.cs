using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSensor : MonoBehaviour
{
    //Room to report to
    [SerializeField]
    private Room room = null;

    void Start()
    {

    }

    //On trigger enter, activate room
    void OnTriggerEnter2D(Collider2D collider)
    {
        TwitchController twitch = collider.GetComponent<TwitchController>();

        if (twitch != null)
        {
            StartCoroutine(room.Activate());
        }
    }

    //On trigger exit, deactivate room
    void OnTriggerExit2D(Collider2D collider)
    {

    }
}
