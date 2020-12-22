using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisibilityRange : MonoBehaviour
{
    //HashSet of Collider2Ds to keep track of who's in invisibility range
    HashSet<Collider2D> inRange;

    // Start is called before the first frame update
    void Awake()
    {
        inRange = new HashSet<Collider2D>();
    }

    // If someone enters a trigger and is an enemy, put them in the hashset
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == "Enemy")
        {
            inRange.Add(collider);
        }
    }

    //If someone exits trigger and is an enemy, remove them from hashset
    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.tag == "Enemy")
        {
            inRange.Remove(collider);
        }
    }

    //Method to check if someone is in range
    public bool IsInRange(Collider2D collider)
    {
        return inRange.Contains(collider);
    }
}
