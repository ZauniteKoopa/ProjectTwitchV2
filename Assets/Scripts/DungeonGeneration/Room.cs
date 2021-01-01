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
    private int numEnemies;
    private List<Transform> enemies;

    private const float X_EXTENTS = 7.0f;
    private const float Y_EXTENTS = 3.0f;

    bool activated = false;
    TwitchController player = null;

    //On awake, initialize variables
    void Awake()
    {
        doors = new List<GameObject>();
        enemies = new List<Transform>();
    }

    //On start, instantiate enemies
    public void SpawnEnemies()
    {
        numEnemies = Random.Range(minEnemies, maxEnemies + 1);
        
        for (int i = 0; i < numEnemies; i++)
        {
            int numPoints = enemy.GetComponent<AbstractEnemy>().GetNumPatrolPoints();
            Vector3[] points = new Vector3[numPoints];

            for (int p = 0; p < points.Length; p++)
            {
                float posY = Random.Range(-Y_EXTENTS, Y_EXTENTS);
                float posX = Random.Range(-X_EXTENTS, X_EXTENTS);

                Vector3 curPoint = new Vector3(posX, posY, -1);
                points[p] = transform.TransformPoint(curPoint);
            }

            Transform curEnemy = Object.Instantiate(enemy, points[points.Length - 1], Quaternion.identity, transform);
            curEnemy.GetComponent<AbstractEnemy>().SetPatrolPoints(points);
            curEnemy.GetComponent<EntityStatus>().onDeathEvent.AddListener(OnEnemyDeath);
            curEnemy.gameObject.SetActive(false);
            enemies.Add(curEnemy);
        }
    }

    //Method to enable openings given an array of booleans in NESW order
    public void SetOpenings(bool[] bpOpenings)
    {
        for (int i = 0; i < bpOpenings.Length; i++)
        {
            if (bpOpenings[i])
            {
                roomOpenings[i].SetActive(false);
                doors.Add(roomOpenings[i]);
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

            //Activate all enemies
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].gameObject.SetActive(true);
            }

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
