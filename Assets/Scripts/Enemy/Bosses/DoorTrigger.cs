using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    //Entity status to focus on
    [SerializeField]
    private EntityStatus enemy = null;
    [SerializeField]
    private GameObject[] doors = null;
    private bool locked;

    // Start is called before the first frame update
    void Start()
    {
        locked = false;
        enemy.onDeathEvent.AddListener(UnlockDoors);
    }

    //Method to Unlock door
    public void UnlockDoors()
    {

        for (int i = 0; i < doors.Length; i++)
        {
            doors[i].SetActive(false);
        }
    }

    //Method to lock doors upon player entering
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (!locked)
        {
            TwitchController twitch = collider.GetComponent<TwitchController>();

            if (twitch != null)
            {
                locked = true;

                for (int i = 0; i < doors.Length; i++)
                    doors[i].SetActive(true);
            }
        }
    }
}
