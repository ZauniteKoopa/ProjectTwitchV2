using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField]
    private Waypoint[] waypoints = null;

    //Method to get path from start to dest
    //  Pre: waypoints array.length > 0
    //  Post: returns a path of waypoints in reverse order
    public List<Waypoint> GetPathTo(Vector3 start, Vector3 tgt)
    {
        Debug.Assert(waypoints != null && waypoints.Length > 0);

        //Reset all waypoints and get the closest waypoint to start and tgt
        Waypoint s = null;
        Waypoint t = null;

        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i].Reset();

            //Check start
            if (s == null)
                s = waypoints[i];
            else
                s = (s.GetDistTo(start) > waypoints[i].GetDistTo(start)) ? waypoints[i] : s;

            //Check tgt
            if (t == null)
                t = waypoints[i];
            else
                t = (t.GetDistTo(tgt) > waypoints[i].GetDistTo(tgt)) ? waypoints[i] : t;
        }

        return ReactiveAStar(s, t);
    }

    //Method to get path away from dest
    //  Pre: waypoints array.length > 0
    //  Post: returns a path of waypoints in reverse order
    public List<Waypoint> GetPathAway(Vector3 start, Vector3 tgt)
    {
        Debug.Assert(waypoints != null && waypoints.Length > 0);

        //Reset all waypoints and get the closest waypoint to start and furtherest waypoint to tgt
        Waypoint s = null;
        Waypoint t = null;

        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i].Reset();

            //Check start
            if (s == null)
                s = waypoints[i];
            else
                s = (s.GetDistTo(start) > waypoints[i].GetDistTo(start)) ? waypoints[i] : s;

            //Check tgt
            if (t == null)
                t = waypoints[i];
            else
                t = (t.GetDistTo(tgt) < waypoints[i].GetDistTo(tgt)) ? waypoints[i] : t;
        }

        return ReactiveAStar(s, t);
    }

    //Reactive A* method: find a path from start waypoint to dest waypoint using A*: O(ElogV)
    //  Returns an array of waypoints with dest at index = 0 and start at index = length - 1;
    //          if there's no path, returns an empty list
    private List<Waypoint> ReactiveAStar(Waypoint s, Waypoint t)
    {
        Debug.Assert(s != null);
        Debug.Assert(t != null);

        //Set up search by pushing start in first
        PriorityQueue<Waypoint> navQueue = new PriorityQueue<Waypoint>();
        Vector3 tgtDest = t.transform.position;
        s.Visit(null, 0, tgtDest);
        navQueue.Enqueue(s);

        //Start making path
        while (!t.visited && !navQueue.IsEmpty())
        {
            //Get the first waypoint in the queue
            Waypoint cur = navQueue.Dequeue();

            //Visit all vertex adjacencies if they have not been visited yet
            for (int i = 0; i < cur.adj.Length; i++)
            {
                Waypoint curAdj = cur.adj[i];

                if (!curAdj.visited)
                {
                    float distTravel = cur.GetDistTo(curAdj.transform.position);
                    curAdj.Visit(cur, distTravel, tgtDest);
                    navQueue.Enqueue(curAdj);
                }
            }
        }

        //Make a path in the list in reverse order using waypoint back pointers
        List<Waypoint> path = new List<Waypoint>();

        if (t.visited)
        {
            Waypoint cur = t;
            path.Add(t);

            while (cur.backPointer != null)
            {
                cur = cur.backPointer;
                path.Add(cur);
            }
        }

        return path;
    }
}
