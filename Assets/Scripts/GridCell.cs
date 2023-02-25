using UnityEngine;

public class GridCell
{
    public Vector2 position = new Vector2();
    public RoomType roomType = RoomType.Void;

    public GridCell(Vector2 pos, RoomType type)
    {
        position = pos;
        roomType = type;
    }
}
