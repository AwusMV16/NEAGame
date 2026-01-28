using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class RoomGen : MonoBehaviour
{
    public bool IsStartingRoom;
    public bool MaxOfSameType;
    public int remainingChildRooms;
    private float rotation = -90f;
    public Transform exitPoint;
    public Transform entryPoint;
    private GameObject[] Pieces;
    private string nextRoomType;
    public string previousRoomType;
    public bool hasCollision = false;
    public GameObject roomParent;
    public GameObject nextArea;
    public SectionLibrary library;
    [SerializeField] private bool temp;
    public int seed;

    void Awake()
    {
        Pieces = library.RoomPrefabs;
    }

    void Start()
    {
        UnityEngine.Random.InitState(seed);
        if (!temp)
        {
            StartCoroutine(WaitAndCheck());
            createNextRoom();
        }
        if (!IsStartingRoom)
        {
            StartCoroutine(WaitAndCheck());
        }
    }
    
    private IEnumerator WaitAndCheck()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        CheckForOverlaps();
    }

    void createNextRoom()
    {
        if (remainingChildRooms > 0 && !hasCollision)
        {
            if (MaxOfSameType)
            {
                nextRoomType = previousRoomType == "Normal" ? "Mirrored" : "Normal";
            }
            else
            {
                nextRoomType = UnityEngine.Random.Range(0, 2) == 1 ? "Normal" : "Mirrored";
            }

            if ((previousRoomType == "Mirrored" && nextRoomType == "Normal") || (previousRoomType == "Mirrored" && nextRoomType == "Mirrored"))
            {
                rotation = 90f;
            }
            else if (previousRoomType == "Normal" && nextRoomType == "Mirrored")
            {
                rotation = -90f;
            }

            int randomIndex = UnityEngine.Random.Range(0, Pieces.Length);

            GameObject nextRoom = Instantiate(
                Pieces[randomIndex],
                exitPoint.position,
                Quaternion.Euler(0f, 0f, transform.eulerAngles.z + rotation)
            );
            //mirror the piece if B was chosen
            if(nextRoomType == "Mirrored") nextRoom.transform.localScale = Vector3.Scale(nextRoom.transform.localScale, new Vector3(-1, 1, 1));
            // Now align entrance to exit
            RoomGen nextRoomScript = nextRoom.GetComponent<RoomGen>();
            Vector3 offset = exitPoint.position - nextRoomScript.entryPoint.position;
            nextRoom.transform.position += offset;
            // Parent afterwards
            nextRoom.transform.SetParent(roomParent.transform, worldPositionStays: true);

            nextRoomScript.IsStartingRoom = false;
            nextRoomScript.remainingChildRooms = remainingChildRooms - 1;
            nextRoomScript.MaxOfSameType = previousRoomType == nextRoomType;
            nextRoomScript.previousRoomType = nextRoomType;
            nextRoomScript.hasCollision = hasCollision;
            nextRoomScript.roomParent = roomParent;
            nextRoomScript.seed = GetNextSeed(seed, remainingChildRooms);
            if (nextArea != null)
            {   
                nextRoomScript.nextArea = nextArea;
            }
            StartCoroutine(WaitAndCheck());
        }
        else if (remainingChildRooms == 0 && !hasCollision)
        {
            if ((previousRoomType == "Normal" && transform.eulerAngles.z == 0) || (previousRoomType == "Mirrored" && math.abs(transform.eulerAngles.z) == 180))
            {
                if (nextArea != null)
                {
                    GameObject newArea = Instantiate(
                        nextArea,
                        exitPoint.position,
                        Quaternion.identity // always 0 rotation for newArea
                    );
                    // Align its entrance to the current room’s exit
                    RoomGen newAreaScript = newArea.GetComponent<RoomGen>();
                    Vector3 offset = exitPoint.position - newAreaScript.entryPoint.position;
                    newArea.transform.position += offset;

                    // Parent afterwards
                    newArea.transform.SetParent(roomParent.transform, worldPositionStays: true);
                    newAreaScript.IsStartingRoom = false;
                    newAreaScript.remainingChildRooms = 0;
                    newAreaScript.hasCollision = hasCollision;
                    newAreaScript.roomParent = roomParent;
                    newAreaScript.seed = GetNextSeed(seed, remainingChildRooms + 1);
                    // newAreaScript.seed = seed;
                }

                StartCoroutine(WaitAndCheck());
            }
            else
            {
                remainingChildRooms += 1;
                createNextRoom();
            }
        }
        else if (hasCollision)
        {
            Destroy(roomParent);
        }
    }

    void CheckForOverlaps()
    {
        Collider2D thisCollider = GetComponent<Collider2D>();
        Collider2D[] results = new Collider2D[10];

        // Configure contact filter properly
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;  
        filter.useLayerMask = true; // check all layers
        filter.useDepth = false;     // ignore depth
        filter.SetLayerMask(LayerMask.GetMask("Room")); 

        int count = Physics2D.OverlapCollider(thisCollider, filter, results);

        if (count > 1) // More than just itself
        {
            for (int i = 0; i < count; i++)
            {
                if (results[i] == thisCollider || results[i] == null) continue;
                RoomGen otherRoom = results[i].GetComponent<RoomGen>();
                if (otherRoom != null)
                {
                    hasCollision = true;
                    Destroy(roomParent);    
                    break;
                }
            }
        }
    }

    int GetNextSeed(int parentSeed, int roomNumber)
    {
        return (parentSeed * 31) ^ roomNumber; // simple deterministic hash
    }
}

