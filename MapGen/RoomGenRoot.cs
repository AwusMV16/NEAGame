using Unity.Mathematics;
using UnityEngine;

public class RoomGenRoot : MonoBehaviour
{
    [SerializeField] private GameObject roomGeneratorPrefab;
    [SerializeField] private GameObject nextArea;
    [SerializeField] private int TunnelLength = 5;
    void Start()
    {
        RoomGen roomGenScript = GetComponentInChildren<RoomGen>(); 
        roomGenScript.remainingChildRooms = TunnelLength;
        roomGenScript.IsStartingRoom = true;
    }
    void Update()
    {
        if (transform.childCount == 0)
        {
            GameObject roomGenerator = Instantiate(roomGeneratorPrefab, transform.position, quaternion.identity, transform);
            RoomGen roomGenScript = roomGenerator.GetComponent<RoomGen>();
            roomGenScript.remainingChildRooms = TunnelLength;
            roomGenScript.IsStartingRoom = true;
            roomGenScript.nextArea = nextArea;
        }
    }
}
