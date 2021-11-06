using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Character", fileName = "CharacterDataScriptableObject")]
public class CharacterDataScriptableObject : ScriptableObject
{
    public int health;
    public int strength;
    public int dexterity;
    public int defense;
    public int speed;
}
