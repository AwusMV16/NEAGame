using UnityEngine;

public static class PlayerService
{
    public static Transform transform { get; private set; }
    public static Vector3 position { get; private set; }

    public static void Register(Transform player)
    {
        transform = player;
        position = player.position;
    }

    public static void UpdatePosition(Vector3 pos)
    {
        position = pos;
    }
}