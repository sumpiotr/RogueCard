using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileDataScriptableObject", menuName = "TileData")]
public class TileDataScriptableObject : ScriptableObject
{
    public Sprite image;
    public string name;
    public int moveCost;
    public bool walkable;
    public string description;
    public LayerMask layer;
}
