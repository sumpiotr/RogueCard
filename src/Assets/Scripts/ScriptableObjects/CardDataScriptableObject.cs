using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardDataScriptableObject", menuName ="Card")]
public class CardDataScriptableObject : ScriptableObject
{
    public string cardName;
    public Cards card;
    public string description;
    public int duration;
    public Sprite image;
    public int range;

    /* Data for AI
        value:
        -1 - cannot do that thing
        0 - stat equal do default character stat
        >0 - bonus
     */


    public int move;
    public int attack;

}


