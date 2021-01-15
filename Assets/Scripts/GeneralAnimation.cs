using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeneralAnimation
{
    /*OFFICIAL MAPPING FOR AHOR AND AVERT:
        0, 4 --> 0  (No Direction)
        1-3 ---> 1  (Right / Up Direction)
        5-7 ---> -1 (Left / Down Direction)
    */

    //Constants for orientation updating
    private const int DEGREE_DIV = 45;
    private const float OFFSET = 22.5f;


    //Method to get animation orientation
    //  Sets aHor and aVert of anim according to key values listed above
    public static void UpdateAnimOrientation(Vector3 dir, Animator anim, SpriteRenderer sprite)
    {
        //Calculate the angle of the vector 
        float deg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        deg = (deg < 0) ? deg + 360 + OFFSET : deg + OFFSET;
        deg = (deg >= 360) ? deg - 360 : deg;

        //Calculate the key value
        int keyValue = (int)(deg / DEGREE_DIV);
        int adjKeyValue = (keyValue + 2) % 8;   //Adjust key value so that aHorizontal has the same mapping as aVertical

        //Calculate the new attack vertical and attack horizontal orientation
        int newAVert = (keyValue % 4 == 0) ? 0 : -1 * (int)Mathf.Sign(keyValue - 4f);
        int newAHor = (adjKeyValue % 4 == 0) ? 0 : -1 * (int)Mathf.Sign(adjKeyValue - 4f);

        //Set new animation parameters and check to see if flipping necessary
        sprite.flipX = (newAHor < 0) ? false : true;
        anim.SetInteger("aHor", newAHor);
        anim.SetInteger("aVert", newAVert);
    }
}
