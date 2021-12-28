using System.Collections.Generic;
using UnityEngine;


// based on https://kairumagames.com/blog/cavetutorial
public class PrimMapGenerator
{
    static public int[,] GenerateMap(int width, int height)
    {
        int[,] map = new int[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                map[i, j] = 1;
            }
        }

        int x = 2;
        int y = 2;
        map[x, y] = 0;
        List<Vector2> toCheck = new List<Vector2>();

        toCheck.Add(new Vector2(x + 1, y));
        toCheck.Add(new Vector2(x, y+1));

        while(toCheck.Count > 0) 
        {
            int randomIndex = Random.Range(0, toCheck.Count - 1);

            Vector2 cell = toCheck[randomIndex];
            x = (int)cell.x;
            y = (int)cell.y;
            map[x, y] = 0;
            toCheck.RemoveAt(randomIndex);

            if (y - 1 > 0) 
            {
                Vector2 newCell = new Vector2(cell.x, cell.y-1);
                if (canPlaceCell(Directions.South, newCell, map)) 
                {
                    map[(int)newCell.x, (int)newCell.y] = 0;
                    toCheck.Add(newCell);
                }
            }
            if (y + 1 < height-1)
            {
                Vector2 newCell = new Vector2(cell.x, cell.y + 1);
                if (canPlaceCell(Directions.North, newCell, map))
                {
                    map[(int)newCell.x, (int)newCell.y] = 0;
                    toCheck.Add(newCell);
                }
            }
            if (x - 1 > 0)
            {
                Vector2 newCell = new Vector2(cell.x-1, cell.y);
                if (canPlaceCell(Directions.West, newCell, map))
                {
                    map[(int)newCell.x, (int)newCell.y] = 0;
                    toCheck.Add(newCell);
                }
            }
            if (x + 1 < width-1)
            {
                Vector2 newCell = new Vector2(cell.x+1, cell.y);
                if (canPlaceCell(Directions.East, newCell, map))
                {
                    map[(int)newCell.x, (int)newCell.y] = 0;
                    toCheck.Add(newCell);
                }
            }
        }

        for(int i = 0; i < 2; i++) 
        {
            RemoveDeadEnds(map);
        }
        for (int i = 0; i < 2; i++)
        {
            CellularAutomata(map);
        }
        while (true) 
        {
            if (!RemoveDeadEnds(map)) break;
        }
        map[2, 2] = 2;
        return map;
    }

    private static bool canPlaceCell(Directions direction, Vector2 cell, int[,] map) 
    {
        switch (direction) 
        {
            case Directions.North:
                for(int x = (int)cell.x-1; x <= cell.x +1; x++) 
                {
                    for (int y = (int)cell.y; y <= cell.y + 1; y++) 
                    {
                        if (x == cell.x && y == cell.y) continue;
                        if(map[x,y] == 0) 
                        {
                            return false;
                        }
                    }
                }
                break;
            case Directions.South:
                for (int x = (int)cell.x - 1; x <= cell.x + 1; x++)
                {
                    for (int y = (int)cell.y-1; y <= cell.y; y++)
                    {
                        if (x == cell.x && y == cell.y) continue;
                        if (map[x, y] == 0)
                        {
                            return false;
                        }
                    }
                }
                break;
            case Directions.East:
                for (int x = (int)cell.x; x <= cell.x + 1; x++)
                {
                    for (int y = (int)cell.y - 1; y <= cell.y+1; y++)
                    {
                        if (x == cell.x && y == cell.y) continue;
                        if (map[x, y] == 0)
                        {
                            return false;
                        }
                    }
                }
                break;
            case Directions.West:
                for (int x = (int)cell.x-1; x <= cell.x; x++)
                {
                    for (int y = (int)cell.y - 1; y <= cell.y + 1; y++)
                    {
                        if (x == cell.x && y == cell.y) continue;
                        if (map[x, y] == 0)
                        {
                            return false;
                        }
                    }
                }
                break;
            default:
                return false;
        }
        return true;
    }

    //Remove dead ends - return true if any dead end exist
    private static bool RemoveDeadEnds(int [,] map) 
    {
        int height = map.GetLength(1);
        int width = map.GetLength(0);
        List<Vector2> deadEnds = new List<Vector2>();
        for(int x = 0; x<width; x++) 
        {
            for (int y = 0; y < height; y++) 
            {
                if (map[x, y] == 1) continue;
                int neighbors = 0;
                if (y - 1 >= 0) 
                {
                    if (map[x, y - 1] == 0) neighbors++;
                }
                if (y + 1 < height)
                {
                    if (map[x, y + 1] == 0) neighbors++;
                }
                if (x - 1 >= 0)
                {
                    if (map[x-1, y] == 0) neighbors++;
                }
                if (x + 1 < width)
                {
                    if (map[x+1, y] == 0) neighbors++;
                }
                if (neighbors <= 1) deadEnds.Add(new Vector2(x, y));
            }
        }
        if (deadEnds.Count == 0) return false;
        foreach(Vector2 end in deadEnds) 
        {
            map[(int)end.x, (int)end.y] = 1;
        }
        return true;
    }

    private static void CellularAutomata(int[,] map) 
    {
        int height = map.GetLength(1);
        int width = map.GetLength(0);
        List<Vector2> toChange = new List<Vector2>();
        for(int x = 0; x < width; x++) 
        {
            for(int y = 0; y < height; y++)
            {
                int neighbors = 0;
                for(int nx = x-1; nx<=x+1; nx++) 
                {
                    for (int ny = y - 1; ny <= y + 1; ny++) 
                    {
                        if (ny < 0 || ny >= height || nx < 0 || nx >= width || (x == nx && y == ny)) continue;
                        if (map[nx, ny] == 0) neighbors++;
                    }
                }

                if(neighbors >= 4) 
                {
                    toChange.Add(new Vector2(x, y));
                }
            }
        }

        foreach(Vector2 cell in toChange) 
        {
            map[(int)cell.x, (int)cell.y] = 0;
        }
    }


    private enum Directions 
    {
        North = 0,
        South,
        East,
        West,
    }

}


