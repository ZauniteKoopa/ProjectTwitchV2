using System.Collections;
using System.Collections.Generic;
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

    //On awake, initialize variables
    void Awake()
    {
        doors = new List<GameObject>();
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

    //Method to snap camera to a room
    public IEnumerator CameraSnap()
    {
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

}
