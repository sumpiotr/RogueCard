  using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpawnable 
{
    (GameObject instance, int value) Spawn(Tile tile);
}
