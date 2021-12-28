using System;
using System.Collections.Generic;
using UnityEngine;


//Based on https://www.youtube.com/watch?v=v7yyZZjF1z4&t
public class MapGenerator
{

    static public int[,] generateMap(Grid grid, string seed, int randomFillPercent)
    {
        int width = (int)grid.getSize().x;
        int height = (int)grid.getSize().y;
        int[,] map = new int[width, height];
        System.Random pseudoGenerator = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map[x, y] = 1;
                    continue;
                }
                map[x, y] = pseudoGenerator.Next(0, 100) < randomFillPercent ? 1 : 0;
            }
        }
        for (int i = 0; i < 5; i++)
        {
            SmoothMap(map);
        }

        ProcessMap(map);

        return map;
    }

    static public int[,] generateMap(Grid grid, int randomFillPercent)
    {
        string seed = getRandomSeed();
        Debug.Log(seed);
        return generateMap(grid, seed, randomFillPercent);
    }

    static string getRandomSeed()
    {
        string seed = "";
        for (int i = 0; i < 10; i++)
        {
            seed += (char)UnityEngine.Random.Range(0, 127);
        }
        return seed;
    }

    static private void SmoothMap(int[,] map)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);
        int[,] smoothMap = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int wallsCount = getSurroundingWallCount(map, x, y);
                if (wallsCount > 4)
                {
                    smoothMap[x, y] = 1;
                }
                else if (wallsCount < 4)
                {
                    smoothMap[x, y] = 0;
                }
                else
                {
                    smoothMap[x, y] = map[x, y];
                }
            }
        }
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = smoothMap[x, y];
            }
        }


    }

    static private void ProcessMap(int[,] map)
    {
        List<List<Vector2>> wallRegions = getAllRegionsOfType(1, map);

        int minWallRegionSize = 50;

        foreach (List<Vector2> wallRegion in wallRegions)
        {
            if (wallRegion.Count < minWallRegionSize)
            {
                foreach (Vector2 tile in wallRegion)
                {
                    map[(int)tile.x, (int)tile.y] = 0;
                }
            }
        }


        List<List<Vector2>> floorRegions = getAllRegionsOfType(0, map);

        int minFloorRegionSize = 50;

        List<Room> rooms = new List<Room>();

        foreach (List<Vector2> floorRegion in floorRegions)
        {
            if (floorRegion.Count < minFloorRegionSize)
            {
                foreach (Vector2 tile in floorRegion)
                {
                    map[(int)tile.x, (int)tile.y] = 1;
                }
            }
            else
            {
                rooms.Add(new Room(floorRegion, map));
            }
        }
        rooms.Sort();
        rooms[0].isMainRoom = true;
        rooms[0].isAccessibleFromMainRoom = true;
        ConnectClosestRooms(rooms, map);

        Vector2 playerPosition = rooms[rooms.Count - 1].tiles[0];
        map[(int)playerPosition.x, (int)playerPosition.y] = 2;
    }

    static private void ConnectClosestRooms(List<Room> rooms, int[,] map, bool forceAccessibilityFromMainRoom = false)
    {

        List<Room> listRoomA = new List<Room>();
        List<Room> listRoomB = new List<Room>();

        if (forceAccessibilityFromMainRoom) 
        {
            foreach(Room room in rooms) 
            {
                if (room.isAccessibleFromMainRoom) listRoomB.Add(room);
                else listRoomA.Add(room);
            }
        }
        else 
        {
            listRoomA = rooms;
            listRoomB = rooms;
        }

        int bestDistance = 0;
        Vector2 bestTileA = Vector2.zero;
        Vector2 bestTileB = Vector2.zero;
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;


        foreach (Room roomA in listRoomA)
        {
            if (!possibleConnectionFound) 
            {
                possibleConnectionFound = false;
                if (roomA.connectedRooms.Count > 0) continue;
            }
            foreach (Room roomB in listRoomB)
            {
                if (roomA == roomB || roomA.IsRoomConnected(roomB)) continue;
                foreach (Vector2 tileA in roomA.edgeTiles)
                {
                    foreach (Vector2 tileB in roomB.edgeTiles)
                    {
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.x - tileB.x, 2) + Mathf.Pow(tileA.y - tileB.y, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                            bestTileA = tileA;
                            bestTileB = tileB;
                        }
                    }
                }
            }

            if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB, map);
            }
        }

        if (forceAccessibilityFromMainRoom && possibleConnectionFound) 
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB, map);
            ConnectClosestRooms(rooms, map, true);
        }
        if (!forceAccessibilityFromMainRoom) 
        {
            ConnectClosestRooms(rooms,map,  true);
        }
    }

    static private void CreatePassage(Room roomA, Room roomB, Vector2 tileA, Vector2 tileB,int[,] map)
    {
        Room.ConnectRooms(roomA, roomB);

        List<Vector2> line = GetLine(tileA, tileB);
        foreach(Vector2 point in line) 
        {
            DrawCircle(point, 1, map);
        }
    }

    static private void DrawCircle(Vector2 c, int r, int[,] map) 
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);
        for (int x = -r; x  <= r; x++) 
        {
            for (int y = -r; y <= r; y++) 
            {
                if(x*x + y*y <= r * r) 
                {
                    int drawX = (int)c.x + x;
                    int drawY = (int)c.y + y;
                    if ((y >= 0 && y < height && x >= 0 && x < width))
                    {
                        map[drawX, drawY] = 0;
                    }
                }
            }
        }
    }

    static private List<Vector2> GetLine(Vector2 start, Vector2 end) 
    {
        List<Vector2> line = new List<Vector2>();

        int x = (int)start.x;
        int y = (int)start.y;

        int deltaX = (int)end.x - x;
        int deltaY = (int)end.y - y;

        int step = Math.Sign(deltaX);
        int gradientStep = Math.Sign(deltaY);

        bool inverted = false;
        int longest = Math.Abs(deltaX);
        int shortest = Math.Abs(deltaY);
        if(longest < shortest) 
        {
            inverted = true;
            longest = Math.Abs(deltaY); 
            shortest = Math.Abs(deltaX);

            step = Math.Sign(deltaY);
            gradientStep = Math.Sign(deltaX);
        }

        int gradientAccumulation = longest / 2;
        for(int i = 0; i < longest; i++) 
        {
            line.Add(new Vector2(x, y));

            if (inverted) 
            {
                y += step;
            }
            else 
            {
                x += step;
            }

            gradientAccumulation += shortest;

            if(gradientAccumulation >= longest) 
            {
                if (inverted) 
                {
                    x += gradientStep;
                }
                else 
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }



    static private List<List<Vector2>> getAllRegionsOfType(int tileType, int[,] map)
    {
        List<List<Vector2>> regions = new List<List<Vector2>>();
        int width = map.GetLength(0);
        int height = map.GetLength(1);
        int[,] mapFlags = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                {
                    List<Vector2> newRegion = getRegionTiles(x, y, map);
                    regions.Add(newRegion);

                    foreach (Vector2 tile in newRegion)
                    {
                        mapFlags[(int)tile.x, (int)tile.y] = 1;
                    }
                }
            }
        }
        return regions;
    }

    static private List<Vector2> getRegionTiles(int startX, int startY, int[,] map)
    {
        List<Vector2> tiles = new List<Vector2>();
        int width = map.GetLength(0);
        int height = map.GetLength(1);
        int tileType = map[startX, startY];
        int[,] mapFlags = new int[width, height];
        Queue<Vector2> queue = new Queue<Vector2>();
        queue.Enqueue(new Vector2(startX, startY));
        mapFlags[startX, startY] = 1;
        while (queue.Count > 0)
        {
            Vector2 tile = queue.Dequeue();
            tiles.Add(tile);
            for (int x = (int)tile.x - 1; x <= tile.x + 1; x++)
            {
                for (int y = (int)tile.y - 1; y <= tile.y + 1; y++)
                {
                    if ((y >= 0 && y < height && x >= 0 && x < width) && (x == tile.x || y == tile.y))
                    {
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Vector2(x, y));
                        }
                    }
                }
            }
        }
        return tiles;

    }

    static private int getSurroundingWallCount(int[,] map, int gridX, int gridY)
    {
        int wallCount = 0;
        int width = map.GetLength(0);
        int height = map.GetLength(1);
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (neighbourY >= 0 && neighbourY < height && neighbourX >= 0 && neighbourX < width)
                {
                    if (neighbourY != gridY || neighbourX != gridX)
                    {
                        wallCount += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }
        return wallCount;
    }
}


class Room : IComparable<Room>
{
    public List<Vector2> tiles;
    public List<Vector2> edgeTiles;
    public List<Room> connectedRooms;
    public bool isMainRoom = false;
    public bool isAccessibleFromMainRoom = false;
    int roomSize;

    public Room() { }

    public Room(List<Vector2> roomTiles, int[,] map)
    {
        tiles = roomTiles;
        roomSize = roomTiles.Count;

        edgeTiles = new List<Vector2>();
        connectedRooms = new List<Room>();

        foreach (Vector2 tile in roomTiles)
        {
            for (int x = (int)tile.x - 1; x <= tile.x + 1; x++)
            {
                for (int y = (int)tile.y - 1; y <= tile.y + 1; y++)
                {
                    if (x == tile.x || y == tile.y)
                    {
                        if (map[x, y] == 1)
                        {
                            edgeTiles.Add(tile);
                        }
                    }
                }
            }
        }
    }

    public void SetAccessibleFromMainRoom() 
    {
        if (isAccessibleFromMainRoom) 
        {
            isAccessibleFromMainRoom = true;
            foreach(Room room in connectedRooms) 
            {
                room.SetAccessibleFromMainRoom();
            }
        }
    }

    public static void ConnectRooms(Room a, Room b)
    {
        if (a.isAccessibleFromMainRoom) 
        {
            b.SetAccessibleFromMainRoom();
        }
        else if (b.isAccessibleFromMainRoom) 
        {
            a.SetAccessibleFromMainRoom();
        }
        a.connectedRooms.Add(b);
        b.connectedRooms.Add(a);
    }

    public int CompareTo(Room other)
    {
        return other.roomSize.CompareTo(roomSize);
    }

    public bool IsRoomConnected(Room other)
    {
        return connectedRooms.Contains(other);
    }
}