using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    //Variables concerning room openings: {N, E, S, W}
    [SerializeField]
    private GameObject[] roomOpenings = null;
    private List<GameObject> doors;

    //On awake, initialize variables
    void Awake()
    {
        doors = new List<GameObject>();
    }

    //Method to enable openings given an array of booleans in NESW order
    public void SetOpenings(bool[] bpOpenings)
    {
        for (int i = 0; i < bpOpenings.Length; i++)
        {
            if (bpOpenings[i])
            {
                roomOpenings[i].SetActive(false);
                doors.Add(roomOpenings[i]);
            }
        }
    }
}
