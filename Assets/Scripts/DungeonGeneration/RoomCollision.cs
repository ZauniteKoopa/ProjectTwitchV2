using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Class that keeps track of potential collisions when spawning enemies
public class RoomCollision : MonoBehaviour
{

    //List of collisions collected on awake (objects tagged "wall" should not move)
    List<Collider2D> collisions;


    // Awake initializes variables
    void Awake()
    {
        collisions = new List<Collider2D>();
    }

    //If a wall collision is in trigger, put it in list
    void OnTriggerEnter2D(Collider2D collider)
    {

        if (collider.tag == "Wall")
        {
            collisions.Add(collider);
        }
    }

    //Method to check if a point is available given a position and size
    public bool ValidPoint(Vector3 point, Vector3 size)
    {
        Bounds testBound = new Bounds(point, size * 1.25f);

        for (int i = 0; i < collisions.Count; i++)
        {
            if (testBound.Intersects(collisions[i].bounds))
            {
                return false;
            }
        }

        return true;
    }
}
