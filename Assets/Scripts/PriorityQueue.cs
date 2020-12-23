using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A priority queue that uses a min heap
public class PriorityQueue<T> where T : IComparable<T>
{
    //The mean heap as represented as an array
    private List<T> queue;

    //Constructor class
    public PriorityQueue()
    {
        queue = new List<T>();
    }

    //Adds an element to the priority queue
    public void Enqueue(T element)
    {
        //Add the element to the end of the list
        queue.Add(element);

        //Continuously swap with parent, if parent element is smaller: i - 1 = actual Index
        int i = queue.Count;
        while (i != 1 && queue[i - 1].CompareTo(queue[(i / 2) - 1]) < 0)
        {
            T temp = queue[i - 1];
            queue[i - 1] = queue[(i/2) - 1];
            queue[(i/2) - 1] = temp;

            i /= 2;
        }
    }

    //Removes an element from the priority queue. Always removes the top of the queue
    //  If queue is empty, returns null
    public T Dequeue()
    {
        if (queue.Count == 0)
            return default(T);
        
        //Get element to return.
        T top = queue[0];

        //Remove the last element and move it to the top
        queue[0] = queue[queue.Count - 1];
        queue.RemoveAt(queue.Count - 1);

        //Continously swap top element with children if children are smaller: i - 1 = actual Index in array
        int i = 1;
        bool digging = true;
        while (2 * i <= queue.Count && digging)
        {
            if (queue[2 * i - 1].CompareTo(queue[i - 1]) < 0)
            {
                T temp = queue[i - 1];
                queue[i - 1] = queue[2 * i - 1];
                queue[2 * i - 1] = temp;
            }
            else if (queue[2 * i].CompareTo(queue[i - 1]) < 0)
            {
                T temp = queue[i - 1];
                queue[i - 1] = queue[2 * i];
                queue[2 * i] = temp;
            }
            else
            {
                digging = false;
            }
        }

        //Return top most element
        return top;
    }

    //Method to look at the topmost element
    public T Front()
    {
        return queue[0];
    }
}
