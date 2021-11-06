using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{

    [SerializeField] GridDataScriptableObject gridData;
    Dictionary<Vector2, Tile> tiles = new Dictionary<Vector2, Tile>();


    public void Initialize()
    {
        CreateGrid();
    }


    public Vector2 getSize()
    {
        return new Vector2(gridData.width, gridData.height);
    }

    void CreateGrid()
    {
        for (float x = gridData.startPosition.x; x < gridData.startPosition.x + gridData.width; x++)
        {
            for (float y = gridData.startPosition.y; y < gridData.startPosition.y + gridData.height; y++)
            {
                GameObject tile = Instantiate(gridData.tilePrefab.gameObject, new Vector3(x, y, gridData.startPosition.z), Quaternion.identity, transform);
                tile.GetComponent<Tile>().Initialize();
                tile.name = $"Tile {x} {y}";
                tiles.Add(new Vector2(x, y), tile.GetComponent<Tile>());
            }
        }

    }

    public Vector2 LoadMap(int[,] map)
    {
        Vector2 spawnPoint = Vector2.zero;
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (map[x, y] == 1) GetTileByPosition(x, y).ChangeTileData(gridData.wallData);
                else if (map[x, y] == 2) spawnPoint = new Vector2(x, y);
            }
        }
        return spawnPoint;
    }

    public Tile GetTileByPosition(Vector2 position)
    {
        return tiles.ContainsKey(position) ? tiles[position] : null;
    }

    public Tile GetTileByPosition(float x, float y)
    {
        Vector2 position = new Vector2(x, y);
        return tiles.ContainsKey(position) ? tiles[position] : null;
    }

    public BaseCharacter GetCharacterByPosition(Vector2 position)
    {
        return GetTileByPosition(position).getOccupyingCharacter();
    }

    public bool IsTileWalkableAndEmpty(Vector2 position)
    {
        Tile tile = GetTileByPosition(position);
        if (tile == null) return false;
        if (!tile.IsEmpty() || !tile.IsWalkable()) return false;
        return true;
    }


    public List<Vector2> GetAdjacentedWalkablesAndEmptyTilesPositions(Vector2 tilePosition)
    {
        List<Vector2> positions = GetAdjacentedTilesPositions(tilePosition);


        List<Vector2> invalidPositions = new List<Vector2>();

        foreach (Vector2 position in positions)
        {
            if (!IsTileWalkableAndEmpty(position)) invalidPositions.Add(position);
        }

        foreach (Vector2 position in invalidPositions)
        {
            positions.Remove(position);
        }

        return positions;

    }

    public List<Vector2> GetAdjacentedWalkablesTilesPositions(Vector2 tilePosition) 
    {
        List<Vector2> positions = GetAdjacentedTilesPositions(tilePosition);

        List<Vector2> invalidPositions = new List<Vector2>();

        foreach (Vector2 position in positions)
        {
            if (!GetTileByPosition(position).IsWalkable()) invalidPositions.Add(position);
        }

        foreach (Vector2 position in invalidPositions)
        {
            positions.Remove(position);
        }
        return positions;
    }

    public List<Vector2> GetAdjacentedTilesPositions(Vector2 tilePosition) 
    {
        List<Vector2> positions = new List<Vector2>();
        if (IsTileWalkableAndEmpty(new Vector2(tilePosition.x + 1, tilePosition.y))
           || IsTileWalkableAndEmpty(new Vector2(tilePosition.x, tilePosition.y + 1)))
        {
            positions.Add(new Vector2(tilePosition.x + 1, tilePosition.y + 1));
        }



        if (IsTileWalkableAndEmpty(new Vector2(tilePosition.x - 1, tilePosition.y))
          || IsTileWalkableAndEmpty(new Vector2(tilePosition.x, tilePosition.y - 1)))
        {
            positions.Add(new Vector2(tilePosition.x - 1, tilePosition.y - 1));
        }



        if (IsTileWalkableAndEmpty(new Vector2(tilePosition.x - 1, tilePosition.y))
           || IsTileWalkableAndEmpty(new Vector2(tilePosition.x, tilePosition.y + 1)))
        {
            positions.Add(new Vector2(tilePosition.x - 1, tilePosition.y + 1));
        }




        if (IsTileWalkableAndEmpty(new Vector2(tilePosition.x + 1, tilePosition.y))
    || IsTileWalkableAndEmpty(new Vector2(tilePosition.x, tilePosition.y - 1)))
        {
            positions.Add(new Vector2(tilePosition.x + 1, tilePosition.y - 1));
        }

        positions.Add(new Vector2(tilePosition.x - 1, tilePosition.y));
        positions.Add(new Vector2(tilePosition.x, tilePosition.y - 1));
        positions.Add(new Vector2(tilePosition.x + 1, tilePosition.y));
        positions.Add(new Vector2(tilePosition.x, tilePosition.y + 1));
        return positions;
    }

    public List<Tile> GetAdjacentedWalkablesTiles(Vector2 tilePosition)
    {

        List<Tile> tiles = new List<Tile>();

        List<Vector2> positions = new List<Vector2>();

        foreach (Vector2 position in positions) tiles.Add(GetTileByPosition(position));
        return tiles;
    }
}
