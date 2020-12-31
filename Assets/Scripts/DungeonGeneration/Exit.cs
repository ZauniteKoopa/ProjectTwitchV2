using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : MonoBehaviour
{
    //Room to teleport to
    private Vector3 dest = Vector3.zero;
    private TwitchController player;

    // Start is called before the first frame update
    void Awake()
    {
        player = null;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //If enter stage, attach camera back to player and teleport to dest
        if (player != null && Input.GetButtonDown("Contaminate"))
        {
            Vector3 camPos = player.transform.position;
            camPos.z = -10;
            Camera.main.transform.position = camPos;
            Camera.main.transform.parent = player.transform;

            player.transform.position = dest;
        }
    }

    //Method to set destination point
    public void SetDest(Vector3 pos)
    {
        pos.z = -1;
        dest = pos;
    }

    //Method to check collisions
    private void OnTriggerEnter2D(Collider2D collider)
    {
        TwitchController twitch = collider.GetComponent<TwitchController>();

        if (twitch != null)
        {
            player = twitch;
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        TwitchController twitch = collider.GetComponent<TwitchController>();

        if (twitch != null)
        {
            player = null;
        }
    }
}
