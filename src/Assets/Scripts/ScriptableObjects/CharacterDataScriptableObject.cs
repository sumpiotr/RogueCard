using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Character", fileName = "CharacterDataScriptableObject")]
public class CharacterDataScriptableObject : ScriptableObject
{
    public int difficulty; //difficulty level
    public int health;
    public int strength;
    public int dexterity;
    public int defense;
    public int speed;
    public int viewDistance;
    public int dropChance; // 0 to 100; if character is player bonus chance to get better items
}
