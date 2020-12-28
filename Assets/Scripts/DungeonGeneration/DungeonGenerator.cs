using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    //Grid size
    [SerializeField]
    private int rows = 3;
    [SerializeField]
    private int cols = 3;

    //Starting location
    [SerializeField]
    private Vector3 originPos = Vector3.zero;
    
    //Room to instance with
    [SerializeField]
    private Transform room = null;

    //Constant variables for offset
    private const float VERTICAL_OFFSET = 12.0f;
    private const float HORIZONTAL_OFFSET = 20.0f;
    private const float Z_POS = 1f;


    // On start generate dungeon
    void Start()
    {
        float originX = originPos.x;
        float originY = originPos.y;

        //Make every room in the grid
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector3 curRoomPos = new Vector3(originX + c * HORIZONTAL_OFFSET, originY + r * VERTICAL_OFFSET, Z_POS);
                Object.Instantiate(room, curRoomPos, Quaternion.identity);
            }
        }

    }
}
