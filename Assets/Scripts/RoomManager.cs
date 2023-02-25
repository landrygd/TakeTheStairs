using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public int roomCount = 10;
    public int roomNextMax = 3;
    public float distanceMin = 50;
    public float distanceMax = 100;

    public Vector2Int stageSize = new Vector2Int(10, 10);
    
    private List<Room> roomList = new List<Room>();
    private List<GridCell> grid = new List<GridCell>();
    
    public int edgeForce = 1000;
    public int anchorForce = 1000;

    public GameObject wallPrefab;
    public GameObject floorPrefab;

    // Start is called before the first frame update
    void Start()
    {
        GenerateStage();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void GenerateStage()
    {
        GenerateGraph();
    }

    void AdjustPosition()
    {
        foreach (Room room in roomList)
        {
            Vector2 resultVector = new Vector2();
            foreach (Room nextRoom in room.GetNextRooms())
            {
                if (Vector2.Distance(room.position, nextRoom.position) < distanceMax)
                {
                    
                    Vector2 attractVector = new Vector2(
                        nextRoom.position.x - room.position.x,
                        nextRoom.position.y - room.position.y
                    ) * edgeForce / 1000000;

                    float angle = Vector2.Angle(attractVector, Vector2.right);

                    Vector2 repulseVector = new Vector2(
                        (float)Math.Cos(angle) * distanceMin,
                        (float)Math.Sin(angle) * distanceMin
                    ) * edgeForce / 1000000;

                    resultVector += attractVector + repulseVector;
                }
                

                Vector2 anchorVector = new Vector2(
                    -room.position.x,
                    -room.position.y
                ) * anchorForce / 1000000;

                resultVector += anchorVector;
            }

            room.position += resultVector;
        }
    }

    void AroundTransforms()
    {
        foreach (Room room in roomList)
        {
            room.position = new Vector2(
                Mathf.Round(room.position.x),
                Mathf.Round(room.position.y)
                );

            room.size = new Vector2(
                Mathf.Round(room.size.x),
                Mathf.Round(room.size.y)
                );
        }
    }

    void GenerateNodes()
    {
        for (int id = 0; id < roomCount; id++)
        {
            int randomX = UnityEngine.Random.Range(0, stageSize.x);
            int randomY = UnityEngine.Random.Range(0, stageSize.y);

            Vector2Int position = new Vector2Int(randomX, randomY);
            Room room = new Room(id, position);
            room.nextRoomsMax = UnityEngine.Random.Range(0, roomNextMax + 1) + 1;
            roomList.Add(room);
        }

    }

    void GenerateEdges()
    {
        Room firstRoom = roomList[0];
        // Parcours en profondeur des salles
        List<Room> visitedRooms = new List<Room>();
        List<Room> roomsToVisit = roomList.ToList();
        while (roomsToVisit.Count > 0)
        {
            Room room = roomsToVisit[0];
            roomsToVisit.RemoveAt(0);
            visitedRooms.Add(room);

            // Get closest room by position
            List<Room> closestRooms = roomList.OrderBy((r) => (r.position - room.position).sqrMagnitude).ToList();
            
            foreach (Room otherRoom in closestRooms)
            {
                if (room.GetNextRooms().Count >= room.nextRoomsMax) break;
                if (
                    otherRoom.id != room.id &&
                    otherRoom.GetNextRooms().Count < otherRoom.nextRoomsMax &&
                    !room.IsNextRoom(otherRoom) &&
                    !otherRoom.IsNextRoom(room)
                    )
                {
                    room.AddNextRoom(otherRoom);
                    otherRoom.AddNextRoom(room);
                }
            }

        }
    }

    void SizeGeneration()
    {
        foreach (Room room in roomList)
        {
            float sizeX = (float)Math.Round(Math.Abs((room.GetNextRooms().Last().position.x - room.position.x)) / 2);
            float sizeY = (float)Math.Round(Math.Abs((room.GetNextRooms().Last().position.y - room.position.y)) / 2);
            float size = Math.Max(sizeY, sizeX);
            Debug.Log(size);
            room.size = new Vector2(size, size);
            Debug.Log(room.size);
        }
    }

    void GenerateGrid()
    {
        foreach (Room room in roomList)
        {
            // Création des salles
            for (int x = room.GetMinX() ; x < room.GetMaxX(); x++)
            {
                for (int y = room.GetMinY(); y < room.GetMaxY(); y++)
                {
                    InstanciateCell(new Vector2(x, y));
                }
            }

            // Création des couloirs
            foreach(Room nextRoom in room.GetNextRooms())
            {
                int minX = (int)Math.Min(room.position.x, nextRoom.position.x);
                int maxX = (int)Math.Max(room.position.x, nextRoom.position.x);
                int minY = (int)Math.Min(room.position.y, nextRoom.position.y);
                int maxY = (int)Math.Max(room.position.y, nextRoom.position.y);

                for (int x = minX; x <= maxX; x++)
                {
                    InstanciateCell(new Vector2(x, room.position.y));
                }

                for (int y = minY; y <= maxY; y++)
                {
                    InstanciateCell(new Vector2(room.position.x, y));
                }
            }
        }
    }

    void InstanciateCell(Vector2 position)
    {
        GridCell cell = new GridCell(position, RoomType.Room);
        grid.Add(cell);
        GameObject floor = Instantiate(floorPrefab);
        floor.transform.position = new Vector3(position.x, 0, position.y);
        floor.transform.localScale = Vector3.one / 10;
    }

    void InstanciateWall(Vector2 position, Vector2 direction)
    {
        GameObject wall = Instantiate(wallPrefab);
        wall.transform.localScale = Vector3.one / 10;

        Vector3 newPosition = new Vector3(position.x, 0, position.y);
        Vector3 newRotation = new Vector3(0, 0, 0);

        if (direction.x == 1.0 || direction.x == -1.0)
        {
            newRotation.y = 90;
            newPosition.z += 1;
        } 
        if (direction.y == -1)
        {
            newPosition.z += 1;
        }
        if (direction.x == -1)
        {
            newPosition.x += 1;
        }

        Debug.Log(direction);
        Debug.Log(newRotation);

        wall.transform.position = newPosition;
        wall.transform.Rotate(newRotation);
    }

    bool IsCellInPosition(Vector2 position)
    {
        // Retourne vrai si une cellule à une position dans la grille
        return grid.Where((cell) => cell.position == position).Count() > 0;
    }

    void AddWalls()
    {
        // Une liste d'entier avec -1 et 1
        List<int> directions = new List<int>() { -1, 0, 1 };
        foreach (GridCell cell in grid)
        {
            foreach (int x in directions)
            {
                foreach (int y in directions)
                {
                    if (Math.Abs(x + y) == 1)
                    {
                        Vector2 checkPosition = cell.position + new Vector2(x, y);
                        if (!IsCellInPosition(checkPosition))
                        {
                            InstanciateWall(checkPosition, new Vector2(x, y));
                        }
                    }

                }
            }
        }
    }

    void GenerateGraph()
    {
        GenerateNodes();
        GenerateEdges();
        for (int i = 0; i < 1000; i++)
        {
            AdjustPosition();
        }
        AroundTransforms();
        SizeGeneration();
        GenerateGrid();
        AddWalls();

        // Edges generation
        /*       foreach (Room room in roomList)
               {
                   // Get closest room by position
                   int randomRoomCount = UnityEngine.Random.Range(1, roomNextMax + 1) + 1;
                   List<Room> closestRooms = roomList.OrderBy((r) => (r.position - room.position).sqrMagnitude).ToList();
                   for (int i = 0; i < closestRooms.Count(); i++)
                   {
                       Room otherRoom = closestRooms[i];
                       if (
                           otherRoom.id != room.id && 
                           otherRoom.GetNextRooms().Count < roomNextMax && 
                           room.GetNextRooms().Count < roomNextMax &&
                           !room.IsNextRoom(otherRoom)
                           )
                       {
                           room.AddNextRoom(otherRoom);
                           otherRoom.AddNextRoom(room);
                       }
                   }
               }*/

        // Size generation

    }
    
    private void OnDrawGizmos()
    {
        foreach (Room room in roomList)
        {
            Vector3 position = new Vector3(room.position.x, 0, room.position.y);
            Vector3 size = new Vector3(room.size.x, 1, room.size.y);
            Gizmos.DrawWireCube(position, size);
            foreach (Room nextRoom in room.GetNextRooms())
            {
                Vector3 nextPosition = new Vector3(nextRoom.position.x, 0, nextRoom.position.y);
                Gizmos.DrawLine(position, nextPosition);
            }
            
        }
    }

}