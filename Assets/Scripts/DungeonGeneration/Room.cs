using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class Room : MonoBehaviour
{
    //Variables concerning room openings: {N, E, S, W}
    [SerializeField]
    private GameObject[] roomOpenings = null;
    private List<GameObject> doors;

    //Variables concerning exit, if it has any
    [SerializeField]
    private Exit exit = null;

    //Constant variables for camera snapping
    [SerializeField]
    private bool willCameraSnap = true;
    private const float SNAP_TIME = 0.4f;
    private const float CAMERA_OFFSET = -10f;

    //Variable to make and keep track of enemies
    [SerializeField]
    private int maxEnemies = 0;
    [SerializeField]
    private int minEnemies = 0;
    [SerializeField]
    private Transform enemy = null;
    [SerializeField]
    private RoomCollision spawnChecker = null;
    private int numEnemies;

    private const float X_EXTENTS = 7.0f;
    private const float Y_EXTENTS = 3.0f;

    bool activated = false;
    TwitchController player = null;

    //On awake, initialize variables
    void Awake()
    {
        doors = new List<GameObject>();
    }

    //Private method to instantiate enemies
    private void SpawnEnemies()
    {
        //Randomly get the number of enemies for this room
        numEnemies = Random.Range(minEnemies, maxEnemies + 1);
        
        //Make enemies 1 by 1
        for (int i = 0; i < numEnemies; i++)
        {
            //Set up enemy and create patrol points
            Vector3 enemySize = enemy.localScale;

            int numPoints = enemy.GetComponent<AbstractEnemy>().GetNumPatrolPoints();
            Vector3[] points = new Vector3[numPoints];

            for (int p = 0; p < points.Length; p++)
            {
                //Find a position within the room and test it
                Vector3 curPoint;

                do 
                {
                    float posY = Random.Range(-Y_EXTENTS, Y_EXTENTS);
                    float posX = Random.Range(-X_EXTENTS, X_EXTENTS);

                    curPoint = new Vector3(posX, posY, -1);
                    curPoint = transform.TransformPoint(curPoint);
                }
                while (!spawnChecker.ValidPoint(curPoint, enemySize));

                points[p] = curPoint;
            }

            //Instantiate enemy
            Transform curEnemy = Object.Instantiate(enemy, points[points.Length - 1], Quaternion.identity, transform);
            curEnemy.GetComponent<AbstractEnemy>().SetPatrolPoints(points);
            curEnemy.GetComponent<EntityStatus>().onDeathEvent.AddListener(OnEnemyDeath);
        }
    }

    //Method to enable openings given an array of booleans in NESW order
    //  If flipped, open door in SWNE
    public void SetOpenings(bool[] bpOpenings, bool flipped)
    {
        for (int i = 0; i < bpOpenings.Length; i++)
        {
            //Specific door you're opening to match with i
            int index = (flipped) ? ((i + (bpOpenings.Length / 2)) % bpOpenings.Length) : i;

            if (bpOpenings[i])
            {
                roomOpenings[index].SetActive(false);
                doors.Add(roomOpenings[index]);
            }
        }
    }

    //Public accessor method to get exit
    //  Returns null if there's no exit
    public Exit GetExit()
    {
        return exit;
    }

    //Method to activate room
    public IEnumerator Activate(TwitchController twitch)
    {
        //if not activated previously, activate
        if (!activated)
        {
            activated = true;

            //spawn and Activate all enemies
            SpawnEnemies();

            //Lock all doors
            if (numEnemies > 0)
            {
                for (int i = 0; i < doors.Count; i++)
                    doors[i].SetActive(true);

                player = twitch;
                twitch.provoked = true;
            }
        }

        //Camera snapping
        if (willCameraSnap)
        {
            //Set up variables
            float timer = 0f;
            Camera mainCam = Camera.main;
            mainCam.transform.parent = null;


            Vector3 start = mainCam.transform.position;
            Vector3 end = transform.position;
            end.z = CAMERA_OFFSET;

            while (timer < SNAP_TIME)
            {
                yield return new WaitForFixedUpdate();
                timer += Time.fixedDeltaTime;
                mainCam.transform.position = Vector3.Lerp(start, end, timer / SNAP_TIME);
            }

            mainCam.transform.position = end;
        }
    }

    //Signal handler method when enemy has died
    public void OnEnemyDeath()
    {
        numEnemies--;

        if (numEnemies <= 0)
            StartCoroutine(Deactivate());
    }

    //IEnumerator to deactivate
    IEnumerator Deactivate()
    {
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < doors.Count; i++)
            doors[i].SetActive(false);
        
        player.provoked = false;
    }
}
