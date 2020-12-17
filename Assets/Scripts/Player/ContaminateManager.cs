using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContaminateManager : MonoBehaviour
{
    //Data structure to hold those in range
    private HashSet<EntityStatus> effected;

    //Initialize data structure on awake
    void Awake()
    {
        effected = new HashSet<EntityStatus>();
    }

    //If an enemy is in range, add them to the effected set
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == "Enemy")
        {
            EntityStatus tgtStatus = collider.GetComponent<EntityStatus>();
            if (tgtStatus != null)
                effected.Add(tgtStatus);
        }
    }

    //If an enemy leaves the range, remove them from the effected set
    void OnTriggerExit2D(Collider2D collider)
    {
        EntityStatus tgtStatus = collider.GetComponent<EntityStatus>();

        if (tgtStatus != null && collider.tag == "Enemy")
            effected.Remove(tgtStatus);
    }

    //Public method to contamate all enemies
    public void ContaminateAll()
    {
        if (effected.Count > 0)
        {
            //Move all enemies to copy to not destroy iteration
            List<EntityStatus> tempEnemies = new List<EntityStatus>();
            foreach(EntityStatus enemy in effected)
            {
                if (enemy != null)
                    tempEnemies.Add(enemy);
            }

            //Damage enemies
            foreach(EntityStatus enemy in tempEnemies)
            {
                if (enemy != null)
                    enemy.Contaminate();
            }
        }
        
    }

    //Method to check if you can contaminate: must have one person poisoned in range
    public bool CanContaminate()
    {
        if (effected.Count == 0)
            return false;

        //Move all enemies to copy to not destroy iteration
        List<EntityStatus> tempEnemies = new List<EntityStatus>();
        foreach(EntityStatus enemy in effected)
        {
            if (enemy != null)
                tempEnemies.Add(enemy);
        }

        //Damage enemies
        foreach(EntityStatus enemy in tempEnemies)
        {
            if (enemy.GetPoisonStacks() > 0)
                return true;
        }

        return false;
    }
}
