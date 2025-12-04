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
        RoomGen roomGenScript = GetComponentInChildren<RoomGen>(); 
        roomGenScript.remainingChildRooms = TunnelLength;
        roomGenScript.IsStartingRoom = true;
        roomGenScript.seed = GetNextSeed(seed, TunnelLength);

        if (transform.CompareTag("FirstRoot")) SaveManager.SaveSeed();
    }
    
    void Update()
    {
        if (transform.childCount == 0)
        {
            count++;
            // seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            seed = GetNextSeed(seed + count, TunnelLength);
            
            GameObject roomGenerator = Instantiate( 
                roomGeneratorPrefab, 
                transform.position, 
                quaternion.identity, 
                transform);
            RoomGen roomGenScript = roomGenerator.GetComponent<RoomGen>();
            roomGenScript.remainingChildRooms = TunnelLength;
            roomGenScript.IsStartingRoom = true;
            roomGenScript.nextArea = nextArea;
            roomGenScript.seed = seed;
        }
    }

    int GetNextSeed(int parentSeed, int roomNumber)
    {
        return (parentSeed * 31) ^ roomNumber; // simple deterministic hash
    }
}
