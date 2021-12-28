using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridDataScriptableObject", menuName ="GridData")]
public class GridDataScriptableObject : ScriptableObject
{
    public int width;
    public int height;
    public Vector3 startPosition;
    public List<SimpleAI> enemies;

    public Tile tilePrefab;
    public TileDataScriptableObject wallData;
}
