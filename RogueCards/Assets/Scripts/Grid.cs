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


    public Vector2 GetSize()
    {
        return new Vector2(gridData.width, gridData.height);
    }

    public void SetGridData(GridDataScriptableObject data) 
    {
        gridData = data;
        tiles.Clear();
    }

    public GridDataScriptableObject GetGridData() 
    {
        return gridData;
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

    public void CreateExit() 
    {
        int mainSize = gridData.width >= gridData.height ? gridData.width : gridData.height;
        int minDistanceFromPlayer = Mathf.RoundToInt(mainSize);
        List<Tile> emptyTiles = new List<Tile>();
        for (float x = gridData.startPosition.x; x < gridData.startPosition.x + gridData.width; x++)
        {
            for (float y = gridData.startPosition.y; y < gridData.startPosition.y + gridData.height; y++)
            {
                Tile tile = GetTileByPosition(new Vector2(x, y));
                if (IsTileWalkableAndEmpty(new Vector2(x, y)))
                {
                    if (Mathf.Abs(Vector2.Distance(tile.transform.position, GameController.Instance.player.transform.position)) < minDistanceFromPlayer)
                    {
                        continue;
                    }
                    emptyTiles.Add(tile);
                }
            }
        }
        Tile randomTile = emptyTiles[Random.Range(0, emptyTiles.Count)];
        GameController.Instance.SpawnExit(randomTile.transform.position);

    }

    public void Spawn(int minDistanceFromPlayer, int minDistanceBetween, int maxValue, List<ISpawnable> objects) 
    {
        List<Tile> emptyTiles = new List<Tile>();
        for (float x = gridData.startPosition.x; x < gridData.startPosition.x + gridData.width; x++)
        {
            for (float y = gridData.startPosition.y; y < gridData.startPosition.y + gridData.height; y++)
            {
                if (IsTileWalkableAndEmpty(new Vector2(x, y)))
                {
                    Tile tile = GetTileByPosition(new Vector2(x, y));
                    if (Mathf.Abs(Vector2.Distance(tile.transform.position, GameController.Instance.player.transform.position)) < minDistanceFromPlayer)
                    {
                        continue;
                    }
                    emptyTiles.Add(tile);
                }
            }
        }

        int level = 0;
        List<GameObject> instances = new List<GameObject>();
        while (level < maxValue)
        {
            if (emptyTiles.Count == 0) break;
            ISpawnable prefab = objects[Random.Range(0, objects.Count)];
            Tile tile = emptyTiles[Random.Range(0, emptyTiles.Count)];
            bool canSpawn = true;
            foreach(GameObject instance in instances) 
            {
                if(Mathf.Abs(Vector2.Distance(tile.transform.position, instance.transform.position)) < minDistanceBetween) 
                {
                    emptyTiles.Remove(tile);
                    canSpawn = false;
                    break;
                }
            }
            if (!canSpawn) continue;
            (GameObject instance, int value) instanceData = prefab.Spawn(tile); 
            level += instanceData.value;
            emptyTiles.Remove(tile);
            instances.Add(instanceData.instance);
        }
    }

    public Dictionary<Vector2, Tile> GetTiles()
    {
        return tiles;
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

        if (GetTileByPosition(new Vector2(tilePosition.x + 1, tilePosition.y)) != null &&
            GetTileByPosition(new Vector2(tilePosition.x, tilePosition.y + 1)) != null)
        {
            if (GetTileByPosition(new Vector2(tilePosition.x + 1, tilePosition.y)).IsWalkable()
            || GetTileByPosition(new Vector2(tilePosition.x, tilePosition.y + 1)).IsWalkable())
            {
                positions.Add(new Vector2(tilePosition.x + 1, tilePosition.y + 1));
            }
        }


        if (GetTileByPosition(new Vector2(tilePosition.x - 1, tilePosition.y)) != null && GetTileByPosition(new Vector2(tilePosition.x, tilePosition.y - 1)) != null)
        {
            if (GetTileByPosition(new Vector2(tilePosition.x - 1, tilePosition.y)).IsWalkable()
            || GetTileByPosition(new Vector2(tilePosition.x, tilePosition.y - 1)).IsWalkable())
            {
                positions.Add(new Vector2(tilePosition.x - 1, tilePosition.y - 1));
            }
        }


        if (GetTileByPosition(new Vector2(tilePosition.x - 1, tilePosition.y)) != null &&
            GetTileByPosition(new Vector2(tilePosition.x, tilePosition.y + 1)) != null)
        {
            if (GetTileByPosition(new Vector2(tilePosition.x - 1, tilePosition.y)).IsWalkable()
           || GetTileByPosition(new Vector2(tilePosition.x, tilePosition.y + 1)).IsWalkable())
            {
                positions.Add(new Vector2(tilePosition.x - 1, tilePosition.y + 1));
            }
        }




        if (GetTileByPosition(new Vector2(tilePosition.x + 1, tilePosition.y)) != null &&
             GetTileByPosition(new Vector2(tilePosition.x, tilePosition.y - 1)) != null)  
        {
            if (GetTileByPosition(new Vector2(tilePosition.x + 1, tilePosition.y)).IsWalkable()
            || GetTileByPosition(new Vector2(tilePosition.x, tilePosition.y - 1)).IsWalkable())
            {
                positions.Add(new Vector2(tilePosition.x + 1, tilePosition.y - 1));
            }
        }


        if(GetTileByPosition(new Vector2(tilePosition.x - 1, tilePosition.y)) != null)positions.Add(new Vector2(tilePosition.x - 1, tilePosition.y));
        if (GetTileByPosition(new Vector2(tilePosition.x, tilePosition.y-1)) != null) positions.Add(new Vector2(tilePosition.x, tilePosition.y - 1));
        if (GetTileByPosition(new Vector2(tilePosition.x + 1, tilePosition.y)) != null) positions.Add(new Vector2(tilePosition.x + 1, tilePosition.y));
        if (GetTileByPosition(new Vector2(tilePosition.x, tilePosition.y+1)) != null) positions.Add(new Vector2(tilePosition.x, tilePosition.y + 1));
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
