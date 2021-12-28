using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AStar 
{
    public static List<Node> findPath(Grid grid, Vector2 startPosition, Vector2 targetPosition, int maxPathLength=-1) 
    {
        Node currentNode = null;
        List<Node> openList = new List<Node>();
        List<Node> closedList = new List<Node>();


        openList.Add(new Node(startPosition, targetPosition, startPosition));

        while(openList.Count > 0) 
        {
            Node lowest = openList[0];
            foreach(Node node in openList) 
            {
                if (node.fCost < lowest.fCost) lowest = node;
            }
            currentNode = lowest;

            closedList.Add(currentNode);
            openList.Remove(currentNode);

            if (getPathLength(currentNode)-1 > maxPathLength && maxPathLength > 0) 
            {
                return null;
            }
            if (currentNode.nodePosition == targetPosition) 
            {
                return GetPathFromNode(currentNode);
            }

            List<Vector2> adjacentedNodes = grid.GetAdjacentedWalkablesTilesPositions(currentNode.nodePosition);

            foreach (Vector2 nodePosition in adjacentedNodes)
            {
                Tile tile = grid.GetTileByPosition(nodePosition);
                if (getNodeByPosition(closedList, nodePosition) != null) continue;
                if (!tile.IsEmpty() && nodePosition != targetPosition) continue;
                Node adjacentedNode = getNodeByPosition(openList, nodePosition);
                if (adjacentedNode == null) adjacentedNode = new Node(nodePosition, targetPosition, startPosition);
                if (openList.Contains(adjacentedNode))
                {
                    float tentativeGCost = currentNode.gCost + Vector2.Distance(currentNode.nodePosition, adjacentedNode.nodePosition);
                    if (tentativeGCost < adjacentedNode.gCost)
                    {
                        adjacentedNode.previousNode = currentNode;
                        adjacentedNode.gCost = tentativeGCost;
                        adjacentedNode.fCost = adjacentedNode.hCost + adjacentedNode.gCost;

                    }
                }
                else
                {
                    adjacentedNode.previousNode = currentNode;
                    openList.Add(adjacentedNode);
                }
               
            }
        }
        return null;
    }

    private static Node getNodeByPosition(List<Node> nodeList, Vector2 position) 
    {
        foreach(Node node in nodeList) 
        {
            if (node.nodePosition == position) return node;
        }
        return null;
    }

    private static List<Node> GetPathFromNode(Node endNode) 
    {
        if (endNode.previousNode == null) return null;
        List<Node> path = new List<Node>();
        Node currentNode = endNode;
        while(currentNode.previousNode != null) 
        {
            path.Add(currentNode);
            currentNode = currentNode.previousNode;
        }
        path.Reverse();
        return path;
    }

    private static int getPathLength(Node endNode) 
    {
        if (endNode.previousNode == null) return 0;
        List<Node> path = new List<Node>();
        path.Add(endNode);
        Node currentNode = endNode;
        while (currentNode.previousNode != null)
        {
            path.Add(currentNode);
            currentNode = currentNode.previousNode;
        }
        return path.Count;
    }

    public static List<Vector2> GetWalkableAndEmptyTilesInRange(Vector2 start, int range,  Grid grid) 
    {
        List<Vector2> tiles = new List<Vector2>();
        Queue<Node> toCheck = new Queue<Node>();
        toCheck.Enqueue(new Node(start));
        while(toCheck.Count > 0) 
        {
            Node tile = toCheck.Dequeue();
            tiles.Add(tile.nodePosition);
            List<Vector2> adjencentTiles = grid.GetAdjacentedWalkablesAndEmptyTilesPositions(tile.nodePosition);
            foreach(Vector2 newTile in adjencentTiles)
            {
                Node node = new Node(newTile);
                node.previousNode = tile;
                bool check = true;
                if(getPathLength(node)-1 > range || tiles.Contains(newTile)) 
                {
                    continue;
                }
                if (!check) continue;
                toCheck.Enqueue(node);
            }
        }
        tiles.Remove(start);
        return tiles;
    }

    public static List<Vector2> GetWalkableTilesInRange(Vector2 start, int range, Grid grid)
    {
        List<Vector2> tiles = new List<Vector2>();
        Queue<Node> toCheck = new Queue<Node>();
        toCheck.Enqueue(new Node(start));
        while (toCheck.Count > 0)
        {
            Node tile = toCheck.Dequeue();
            tiles.Add(tile.nodePosition);
            List<Vector2> adjencentTiles = grid.GetAdjacentedWalkablesTilesPositions(tile.nodePosition);
            foreach (Vector2 newTile in adjencentTiles)
            {
                Node node = new Node(newTile);
                node.previousNode = tile;
                if (getPathLength(node) - 1 > range || tiles.Contains(newTile))
                {
                    continue;
                }
                toCheck.Enqueue(node);
            }
        }
        tiles.Remove(start);
        return tiles;
    }
    public static List<Vector2> GetTilesInRange(Vector2 start, int range, Grid grid)
    {
        List<Vector2> tiles = new List<Vector2>();
        for (int x = (int)start.x - range; x <= start.x + range; x++) 
        {
            for (int y = (int)start.y - range; y <= start.y + range; y++)
            {
                if (x < 0 || y < 0 || x >= grid.getSize().x || y >= grid.getSize().y || (x == start.x && y == start.y)) continue;
                tiles.Add(new Vector2(x, y));
            }
        }
        return tiles;
    }
    public static List<Vector2> GetVisibleTilesInRange(Vector2 start, int range, Grid grid)
    {
        List<Vector2> tiles = GetTilesInRange(start, range, grid);
        foreach(Vector2 pos in tiles)
        {
            List<Vector2> path = findShortestPath(grid, start, pos, null);

        }
        return tiles;
    }

    public static List<Vector2> findShortestPath(Grid grid, Vector2 startPosition, Vector2 targetPosition, List<Vector2> tiles = null)
    {
        //zwraca najkrotsza sciezke bez uwzgledniania zadnych zmiennych
        if (tiles == null) tiles = new List<Vector2>();

        int dirx = (targetPosition.x - startPosition.x) == 0 ? 0 : (int)((targetPosition.x - startPosition.x) / Math.Abs(targetPosition.x - startPosition.x));
        int diry = (targetPosition.y - startPosition.y) == 0 ? 0 : (int)((targetPosition.y - startPosition.y) / Math.Abs(targetPosition.y - startPosition.y));
        Vector2 newStartPos = new Vector2(startPosition.x + dirx, startPosition.y + diry);
        if (grid.GetTileByPosition(newStartPos) == grid.GetTileByPosition(targetPosition)) return tiles;
        tiles.Add(newStartPos);
        return findShortestPath(grid, newStartPos, targetPosition, tiles);
    }
}

public class Node
{
    public float fCost;
    public float gCost;
    public float hCost;
    
    public Vector2 nodePosition;

    public Node previousNode;

    public Node(Vector2 nodePosition) { this.nodePosition = nodePosition; }

    public Node(Vector2 nodePosition, Vector2 targetNodePosition, Vector2 startNodePosition, Node previousNode = null)
    {
        this.gCost = Vector2.Distance(startNodePosition, nodePosition);
        this.hCost = Vector2.Distance(nodePosition, targetNodePosition);
        this.nodePosition = nodePosition;
        fCost = gCost + hCost;

        this.previousNode = previousNode;
    }
}
