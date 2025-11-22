using System.Collections.Generic;
using UnityEngine;

public class GenerateMap : MonoBehaviour
{
    [SerializeField] private GameObject LD;
    [SerializeField] private GameObject RD;
    [SerializeField] private GameObject RL;
    [SerializeField] private GameObject SD;
    [SerializeField] private GameObject SL;
    [SerializeField] private GameObject SOLID;
    [SerializeField] private GameObject SR;
    [SerializeField] private GameObject UD;
    [SerializeField] private GameObject UL;
    [SerializeField] private GameObject UR;
    private Dictionary<string, GameObject> RoomPrefabs;

    void Start()
    {
        RoomPrefabs = new Dictionary<string, GameObject>
        {
            { "LD", LD },
            { "RD", RD },
            { "RL", RL },
            { "LR", RL },
            { "SD", UD },
            { "SL", UL },
            { "SOLID", SOLID },
            { "SR", UR },
            { "UD", UD },
            { "UL", UL },
            { "UR", UR }
        };

        SpreadPoints();
        transform.Rotate(0, 0, 90f);
        transform.Translate(0, 10.5f - 21f, 0);
    }

    void SpreadPoints()
    {
        List<string>[,] grid = new List<string>[6, 6];
        for (int y = 0; y < 6; y++)
        {
            for (int x = 0; x < 6; x++)
            {
                grid[x, y] = new List<string>();
            }
        }

        int currentY = 0;
        int currentX = 0;

        grid[currentX, currentY].Add("S");

        List<(string dir, Vector2Int pos)> roomList = new List<(string, Vector2Int)>
        {
            ("S", new Vector2Int(currentX, currentY))
        };

        //step 2
        bool done = false;
        int count = 0;
        int max_RL = 5;
        while (!done)
        {
            int choice = UnityEngine.Random.Range(0, 3);

            if (choice == 0 && currentX + 1 <= 5 && grid[currentX + 1, currentY].Count == 0 && count < max_RL)
            {
                currentX += 1;
                count += 1;
                roomList.Add(("R", new Vector2Int(currentX, currentY))); //RL
                grid[currentX, currentY].Add(">");
            }
            else if (choice == 1 && currentX - 1 >= 0 && grid[currentX - 1, currentY].Count == 0 && count < max_RL)
            {
                currentX -= 1;
                count += 1;
                roomList.Add(("L", new Vector2Int(currentX, currentY))); //RL
                grid[currentX, currentY].Add("<");
            }
            else if (choice == 2 && currentY + 1 <= 5 && grid[currentX, currentY + 1].Count == 0)
            {
                currentY += 1;
                count = 0;
                roomList.Add(("D", new Vector2Int(currentX, currentY)));
                grid[currentX, currentY].Add("v");
            }

            if (currentY == 5)
            {
                int endChoice = UnityEngine.Random.Range(0, 2);
                if (endChoice == 0 && currentX + 1 <= 5 && grid[currentX + 1, currentY].Count == 0 && count < max_RL)
                {
                    currentX += 1;
                    roomList.Add(("R", new Vector2Int(currentX, currentY)));
                    roomList.Add(("D", new Vector2Int(currentX, currentY)));
                    grid[currentX, currentY].Add("E");
                }
                else if (endChoice == 1 && currentX - 1 >= 0 && grid[currentX - 1, currentY].Count == 0 && count < max_RL)
                {
                    currentX -= 1;
                    roomList.Add(("L", new Vector2Int(currentX, currentY)));
                    roomList.Add(("D", new Vector2Int(currentX, currentY)));
                    grid[currentX, currentY].Add("E");
                }
                done = true;
            }
        }

        //step 3
        List<(string, string)> doorList = new List<(string, string)>();
        List<(string dir, Vector2Int pos)> doorListCombined = new List<(string dir, Vector2Int pos)>();


        // Build door pairs
        for (int i = 0; i < roomList.Count - 1; i++)
        {
            doorList.Add((roomList[i].dir, roomList[i + 1].dir));
        }

        // Process each door pair
        for (int i = 0; i < doorList.Count; i++)
        {
            string door1 = "";
            string door2 = "";

            switch (doorList[i].Item1)
            {
                case "S":
                    door1 = "S";
                    break;
                case "R":
                    door1 = "L";
                    break;
                case "L":
                    door1 = "R";
                    break;
                case "D":
                    door1 = "U";
                    break;
            }

            if (doorList[i].Item2 != "E")
            {
                door2 = doorList[i].Item2;
            }

            doorListCombined.Add((door1 + door2, roomList[i].pos));
        }
        
        for (int y = 0; y < 6; y++)
        {
            for (int x = 0; x < 6; x++)
            {
                bool doorFound = false;
                for (int k = 0; k < doorListCombined.Count; k++)
                {
                    if (doorListCombined[k].pos.x == x && doorListCombined[k].pos.y == y)
                    {
                        doorFound = true;
                        UnityEngine.Quaternion rot = UnityEngine.Quaternion.identity;
                        if (doorListCombined[k].dir == "RD" || doorListCombined[k].dir == "SD" || doorListCombined[k].dir == "UD")
                        {
                            rot = UnityEngine.Quaternion.Euler(0, 0, 90);
                        }
                        else if (doorListCombined[k].dir == "SR" || doorListCombined[k].dir == "UR")
                        {
                            rot = UnityEngine.Quaternion.Euler(0, 0, 180);
                        }
                        else if (doorListCombined[k].dir == "UL" || doorListCombined[k].dir == "LU")
                        {
                            rot = UnityEngine.Quaternion.Euler(0, 0, -90);
                        }


                        // Debug.Log(roomList[0].pos.x * 21 + ", " + roomList[0].pos.y * 21);
                        
                        Instantiate(RoomPrefabs[doorListCombined[k].dir], transform.position + new UnityEngine.Vector3(x * 21, -y * 21, 0), rot, transform);

                        break;
                    }
                }
                if (!doorFound)
                {
                    Instantiate(RoomPrefabs["SOLID"], transform.position + new UnityEngine.Vector3(x * 21, -y * 21, 0), UnityEngine.Quaternion.identity, transform);
                }
            }
        }
    }
}