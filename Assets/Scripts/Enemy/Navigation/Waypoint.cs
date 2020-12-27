using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour, IComparable<Waypoint>
{
    //Vertex adjacencies
    public Waypoint[] adj = null;

    //Navigation variables
    public bool visited;
    public float distFromStart;
    public float weight;
    public Waypoint backPointer;

    //On awake, initialize variables
    void Awake()
    {
        Reset();
    }

    //Reset method
    public void Reset()
    {
        visited = false;
        distFromStart = 0f;
        weight = 0f;
        backPointer = null;
    }

    //Method to get distance from vector
    //  Pre: pos MUST be in world space
    public float GetDistTo(Vector3 pos)
    {
        return Vector3.Distance(pos, transform.position);
    }

    //Method to visit this waypoint with a destination in mind
    //  if from is nullptr, assume that this is the start vertex
    public void Visit(Waypoint from, float distTravelled, Vector3 dest)
    {
        visited = true;
        backPointer = from;
        distFromStart = (from != null) ? distTravelled + from.distFromStart : 0;
        weight = distFromStart + Vector3.Distance(dest, transform.position);
    }

    //Accessor method to get waypoint position
    public Vector3 GetPos()
    {
        return transform.position;
    }

    //Method to compare waypoints to eachother
    //  Pre: Both waypoints MUST be visited for accurate comparison
    //  Post: Returns -1 if this instance is less, 0 if equal, and 1 if instance is more
    public int CompareTo(Waypoint other)
    {
        float ret = weight - other.weight;
        
        if (ret > 0f)
            return 1;
        else if (ret == 0f)
            return 0;
        else
            return -1;
    }
}
