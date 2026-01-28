using Unity.Mathematics;
using UnityEngine;

public class RoomGenRoot : MonoBehaviour
{
    [SerializeField] private GameObject roomGeneratorPrefab;
    [SerializeField] private GameObject nextArea;
    [SerializeField] private int TunnelLength = 5;
    public int seed;
    private int count;

    void Start()
    {
        UnityEngine.Random.InitState((int)System.DateTime.UtcNow.Ticks);
        if (transform.CompareTag("FirstRoot"))
        {
            if (!GameSession.loadSavedGame)
            {
                seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                SaveManager.SaveSeed();
            }
            Debug.Log(seed);
        }
        else
        {
            seed = transform.parent.GetComponent<RoomGen>().seed;
        }

        RoomGen roomGenScript = GetComponentInChildren<RoomGen>(); 
        roomGenScript.remainingChildRooms = TunnelLength;
        roomGenScript.IsStartingRoom = true;
        roomGenScript.seed = GetNextSeed(seed, TunnelLength);
    }
    
    void Update()
    {
        if (transform.childCount == 0)
        {
            count++;
            
            GameObject roomGenerator = Instantiate( 
                roomGeneratorPrefab, 
                transform.position, 
                quaternion.identity, 
                transform);
            RoomGen roomGenScript = roomGenerator.GetComponent<RoomGen>();
            roomGenScript.remainingChildRooms = TunnelLength;
            roomGenScript.IsStartingRoom = true;
            roomGenScript.nextArea = nextArea;
            roomGenScript.seed = GetNextSeed(seed + count, TunnelLength);;
        }
    }

    int GetNextSeed(int parentSeed, int roomNumber)
    {
        return (parentSeed * 31) ^ roomNumber; // simple deterministic hash
    }
}
