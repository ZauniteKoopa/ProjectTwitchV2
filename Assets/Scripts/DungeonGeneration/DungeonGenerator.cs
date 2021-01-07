using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    //Enums for directions
    public enum DoorDir {North, East, South, West, NoDir};
    private static readonly int[] X_DIRS = {0, 1, 0, -1, 0};
    private static readonly int[] Y_DIRS = {1, 0, -1, 0, 0};
    private const int NUM_DIR = 4;

    //Grid size
    [SerializeField]
    private int rows = 3;
    [SerializeField]
    private int cols = 3;
    [SerializeField]
    private int numDungeons = 1;

    //Starting location
    [SerializeField]
    private Vector3 originPos = Vector3.zero;
    [SerializeField]
    private Exit initialEntry = null;
    [SerializeField]
    private Vector3 tgtDest = Vector3.zero;
    private Exit nextExit;
    
    //Room types to instance with
    public enum RoomType {Enemy, Start, End, Treasure}
    [SerializeField]
    private Transform[] rooms = null;
    [SerializeField]
    private Transform startRoom = null;
    [SerializeField]
    private Transform endRoom = null;
    [SerializeField]
    private Transform[] treasureRooms = null;
    [SerializeField]
    private int treasureRarity = 7;

    //Constant variables for offset
    private const float VERTICAL_OFFSET = 12.0f;
    private const float HORIZONTAL_OFFSET = 20.0f;
    private const float Z_POS = 1f;
    private const float DUNGEON_OFFSET = 100.0f;


    // On start generate dungeon
    void Start()
    {
        //Set initial exit as next exit
        nextExit = initialEntry;

        for (int i = 0; i < numDungeons; i++)
        {
            //Get initial blueprint of dungeon first and then make room from blueprint
            BP_Vertex[,] blueprint = MakeBlueprint(rows, cols);
            MakeRooms(blueprint, i);

            //Increment originPos
            originPos.x -= DUNGEON_OFFSET;
        }
    }


    //Method to create blueprint of map to be made using Prim's Algorithm
    //  Pre: rows > 0, cols > 0
    private BP_Vertex[,] MakeBlueprint(int rows, int cols)
    {
        //Set up entire grid
        BP_Vertex[,] blueprint = new BP_Vertex[rows, cols];
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                blueprint[r, c] = new BP_Vertex(r, c);

        //Total number of edges in grid where rows and cols > 1:
        //  CORNERS (8) + ROW_SIDES (3 * (R - 2) * 2) + COL_SIDES (3 * (C - 2) * 2) + CENTER (4 * (R - 2) * (C - 2))
        int totalEdges = 8 + (6 * (rows - 2)) + (6 * (cols - 2)) + (4 * (rows - 2) * (cols - 2));
        totalEdges /= 2;

        //Get min spanning tree in the grid graph
        int minEdges = GetMinSpanTree(blueprint);

        //Readd some edges
        ReaddEdges(blueprint, totalEdges - minEdges);

        //Assign room types in blueprint
        AssignRoomTypes(blueprint);

        return blueprint;
    }


    //Helper method to run Prim's Algorithm on blueprint graph
    //  Pre: Blueprint must have matching row and col indicators AND all booleans set to false
    //  Post: returns how many edges are in the min span tree
    private int GetMinSpanTree(BP_Vertex[,] blueprint)
    {
        int edgesAdded = 0;

        //Choose a random vertex in the grid as the start point and put in queue
        PriorityQueue<BP_Edge> primQueue = new PriorityQueue<BP_Edge>();
        int startRow = UnityEngine.Random.Range(0, blueprint.GetLength(0));
        int startCol = UnityEngine.Random.Range(0, blueprint.GetLength(1));
        BP_Edge initialEdge = new BP_Edge(blueprint[startRow, startCol], DoorDir.NoDir);
        primQueue.Enqueue(initialEdge);

        //Continously go through all the graph until all vertices have been visited
        while (!primQueue.IsEmpty())
        {
            //Pop edge out. Mark dest as marked and open doors on both vertices if any
            BP_Edge curEdge = primQueue.Dequeue();
            
            if (!curEdge.dest.visited)
            {
                curEdge.dest.visited = true;

                if (curEdge.direction != DoorDir.NoDir)
                {
                    edgesAdded++;

                    //Open dest's door
                    int oppositeDir = (int)curEdge.GetOppositeDir();
                    curEdge.dest.openings[oppositeDir] = true;
                    
                    //Open source door
                    BP_Vertex src = blueprint[curEdge.dest.row + Y_DIRS[oppositeDir], curEdge.dest.col + X_DIRS[oppositeDir]];
                    src.openings[(int)curEdge.direction] = true;
                }

                //Add all of this BP_Vertex's edges in queue if the edge exists AND dest is not visited
                for (int d = 0; d < NUM_DIR; d++)
                {
                    int destRow = curEdge.dest.row + Y_DIRS[d];
                    int destCol = curEdge.dest.col + X_DIRS[d];

                    if (WithinBPBounds(destRow, destCol, blueprint.GetLength(0), blueprint.GetLength(1)))
                    {
                        if (!blueprint[destRow, destCol].visited)
                        {
                            BP_Edge newEdge = new BP_Edge(blueprint[destRow, destCol], (DoorDir)d);
                            primQueue.Enqueue(newEdge);
                        }
                    }
                }
            }
        }

        return edgesAdded;
    }


    //Private helper method to attempt to readd edges to the blueprint
    //  Called after getting the min span tree for this graph
    private void ReaddEdges(BP_Vertex[,] blueprint, int edgesLeft)
    {
        int edgesToAdd = UnityEngine.Random.Range(1, (edgesLeft / 2) + 1);
        int numRows = blueprint.GetLength(0);
        int numCols = blueprint.GetLength(1);

        while (edgesToAdd > 0)
        {
            //Choose a random source vertex to add to
            int row = UnityEngine.Random.Range(0, numRows);
            int col = UnityEngine.Random.Range(0, numCols);

            DoorDir dir = blueprint[row, col].AddEdge(numRows, numCols);

            //Go to dest vertex and add edge there if successful
            if (dir != DoorDir.NoDir)
            {
                BP_Vertex dest = blueprint[row + Y_DIRS[(int)dir], col + X_DIRS[(int)dir]];
                int oppositeDir = ((int)dir + (NUM_DIR / 2)) % NUM_DIR;
                dest.openings[oppositeDir] = true;
                edgesToAdd--;
            }
        }
    }


    //Private helper method to assign room their own typing
    //  Blueprint must have bigger dimensions than 1x2 or 2x1 and all rooms are type "Enemy"
    //  TreasureRarity MUST have a bigger rarity than 1
    private void AssignRoomTypes(BP_Vertex[,] blueprint)
    {
        //Get numRows and numCols for reference
        int numRows = blueprint.GetLength(0);
        int numCols = blueprint.GetLength(1);
        Debug.Assert(numRows > 1);
        Debug.Assert(numCols > 1);
        Debug.Assert(treasureRarity > 1);

        //Assign start and end rooms. length between start and end room should be greater than 1
        BP_Vertex start = blueprint[UnityEngine.Random.Range(0, numRows), UnityEngine.Random.Range(0, numCols)];
        start.roomType = RoomType.Start;

        bool foundEnd = false;
        while (!foundEnd)
        {
            //Get a random end room and verify. Once verified, set that as the end
            BP_Vertex end = blueprint[UnityEngine.Random.Range(0, numRows), UnityEngine.Random.Range(0, numCols)];

            if (end.roomType == RoomType.Enemy && !AreNeighbors(start, end))
            {
                foundEnd = true;
                end.roomType = RoomType.End;
            }
        }

        //Set treasure rooms based on rarity
        int maxTreasure = ((numRows * numCols) / treasureRarity) + 1;
        int numTreasure = UnityEngine.Random.Range(0, maxTreasure);
        while(numTreasure > 0)
        {
            BP_Vertex room = blueprint[UnityEngine.Random.Range(0, numRows), UnityEngine.Random.Range(0, numCols)];

            if (room.roomType == RoomType.Enemy)
            {
                room.roomType = RoomType.Treasure;
                numTreasure--;
            }
        }
    }


    //Private helper method to actually make rooms from blueprint
    //  Pre: blueprint must be all set
    private void MakeRooms(BP_Vertex[,] blueprint, int dungeonNum)
    {
        //Get variables
        int rows = blueprint.GetLength(0);
        int cols = blueprint.GetLength(1);

        float originX = originPos.x;
        float originY = originPos.y;

        Exit curExit = nextExit;

        //Make every room in the grid
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                //Get current room position
                Vector3 curRoomPos = new Vector3(originX + c * HORIZONTAL_OFFSET, originY + r * VERTICAL_OFFSET, Z_POS);

                //Get template to use
                Transform curTemplate;
                RoomType curType = blueprint[r, c].roomType;

                if (curType == RoomType.Start)
                    curTemplate = startRoom;
                else if (curType == RoomType.End)
                    curTemplate = endRoom;
                else if (curType == RoomType.Treasure)
                    curTemplate = treasureRooms[UnityEngine.Random.Range(0, treasureRooms.Length)];
                else
                    curTemplate = rooms[UnityEngine.Random.Range(0, rooms.Length)];

                //Instatiate object and edit properties. 
                Transform curRoom = UnityEngine.Object.Instantiate(curTemplate, curRoomPos, Quaternion.identity);
                bool flipped = (curType == RoomType.Enemy && UnityEngine.Random.Range(0, 2) == 0);
                if (flipped)
                    curRoom.Rotate(0f, 0f, 180f);

                curRoom.GetComponent<Room>().SetOpenings(blueprint[r, c].openings, flipped);

                //If startRoom, set curExit's dest to that room. If endRoom, set up nextExit
                if (curType == RoomType.Start)
                    curExit.SetDest(curRoomPos);
                else if (curType == RoomType.End)
                {
                    Exit tempExit = curRoom.GetComponent<Room>().GetExit();

                    if (dungeonNum == numDungeons - 1)      //If last exit, just teleport to finishing dest
                        tempExit.SetDest(tgtDest);
                    else                                    //Else, set this exit as the nextExit
                        nextExit = tempExit;
                }
            }
        }
    }


    //Private helper method that checks if a number is within bounds of the 2D array
    private static bool WithinBPBounds(int r, int c, int numRows, int numCols)
    {
        bool rowInBounds = (r >= 0) && (r < numRows);
        bool colInBounds = (c >= 0) && (c < numCols);

        return rowInBounds && colInBounds;
    }

    
    //Private helper method to check if BP Vertices are next to each other and easily accessible
    //  If vertices are equal to each other, return true
    private bool AreNeighbors(BP_Vertex v1, BP_Vertex v2)
    {
        if (v1 == v2)
            return true;
        
        //Go through each directional neighbor v1 has
        for (int i = 0; i < NUM_DIR; i++)
        {
            int neighborRow = v1.row + Y_DIRS[i];
            int neighborCol = v1.col + X_DIRS[i];

            //If found to be a neighbor, check if it's accessible from v1
            if (v2.row == neighborRow && v2.col == neighborCol)
            {
                return v1.openings[i];
            }
        }

        //If found to not be a neighbor, return false.
        return false;
    }


    //Nested classes for making the blueprint for rooms: a vertex represents a room
    class BP_Vertex
    {
        public int row;
        public int col;
        public bool[] openings;
        public bool visited;
        public RoomType roomType;

        //Constructor
        public BP_Vertex(int r, int c)
        {
            row = r;
            col = c;

            roomType = RoomType.Enemy;
            visited = false;
            openings = new bool[NUM_DIR];
            for (int i = 0; i < NUM_DIR; i++)
                openings[i] = false;
        }

        //Method to randomly add an edge
        //  Return the doorDir of opened edge if successful. Return NoDir if not
        public DoorDir AddEdge(int numRows, int numCols)
        {
            //Randomly pick a first choice
            int firstChoice = UnityEngine.Random.Range(0, NUM_DIR);
            int curChoice = firstChoice;
            bool found = false;

            do
            {
                //Check if curChoice is both open and is still within bounds of the blueprint
                bool inBounds = DungeonGenerator.WithinBPBounds(row + Y_DIRS[curChoice], col + X_DIRS[curChoice], numRows, numCols);
                found = inBounds && !openings[curChoice];

                //If found, open the door. Else, go to next choice
                if (found)
                    openings[curChoice] = true;
                else
                    curChoice = (curChoice + 1) % NUM_DIR;

            }while(!found && curChoice != firstChoice);

            return (found) ? (DoorDir)curChoice : DoorDir.NoDir;
        }
    }


    //Nested class for handling Prim's Algorithm: a BP edge
    class BP_Edge : IComparable<BP_Edge>
    {
        // src --(direction)-> dest 
        public BP_Vertex dest;
        public DoorDir direction;

        // weight used for priority queue
        public int weight;

        //BP Edge constructor that gives each edge a random weight
        public BP_Edge(BP_Vertex tgt, DoorDir dir)
        {
            dest = tgt;
            direction = dir;
            weight = UnityEngine.Random.Range(1, 6);
        }

        //Get opposite Direction
        public DoorDir GetOppositeDir()
        {
            if (direction == DoorDir.NoDir)
                return DoorDir.NoDir;
            
            int opposite = ((int)direction + (NUM_DIR / 2)) % NUM_DIR;
            return (DoorDir)opposite;
        }

        //Compare to function
        public int CompareTo(BP_Edge other)
        {
            return weight - other.weight;
        }
    }
}
