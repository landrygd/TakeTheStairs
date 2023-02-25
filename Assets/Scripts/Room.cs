using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public readonly int id;
    public Vector2 position;
    private List<Room> nextRooms = new List<Room>();
    public int nextRoomsMax = 1;
    public Vector2 size = new Vector2(1,1);

    public Room(int id, Vector2 position)
    {
        this.id = id;
        this.position = position;
    }

    public void AddNextRoom(Room room)
    {
        nextRooms.Add(room);
    }

    public bool IsNextRoom(Room room)
    {
        return nextRooms.Find((r) => r.id == room.id) != null;
    }

    public List<Room> GetNextRooms()
    {
        return nextRooms;
    }

    public int GetMinX()
    {
        return ((int)position.x - (int)size.x / 2);
    }

    public int GetMaxX()
    {
        return ((int)position.x + (int)size.x / 2) + 1;
    }

    public int GetMinY()
    {
        return ((int)position.y - (int)size.y / 2);
    }

    public int GetMaxY()
    {
        return ((int)position.y + (int)size.y / 2) + 1;
    }
}
